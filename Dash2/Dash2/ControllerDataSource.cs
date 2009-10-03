using System;
using System.IO.Ports;

namespace Dash2
{
    class ControllerDataSource : DataSource
    {
        private SerialPort _serialPort;
        private byte[] sendData = new byte[7];
        private byte[] recvData = new byte[7];

        const byte SEND_FLAG = 0x5B;
        const byte RECV_FLAG = 0xB5;
        const byte READ_RAM  = 0x04;
        const byte THROTTLE_POS = 0x20;
        const byte THROTTLE_POS_CSUM = 0x81;
        const byte DIODE_TEMP = 0x2C;
        const byte DIODE_TEMP_CSUM = 0x75;
        const byte BATT_VOLT_AND_ERR = 0x39;
        const byte BATT_VOLT_AND_ERR_CSUM = 0x68;
        const byte OUTPUT_CURRENT = 0x60;
        const byte OUTPUT_CURRENT_CSUM = 0x41;

        const int ZERO_DEGREES_COUNTS = 559;
        const float VOLTS_PER_COUNT = 0.1025f;


        public void InitDataSource()
        {
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = "COM1";
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 50000;
            _serialPort.WriteTimeout = 50000;

            _serialPort.Open();

            sendData[0] = SEND_FLAG;
            sendData[1] = READ_RAM;
            sendData[3] = 0;
            sendData[4] = 0;
            sendData[5] = 0;
        }

        public void CloseDataSource()
        {
            _serialPort.Close();
        }

        private float lastThrottlePos = 0;
        public float GetThrottlePos()
        {
            sendData[2] = THROTTLE_POS;
            sendData[6] = THROTTLE_POS_CSUM;
            readWriteLoop(THROTTLE_POS);
            lastThrottlePos = (float)recvData[3] / 0xff;
            return lastThrottlePos;
        }

        private void printBytes(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                Console.WriteLine("\t{0:d2}: {1:x2}", i, b[i]);
            }
        }

        private bool expectedMsg(byte[] rcv, int type)
        {
            if (rcv[0] != RECV_FLAG) return false;
            if (rcv[1] != READ_RAM)  return false;
            if (rcv[2] != type)      return false;
            return true;
        }

        public void readWriteLoop(int type)
        {
            int bytes_read = 0;

            // Console.WriteLine("Sending {0:x2}", type);
            _serialPort.Write(sendData, 0, 7);
            while (bytes_read < 7)
            {
                bytes_read += _serialPort.Read(recvData, bytes_read, 7 - bytes_read);
            }

            if (!expectedMsg(recvData, type)) Console.WriteLine("Bad read !!{0:x2}.", type);
        }

        public float GetDiodeTemp()
        {
            sendData[2] = DIODE_TEMP;
            sendData[6] = DIODE_TEMP_CSUM;

            readWriteLoop(DIODE_TEMP);

            int counts = (recvData[4] << 8) + recvData[3];
            return (float)(counts - ZERO_DEGREES_COUNTS) / 2.048f;          
        }

        private bool voltNeedsUpdate = true;
        private bool errNeedsUpdate = true;

        private void updateVoltageAndError()
        {
            sendData[2] = BATT_VOLT_AND_ERR;
            sendData[6] = BATT_VOLT_AND_ERR_CSUM;

            readWriteLoop(BATT_VOLT_AND_ERR);

            voltNeedsUpdate = false;
            errNeedsUpdate = false;
        }

        public float GetBatteryVoltage()
        {
            if (voltNeedsUpdate) {
                updateVoltageAndError();
            }
            voltNeedsUpdate = true;
            int counts = (recvData[4] << 8) + recvData[3];
            return counts * VOLTS_PER_COUNT;
        }

        public DataSourceError getErrorStatus()
        {
            if (errNeedsUpdate)
            {
                updateVoltageAndError();
            }
            errNeedsUpdate = true;

            return (DataSourceError)recvData[5];
        }

        private float lastOutputCurrent = 0;
        public float GetOutputCurrent()
        {
            sendData[2] = OUTPUT_CURRENT;
            sendData[6] = OUTPUT_CURRENT_CSUM;

            readWriteLoop(OUTPUT_CURRENT);

            lastOutputCurrent = (recvData[4] << 8) + recvData[3];
            return lastOutputCurrent;    
        }

        public float GetBatteryCurrent()
        {
            return lastThrottlePos * lastOutputCurrent;
        }
    }
}

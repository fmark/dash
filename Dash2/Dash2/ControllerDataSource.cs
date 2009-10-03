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

        public float GetThrottlePos()
        {
            sendData[2] = THROTTLE_POS;
            sendData[6] = THROTTLE_POS_CSUM;

            _serialPort.Write(sendData, 0, 7);
            _serialPort.Read(recvData, 0, 7);

            return (float)recvData[3] / 255f;
        }

        public float GetBatteryCurrent()
        {
            return 0;
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

        public float GetDiodeTemp()
        {
            sendData[2] = DIODE_TEMP;
            sendData[6] = DIODE_TEMP_CSUM;

            _serialPort.Write(sendData, 0, 7);

            _serialPort.Read(recvData, 0, 7);

            if (expectedMsg(recvData, DIODE_TEMP))
            {
                int counts = (recvData[4] << 8) + recvData[3];
                
                return (float)(counts - ZERO_DEGREES_COUNTS) / 2.048f;
            }
            else
            {
                return -99;
            }
            
        }

        public float GetBatteryVoltage()
        {
            sendData[2] = BATT_VOLT_AND_ERR;
            sendData[6] = BATT_VOLT_AND_ERR_CSUM;

            Console.WriteLine("Writing values:");
            printBytes(sendData);

            _serialPort.Write(sendData, 0, 7);

            _serialPort.Read(recvData, 0, 7);

            Console.WriteLine("Reading values:");
            printBytes(recvData);

            if (expectedMsg(recvData, BATT_VOLT_AND_ERR))
            {
                int counts = (recvData[4] << 8) + recvData[3];
                return counts * VOLTS_PER_COUNT;
            }
            else
            {
                return -99;
            }
        }

        public DataSourceError getErrorStatus()
        {
            return DataSourceError.NoError;
        }

        public float GetOutputCurrent()
        {
            return 1;
        }

    }
}

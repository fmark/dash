using System;
using System.IO.Ports;

namespace Dash2
{
    class ControllerDataSource : DataSource
    {
        private SerialPort _serialPort;
        private byte[] sendData = new byte[7];
        private byte[] recvData = new byte[7];


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

            sendData[0] = 0x5B;
            sendData[1] = 0x04;
            sendData[3] = 0;
            sendData[4] = 0;
            sendData[5] = 0;
        }

        public void CloseDataSource()
        {
            _serialPort.Close();
        }

        public decimal getThrottlePos()
        {
            sendData[2] = 0x20;
            sendData[6] = 0x81;

            _serialPort.Write(sendData, 0, 7);
            _serialPort.Read(recvData, 0, 7);

            return recvData[3] / 255;
        }

        public decimal getBatteryCurrent()
        {
            return 0m;
        }

        public decimal getDiodeTemp()
        {
            sendData[2] = 0x2C;
            sendData[6] = 0x75;

            Console.WriteLine("Writing values: 0->{0}, 1->{1}, 2->{2}, 3->{3}, 4->{4}, 5->{5}, 6->{6}",
                sendData[0], sendData[1], sendData[2], sendData[3], sendData[4], sendData[5],
                sendData[6]);

            _serialPort.Write(sendData, 0, 7);

            _serialPort.Read(recvData, 0, 7);

            Console.WriteLine("Read values: 0->{0}, 1->{1}, 2->{2}, 3->{3}, 4->{4}, 5->{5}, 6->{6}",
                recvData[0], recvData[1], recvData[2], recvData[3], recvData[4], recvData[5],
                recvData[6]);
            return 0.1m;
        }

        public decimal getBatteryVoltage()
        {
            return 0.1m;
        }

        public DataSourceError getErrorStatus()
        {
            return DataSourceError.NoError;
        }

        public decimal getOutputCurrent()
        {
            return 0.1m;
        }

    }
}

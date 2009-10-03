using System;

namespace Dash2
{
    class Program
    {
    static bool _continue;
    static DataSource data;

    public static void Main()
    {
        string message;

        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        _continue = true;

		//Swap around the lines below to toggle between the real and dummy contollers.
        data = new ControllerDataSource();
		//data = new TestDataSource();
        data.InitDataSource();
        for (int i = 0; i < 5; i++)
        {
            int time1 = System.Environment.TickCount;
            int c = 0;
            while (System.Environment.TickCount < time1 + 1000 * 10)
            {
                data.GetThrottlePos();
                data.GetDiodeTemp();
                data.GetBatteryVoltage();
                data.GetOutputCurrent();
                data.GetBatteryCurrent();
                data.getErrorStatus();
                c++;
            }
            Console.WriteLine("Hello {0}", (float)c / 10f);
            
        }
        Console.Read();
/*        Console.WriteLine("Type QUIT to exit");
        while (_continue)
        {
            Console.WriteLine("Throttle position: {0:N1}%", data.GetThrottlePos());
            Console.WriteLine("Diode temperature: {0:N1} degrees C", data.GetDiodeTemp());
            Console.WriteLine("Battery voltage: {0:N1} V", data.GetBatteryVoltage());
            Console.WriteLine("Battery current: {0:N1} anps", data.GetBatteryCurrent());
            Console.WriteLine("Output current: {0:N1} amps", data.GetOutputCurrent());
			Console.WriteLine("Error status: {0}", data.getErrorStatus());	
            message = Console.ReadLine();

            if (stringComparer.Equals("quit", message) || stringComparer.Equals("q", message))
            {
                _continue = false;
            }
        }
 * */

        data.CloseDataSource();
    }
    }
}


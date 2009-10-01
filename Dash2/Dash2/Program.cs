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
        // data = new ControllerDataSource();
		data = new TestDataSource();
        data.InitDataSource();
        
        Console.WriteLine("Type QUIT to exit");
        while (_continue)
        {
            Console.WriteLine("Throttle position: {0}", data.GetThrottlePos());
            Console.WriteLine("Diode temperature: {0}", data.GetDiodeTemp());
            Console.WriteLine("Battery voltage: {0}", data.GetBatteryVoltage());
            Console.WriteLine("Battery current: {0}", data.GetBatteryCurrent());
			Console.WriteLine("Output current: {0}", data.GetOutputCurrent());
			Console.WriteLine("Current error: {0}", data.getErrorStatus());	
            message = Console.ReadLine();          

            if (stringComparer.Equals("quit", message))
            {
                _continue = false;
            }
        }
        data.CloseDataSource();
    }
    }
}


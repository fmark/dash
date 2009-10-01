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
        //Thread readThread = new Thread(Read);

        // Create a new SerialPort object with default settings.
        
        _continue = true;
        // readThread.Start();
        data = new ControllerDataSource();
        data.InitDataSource();
        

        Console.WriteLine("Type QUIT to exit");

        while (_continue)
        {
            Console.WriteLine("Throttle position: {0}", 100 * data.getThrottlePos());
            Console.WriteLine("Diode temperature: {0}", 100 * data.getDiodeTemp());
            
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


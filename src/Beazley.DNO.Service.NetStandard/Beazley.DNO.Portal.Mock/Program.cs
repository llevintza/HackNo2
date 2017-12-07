using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;

namespace Beazley.DNO.Portal.Mock
{
    public class Program
    {
        static void Main()
        {
            string baseAddress = "http://localhost:9000/";

            Console.WriteLine("Starting web server...");

            //// Start OWIN host 
            //using (WebApp.Start<Startup>(url: baseAddress))
            //{
            //    Console.WriteLine($"Web Server is running on {baseAddress}.");
            //    Console.WriteLine("Press any key to quit.");
            //    Console.ReadLine();
            //}

            WebApp.Start<Startup>(new StartOptions(baseAddress)
            {
                ServerFactory = "Microsoft.Owin.Host.HttpListener"
            });

            // Launch default browser
            Process.Start($"{baseAddress}swagger/ui/index");

            // Kick off other program logic
            // ...

            // In my case wait for ENTER to close app
            Console.ReadLine();
        }

    }
}

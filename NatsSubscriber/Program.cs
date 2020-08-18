using System;

namespace NatsSubscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Subscriber().Run(args);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error at NatsPublisher startup. Press any key...");
                Console.Error.WriteLine(ex);
            }
        }
    }
}

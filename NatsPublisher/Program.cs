using System;

namespace NatsPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Publisher().Run(args);

                Console.WriteLine("The NatsPublisher is running. For turn off, press any key...");
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

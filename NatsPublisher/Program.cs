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
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error at NatsPublisher startup. Press any key...");
                Console.Error.WriteLine(ex);
            }
        }
    }
}

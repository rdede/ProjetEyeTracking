using System;
using Tobii.Interaction;

namespace ProjetEyeTracking
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Everything starts with initializing Host, which manages connection to the 
            // Tobii Engine and provides all the Tobii Core SDK functionality.
            // NOTE: Make sure that Tobii.EyeX.exe is running
            var host = new Host();

            // 2. Create stream. 
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();

            // 3. Get the gaze data!
            gazePointDataStream.GazePoint((x, y, ts) => Console.WriteLine("Timestamp: {0}\t X: {1} Y:{2}", ts, x, y));

            // okay, it is 4 lines, but you won't be able to see much without this one :)
            Console.ReadKey();

            // we will close the coonection to the Tobii Engine before exit.
            host.DisableConnection();
        }
    }
}

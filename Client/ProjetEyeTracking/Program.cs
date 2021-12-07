using System;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using Tobii.Research;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace ProjetEyeTracking
{
    class Program
    {
        public static string url = "http://localhost:8080/";

        public static void Main(string[] args)
        {
            /*EyeTrackerCollection eyeTrackers = EyeTrackingOperations.FindAllEyeTrackers();
            Console.WriteLine(eyeTrackers.Count);
            */


            // Everything starts with initializing Host, which manages the connection to the 
            // Tobii Engine and provides all the Tobii Core SDK functionality.
            // NOTE: Make sure that Tobii.EyeX.exe is running
            var host = new Host();

            // Initialize Fixation data stream.
            var fixationDataStream = host.Streams.CreateFixationDataStream();

            // Because timestamp of fixation events is relative to the previous ones
            // only, we will store them in this variable.
            var fixationBeginTime = 0d;

            fixationDataStream.Next += (o, fixation) =>
            {
                // On the Next event, data comes as FixationData objects, wrapped in a StreamData<T> object.
                var fixationPointX = fixation.Data.X;
                var fixationPointY = fixation.Data.Y;

                switch (fixation.Data.EventType)
                {
                    case FixationDataEventType.Begin:
                        fixationBeginTime = fixation.Data.Timestamp;
                        Console.WriteLine("Begin fixation at X: {0}, Y: {1}", fixationPointX, fixationPointY);
                        break;

                    case FixationDataEventType.Data:
                        Console.WriteLine("During fixation, currently at X: {0}, Y: {1}", fixationPointX, fixationPointY);
                        break;

                    case FixationDataEventType.End:
                        Console.WriteLine("End fixation at X: {0}, Y: {1}", fixationPointX, fixationPointY);
                        Console.WriteLine("Fixation duration: {0}",
                            fixationBeginTime > 0
                                ? TimeSpan.FromMilliseconds(fixation.Data.Timestamp - fixationBeginTime)
                                : TimeSpan.Zero);
                        Console.WriteLine();
                        break;

                    default:
                        throw new InvalidOperationException("Unknown fixation event type, which doesn't have explicit handling.");
                }
            };

            Console.ReadKey();

            // we will close the coonection to the Tobii Engine before exit.
            host.DisableConnection();

        }
    }
}

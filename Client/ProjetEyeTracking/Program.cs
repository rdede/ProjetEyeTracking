using System;
using System.Threading.Tasks;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

using System.Net.Sockets;
using System.Text;
using System.Net;

namespace ProjetEyeTracking
{
    public class Program
    {
        // Fixation variables
        private static FixationDataStream _fixationDataStream;
        private static Host _host;
        private static DateTime _fixationBeginTime = default(DateTime);

        // Socket variables 
        private static IPHostEntry host = Dns.GetHostEntry("localhost");
        private static IPAddress ipAddress = host.AddressList[0];
        private static IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

        private static byte[] bytes = new byte[1024];

        // Create a TCP/IP  socket.
        private static Socket sender = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        public static void Main(string[] args)
        {
            StartClient();

            InitializeHost();

            CreateFixationsStream();

            int bytesRec = sender.Receive(bytes);
            Console.WriteLine("{0}",
                Encoding.ASCII.GetString(bytes, 0, bytesRec));

            if (Encoding.ASCII.GetString(bytes, 0, bytesRec) == "suivant")
            {
                ToggleFixationStream();
            }

            Console.ReadKey(true);

            DisableConnectionWithTobiiEngine();
        }

        private static void InitializeHost()
        {
            // Initialyzing the Tobii host
            // Make sure that Tobii.EyeX.exe is running
            _host = new Host();
        }

        private static void DisableConnectionWithTobiiEngine()
        {
            // Disabling connection with TobiiEngine before exit the application
            _host.DisableConnection();
        }

        private static void ToggleFixationStream()
        {
            // Toggling the FixationDataStream on or off
            if (_fixationDataStream != null)
                _fixationDataStream.IsEnabled = !_fixationDataStream.IsEnabled;
        }

        private static void CreateFixationsStream()
        {
            _fixationDataStream = _host.Streams.CreateFixationDataStream();
            _fixationDataStream
                .Begin((x, y, _) =>
                {
                    Console.WriteLine("\n" +
                                      "Fixation started at X: {0}, Y: {1}", x, y);
                    _fixationBeginTime = DateTime.Now;
                })
                .Data((x, y, _) =>
                {
                    //Console.WriteLine("During fixation, currently at: X: {0}, Y: {1}", x, y);
                })
                .End((x, y, _) =>
                {
                    Console.WriteLine("Fixation ended at X: {0}, Y: {1}", x, y);
                    if (_fixationBeginTime != default(DateTime))
                    {
                        Console.WriteLine("Fixation duration: {0}", DateTime.Now - _fixationBeginTime);
                    }
                });
        }

        public static void StartClient()
        {
            try
            {
                // Connect to Remote EndPoint
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                    sender.RemoteEndPoint.ToString());

                // Send encoded message through the socket
                byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
                int bytesSent = sender.Send(msg);

                // Receive the response from the server
                int bytesRec = sender.Receive(bytes);
                Console.WriteLine("Echoed test = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
    }
}

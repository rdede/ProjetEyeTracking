using System;
using System.Threading.Tasks;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Text.Json;

namespace ProjetEyeTracking
{
    public class Program
    {
        // Fixation variables
        private static FixationDataStream _fixationDataStream;
        private static Host _host;
        private static DateTime _fixationBeginTime = default(DateTime);
        private static List<Fixation> fixationList = new List<Fixation>();
        private static StartedAt startFixation;
        private static EndedAt endFixation;
        private static TimeSpan fixationDuration;
        private static List<Page> pagesList = new List<Page>();
        private static int pageCpt = 0;

        private static int nbPages = 3;

        // Socket variables 
        private static IPHostEntry host = Dns.GetHostEntry("localhost");
        private static IPAddress ipAddress = host.AddressList[0];
        private static IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
        private static byte[] bytes = new byte[1024];

        private static int numericValue;
        private static string currentUser;


        // Create a TCP/IP  socket.
        private static Socket sender = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        public static void Main(string[] args)
        {
            StartClient();

            InitializeHost();

            int bytesRec = sender.Receive(bytes);
            string response = Encoding.ASCII.GetString(bytes, 0, bytesRec);

            string[] res = response.Split(':', ';');

            switch (res[0])
            {
                case "start":
                    if(res[1] == "0")
                    {
                        CreateFixationsStream();
                    }
                    else
                    {
                        ToggleFixationStream();
                    }
                    break;
                case "stop":
                    UserSuivant(res);
                    break;
                case "etape":
                    PageSuivante(res);
                    break;
            }
            PageSuivante(res);

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
                    startFixation = new StartedAt { x = x, y = y };
                    //Console.WriteLine("\n" +
                                      //"Fixation started at X: {0}, Y: {1}", x, y);
                    _fixationBeginTime = DateTime.Now;
                })
                .Data((x, y, _) =>
                {
                    //Console.WriteLine("During fixation, currently at: X: {0}, Y: {1}", x, y);
                })
                .End((x, y, _) =>
                {
                    //Console.WriteLine("Fixation ended at X: {0}, Y: {1}", x, y);
                    endFixation = new EndedAt { x = x, y = y };
                    fixationDuration = DateTime.Now - _fixationBeginTime;
                    fixationList.Add(new Fixation { startedAt = startFixation, endedAt = endFixation, duration = fixationDuration });
                    if (_fixationBeginTime != default(DateTime))
                    {
                        //Console.WriteLine("Fixation duration: {0}", DateTime.Now - _fixationBeginTime);
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

        public static void PageSuivante(string[] res)
        {
            pagesList.Add(new Page { pageNb = res[1], imgSelect = res[5], fixations = fixationList});
        }

        public static void UserSuivant(string[] res)
        {
            var data = new Data
            {
                idUser = res[3],
                pages = pagesList
            };
            WriteJson(data);
            ToggleFixationStream();
        }

        public static void WriteJson(Data data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options);

            Console.WriteLine(jsonString);
        }
    }
}

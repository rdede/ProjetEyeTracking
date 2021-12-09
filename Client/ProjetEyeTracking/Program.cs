using System;
using System.Threading.Tasks;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace ProjetEyeTracking
{
    public class Program
    {
        // Images Coordinates
        private static Dictionary<string, double> coordsImg1 = new Dictionary<string, double>();
        private static Dictionary<string, double> coordsImg2 = new Dictionary<string, double>();
        private static Dictionary<string, double> coordsImg3 = new Dictionary<string, double>();

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

            coordsImg1.Add("X", 175.0);
            coordsImg1.Add("Y", 122.0);
            coordsImg1.Add("XMax", 460.0);
            coordsImg1.Add("YMax", 537.0);

            coordsImg2.Add("X", 809.0);
            coordsImg2.Add("Y", 122.0);
            coordsImg2.Add("XMax", 1094.0);
            coordsImg2.Add("YMax", 537.0);

            coordsImg3.Add("X", 1443.0);
            coordsImg3.Add("Y", 122.0);
            coordsImg3.Add("XMax", 1728.0);
            coordsImg3.Add("YMax", 537.0);


            StartClient();

            InitializeHost();


            while (sender.Connected)
            {
                int bytesRec = sender.Receive(bytes);
                string response = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                string[] res = response.Split(':', ';');

                switch (res[0])
                {
                    case "start":
                        if (res[1] == "1")
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
                    startFixation = new StartedAt { x = x, y = y };
                  
                    _fixationBeginTime = DateTime.Now;
                })
                .End((x, y, _) =>
                {
                    string img = GetImageNumber(x, y);

                    if (img != "else")
                    {
                        endFixation = new EndedAt { x = x, y = y };
                        fixationDuration = DateTime.Now - _fixationBeginTime;
                        fixationList.Add(new Fixation { imgLooked = img, startedAt = startFixation, endedAt = endFixation, duration = fixationDuration });
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
            
            fixationList = new List<Fixation>();
        }

        public static void UserSuivant(string[] res)
        {
            var data = new Data
            {
                idUser = res[1],
                pages = pagesList
            };
            pagesList = new List<Page>();
            WriteJson(data, res[1]);
            ToggleFixationStream();
        }

        public static void WriteJson(Data data, string idUser)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options);

            File.WriteAllText(@"../../output/" + idUser + ".json", jsonString);
        }

        public static string GetImageNumber(double x, double y)
        {

            if (coordsImg1["X"] <= x && coordsImg1["Y"] <= y
                && coordsImg1["XMax"] >= x && coordsImg1["YMax"] >= y)
            {
                return "A";
            }
            else if (coordsImg2["X"] <= x && coordsImg2["Y"] <= y
                && coordsImg2["XMax"] >= x && coordsImg2["YMax"] >= y)
            {
                return "B";
            }
            else if (coordsImg3["X"] <= x && coordsImg3["Y"] <= y
                && coordsImg3["XMax"] >= x && coordsImg3["YMax"] >= y)
            {
                return "C";
            }
            else
            {
                return "else";
            }
        }
    }
}

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;

namespace HttpServer
{
    class HttpServer
    {
        // Variables serveur web    
        public static HttpListener httpListener;
        public static string url = "http://localhost:8070/";
        public static int actual_user = 0;
        public static int page_number = 1;
        public static string main_web_page = File.ReadAllText("index.html");
        public static string interface_1_web_page = File.ReadAllText("inter1.html");
        public static string interface_2_web_page = File.ReadAllText("inter2.html");
        public static string actual_page = main_web_page;

        public static IPHostEntry host = Dns.GetHostEntry("localhost");
        public static IPAddress ipAddress = host.AddressList[0];
        public static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        public static Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        public static Socket handler;

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            List<List<string>> answers = new List<List<string>>();
            List<string> user_answers = new List<string>();

            while (runServer)
            {
                HttpListenerContext ctx = await httpListener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/etape1"))
                {
                    page_number++;
                    string inter1 = new System.IO.StreamReader(req.InputStream, req.ContentEncoding).ReadToEnd();
                    user_answers.Add(inter1);
                    actual_page = interface_1_web_page;
                }

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/etape2"))
                {
                    page_number++;
                    string inter2 = new System.IO.StreamReader(req.InputStream, req.ContentEncoding).ReadToEnd();
                    user_answers.Add(inter2);
                    actual_page = interface_2_web_page;
                }

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/suivant"))
                {
                    actual_user++;
                    string inter3 = new System.IO.StreamReader(req.InputStream, req.ContentEncoding).ReadToEnd();
                    user_answers.Add(inter3);
                    foreach (string ans in user_answers)
                        Console.WriteLine(ans);
                    actual_page = main_web_page;
                    answers.Add(user_answers);
                    user_answers.Clear();

                    Console.WriteLine("SUIVANT");
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes("suivant");
                    handler.Send(byData);
                }

                Console.WriteLine(page_number);

                byte[] img1 = System.IO.File.ReadAllBytes(@"./assets/" + page_number + "/img1.jpg");
                byte[] img2 = System.IO.File.ReadAllBytes(@"./assets/" + page_number + "/img2.jpg");
                byte[] img3 = System.IO.File.ReadAllBytes(@"./assets/" + page_number + "/img3.jpg");

                string base64Img1 = Convert.ToBase64String(img1);
                string base64Img2 = Convert.ToBase64String(img2);
                string base64Img3 = Convert.ToBase64String(img3);
                byte[] data = Encoding.UTF8.GetBytes(String.Format(actual_page, base64Img1, base64Img2, base64Img3));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }


        public static void Main(string[] args)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(url);
            httpListener.Start();
            Console.WriteLine("Serveur démarré sur : {0}", url);

            // StartServer();

            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();
            httpListener.Close();
        }

        public static void StartServer()
        {
            try
            {
                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will listen 10 requests at a time
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");
                handler = listener.Accept();

                // Incoming data from the client.
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                Console.WriteLine("Text received : {0}", data);

                byte[] msg = Encoding.ASCII.GetBytes(data);
                handler.Send(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebApp.Controllers
{

    public class HomeController : Controller
    {
        
        private static int ID_USER = 1;
        private static Socket handler;
        private static bool isStarted = false;
        private static int etapes = 3;

        // Use Dictionary as a map.
        private Dictionary<string, double> coordsImg1 = new Dictionary<string, double>();
        private Dictionary<string, double> coordsImg2 = new Dictionary<string, double>();
        private Dictionary<string, double> coordsImg3 = new Dictionary<string, double>();

        public HomeController()
        {

            this.coordsImg1.Add("X", 175.0);
            this.coordsImg1.Add("Y", 122.0);
            this.coordsImg1.Add("XMax", 460.0);
            this.coordsImg1.Add("YMax", 537.0);

            this.coordsImg2.Add("X", 809.0);
            this.coordsImg2.Add("Y", 122.0);
            this.coordsImg2.Add("XMax", 1094.0);
            this.coordsImg2.Add("YMax", 537.0);

            this.coordsImg3.Add("X", 1443.0);
            this.coordsImg3.Add("Y", 122.0);
            this.coordsImg3.Add("XMax", 1728.0);
            this.coordsImg3.Add("YMax", 537.0);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!isStarted)
            {
                StartServer();
            }
            

            return View();
        }

        private void StartServer()
        {

            var host = Dns.GetHostEntry("localhost");
            var ipAddress = host.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, 11000);
            var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

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
                string data = "";
                byte[] bytes;

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
                isStarted = true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [HttpGet]
        public IActionResult Test(int id)
        {
            Console.WriteLine(handler);
            Console.WriteLine(id);
            switch (id)
            {
                case 0:
                    byte[] msg = Encoding.ASCII.GetBytes("start:" + ID_USER);
                    handler.Send(msg);
                    break;
                default:
                    byte[] msgEtape = Encoding.ASCII.GetBytes("etape:" + id + ";user:" + ID_USER);
                    Console.WriteLine(msgEtape);
                    handler.Send(msgEtape);
                    break;

            }
            ViewData["Id"] = id;
            return View();
        }

        [HttpPost]
        public IActionResult FormPostValue()
        {

            var id = int.Parse(HttpContext.Request.Form["pageId"]) + 1;
            var reponse = HttpContext.Request.Form["value"];
            Console.WriteLine("test " + id + " : " + reponse);
            if (id < etapes)
            {
                return RedirectToAction("Test", new { id });
            }
            else
            {
                ID_USER++;
                Console.WriteLine(ID_USER);
                return RedirectToAction("Resultats");
            }
        }

        [HttpGet]
        public IActionResult Resultats()
        {
            return View();
        }
    }
}
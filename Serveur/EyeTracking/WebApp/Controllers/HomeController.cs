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

        [HttpGet]
        public IActionResult Index()
        {
            if (!isStarted)
            {
                StartServer();
            }

            ViewData["idUser"] = ID_USER;
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

                Console.WriteLine("En attente du lancement du client Tobii...");
                handler = listener.Accept();

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
            if (id == 0)
            {
                byte[] msg = Encoding.ASCII.GetBytes("start:" + ID_USER);
                handler.Send(msg);
            }
            ViewData["Id"] = id;
            return View();
        }

        [HttpPost]
        public IActionResult FormPostValue()
        {

            var id = int.Parse(HttpContext.Request.Form["pageId"]) + 1;
            var reponse = HttpContext.Request.Form["value"];
            if (id < etapes)
            {
                if (id > 0)
                {
                    byte[] msgEtape = Encoding.ASCII.GetBytes("etape:" + id + ";user:" + ID_USER + ";img:" + reponse);
                    handler.Send(msgEtape);
                }   
                
                return RedirectToAction("Test", new { id });
            }
            else
            {
                byte[] msgEtapeFinal = Encoding.ASCII.GetBytes("etape:" + id + ";user:" + ID_USER + ";img:" + reponse);
                handler.Send(msgEtapeFinal);
                byte[] msgStop = Encoding.ASCII.GetBytes("stop:" + ID_USER);
                handler.Send(msgStop);

                ID_USER++;
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
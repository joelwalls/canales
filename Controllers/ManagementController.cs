using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.IO;
using RestSharp;
using RestSharp.Authenticators;
using AdministradorCanales.Entities;
using AdministradorCanales.Models;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AdministradorCanales.Controllers
{
    public class ManagementController : Controller
    {
        private ChatModel chatModel = new ChatModel();
        private MensajeModel mensajeModel = new MensajeModel();
        private UsuarioModel usuarioModel = new UsuarioModel();
        private FacebookModel facebookModel = new FacebookModel();
        private ContactoModel contactoModel = new ContactoModel();
        private string user_ref;
        public ActionResult Index()
        {
            ViewBag.Logged = Session["usuario_id"];
            return View();
        }

        
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View("About", new Usuario());
        }

        [HttpPost]
        public ActionResult About(string asunto, string destinatario, string texto)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator = new HttpBasicAuthenticator("api", "7dada59033608b2131b4e0d39f6947b8-afab6073-99095e76");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "sandboxbcda704e206f4ac29f6e640492eff81b.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Usuario Tesis <mailgun@sandboxbcda704e206f4ac29f6e640492eff81b.mailgun.org>");
            request.AddParameter("to", destinatario);
            request.AddParameter("subject", asunto);
            request.AddParameter("text", texto);
            request.Method = Method.POST;
            client.Execute(request);
            return About();
        }

        public ActionResult Chat()
        {
            ViewBag.userId = Session["usuario_id"];
            ViewBag.CreateChat = true;
            ViewBag.ListaChat = getListChats();
            return View("Chat");
        }

        [HttpPost]
        public ActionResult Chat(string chatId, string mensaje)
        {
            if (mensaje != null)
            {
                if(mensaje.Length > 0)
                {
                    Mensaje nuevoMensaje = new Mensaje();
                    nuevoMensaje.IdChat = Session["chat_id"].ToString();
                    nuevoMensaje.IdUsuario = Session["usuario_id"].ToString();
                    nuevoMensaje.Texto = mensaje;
                    nuevoMensaje.Fecha = DateTime.Now;
                    mensajeModel.ingresar(nuevoMensaje);
                }
            }
            if (chatId == null)
            {
                chatId = Session["chat_id"].ToString();
                ViewBag.userId = Session["usuario_id"];
                List<Mensaje> mensajes = mensajeModel.mensajesPorChat(chatId);
                ViewBag.ListaMensajes = mensajes;
                ViewBag.ListaChat = getListChats();
            }
            else
            {
                Session["chat_id"] = chatId;
                ViewBag.userId = Session["usuario_id"];
                List<Mensaje> mensajes = mensajeModel.mensajesPorChat(chatId);
                ViewBag.ListaMensajes = mensajes;
                ViewBag.ListaChat = getListChats();
            }
            return View("Chat");
        }

        public List<ItemChat> getListChats()
        {
            List<Chat> chats = chatModel.chatNoCerrados();
            List<ItemChat> arregloChats = new List<ItemChat>();
            foreach (var chat in chats)
            {
                Usuario usuario = usuarioModel.buscar(chat.IdUsuario);
                ItemChat item = new ItemChat();
                List<Mensaje> mensajes = mensajeModel.mensajesPorChat(chat.Id.ToString()); ;
                item.Asunto = chat.Asunto;
                item.ChatId = chat.Id.ToString();
                item.UserId = chat.IdUsuario.ToString();
                item.UserName = usuario.Nombre + " " + usuario.Apellido;
                item.Mensajes = mensajes.Count();
                arregloChats.Add(item);
            }

            return arregloChats;
        }

        public ActionResult Twitter()
        {
            ViewBag.Message = "Your application description page.";
            return View("Twitter", new Usuario());
        }

        [HttpPost]
        public ActionResult Twitter(string username, string tweet)
        {
            var twitter = new TwitterApi();
            twitter.SendTweet(tweet + " @" + username);
            return View("Twitter");
        }

        public ActionResult Facebook()
        {
            List<Contacto> contactos = contactoModel.contactos();
            return View(contactos);
        }

        [HttpPost]
        public ActionResult Facebook(string user_ref, string msj)
        {
            Facebook fb = new Facebook();
            fb.user_ref = user_ref;
            fb.msj = msj;
            fb.type = "sent";
            fb.timestamp = DateTime.Now;
            facebookModel.insertar(fb);
            var json = $@" {{recipient: {{  user_ref: ""{user_ref}""}},message: {{text: ""{msj}"" }}}}";
            PostRaw("https://graph.facebook.com/v2.6/me/messages?access_token=EAAGmETM9mBIBAMhKhnQgiit55ZAU7AIzZB2qsr5CEenZBIsUa9SFiRz7GGBEazUECBBHdGnVb5B7tv4aaNSAWHYYebSvIAkNxBGxaAw0vzWOieEBdZBBe2YjXXJBjZB0dr4VNIT5XDiJvbqYZBCKOjjqlTaT4waHUVpOx43XYLy4XrzZCle6ZAKS", json);
            return Content("Exito");
        }

        public ActionResult FacebookChat(string user_ref)
        {
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(facebookModel.mensajes(user_ref));
            return Content(json.ToString(), "application/json");
        }

        public ActionResult Checkbox()
        {
            ViewBag.Message = "Your application description page.";
            return View("Checkbox", new Usuario());
        }

        [HttpPost]
        public ActionResult CheckBoxContacto(string nombre, string user_ref)
        {
            Contacto contacto = new Contacto();
            contacto.nombre = nombre;
            contacto.user_ref = user_ref;
            contactoModel.insertar(contacto);
            return Content("Exito");
        }

        public ActionResult Receive()
        {
            var query = Request.QueryString;
            

            if (query["hub.mode"] == "subscribe" &&
                query["hub.verify_token"] == "speak_friend_and_enter")
            {
                //string type = Request.QueryString["type"];
                var retVal = query["hub.challenge"];
                return Json(int.Parse(retVal), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [ActionName("Receive")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ReceivePost(BotRequest data)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var entry in data.entry)
                {
                    foreach (var message in entry.messaging)
                    {
                        if (string.IsNullOrWhiteSpace(message?.message?.text))
                            continue;
                        
                        if(message.prior_message != null) {
                            contactoModel.update(message.prior_message.identifier, message.sender.id);
                        }
                        Facebook fb = new Facebook();
                        fb.user_id = message.sender.id;
                        fb.msj = message.message.text;
                        fb.type = "received";
                        fb.timestamp = DateTime.Now;
                        facebookModel.insertar(fb);
                    }
                }
            });

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private string PostRaw(string url, string data)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json";
            request.Method = "POST";
            using (var requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.Write(data);
            }

            var response = (HttpWebResponse)request.GetResponse();
            if (response == null)
                throw new InvalidOperationException("GetResponse returns null");

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

    }
}
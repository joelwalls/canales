using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdministradorCanales.Entities;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Configuration;

namespace AdministradorCanales.Models
{
    public class MensajeModel
    {
        private MongoClient mongoClient;
        private IMongoCollection<Mensaje> mensajeCollection;

        public MensajeModel()
        {
            mongoClient = new MongoClient(ConfigurationManager.AppSettings["mongoDBHost"]);
            var db = mongoClient.GetDatabase(ConfigurationManager.AppSettings["mongoDBName"]);

            mensajeCollection = db.GetCollection<Mensaje>("mensaje");
        }

        public List<Mensaje> todos()
        {
            return mensajeCollection.AsQueryable<Mensaje>().ToList();
        }

        public List<Mensaje> mensajesPorChat(string chatId)
        {
            return mensajeCollection.AsQueryable<Mensaje>().Where(m => m.IdChat == chatId).ToList();
        }

        public Mensaje buscar(string id)
        {
            var mensajeId = new ObjectId(id);
            return mensajeCollection.AsQueryable<Mensaje>().SingleOrDefault(mensaje => mensaje.Id == mensajeId);
        }

        public List<Mensaje> ingresar(Mensaje mensaje)
        {
            mensajeCollection.InsertOne(mensaje);
            return mensajeCollection.AsQueryable<Mensaje>().Where(m => m.IdChat == mensaje.IdChat).ToList();   
        }

        public void actualizar(Mensaje mensaje)
        {
            mensajeCollection.UpdateOne(
                Builders<Mensaje>.Filter.Eq("_id", mensaje.Id),
                Builders<Mensaje>.Update
                .Set("fecha", mensaje.Fecha)
                .Set("idusuario", mensaje.IdUsuario)
                .Set("idchat", mensaje.IdChat)
                .Set("texto", mensaje.Texto)
            );
        }

        public void eliminar(string id)
        {
            mensajeCollection.DeleteOne(Builders<Mensaje>.Filter.Eq("_id", ObjectId.Parse(id)));
        }
    }
}
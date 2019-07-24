using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using AdministradorCanales.Entities;
using MongoDB.Driver;

namespace AdministradorCanales.Models
{
    public class FacebookModel
    {
        private MongoClient mongoClient;
        private IMongoCollection<Facebook> facebookCollection;

        public FacebookModel()
        {
            mongoClient = new MongoClient(ConfigurationManager.AppSettings["mongoDBHost"]);
            var db = mongoClient.GetDatabase(ConfigurationManager.AppSettings["mongoDBName"]);

            facebookCollection = db.GetCollection<Facebook>("facebook");
        }

        public void insertar(Facebook msj)
        {
            facebookCollection.InsertOne(msj);
        }

        public List<Facebook> mensajes(string user_ref)
        {
            ContactoModel model = new ContactoModel();
            Contacto contacto = model.userByref(user_ref);
            if (contacto.user_id != null)
            {
                return facebookCollection.AsQueryable<Facebook>().Where(m => m.user_ref == user_ref || m.user_id == contacto.user_id).ToList();
            } else {
                return facebookCollection.AsQueryable<Facebook>().Where(m => m.user_ref == user_ref).ToList();
            }
        }
    }
}
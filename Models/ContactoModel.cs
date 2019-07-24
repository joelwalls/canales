using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using AdministradorCanales.Entities;
using MongoDB.Driver;

namespace AdministradorCanales.Models
{
    public class ContactoModel
    {
        private MongoClient mongoClient;
        private IMongoCollection<Contacto> facebookCollection;

        public ContactoModel()
        {
            mongoClient = new MongoClient(ConfigurationManager.AppSettings["mongoDBHost"]);
            var db = mongoClient.GetDatabase(ConfigurationManager.AppSettings["mongoDBName"]);

            facebookCollection = db.GetCollection<Contacto>("contacto");
        }

        public void insertar(Contacto contacto)
        {
            facebookCollection.InsertOne(contacto);
        }

        public List<Contacto> contactos()
        {
            return facebookCollection.AsQueryable<Contacto>().ToList();
        }

        public Contacto userByref(string user_ref)
        {
            return facebookCollection.AsQueryable<Contacto>().Where(m => m.user_ref == user_ref).First();
        }

        public void update(string user_ref, string user_id)
        {
            var filter = Builders<Contacto>.Filter.Eq("user_ref", user_ref);
            var update = Builders<Contacto>.Update.Set("user_id", user_id);
            var res = facebookCollection.UpdateOne(filter, update);
        }
    }
}
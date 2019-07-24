using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AdministradorCanales.Entities
{
    public class Facebook
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string user_ref { get; set; }
        public string user_id { get; set; }

        public string msj { get; set; }

        public string type { get; set; }

        public DateTime timestamp { get; set; }
    }
}
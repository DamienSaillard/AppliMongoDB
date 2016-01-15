using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppliMongoDB
{
    [BsonIgnoreExtraElements]
    public class Pages
    {
        public Pages()
        {
        }

        //[BsonIgnore]
        public BsonInt32 start
        {
            get; set;
        }
        //[BsonIgnore]
        public BsonInt32 end
        {
            get; set;
        }
    }

    [BsonIgnoreExtraElements]
    public class DocMongo
    {
        /*public string id;
        public string type;
        public string title;
        public Pages pages;
        public int year;
        public string booktitle;
        public string url;
        public List<string> authors = new List<string>();*/

        public DocMongo()
        {
        }

        public object id
        {
            get; set;
        }
        public string type
        {
            get; set;
        }
        public string title
        {
            get; set;
        }
        //[BsonIgnore]
        public object pages
        {
            get; set;
        }
        public int year
        {
            get; set;
        }
        public string booktitle
        {
            get; set;
        }
        public string url
        {
            get; set;
        }
        public List<string> authors
        {
            get; set;
        }
        public string publisher
        {
            get; set;
        }
        public List<string> isbn
        {
            get; set;
        }
        public string series
        {
            get; set;
        }
        public string editor
        {
            get; set;
        }
        public string volume
        {
            get; set;
        }
        public List<string> cites
        {
            get; set;
        }
        public object number
        {
            get; set;
        }
    }
}

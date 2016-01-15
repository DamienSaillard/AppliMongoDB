using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AppliMongoDB
{
    [BsonIgnoreExtraElements]
    public class MyMongoClient {
		private const string defaultAdress = "localhost";
		private const int defaultPort = 27017;
		private const string defaultDB = "dblp";
		private const string defaultCollection = "publis2";
		public IMongoCollection<BsonDocument> publis { get; set; }

		public MyMongoClient() {
			publis = GetCollection(defaultDB, defaultCollection);
		}

		#region Connexion
		public MongoClient Connexion() {
			return new MongoClient("mongodb://" + defaultAdress + ":" + defaultPort + "/");
		}

        public MongoClient Connexion(string host, int port)
        {
            return new MongoClient("mongodb://" + host + ":" + port + "/");
        }

		public MongoClient Connexion(Dictionary<string, int> listAdressPort) {
			string conn = "mongodb://";

			for (int i = 0; i < listAdressPort.Keys.Count; i++) {
				conn += listAdressPort.ElementAt(i).Key + ":" + listAdressPort.ElementAt(i).Value;
				if (i != listAdressPort.Keys.Count - 1)
					conn += ',';
			}
			conn += "/";

			return new MongoClient(conn);
		}
		#endregion


		#region Get Database
		public IMongoDatabase GetDatabase(MongoClient mc, string db = defaultDB) {
			return mc.GetDatabase(db);
		}

		public IMongoDatabase GetDatabase(string db = defaultDB) {
			MongoClient client = Connexion();
			return GetDatabase(client, db);
		}

		public IMongoDatabase GetDatabase(Dictionary<string, int> listAdressPort, string db = defaultDB) {
			MongoClient client = Connexion(listAdressPort);
			return GetDatabase(client, db);
		}

		public IMongoDatabase GetDatabase(string user, string password, string db = defaultDB) {
            var credential = MongoCredential.CreateMongoCRCredential(db, user, password);
            var settings = new MongoClientSettings { Credentials = new[] { credential } };
            var mongoClient = new MongoClient(settings);

			return GetDatabase(mongoClient, db);
		}

		public IMongoDatabase GetDatabase(Dictionary<string, int> listAdressPort, string user, string password, string db = defaultDB) {
            var credential = MongoCredential.CreateMongoCRCredential(db, user, password);
            var settings = new MongoClientSettings { Credentials = new[] { credential } };
            string conn = "mongodb://";
            List<MongoServerAddress> adresses = new List<MongoServerAddress>();
            foreach(var item in listAdressPort)
            {
                MongoServerAddress cur = new MongoServerAddress(item.Key, item.Value);
                adresses.Add(cur);
            }
            conn += "/";
            settings.Servers = adresses.ToArray(); 
            var mongoClient = new MongoClient(settings);
            return GetDatabase(mongoClient, db);
        }
		#endregion


		#region GetCollection
		public IMongoCollection<BsonDocument> GetCollection(IMongoDatabase db, string collection = defaultCollection) {
			return db.GetCollection<BsonDocument>(collection);
		}

		public IMongoCollection<BsonDocument> GetCollection(string db = defaultDB, string collection = defaultCollection) {
			IMongoDatabase database = GetDatabase(db);
			return GetCollection(database, collection);
		}
        #endregion


        #region Finds
        public List<DocMongo> FindArticles(string title = "") {
            IMongoCollection<DocMongo> collection = GetDatabase(defaultDB).GetCollection<DocMongo>("publis2");
            if (title != "")
                return collection.Find(Builders<DocMongo>.Filter.Regex("title", title)).ToList();
            else
                return collection.Find(new BsonDocument()).ToList();

        }

        public List<DocMongo> FindArticlesByAuthor(string author = "")
        {
            IMongoCollection<DocMongo> collection = GetDatabase(defaultDB).GetCollection<DocMongo>("publis2");
            if (author != "")
                return collection.Find(Builders<DocMongo>.Filter.Regex("authors", author)).ToList();
            else
                return collection.Find(new BsonDocument()).ToList();

        }

        public List<BsonDocument> StatsByAuthor(string author = "")
        {
            List<DocMongo> listDoc = FindArticlesByAuthor(author);
            IMongoCollection<BsonDocument> collection = GetDatabase(defaultDB).GetCollection<BsonDocument>("publis2");
            List<BsonDocument> stats = new List<BsonDocument>();
            List<BsonDocument> result = collection.Aggregate()
                .Unwind("authors")
                .Match(Builders<BsonDocument>.Filter.Regex("authors", author))
                .Group(new BsonDocument { { "_id", "$type" }, { "count", new BsonDocument("$sum", 1) } }).ToList();
            foreach(BsonDocument doc in result)
            {
                doc.Add("type", "type");
            }
            stats.AddRange(result);
            result = collection.Aggregate()
                .Unwind("authors")
                .Match(Builders<BsonDocument>.Filter.Regex("authors", author))
                .Group(new BsonDocument { { "_id", "$year" },
                    { "count", new BsonDocument("$sum", 1) },
                    { "cites", new BsonDocument("$sum", "cites.length") }}).ToList();
            foreach (BsonDocument doc in result)
            {
                doc.Add("type", "year");
            }
            stats.AddRange(result);
            result = collection.Aggregate()
                .Unwind("authors")
                .Match(Builders<BsonDocument>.Filter.Regex("authors", author))
                .Group(new BsonDocument { { "_id", "$publisher" }, { "count", new BsonDocument("$sum", 1) },
                { "cites", new BsonDocument("$sum", "cites.length") } }).ToList();
            foreach (BsonDocument doc in result)
            {
                doc.Add("type", "publisher");
            }
            stats.AddRange(result);
            result = collection.Aggregate()
                .Match(Builders<BsonDocument>.Filter.Regex("authors", author))
                .Unwind("authors")
                .Group(new BsonDocument { { "_id", "$authors" } }).ToList();
            BsonDocument doc2 = new BsonDocument();
            
            doc2.Add("_id", "null");
            doc2.Add("count", result.Select(x => x["_id"]).Distinct().ToArray().Length);
            doc2.Add("type", "coauthors");
            stats.Add(doc2);
            return stats;
        }

        public DocMongo FindById(string id = "")
        {
            IMongoCollection<DocMongo> collection = GetDatabase(defaultDB).GetCollection<DocMongo>("publis2");
            FilterDefinition<DocMongo> filter = null;
            if (id != "")
                filter = (Builders<DocMongo>.Filter.Eq("_id", id));
            else
                filter = new BsonDocument();
            var result = collection.Find(filter).ToList();
            if(result != null && result.Count > 0)
                return result.First();
            return null;

        }

        public Dictionary<string, string> FindAllTitlesById()
        {
            Dictionary<string, string> dico = new Dictionary<string, string>();
            IMongoCollection<BsonDocument> collection = GetDatabase(defaultDB).GetCollection<BsonDocument>("publis2");
            FilterDefinition<BsonDocument> filter = new BsonDocument();
            var result = collection.Find(filter).ToList();
            for(int i = 0; i < result.Count; i++)
            {
                dico.Add(result[i]["_id"].ToString(), result[i]["title"].ToString());
            }
            return dico;
        }
        #endregion


        #region MapReduce
        public Dictionary<int, int> StatYearsMapReduce(string auteur) {
            Dictionary<int, int> statYears = new Dictionary<int, int>();
            BsonJavaScript mapFunction = @"function () { emit(this.year , 1);};";
            BsonJavaScript reduceFunction = @"function (key, values) { return Array.sum(values); }";
            var results = GetDatabase()
                .GetCollection<BsonDocument>("publis2")
                .MapReduce<BsonDocument>(mapFunction, reduceFunction);

            foreach (BsonDocument result in results.ToList())
            {
                int key = result["_id"].ToInt32();
                int value = result["value"].ToInt32();
                statYears.Add(key, value);
            }
            return statYears;
		}
		#endregion
	}
}

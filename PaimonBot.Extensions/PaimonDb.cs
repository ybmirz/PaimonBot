using MongoDB.Bson;
using MongoDB.Driver;
using PaimonBot.Extensions.DataModels;
using System;

namespace PaimonBot.Extensions
{
    public class PaimonDb
    {
        private IMongoDatabase db;
        public PaimonDb(string connectionString, string database)
        {
            var client = new MongoClient(connectionString);
            db = client.GetDatabase(database);
        }

        public void InsertTraveler(Traveler input)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            coll.InsertOne(input);
        }

        public Traveler GetTravelerBy<T>(string FieldName,T FieldValue)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq(FieldName, FieldValue);

            if (coll.Find(filter).CountDocuments() > 0)
            {
                var found = coll.Find(filter).First();
                return found;
            }
            else
                return null;
        }

        public void ReplaceTraveler(Traveler input)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq("DiscordID", input.DiscordID);
            coll.DeleteOne(filter);
            coll.InsertOne(input);
        }

        public void DeleteTravelerBy<T>(string FieldName, T FieldValue)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq(FieldName, FieldValue);
            coll.DeleteOne(filter);            
        }

        public void DeleteTraveler(Traveler input)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq("DiscordID", input.DiscordID);
            coll.DeleteOne(filter);
        }

        public bool TravelerExists(ulong TravelerId)
        {
            var traveler = GetTravelerBy<ulong>("DiscordID", TravelerId);
            if (traveler != null)
                return true;
            else
                return false;
        }

        public void UpdateTraveler<T>(Traveler traveler, string fieldName,T updateValue)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq("DiscordID", traveler.DiscordID);
            var update = Builders<Traveler>.Update.Set(fieldName, updateValue);
        }

        public long GetTravelersCount()
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            return coll.CountDocuments(new BsonDocument());
        }

        public IMongoDatabase GetDBInstance()
        {
            return db;
        }
    }
}

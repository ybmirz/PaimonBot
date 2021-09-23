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

        public async void InsertTraveler(Traveler input)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            await coll.InsertOneAsync(input);
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

        public async void ReplaceTraveler(Traveler input)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq("DiscordID", input.DiscordID);
            await coll.DeleteOneAsync(filter);
            await coll.InsertOneAsync(input);
        }

        public async void DeleteTravelerBy<T>(string FieldName, T FieldValue)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq(FieldName, FieldValue);
            await coll.DeleteOneAsync(filter);            
        }

        public async void DeleteTraveler(Traveler input)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq("DiscordID", input.DiscordID);
            await coll.DeleteOneAsync(filter);
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

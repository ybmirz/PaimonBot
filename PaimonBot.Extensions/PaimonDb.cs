using MongoDB.Bson;
using MongoDB.Driver;
using PaimonBot.Extensions.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        #region DBTravelerMethods
        public async void InsertTraveler(Traveler input)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            await coll.InsertOneAsync(input);
            TravelerAdded?.Invoke(this, EventArgs.Empty);
        }
        public Traveler GetTravelerBy<T>(string FieldName,T FieldValue)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq(FieldName, FieldValue);

            if (coll.CountDocuments(new BsonDocument()) > 0)
            {
                return coll.Find(filter).First();
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
        public async void UpdateTraveler<T>(Traveler traveler, string fieldName,T updateValue)
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            var filter = Builders<Traveler>.Filter.Eq("DiscordID", traveler.DiscordID);
            var update = Builders<Traveler>.Update.Set(fieldName, updateValue);
            await coll.UpdateOneAsync(filter, update);
        }
        public async Task<List<ulong>> GetTravelerIDs()
        {
            var TravelerIds = new List<ulong>();
            var coll = db.GetCollection<Traveler>("Travelers");
            // return coll.Find(new BsonDocument()).ToList().Select(x => x.DiscordID);
            await coll.Find(new BsonDocument()).ForEachAsync(x => TravelerIds.Add(x.DiscordID));
            return TravelerIds;
        }
        public async Task<List<Traveler>> GetTravelersAsync()
        {
            var Travelers = new List<Traveler>();
            var coll = db.GetCollection<Traveler>("Travelers");
            await coll.Find(new BsonDocument()).ForEachAsync(x => Travelers.Add(x));
            return Travelers;
        }
        public long GetTravelersCount()
        {
            var coll = db.GetCollection<Traveler>("Travelers");
            return coll.CountDocuments(new BsonDocument());
        }
        #endregion DBTravelerMethods                

        public IMongoDatabase GetDBInstance()
        {
            return db;
        }

        public event EventHandler TravelerAdded;
    }
}

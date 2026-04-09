using System.Collections.Generic;
using System.IO;
using Lab_1.Models;
using Newtonsoft.Json;

namespace Lab_1.Services
{
    public class JsonDataContext
    {
        private readonly string _filePath = "database.json";

        public List<User> Users { get; set; } = new List<User>();
        public List<BankAccount> Accounts { get; set; } = new List<BankAccount>();
        public List<Bank> Banks { get; set; } = new List<Bank>();
        public List<Company> Companies { get; set; } = new List<Company>();
        public List<TransactionRecord> Transactions { get; set; } = new List<TransactionRecord>();

        public void SaveChanges()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto, 
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(_filePath, json);
        }

        public static JsonDataContext Load()
        {
            if (!File.Exists("database.json"))
            {
                return SeedDefaultData(); 
            }

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            string json = File.ReadAllText("database.json");
            return JsonConvert.DeserializeObject<JsonDataContext>(json, settings) ?? SeedDefaultData();
        }

        private static JsonDataContext SeedDefaultData()
        {
            var context = new JsonDataContext();
            context.Users.Add(new Admin { Login = "admin", PasswordHash = "admin" });
            context.Banks.Add(new Bank { Name = "ПриорБанк" });
            context.Banks.Add(new Bank { Name = "БеларусБанк" });
            context.Companies.Add(new Company { Name = "ТехноБанк" });

            context.SaveChanges();
            return context;
        }
    }
}
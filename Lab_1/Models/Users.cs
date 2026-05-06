using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lab_1.Models
{
    public abstract class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public abstract Role Role { get; }
    }

    public class Client : User
    {
        public override Role Role => Role.Client;
        public bool IsApproved { get; set; } = false; 
        public string CompanyId { get; set; }
        [JsonIgnore]
        public string DisplayCompanyName { get; set; } = "Не трудоустроен";
    }

    public class Manager : User
    {
        public override Role Role => Role.Manager;
    }

    public class Admin : User
    {
        public override Role Role => Role.Admin;
    }
}
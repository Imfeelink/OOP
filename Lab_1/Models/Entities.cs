using System;

namespace Lab_1.Models
{
    public class Bank
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
    }

    public class Company
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Type { get; set; }

        public bool IsSalaryProjectRequested { get; set; } = false;
        public bool IsSalaryProjectApproved { get; set; } = false;
    }

    public class TransactionRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string FromAccountId { get; set; }
        public string ToAccountId { get; set; }   
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
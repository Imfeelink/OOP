using System;
using Lab_1.Models;
using Newtonsoft.Json;

namespace Lab_1.Models
{
    public class BankAccount : IAccount
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ClientId { get; set; }
        public string BankId { get; set; }

        [JsonProperty]
        public decimal Balance { get; protected set; } 
        public bool IsBlocked { get; set; }
        [JsonIgnore]
        public string AccountType => this is DepositAccount
            ? $"Вклад ({((DepositAccount)this).InterestRate}%)"
            : "Обычный счет";

        public BankAccount(string clientId, string bankId, decimal initialBalance = 0)
        {
            ClientId = clientId;
            BankId = bankId;
            Balance = initialBalance;
            IsBlocked = false;
        }

        public virtual bool Deposit(decimal amount)
        {
            if (IsBlocked || amount <= 0) return false;
            Balance += amount;
            return true;
        }

        public virtual bool Withdraw(decimal amount)
        {
            if (IsBlocked || amount <= 0 || Balance < amount) return false;
            Balance -= amount;
            return true;
        }
    }

    public class DepositAccount : BankAccount, IInterestBearing
    {
        public decimal InterestRate { get; set; }

        public DepositAccount(string clientId, string bankId, decimal initialBalance, decimal interestRate)
            : base(clientId, bankId, initialBalance)
        {
            InterestRate = interestRate;
        }

        public void AccrueInterest()
        {
            if (!IsBlocked && Balance > 0)
            {
                decimal interest = Balance * (InterestRate / 100);
                Balance += interest;
            }
        }
    }
}
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

        public virtual void Deposit(decimal amount, bool force = false)
        {
            if (IsBlocked && !force) throw new System.Exception("Счет получателя заблокирован.");
            if (amount <= 0) throw new System.Exception("Сумма должна быть больше нуля.");
            Balance = System.Math.Round(Balance + amount, 2); 
        }

        public virtual void Withdraw(decimal amount, bool force = false)
        {
            if (IsBlocked && !force) throw new System.Exception("Ваш счет заблокирован.");
            if (amount <= 0) throw new System.Exception("Сумма должна быть больше нуля.");
            if (Balance < amount) throw new System.Exception("Недостаточно средств на счете.");
            Balance = System.Math.Round(Balance - amount, 2); 
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
                Balance = System.Math.Round(Balance + interest, 2);
            }
        }
    }
}
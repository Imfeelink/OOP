using System;

namespace Lab_1.Models
{
    public enum Role
    {
        Client,
        Manager,
        Admin
    }

    public interface IAccount
    {
        string Id { get; }
        string ClientId { get; }
        string BankId { get; }
        decimal Balance { get; }
        bool IsBlocked { get; set; }

        void Deposit(decimal amount, bool force = false);
        void Withdraw(decimal amount, bool force = false);
    }

    public interface IInterestBearing
    {
        decimal InterestRate { get; }
        void AccrueInterest();        
    }
}
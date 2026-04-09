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

        bool Deposit(decimal amount);
        bool Withdraw(decimal amount);
    }

    public interface IInterestBearing
    {
        decimal InterestRate { get; }
        void AccrueInterest();        
    }
}
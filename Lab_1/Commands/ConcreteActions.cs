using System;
using Lab_1.Models;
using Lab_1.Services;

namespace Lab_1.Commands
{
    public class TransferMoneyAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Перевод {Amount} от {_fromAccount.Id} к {_toAccount.Id}";
        public string InitiatorLogin { get; }

        private readonly BankAccount _fromAccount;
        private readonly BankAccount _toAccount;
        public readonly decimal Amount;
        private readonly JsonDataContext _context;

        //ссылка на запись в истории
        private TransactionRecord _record; 

        public TransferMoneyAction(BankAccount from, BankAccount to, decimal amount, string initiatorLogin, JsonDataContext context)
        {
            _fromAccount = from;
            _toAccount = to;
            Amount = amount;
            InitiatorLogin = initiatorLogin;
            _context = context;
        }

        public void Execute()
        {
            if (_fromAccount.Withdraw(Amount))
            {
                _toAccount.Deposit(Amount);

                //запись для истории перевод
                _record = new TransactionRecord
                {
                    FromAccountId = _fromAccount.Id,
                    ToAccountId = _toAccount.Id,
                    Amount = Amount,
                    Description = "Перевод средств"
                };
                _context.Transactions.Add(_record);
            }
            else
            {
                throw new Exception("Недостаточно средств или счет заблокирован.");
            }
        }

        //отмена действия
        public void Undo()
        {
            _toAccount.Withdraw(Amount);
            _fromAccount.Deposit(Amount);

            if (_record != null)
            {
                _context.Transactions.Remove(_record);
            }
        }
    }

    public class RegisterClientAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Регистрация клиента: {_client.Login}";
        public string InitiatorLogin { get; }

        private readonly Client _client;
        private readonly JsonDataContext _context;

        public RegisterClientAction(Client client, JsonDataContext context)
        {
            _client = client;
            InitiatorLogin = client.Login; 
            _context = context;
        }

        public void Execute()
        {
            _context.Users.Add(_client);
        }

        public void Undo()
        {
            //удаление клиента админом
            _context.Users.Remove(_client);
        }
    }

    //одобрение клиента менеджером
    public class ApproveClientAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Менеджер {InitiatorLogin} одобрил клиента: {_client.Login}";
        public string InitiatorLogin { get; }

        private readonly Client _client;

        public ApproveClientAction(Client client, string initiatorLogin)
        {
            _client = client;
            InitiatorLogin = initiatorLogin;
        }

        public void Execute()
        {
            _client.IsApproved = true;
        }

        //отмена подтверждения клиента
        public void Undo()
        {
            _client.IsApproved = false; 
        }
    }

    // блокировка/разблокировка счета
    public class ToggleAccountBlockAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Менеджер {InitiatorLogin} изменил статус блокировки счета: {_account.Id}";
        public string InitiatorLogin { get; }

        private readonly BankAccount _account;

        public ToggleAccountBlockAction(BankAccount account, string initiatorLogin)
        {
            _account = account;
            InitiatorLogin = initiatorLogin;
        }

        public void Execute()
        {
            _account.IsBlocked = !_account.IsBlocked; 
        }

        public void Undo()
        {
            _account.IsBlocked = !_account.IsBlocked; 
        }
    }

    public class OpenAccountAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Клиент {InitiatorLogin} открыл счет/вклад: {_account.Id}";
        public string InitiatorLogin { get; }

        private readonly BankAccount _account;
        private readonly JsonDataContext _context;

        public OpenAccountAction(BankAccount account, string initiatorLogin, JsonDataContext context)
        {
            _account = account;
            InitiatorLogin = initiatorLogin;
            _context = context;
        }

        public void Execute()
        {
            _context.Accounts.Add(_account);
        }

        public void Undo()
        {
            _context.Accounts.Remove(_account); 
        }
    }

    public class CloseAccountAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Клиент {InitiatorLogin} закрыл счет/вклад: {_account.Id}";
        public string InitiatorLogin { get; }

        private readonly BankAccount _account;
        private readonly JsonDataContext _context;

        public CloseAccountAction(BankAccount account, string initiatorLogin, JsonDataContext context)
        {
            _account = account;
            InitiatorLogin = initiatorLogin;
            _context = context;
        }

        public void Execute()
        {
            if (_account.Balance > 0)
                throw new System.Exception("Нельзя закрыть счет с положительным балансом!");
            _context.Accounts.Remove(_account);
        }

        public void Undo()
        {
            _context.Accounts.Add(_account); 
        }
    }
}
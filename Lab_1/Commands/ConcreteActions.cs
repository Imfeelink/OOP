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
        private TransactionRecord _record;

        public TransferMoneyAction(BankAccount from, BankAccount to, decimal amount, string initiatorLogin, JsonDataContext context)
        {
            _fromAccount = from; _toAccount = to; Amount = amount;
            InitiatorLogin = initiatorLogin; _context = context;
        }

        public void Execute()
        {
            _fromAccount.Withdraw(Amount);

            try
            {
                _toAccount.Deposit(Amount);
            }
            catch
            {
                // Если положить не удалось (счет получателя заблокирован), возвращаем деньги обратно
                _fromAccount.Deposit(Amount, force: true);
                throw; 
            }

            _record = new TransactionRecord
            {
                FromAccountId = _fromAccount.Id,
                ToAccountId = _toAccount.Id,
                Amount = Amount,
                Description = "Перевод средств"
            };
            _context.Transactions.Add(_record);
        }

        public void Undo()
        {
            // force: true позволяет админу отменять переводы даже на заблокированных счетах
            _toAccount.Withdraw(Amount, force: true);
            _fromAccount.Deposit(Amount, force: true);
            if (_record != null) _context.Transactions.Remove(_record);
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

    public class RejectClientAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Менеджер {InitiatorLogin} отклонил регистрацию: {_client.Login}";
        public string InitiatorLogin { get; }

        private readonly Client _client;
        private readonly JsonDataContext _context;

        public RejectClientAction(Client client, string initiatorLogin, JsonDataContext context)
        {
            _client = client;
            InitiatorLogin = initiatorLogin;
            _context = context;
        }

        public void Execute()
        {
            _context.Users.Remove(_client);
        }

        public void Undo()
        {
            _client.IsApproved = false;
            _context.Users.Add(_client);
        }
    }

    public class ChangeEmployeeCompanyAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => _newCompanyId != null
            ? $"Менеджер {InitiatorLogin} устроил клиента {_client.Login} в компанию"
            : $"Менеджер {InitiatorLogin} уволил клиента {_client.Login}";
        public string InitiatorLogin { get; }

        private readonly Client _client;
        private readonly string _newCompanyId;
        private readonly string _oldCompanyId;

        public ChangeEmployeeCompanyAction(Client client, string newCompanyId, string initiatorLogin)
        {
            _client = client;
            _newCompanyId = newCompanyId;
            _oldCompanyId = client.CompanyId;
            InitiatorLogin = initiatorLogin;
        }

        public void Execute() => _client.CompanyId = _newCompanyId;
        public void Undo() => _client.CompanyId = _oldCompanyId;
    }

    public class UpdateSalaryProjectAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Изменен статус зарплатного проекта для: {_company.Name}";
        public string InitiatorLogin { get; }

        private readonly Company _company;
        private readonly bool _newRequestState;
        private readonly bool _newApproveState;
        private readonly bool _oldRequestState;
        private readonly bool _oldApproveState;

        public UpdateSalaryProjectAction(Company company, bool isRequested, bool isApproved, string initiatorLogin)
        {
            _company = company;
            _newRequestState = isRequested;
            _newApproveState = isApproved;
            _oldRequestState = company.IsSalaryProjectRequested;
            _oldApproveState = company.IsSalaryProjectApproved;
            InitiatorLogin = initiatorLogin;
        }

        public void Execute()
        {
            _company.IsSalaryProjectRequested = _newRequestState;
            _company.IsSalaryProjectApproved = _newApproveState;
        }

        public void Undo()
        {
            _company.IsSalaryProjectRequested = _oldRequestState;
            _company.IsSalaryProjectApproved = _oldApproveState;
        }
    }

    public class ReceiveSalaryAction : ISystemAction
    {
        public string ActionId { get; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; } = DateTime.Now;
        public string LogMessage => $"Клиент {InitiatorLogin} получил зарплату на счет {_account.Id}";
        public string InitiatorLogin { get; }

        private readonly BankAccount _account;
        private readonly decimal _amount;
        private readonly JsonDataContext _context;
        private TransactionRecord _record;

        public ReceiveSalaryAction(BankAccount account, decimal amount, string initiatorLogin, JsonDataContext context)
        {
            _account = account;
            _amount = amount;
            InitiatorLogin = initiatorLogin;
            _context = context;
        }

        public void Execute()
        {
            _account.Deposit(_amount, force: true);
            _record = new TransactionRecord
            {
                FromAccountId = "ПРЕДПРИЯТИЕ",
                ToAccountId = _account.Id,
                Amount = _amount,
                Description = "Зарплата"
            };
            _context.Transactions.Add(_record);
        }

        public void Undo()
        {
            _account.Withdraw(_amount, force: true); 
            if (_record != null) _context.Transactions.Remove(_record);
        }
    }
}
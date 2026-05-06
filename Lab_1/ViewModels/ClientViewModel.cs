using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Lab_1.Commands;
using Lab_1.Models;

namespace Lab_1.ViewModels
{
    public class ClientViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly Client _currentClient;

        public ObservableCollection<BankAccount> MyAccounts { get; set; }
        public ObservableCollection<Bank> AllBanks { get; set; }
        public ObservableCollection<Company> AllCompanies { get; set; }
        public ObservableCollection<TransactionRecord> MyTransactions { get; set; }

        public Bank SelectedBank { get; set; }
        public BankAccount SelectedAccount { get; set; }
        public BankAccount SelectedFromAccount { get; set; }

        public Company SelectedCompany { get; set; } 

        public string ToAccountId { get; set; }

        private string _transferAmount;
        public string TransferAmount
        {
            get => _transferAmount;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _transferAmount = value;
                    OnPropertyChanged();
                    return;
                }

                if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^[0-9]*([.,][0-9]{0,2})?$"))
                {
                    _transferAmount = value;
                }

                OnPropertyChanged();
            }
        }

        public ICommand OpenAccountCommand { get; }
        public ICommand OpenDepositCommand { get; }
        public ICommand CloseAccountCommand { get; }
        public ICommand TransferCommand { get; }
        public ICommand LogoutCommand { get; }

        // Команды для предприятий
        public ICommand RequestSalaryProjectCommand { get; }
        public ICommand ReceiveSalaryCommand { get; }

        public ClientViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _currentClient = (Client)_mainViewModel.AuthService.CurrentUser;

            OpenAccountCommand = new RelayCommand(ExecuteOpenAccount, p => SelectedBank != null);
            OpenDepositCommand = new RelayCommand(ExecuteOpenDeposit, p => SelectedBank != null);
            CloseAccountCommand = new RelayCommand(ExecuteCloseAccount, p => SelectedAccount != null);
            TransferCommand = new RelayCommand(ExecuteTransfer);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            RequestSalaryProjectCommand = new RelayCommand(ExecuteRequestSalaryProject, p => SelectedCompany != null && !SelectedCompany.IsSalaryProjectRequested);
            ReceiveSalaryCommand = new RelayCommand(ExecuteReceiveSalary, p => SelectedAccount != null);

            RefreshData();
        }

        private void RefreshData()
        {
            var accounts = _mainViewModel.Context.Accounts.Where(a => a.ClientId == _currentClient.Id).ToList();
            MyAccounts = new ObservableCollection<BankAccount>(accounts);
            AllBanks = new ObservableCollection<Bank>(_mainViewModel.Context.Banks);
            AllCompanies = new ObservableCollection<Company>(_mainViewModel.Context.Companies);

            var myAccountIds = accounts.Select(a => a.Id).ToList();
            var history = _mainViewModel.Context.Transactions
                .Where(t => myAccountIds.Contains(t.FromAccountId) || myAccountIds.Contains(t.ToAccountId)).ToList();
            MyTransactions = new ObservableCollection<TransactionRecord>(history);

            OnPropertyChanged(nameof(MyAccounts));
            OnPropertyChanged(nameof(AllBanks));
            OnPropertyChanged(nameof(AllCompanies));
            OnPropertyChanged(nameof(MyTransactions));
        }

        private void ExecuteOpenAccount(object parameter)
        {
            var action = new OpenAccountAction(new BankAccount(_currentClient.Id, SelectedBank.Id, 0), _currentClient.Login, _mainViewModel.Context);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
        }

        private void ExecuteOpenDeposit(object parameter)
        {
            var action = new OpenAccountAction(new DepositAccount(_currentClient.Id, SelectedBank.Id, 0, 10m), _currentClient.Login, _mainViewModel.Context);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
        }

        private void ExecuteCloseAccount(object parameter)
        {
            try { _mainViewModel.ActionManager.ExecuteAction(new CloseAccountAction(SelectedAccount, _currentClient.Login, _mainViewModel.Context)); RefreshData(); }
            catch (System.Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ExecuteTransfer(object parameter)
        {
            if (SelectedFromAccount == null || string.IsNullOrWhiteSpace(ToAccountId) || string.IsNullOrWhiteSpace(TransferAmount)) return;

            // Превращаем точку в запятую (зависит от настроек Windows)
            string amountStr = TransferAmount.Replace(".", ",");
            if (!decimal.TryParse(amountStr, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму перевода больше нуля (например: 18,65)");
                return;
            }
            amount = System.Math.Round(amount, 2); 

            var toAccount = _mainViewModel.Context.Accounts.FirstOrDefault(a => a.Id == ToAccountId);
            if (toAccount == null) { MessageBox.Show("Счет получателя не найден!"); return; }
            try
            {
                _mainViewModel.ActionManager.ExecuteAction(new TransferMoneyAction(SelectedFromAccount, toAccount, amount, _currentClient.Login, _mainViewModel.Context));
                ToAccountId = string.Empty; TransferAmount = string.Empty;
                OnPropertyChanged(nameof(ToAccountId)); OnPropertyChanged(nameof(TransferAmount));
                RefreshData();
                MessageBox.Show("Перевод выполнен!");
            }
            catch (System.Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ExecuteRequestSalaryProject(object parameter)
        {
            var action = new UpdateSalaryProjectAction(SelectedCompany, true, false, _currentClient.Login);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
            MessageBox.Show("Заявка на зарплатный проект отправлена менеджеру!");
        }

        private void ExecuteReceiveSalary(object parameter)
        {
            if (_currentClient.CompanyId == null) { MessageBox.Show("Вы нигде не работаете!"); return; }
            var myComp = _mainViewModel.Context.Companies.FirstOrDefault(c => c.Id == _currentClient.CompanyId);
            if (myComp == null || !myComp.IsSalaryProjectApproved) { MessageBox.Show("У вашего предприятия нет одобренного зарплатного проекта!"); return; }

            var action = new ReceiveSalaryAction(SelectedAccount, myComp.SalaryAmount, myComp.Name, _currentClient.Login, _mainViewModel.Context);
            _mainViewModel.ActionManager.ExecuteAction(action);

            RefreshData();
            MessageBox.Show($"Зарплата в размере {myComp.SalaryAmount} от компании {myComp.Name} успешно зачислена!");
        }

        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.AuthService.Logout();
            _mainViewModel.CurrentViewModel = new AuthViewModel(_mainViewModel);
        }
    }
}
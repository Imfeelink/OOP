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

        public string ToAccountId { get; set; }
        public decimal TransferAmount { get; set; }

        public ICommand OpenAccountCommand { get; }
        public ICommand OpenDepositCommand { get; }
        public ICommand CloseAccountCommand { get; }
        public ICommand TransferCommand { get; }
        public ICommand LogoutCommand { get; }

        public ClientViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _currentClient = (Client)_mainViewModel.AuthService.CurrentUser;

            OpenAccountCommand = new RelayCommand(ExecuteOpenAccount, CanExecuteOpenAccount);
            OpenDepositCommand = new RelayCommand(ExecuteOpenDeposit, CanExecuteOpenAccount);
            CloseAccountCommand = new RelayCommand(ExecuteCloseAccount, CanExecuteCloseAccount);
            TransferCommand = new RelayCommand(ExecuteTransfer);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            RefreshData();
        }

        private void RefreshData()
        {
            var accounts = _mainViewModel.Context.Accounts.Where(a => a.ClientId == _currentClient.Id).ToList();
            MyAccounts = new ObservableCollection<BankAccount>(accounts);
            OnPropertyChanged(nameof(MyAccounts));

            AllBanks = new ObservableCollection<Bank>(_mainViewModel.Context.Banks);
            OnPropertyChanged(nameof(AllBanks));

            AllCompanies = new ObservableCollection<Company>(_mainViewModel.Context.Companies);
            OnPropertyChanged(nameof(AllCompanies));
            
            //история транзакций клиента
            var myAccountIds = accounts.Select(a => a.Id).ToList();
            var history = _mainViewModel.Context.Transactions
                .Where(t => myAccountIds.Contains(t.FromAccountId) || myAccountIds.Contains(t.ToAccountId))
                .ToList();
            MyTransactions = new ObservableCollection<TransactionRecord>(history);
            OnPropertyChanged(nameof(MyTransactions));
        }

        private bool CanExecuteOpenAccount(object parameter) => SelectedBank != null;

        private void ExecuteOpenAccount(object parameter)
        {
            var newAccount = new BankAccount(_currentClient.Id, SelectedBank.Id, initialBalance: 0);
            var action = new OpenAccountAction(newAccount, _currentClient.Login, _mainViewModel.Context);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
            MessageBox.Show("Обычный счет успешно открыт!");
        }

        private void ExecuteOpenDeposit(object parameter)
        {
            var newDeposit = new DepositAccount(_currentClient.Id, SelectedBank.Id, initialBalance: 0, interestRate: 10m);
            var action = new OpenAccountAction(newDeposit, _currentClient.Login, _mainViewModel.Context);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
            MessageBox.Show("Вклад под 10% успешно открыт!");
        }

        private bool CanExecuteCloseAccount(object parameter) => SelectedAccount != null;
        private void ExecuteCloseAccount(object parameter)
        {
            try
            {
                var action = new CloseAccountAction(SelectedAccount, _currentClient.Login, _mainViewModel.Context);
                _mainViewModel.ActionManager.ExecuteAction(action);
                RefreshData();
                MessageBox.Show("Счет закрыт.");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
        }

        private void ExecuteTransfer(object parameter)
        {
            if (SelectedFromAccount == null || string.IsNullOrWhiteSpace(ToAccountId) || TransferAmount <= 0)
            {
                MessageBox.Show("Заполните все поля для перевода корректно.");
                return;
            }

            var toAccount = _mainViewModel.Context.Accounts.FirstOrDefault(a => a.Id == ToAccountId);
            if (toAccount == null)
            {
                MessageBox.Show("Счет получателя не найден в системе!");
                return;
            }

            try
            {
                var action = new TransferMoneyAction(SelectedFromAccount, toAccount, TransferAmount, _currentClient.Login, _mainViewModel.Context);
                _mainViewModel.ActionManager.ExecuteAction(action);

                MessageBox.Show("Перевод успешно выполнен!");
                ToAccountId = string.Empty;
                TransferAmount = 0;
                OnPropertyChanged(nameof(ToAccountId));
                OnPropertyChanged(nameof(TransferAmount));
                RefreshData();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
        }

        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.AuthService.Logout();
            _mainViewModel.CurrentViewModel = new AuthViewModel(_mainViewModel);
        }
    }
}
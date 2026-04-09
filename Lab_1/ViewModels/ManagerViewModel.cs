using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Lab_1.Commands;
using Lab_1.Models;

namespace Lab_1.ViewModels
{
    public class ManagerViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        public ObservableCollection<Client> UnapprovedClients { get; set; }
        public ObservableCollection<BankAccount> AllAccounts { get; set; }
        public ObservableCollection<TransactionRecord> AllTransactions { get; set; }

        private Client _selectedClient;
        public Client SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(); }
        }

        private BankAccount _selectedAccount;
        public BankAccount SelectedAccount
        {
            get => _selectedAccount;
            set { _selectedAccount = value; OnPropertyChanged(); }
        }

        public ICommand ApproveClientCommand { get; }
        public ICommand ToggleBlockCommand { get; }
        public ICommand SkipMonthCommand { get; }
        public ICommand LogoutCommand { get; }

        public ManagerViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            ApproveClientCommand = new RelayCommand(ExecuteApproveClient, CanExecuteApproveClient);
            ToggleBlockCommand = new RelayCommand(ExecuteToggleBlock, CanExecuteToggleBlock);
            SkipMonthCommand = new RelayCommand(ExecuteSkipMonth);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            RefreshData();
        }
        
        //метод для обновление данных на экране
        private void RefreshData()
        {
            var unapproved = _mainViewModel.Context.Users.OfType<Client>().Where(c => !c.IsApproved).ToList();
            UnapprovedClients = new ObservableCollection<Client>(unapproved);
            OnPropertyChanged(nameof(UnapprovedClients));

            AllAccounts = new ObservableCollection<BankAccount>(_mainViewModel.Context.Accounts);
            OnPropertyChanged(nameof(AllAccounts));

            AllTransactions = new ObservableCollection<TransactionRecord>(_mainViewModel.Context.Transactions);
            OnPropertyChanged(nameof(AllTransactions));
        }

        private bool CanExecuteApproveClient(object parameter) => SelectedClient != null;
        private void ExecuteApproveClient(object parameter)
        {
            var action = new ApproveClientAction(SelectedClient, _mainViewModel.AuthService.CurrentUser.Login);
            _mainViewModel.ActionManager.ExecuteAction(action);

            MessageBox.Show($"Клиент {SelectedClient.Login} успешно одобрен!");
            RefreshData();
        }

        private bool CanExecuteToggleBlock(object parameter) => SelectedAccount != null;
        private void ExecuteToggleBlock(object parameter)
        {
            var action = new ToggleAccountBlockAction(SelectedAccount, _mainViewModel.AuthService.CurrentUser.Login);
            _mainViewModel.ActionManager.ExecuteAction(action);

            RefreshData();
        }

        private void ExecuteSkipMonth(object parameter)
        {
            _mainViewModel.BankService.SkipMonth(); 
            MessageBox.Show("Прошел 1 месяц. Проценты по вкладам начислены!");
            RefreshData();
        }

        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.AuthService.Logout();
            _mainViewModel.CurrentViewModel = new AuthViewModel(_mainViewModel);
        }
    }
}
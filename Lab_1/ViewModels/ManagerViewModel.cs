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

        // Новые коллекции для предприятий
        public ObservableCollection<Company> AllCompanies { get; set; }
        public ObservableCollection<Client> AllClients { get; set; } // Все подтвержденные клиенты для найма

        private Client _selectedClient;
        public Client SelectedClient { get => _selectedClient; set { _selectedClient = value; OnPropertyChanged(); } }

        private BankAccount _selectedAccount;
        public BankAccount SelectedAccount { get => _selectedAccount; set { _selectedAccount = value; OnPropertyChanged(); } }

        private Company _selectedCompany;
        public Company SelectedCompany { get => _selectedCompany; set { _selectedCompany = value; OnPropertyChanged(); } }

        private Client _selectedEmployee;
        public Client SelectedEmployee { get => _selectedEmployee; set { _selectedEmployee = value; OnPropertyChanged(); } }

        public ICommand ApproveClientCommand { get; }
        public ICommand RejectClientCommand { get; }
        public ICommand ToggleBlockCommand { get; }
        public ICommand SkipMonthCommand { get; }
        public ICommand LogoutCommand { get; }

        // Новые команды
        public ICommand AddEmployeeCommand { get; }
        public ICommand RemoveEmployeeCommand { get; }
        public ICommand ApproveSalaryProjectCommand { get; }

        public ManagerViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            ApproveClientCommand = new RelayCommand(ExecuteApproveClient, p => SelectedClient != null);
            RejectClientCommand = new RelayCommand(ExecuteRejectClient, p => SelectedClient != null);
            ToggleBlockCommand = new RelayCommand(ExecuteToggleBlock, p => SelectedAccount != null);
            SkipMonthCommand = new RelayCommand(ExecuteSkipMonth);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            AddEmployeeCommand = new RelayCommand(ExecuteAddEmployee, p => SelectedCompany != null && SelectedEmployee != null);
            RemoveEmployeeCommand = new RelayCommand(ExecuteRemoveEmployee, p => SelectedEmployee != null && SelectedEmployee.CompanyId != null);
            ApproveSalaryProjectCommand = new RelayCommand(ExecuteApproveSalaryProject, p => SelectedCompany != null && SelectedCompany.IsSalaryProjectRequested);

            RefreshData();
        }

        private void RefreshData()
        {
            UnapprovedClients = new ObservableCollection<Client>(_mainViewModel.Context.Users.OfType<Client>().Where(c => !c.IsApproved));
 
            var approvedClients = _mainViewModel.Context.Users.OfType<Client>().Where(c => c.IsApproved).ToList();
            foreach (var c in approvedClients)
            {
                var comp = _mainViewModel.Context.Companies.FirstOrDefault(x => x.Id == c.CompanyId);
                c.DisplayCompanyName = comp != null ? comp.Name : "Не трудоустроен";
            }

            AllClients = new ObservableCollection<Client>(approvedClients);
            AllAccounts = new ObservableCollection<BankAccount>(_mainViewModel.Context.Accounts);
            AllTransactions = new ObservableCollection<TransactionRecord>(_mainViewModel.Context.Transactions);
            AllCompanies = new ObservableCollection<Company>(_mainViewModel.Context.Companies);

            OnPropertyChanged(nameof(UnapprovedClients));
            OnPropertyChanged(nameof(AllClients));
            OnPropertyChanged(nameof(AllAccounts));
            OnPropertyChanged(nameof(AllTransactions));
            OnPropertyChanged(nameof(AllCompanies));
        }

        private void ExecuteApproveClient(object parameter)
        {
            var action = new ApproveClientAction(SelectedClient, _mainViewModel.AuthService.CurrentUser.Login);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
        }

        private void ExecuteRejectClient(object parameter)
        {
            var action = new RejectClientAction(SelectedClient, _mainViewModel.AuthService.CurrentUser.Login, _mainViewModel.Context);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
        }

        private void ExecuteToggleBlock(object parameter)
        {
            var action = new ToggleAccountBlockAction(SelectedAccount, _mainViewModel.AuthService.CurrentUser.Login);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
        }

        private void ExecuteSkipMonth(object parameter)
        {
            _mainViewModel.BankService.SkipMonth();
            MessageBox.Show("Прошел 1 месяц. Проценты начислены!");
            RefreshData();
        }

        private void ExecuteAddEmployee(object parameter)
        {
            var action = new ChangeEmployeeCompanyAction(SelectedEmployee, SelectedCompany.Id, _mainViewModel.AuthService.CurrentUser.Login);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
        }

        private void ExecuteRemoveEmployee(object parameter)
        {
            var action = new ChangeEmployeeCompanyAction(SelectedEmployee, null, _mainViewModel.AuthService.CurrentUser.Login);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
        }

        private void ExecuteApproveSalaryProject(object parameter)
        {
            var action = new UpdateSalaryProjectAction(SelectedCompany, true, true, _mainViewModel.AuthService.CurrentUser.Login);
            _mainViewModel.ActionManager.ExecuteAction(action);
            RefreshData();
        }

        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.AuthService.Logout();
            _mainViewModel.CurrentViewModel = new AuthViewModel(_mainViewModel);
        }
    }
}
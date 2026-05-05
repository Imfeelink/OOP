using System.Windows;
using System.Windows.Input;
using Lab_1.Models;

namespace Lab_1.ViewModels
{
    public class AuthViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        private string _login;
        public string Login
        {
            get => _login;

            //обновление UI при вводе
            set { _login = value; OnPropertyChanged(); } 
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterClientCommand { get; }

        public AuthViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            RegisterClientCommand = new RelayCommand(ExecuteRegister);
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object parameter)
        {
            ErrorMessage = string.Empty;
            try
            {
                if (_mainViewModel.AuthService.Login(Login, Password))
                {
                    var user = _mainViewModel.AuthService.CurrentUser;
                    if (user is Admin)
                    {
                        _mainViewModel.CurrentViewModel = new AdminViewModel(_mainViewModel); 
                    }
                    else if (user is Manager)
                    {
                         _mainViewModel.CurrentViewModel = new ManagerViewModel(_mainViewModel); 
                    }
                    else if (user is Client)
                    {
                         _mainViewModel.CurrentViewModel = new ClientViewModel(_mainViewModel);
                    }
                }
                else
                {
                    ErrorMessage = "Неверный логин или пароль!";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message; 
            }
        }

        private void ExecuteRegister(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Для регистрации заполните логин и пароль!";
                return;
            }

            var newClient = new Client { Login = Login, PasswordHash = Password };

            var registerAction = new Commands.RegisterClientAction(newClient, _mainViewModel.Context);
            _mainViewModel.ActionManager.ExecuteAction(registerAction);

            ErrorMessage = "Регистрация успешна! Ожидайте подтверждения менеджера.";
        }
    }
}
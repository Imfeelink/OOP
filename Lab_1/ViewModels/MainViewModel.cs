using System.Windows.Input;
using Lab_1.Services;

namespace Lab_1.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public JsonDataContext Context { get; }
        public AuthService AuthService { get; }
        public ActionManager ActionManager { get; }
        public BankService BankService { get; }

        //текущая вкладка
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;

                //уведомляет окно о смене экрана
                OnPropertyChanged(); 
            }
        }

        public MainViewModel()
        {
            Context = JsonDataContext.Load();
            AuthService = new AuthService(Context);
            ActionManager = new ActionManager(Context);
            BankService = new BankService(Context);

            CurrentViewModel = new AuthViewModel(this);
        }
    }
}
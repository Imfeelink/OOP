using System.Collections.ObjectModel;
using System.Windows.Input;
using Lab_1.Commands;

namespace Lab_1.ViewModels
{
    public class AdminViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        //уведомляет окно, если что-то изменилось
        public ObservableCollection<ISystemAction> ActionLogs { get; set; }

        private ISystemAction _selectedAction;
        public ISystemAction SelectedAction
        {
            get => _selectedAction;
            set { _selectedAction = value; OnPropertyChanged(); }
        }

        public ICommand UndoCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            ActionLogs = new ObservableCollection<ISystemAction>(_mainViewModel.ActionManager.History);

            UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        private bool CanExecuteUndo(object parameter)
        {
            return SelectedAction != null;
        }

        private void ExecuteUndo(object parameter)
        {
            _mainViewModel.ActionManager.UndoAction(SelectedAction);

            ActionLogs.Remove(SelectedAction);

            System.Windows.MessageBox.Show("Действие успешно отменено и данные возвращены в исходное состояние.");
        }

        private void ExecuteLogout(object parameter)
        {
            _mainViewModel.AuthService.Logout();
            _mainViewModel.CurrentViewModel = new AuthViewModel(_mainViewModel); 
        }
    }
}
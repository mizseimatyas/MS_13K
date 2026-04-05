using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WorkerApp.Dto;
using WorkerApp.Model;
using WorkerApp.Utils;

namespace WorkerApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        public string UserName
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _role;
        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isWorkerSelected = true;
        public bool IsWorkerSelected
        {
            get => _isWorkerSelected;
            set
            {
                if (_isWorkerSelected != value)
                {
                    _isWorkerSelected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsAdminSelected));
                }
            }
        }

        public bool IsAdminSelected
        {
            get => !_isWorkerSelected;
            set
            {
                if (value != !_isWorkerSelected)
                {
                    _isWorkerSelected = !value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsWorkerSelected));
                }
            }
        }

        private readonly MainViewModel _main;
        private readonly AuthModel _auth;
        private readonly ItemsModel _items;
        private readonly OrdersModel _orders;

        public ICommand LoginWithSelectedRoleCommand { get; }

        public LoginViewModel(
            MainViewModel main,
            AuthModel auth,
            ItemsModel items,
            OrdersModel orders)
        {
            _main = main;
            _auth = auth;
            _items = items;
            _orders = orders;

            LoginWithSelectedRoleCommand =
                new Utils.RelayCommand(async _ => await ExecuteLogin());
        }

        private async Task ExecuteLogin()
        {
            ErrorMessage = string.Empty;

            var roleKey = IsWorkerSelected ? "worker" : "admin";

            var result = await _auth.LoginAsync(UserName, Password, roleKey);
            if (result == null)
            {
                ErrorMessage = "Hibás felhasználónév vagy jelszó.";
                return;
            }

            Role = result.Role;

            if (Role != "Worker" && Role != "Admin")
            {
                ErrorMessage = "Nincs jogosultságod a belépéshez!";
                return;
            }

            _main.CurrentPage = new SelectionViewModel(_main, Role, _items, _orders, _auth);
        }
    }
}
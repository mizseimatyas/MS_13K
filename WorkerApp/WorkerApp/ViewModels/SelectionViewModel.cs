using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WorkerApp.Model;

namespace WorkerApp.ViewModels
{
    public class SelectionViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly string _role;
        private readonly ItemsModel _items;
        private readonly OrdersModel _orders;
        private readonly AuthModel _auth;

        public string HeaderText => _role == "Worker" ? "Raktáros nézet" : "Admin nézet";

        public ICommand OpenItemsCommand { get; }
        public ICommand OpenOrdersCommand { get; }
        public ICommand LogoutCommand { get; }

        public SelectionViewModel(
            MainViewModel main,
            string role,
            ItemsModel items,
            OrdersModel orders,
            AuthModel auth)
        {
            _main = main;
            _role = role;
            _items = items;
            _orders = orders;
            _auth = auth;

            OpenItemsCommand = new Utils.RelayCommand(_ =>
            {
                _main.CurrentPage = new ItemsListViewModel(_main, _role, _items, _orders, _auth);
                return Task.CompletedTask;
            });

            OpenOrdersCommand = new Utils.RelayCommand(_ =>
            {
                _main.CurrentPage = new OrdersListViewModel(_main, _role, _items, _orders, _auth);
                return Task.CompletedTask;
            });

            LogoutCommand = new Utils.RelayCommand(_ =>
            {
                _main.CurrentPage = new LoginViewModel(_main, _auth, _items, _orders);
                return Task.CompletedTask;
            });
        }
    }
}
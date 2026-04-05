using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WorkerApp.Dto;
using WorkerApp.Model;

namespace WorkerApp.ViewModels
{
    public class OrdersListViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly string _role;
        private readonly ItemsModel _itemsModel;
        private readonly OrdersModel _ordersModel;
        private readonly AuthModel _authModel;


        public ObservableCollection<OrderAllDto> Orders { get; } = new();

        private OrderAllDto _selectedOrder;
        public OrderAllDto SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoadOrdersCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand BackCommand { get; }

        public OrdersListViewModel(MainViewModel main, string role, ItemsModel itemsModel, OrdersModel ordersModel, AuthModel authModel)
        {
            _main = main;
            _role = role;
            _itemsModel = itemsModel;
            _ordersModel = ordersModel;
            _authModel = authModel;

            LoadOrdersCommand = new Utils.RelayCommand(async _ => await LoadOrders());
            EditOrderCommand = new Utils.RelayCommand(async _ => await GoEditOrder());

            BackCommand = new Utils.RelayCommand(_ =>
            {
                _main.CurrentPage = new SelectionViewModel(_main, _role, _itemsModel, _ordersModel, _authModel);
                return Task.CompletedTask;
            });

            _ = LoadOrders();
        }

        private async Task LoadOrders()
        {
            var data = await _ordersModel.GetAllOrdersAsync();

            Orders.Clear();
            if (data != null)
            {
                foreach (var order in data)
                    Orders.Add(order);
            }
        }

        private Task GoEditOrder()
        {
            if (SelectedOrder == null)
                return Task.CompletedTask;

            _main.CurrentPage = new OrderEditViewModel(
                _main,
                SelectedOrder,
                _role,
                _itemsModel,
                _ordersModel,
                _authModel);

            return Task.CompletedTask;
        }
    }
}

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
    public class OrderEditViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly string _role;
        private readonly ItemsModel _itemsModel;
        private readonly OrdersModel _ordersModel;
        private readonly AuthModel _authModel;

        private OrderAllDto _order;
        public OrderAllDto Order
        {
            get => _order;
            set { _order = value; OnPropertyChanged(); }
        }

        private OrderDetailsDto? _orderDetails;
        public OrderDetailsDto? OrderDetails
        {
            get => _orderDetails;
            set { _orderDetails = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDetailsLoaded)); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDetailsLoaded)); }
        }

        public bool IsDetailsLoaded => !_isLoading && _orderDetails != null;

        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string>
        {
            "Cancelled", "DataConfirmed", "PendingPayment",
            "PaymentSuccess", "Delivering", "OrderCompleted"
        };

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public ICommand SaveStatusCommand { get; }
        public ICommand CancelEditCommand { get; }

        public OrderEditViewModel(MainViewModel main, OrderAllDto order, string role, ItemsModel itemsModel, OrdersModel ordersModel, AuthModel authModel)
        {
            _main = main;
            _role = role;
            _itemsModel = itemsModel;
            _ordersModel = ordersModel;
            _authModel = authModel;

            Order = order;
            SelectedStatus = order.Status;

            SaveStatusCommand = new Utils.RelayCommand(async _ => await SaveStatus());
            CancelEditCommand = new Utils.RelayCommand(_ => CancelEdit());

            _ = LoadOrderDetails();
        }

        private async Task LoadOrderDetails()
        {
            IsLoading = true;
            OrderDetails = await _ordersModel.GetOrderDetailsAsync(Order.OrderId);
            IsLoading = false;
        }

        private async Task SaveStatus()
        {
            Message = string.Empty;
            var dto = new UpdateOrderStatusDto
            {
                OrderId = Order.OrderId,
                OrderStatus = SelectedStatus
            };

            var ok = await _ordersModel.UpdateOrderStatusAsync(dto);
            if (!ok) { Message = "Nem sikerült frissíteni a státuszt"; return; }

            Order.Status = SelectedStatus;
            OnPropertyChanged(nameof(Order));
            _main.CurrentPage = new OrdersListViewModel(_main, _role, _itemsModel, _ordersModel, _authModel);
        }

        private Task CancelEdit()
        {
            _main.CurrentPage = new OrdersListViewModel(_main, _role, _itemsModel, _ordersModel, _authModel);
            return Task.CompletedTask;
        }
    }
}
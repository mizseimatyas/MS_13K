using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WorkerApp.Dto;
using WorkerApp.Model;

namespace WorkerApp.ViewModels
{
    public class ItemsEditViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly string _role;
        private readonly ItemsModel _itemsModel;
        private readonly OrdersModel _ordersModel;
        private readonly AuthModel _authModel;

        public ItemDto Item { get; }

        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set { if (_itemName != value) { _itemName = value; OnPropertyChanged(); } }
        }

        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set { if (_categoryName != value) { _categoryName = value; OnPropertyChanged(); } }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { if (_quantity != value) { _quantity = value; OnPropertyChanged(); } }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { if (_description != value) { _description = value; OnPropertyChanged(); } }
        }

        private int _price;
        public int Price
        {
            get => _price;
            set { if (_price != value) { _price = value; OnPropertyChanged(); } }
        }

        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ItemsEditViewModel(
            MainViewModel main,
            string role,
            ItemsModel itemsModel,
            OrdersModel ordersModel,
            AuthModel authModel,
            ItemDto item)
        {
            _main = main;
            _role = role;
            _ordersModel = ordersModel;
            _itemsModel = itemsModel;
            _authModel = authModel;

            Item = item;

            ItemName = item.ItemName;
            CategoryName = item.CategoryName;
            Quantity = item.Quantity;
            Description = item.Description;
            Price = item.Price;

            SaveCommand = new AsyncRelayCommand(Save);
            CancelCommand = new RelayCommand(() =>
            {
                _main.CurrentPage = new ItemsListViewModel(_main, _role, _itemsModel, _ordersModel, _authModel);
            });
        }

        private async Task Save()
        {
            Item.ItemName = ItemName;
            Item.CategoryName = CategoryName;
            Item.Quantity = Quantity;
            Item.Description = Description;
            Item.Price = Price;

            await _itemsModel.UpdateItemAsync(Item);

            _main.CurrentPage = new ItemsListViewModel(_main, _role, _itemsModel, _ordersModel, _authModel);
        }

    }
}
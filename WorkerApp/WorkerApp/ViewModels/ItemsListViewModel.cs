using CommunityToolkit.Mvvm.Input;
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
    public class ItemsListViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly string _role;
        private readonly ItemsModel _itemsModel;
        private readonly OrdersModel _ordersModel;
        private readonly AuthModel _authModel;

        public ObservableCollection<ItemDto> Items { get; } = new();

        private ItemDto _selectedItem;
        public ItemDto SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                }
            }
        }

        public IAsyncRelayCommand LoadItemsCommand { get; }
        public IRelayCommand BackCommand { get; }
        public IRelayCommand EditItemCommand { get; }

        public ItemsListViewModel(
            MainViewModel main,
            string role,
            ItemsModel itemsModel,
            OrdersModel ordersModel,
            AuthModel authModel)
        {
            _main = main;
            _role = role;
            _itemsModel = itemsModel;
            _ordersModel = ordersModel;
            _authModel = authModel;

            LoadItemsCommand = new AsyncRelayCommand(LoadItems);
            BackCommand = new RelayCommand(() =>
            {
                _main.CurrentPage = new SelectionViewModel(_main, _role, _itemsModel, _ordersModel, _authModel);
            });
            EditItemCommand = new RelayCommand(GoEditItem);

            _ = LoadItems();
        }

        private async Task LoadItems()
        {
            var data = await _itemsModel.GetAllItemsAsync();
            Items.Clear();

            if (data != null)
            {
                foreach (var item in data)
                    Items.Add(item);
            }
        }

        private void GoEditItem()
        {
            if (SelectedItem == null) return;

            _main.CurrentPage = new ItemsEditViewModel(
                _main,
                _role,
                _itemsModel,
                _ordersModel,
                _authModel,
                SelectedItem);
        }
    }
}

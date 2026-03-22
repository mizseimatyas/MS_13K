using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WorkerApp.Model;

namespace WorkerApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand ExitCommand { get; }

    private readonly AuthModel _authModel;
    private readonly ItemsModel _itemsModel;
    private readonly OrdersModel _ordersModel;

    public MainViewModel(AuthModel authModel, ItemsModel itemsModel, OrdersModel ordersModel)
    {
        _authModel = authModel ?? throw new ArgumentNullException(nameof(authModel));
        _itemsModel = itemsModel ?? throw new ArgumentNullException(nameof(itemsModel));
        _ordersModel = ordersModel ?? throw new ArgumentNullException(nameof(ordersModel));

        // itt már garantáltan nem null semmi
        CurrentPage = new LoginViewModel(this, _authModel, _itemsModel, _ordersModel);

        ExitCommand = new Utils.RelayCommand(_ =>
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        });
    }
}
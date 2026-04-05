using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WorkerApp.Model;
using WorkerApp.ViewModels;
using WorkerApp.Views;

namespace WorkerApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            const string baseUrl = "https://localhost:7149/";

            var authModel = new AuthModel(baseUrl);
            var itemsModel = new ItemsModel(baseUrl);
            var ordersModel = new OrdersModel(baseUrl);

            var mainVm = new MainViewModel(authModel, itemsModel, ordersModel);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
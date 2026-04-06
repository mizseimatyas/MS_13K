using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Net.Http;
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

            var handler = new System.Net.Http.HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };
            var sharedClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            var authModel = new AuthModel(sharedClient);
            var itemsModel = new ItemsModel(sharedClient);
            var ordersModel = new OrdersModel(sharedClient);

            var mainVm = new MainViewModel(authModel, itemsModel, ordersModel);

            desktop.MainWindow = new MainWindow { DataContext = mainVm };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
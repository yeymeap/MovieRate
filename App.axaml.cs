using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using MovieRate.ViewModels;
using MovieRate.Views;
using MovieRate.Services;

namespace MovieRate;

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
            DisableAvaloniaDataAnnotationValidation();
            var authService = new AuthService();
            Task.Run(async () => await authService.InitializeAsync()).Wait();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(authService),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
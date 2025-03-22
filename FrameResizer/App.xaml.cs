using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using FrameResizer.Interface;
using FrameResizer.Service;
using FrameResizer.ViewModel;

namespace FrameResizer;

public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App()
    {
        this.Services = ConfigureServices();
        InitializeComponent();
    }

    public new static App Current => (App)Application.Current;

    // DI設定
    private static IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new ServiceCollection();

        // Service
        services.AddSingleton<ICustomiseImage, CustomiseImage>();
        // ViewModel
        services.AddTransient<ImageConfig>();

        return services.BuildServiceProvider();
    }
}

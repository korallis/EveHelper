using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using EveHelper.Core.Extensions;
using EveHelper.Data.Extensions;
using EveHelper.Services.Extensions;
using EveHelper.App.Extensions;
using System;

namespace EveHelper.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Gets the current service provider instance
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// Gets a service of the specified type
    /// </summary>
    /// <typeparam name="T">The type of service to get</typeparam>
    /// <returns>The service instance</returns>
    public static T GetService<T>() where T : class
    {
        return ServiceProvider?.GetRequiredService<T>() ?? throw new InvalidOperationException("Service provider not initialized");
    }

    /// <summary>
    /// Application constructor
    /// </summary>
    public App()
    {
        InitializeComponent();
        ConfigureServices();
    }

    /// <summary>
    /// Configures the dependency injection services
    /// </summary>
    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register services from all layers
        services.AddCore();           // Core interfaces and models
        services.AddData();           // Data access layer
        services.AddServices();       // Business services layer  
        services.AddApp();            // Application layer (ViewModels, Views, Navigation)

        // Register ViewModels (will be added as we create them)
        // services.AddTransient<MainViewModel>();

        ServiceProvider = services.BuildServiceProvider();
    }
}


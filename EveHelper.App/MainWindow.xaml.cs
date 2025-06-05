using System.Windows;
using EveHelper.Core.Interfaces;
using EveHelper.App.Views;
using EveHelper.App.ViewModels;
using EveHelper.App.Services;

namespace EveHelper.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow()
    {
        InitializeComponent();
        
        // Get navigation service from DI container
        _navigationService = App.GetService<INavigationService>();
        
        // Configure navigation service
        SetupNavigation();
    }

    /// <summary>
    /// Sets up the navigation service with the main content frame
    /// </summary>
    private void SetupNavigation()
    {
        if (_navigationService is NavigationService navService)
        {
            // Set the navigation frame
            navService.SetNavigationFrame(MainContentFrame);
            
            // Register views
            navService.RegisterView("HomeView", typeof(HomeView));
            
            // Register ViewModel to View mappings
            navService.RegisterViewMapping<HomeViewModel, HomeView>();
        }
    }

    /// <summary>
    /// Handles the home button click to test navigation
    /// </summary>
    /// <param name="sender">The button that was clicked</param>
    /// <param name="e">Event arguments</param>
    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Hide default content
            DefaultContent.Visibility = Visibility.Collapsed;
            
            // Navigate to HomeView using ViewModel
            _navigationService.NavigateTo<HomeViewModel>("Welcome from Navigation!");
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Navigation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
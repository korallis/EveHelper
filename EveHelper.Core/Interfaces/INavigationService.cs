using System;

namespace EveHelper.Core.Interfaces
{
    /// <summary>
    /// Interface for navigation service
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to a view associated with the specified ViewModel type
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type</typeparam>
        void NavigateTo<TViewModel>() where TViewModel : class;

        /// <summary>
        /// Navigates to a view associated with the specified ViewModel type with a parameter
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type</typeparam>
        /// <param name="parameter">Parameter to pass to the ViewModel</param>
        void NavigateTo<TViewModel>(object parameter) where TViewModel : class;

        /// <summary>
        /// Navigates to a view by name
        /// </summary>
        /// <param name="viewName">Name of the view to navigate to</param>
        void NavigateTo(string viewName);

        /// <summary>
        /// Navigates to a view by name with a parameter
        /// </summary>
        /// <param name="viewName">Name of the view to navigate to</param>
        /// <param name="parameter">Parameter to pass to the view</param>
        void NavigateTo(string viewName, object parameter);

        /// <summary>
        /// Goes back to the previous view if possible
        /// </summary>
        void GoBack();

        /// <summary>
        /// Gets whether navigation can go back
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Clears the navigation history
        /// </summary>
        void ClearHistory();

        /// <summary>
        /// Event raised when navigation occurs
        /// </summary>
        event EventHandler<NavigationEventArgs>? Navigated;
    }

    /// <summary>
    /// Event arguments for navigation events
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the view being navigated to
        /// </summary>
        public string ViewName { get; }

        /// <summary>
        /// Gets the parameter passed to the view
        /// </summary>
        public object? Parameter { get; }

        /// <summary>
        /// Initializes a new instance of NavigationEventArgs
        /// </summary>
        /// <param name="viewName">Name of the view</param>
        /// <param name="parameter">Parameter passed to the view</param>
        public NavigationEventArgs(string viewName, object? parameter = null)
        {
            ViewName = viewName;
            Parameter = parameter;
        }
    }
} 
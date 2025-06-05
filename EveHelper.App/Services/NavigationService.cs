using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using EveHelper.Core.Interfaces;

namespace EveHelper.App.Services
{
    /// <summary>
    /// Service for handling navigation between views in the application
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _viewRegistry;
        private readonly Dictionary<Type, Type> _viewModelToViewMap;
        private readonly Stack<NavigationEntry> _navigationHistory;
        private Frame? _navigationFrame;

        /// <summary>
        /// Event raised when navigation occurs
        /// </summary>
        public event EventHandler<NavigationEventArgs>? Navigated;

        /// <summary>
        /// Gets whether navigation can go back
        /// </summary>
        public bool CanGoBack => _navigationHistory.Count > 1;

        /// <summary>
        /// Initializes a new instance of NavigationService
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving dependencies</param>
        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _viewRegistry = new Dictionary<string, Type>();
            _viewModelToViewMap = new Dictionary<Type, Type>();
            _navigationHistory = new Stack<NavigationEntry>();
        }

        /// <summary>
        /// Sets the navigation frame for this service
        /// </summary>
        /// <param name="frame">The frame to use for navigation</param>
        public void SetNavigationFrame(Frame frame)
        {
            _navigationFrame = frame;
        }

        /// <summary>
        /// Registers a view with the navigation service
        /// </summary>
        /// <param name="viewName">Name of the view</param>
        /// <param name="viewType">Type of the view</param>
        public void RegisterView(string viewName, Type viewType)
        {
            _viewRegistry[viewName] = viewType;
        }

        /// <summary>
        /// Registers a ViewModel to View mapping
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel type</typeparam>
        /// <typeparam name="TView">View type</typeparam>
        public void RegisterViewMapping<TViewModel, TView>() 
            where TViewModel : class 
            where TView : class
        {
            _viewModelToViewMap[typeof(TViewModel)] = typeof(TView);
        }

        /// <summary>
        /// Navigates to a view associated with the specified ViewModel type
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type</typeparam>
        public void NavigateTo<TViewModel>() where TViewModel : class
        {
            NavigateTo<TViewModel>(null);
        }

        /// <summary>
        /// Navigates to a view associated with the specified ViewModel type with a parameter
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type</typeparam>
        /// <param name="parameter">Parameter to pass to the ViewModel</param>
        public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
        {
            var viewModelType = typeof(TViewModel);
            if (!_viewModelToViewMap.TryGetValue(viewModelType, out var viewType))
            {
                throw new InvalidOperationException($"No view registered for ViewModel {viewModelType.Name}");
            }

            var viewName = viewType.Name;
            NavigateToView(viewName, viewType, parameter);
        }

        /// <summary>
        /// Navigates to a view by name
        /// </summary>
        /// <param name="viewName">Name of the view to navigate to</param>
        public void NavigateTo(string viewName)
        {
            NavigateTo(viewName, null);
        }

        /// <summary>
        /// Navigates to a view by name with a parameter
        /// </summary>
        /// <param name="viewName">Name of the view to navigate to</param>
        /// <param name="parameter">Parameter to pass to the view</param>
        public void NavigateTo(string viewName, object parameter)
        {
            if (!_viewRegistry.TryGetValue(viewName, out var viewType))
            {
                throw new InvalidOperationException($"View '{viewName}' is not registered");
            }

            NavigateToView(viewName, viewType, parameter);
        }

        /// <summary>
        /// Goes back to the previous view if possible
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack) return;

            // Remove current entry
            _navigationHistory.Pop();

            // Get previous entry
            var previousEntry = _navigationHistory.Peek();
            
            // Navigate without adding to history
            NavigateToView(previousEntry.ViewName, previousEntry.ViewType, previousEntry.Parameter, false);
        }

        /// <summary>
        /// Clears the navigation history
        /// </summary>
        public void ClearHistory()
        {
            _navigationHistory.Clear();
        }

        /// <summary>
        /// Performs the actual navigation to a view
        /// </summary>
        /// <param name="viewName">Name of the view</param>
        /// <param name="viewType">Type of the view</param>
        /// <param name="parameter">Parameter to pass</param>
        /// <param name="addToHistory">Whether to add this navigation to history</param>
        private void NavigateToView(string viewName, Type viewType, object? parameter, bool addToHistory = true)
        {
            if (_navigationFrame == null)
            {
                throw new InvalidOperationException("Navigation frame not set. Call SetNavigationFrame first.");
            }

            try
            {
                // Create the view instance
                var view = _serviceProvider.GetService(viewType) ?? Activator.CreateInstance(viewType);
                
                if (view is not UserControl userControl)
                {
                    throw new InvalidOperationException($"View {viewType.Name} must inherit from UserControl");
                }

                // Set the DataContext if the view has a corresponding ViewModel
                if (view is FrameworkElement frameworkElement)
                {
                    var viewModel = TryCreateViewModel(viewType, parameter);
                    if (viewModel != null)
                    {
                        frameworkElement.DataContext = viewModel;
                    }
                }

                // Navigate to the view
                _navigationFrame.Navigate(userControl);

                // Add to history if requested
                if (addToHistory)
                {
                    _navigationHistory.Push(new NavigationEntry(viewName, viewType, parameter));
                }

                // Raise navigation event
                Navigated?.Invoke(this, new NavigationEventArgs(viewName, parameter));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to navigate to view '{viewName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Attempts to create a ViewModel for the given view type
        /// </summary>
        /// <param name="viewType">Type of the view</param>
        /// <param name="parameter">Parameter to pass to the ViewModel</param>
        /// <returns>The created ViewModel or null if none found</returns>
        private object? TryCreateViewModel(Type viewType, object? parameter)
        {
            // Look for a corresponding ViewModel type
            var viewModelTypeName = viewType.Name.Replace("View", "ViewModel");
            var viewModelType = viewType.Assembly.GetType($"{viewType.Namespace}.{viewModelTypeName}") 
                               ?? viewType.Assembly.GetType($"{viewType.Namespace}ViewModels.{viewModelTypeName}");

            if (viewModelType == null) return null;

            try
            {
                var viewModel = _serviceProvider.GetService(viewModelType) ?? Activator.CreateInstance(viewModelType);
                
                // If the ViewModel has an Initialize method, call it with the parameter
                var initializeMethod = viewModelType.GetMethod("Initialize");
                if (initializeMethod != null && parameter != null)
                {
                    initializeMethod.Invoke(viewModel, new[] { parameter });
                }

                return viewModel;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Represents a navigation history entry
        /// </summary>
        private class NavigationEntry
        {
            public string ViewName { get; }
            public Type ViewType { get; }
            public object? Parameter { get; }

            public NavigationEntry(string viewName, Type viewType, object? parameter)
            {
                ViewName = viewName;
                ViewType = viewType;
                Parameter = parameter;
            }
        }
    }
} 
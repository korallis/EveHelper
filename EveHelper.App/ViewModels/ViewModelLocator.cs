using Microsoft.Extensions.DependencyInjection;
using System;

namespace EveHelper.App.ViewModels
{
    /// <summary>
    /// Locator for ViewModels, provides easy access to ViewModels for XAML binding
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Gets the current instance of the ViewModelLocator
        /// </summary>
        public static ViewModelLocator Instance { get; } = new ViewModelLocator();

        /// <summary>
        /// Gets a ViewModel of the specified type
        /// </summary>
        /// <typeparam name="T">Type of ViewModel to get</typeparam>
        /// <returns>The ViewModel instance</returns>
        public T GetViewModel<T>() where T : class
        {
            return App.GetService<T>();
        }

        /// <summary>
        /// Gets a ViewModel by type
        /// </summary>
        /// <param name="viewModelType">Type of the ViewModel</param>
        /// <returns>The ViewModel instance</returns>
        public object GetViewModel(Type viewModelType)
        {
            return App.ServiceProvider?.GetService(viewModelType) 
                   ?? throw new InvalidOperationException($"Could not resolve ViewModel of type {viewModelType.Name}");
        }

        /// <summary>
        /// Creates a new instance of the specified ViewModel type
        /// </summary>
        /// <typeparam name="T">Type of ViewModel to create</typeparam>
        /// <returns>A new ViewModel instance</returns>
        public T CreateViewModel<T>() where T : class, new()
        {
            // Try to get from DI container first, otherwise create new instance
            return App.ServiceProvider?.GetService<T>() ?? new T();
        }
    }
} 
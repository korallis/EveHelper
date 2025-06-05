using System;

namespace EveHelper.Core.Interfaces
{
    /// <summary>
    /// Base interface for data services
    /// </summary>
    public interface IDataService
    {
        Task InitializeAsync();
        Task<bool> IsInitializedAsync();
    }
} 
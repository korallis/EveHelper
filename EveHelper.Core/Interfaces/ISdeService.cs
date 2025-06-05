using System;
using System.Threading;
using System.Threading.Tasks;
using EveHelper.Core.Models.Sde;

namespace EveHelper.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that manages the EVE Online Static Data Export (SDE).
    /// This includes checking for updates, downloading, verifying, and applying the SDE.
    /// </summary>
    public interface ISdeService
    {
        /// <summary>
        /// Event raised when a new SDE version is detected and available for download.
        /// </summary>
        event EventHandler<SdeUpdateAvailableEventArgs>? SdeUpdateAvailable;

        /// <summary>
        /// Event raised to report progress during SDE download, verification, or application.
        /// </summary>
        event EventHandler<SdeUpdateProgressEventArgs>? SdeUpdateProgress;

        /// <summary>
        /// Asynchronously retrieves information about the latest available SDE version from the source.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// <see cref="SdeVersionInfo"/> for the latest version, or null if an error occurs or no version is found.
        /// </returns>
        Task<SdeVersionInfo?> GetLatestSdeVersionInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves the version identifier of the currently installed local SDE.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the version string
        /// of the local SDE, or null if no SDE is installed or the version cannot be determined.
        /// </returns>
        Task<string?> GetCurrentLocalSdeVersionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously downloads, verifies, and applies the specified SDE version.
        /// Progress will be reported via the <see cref="SdeUpdateProgress"/> event.
        /// </summary>
        /// <param name="sdeVersionToApply">Information about the SDE version to download and apply.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is true if the SDE
        /// was successfully downloaded, verified, and applied; otherwise, false.
        /// </returns>
        Task<bool> DownloadAndApplySdeAsync(SdeVersionInfo sdeVersionToApply, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously checks for SDE updates by comparing the latest available version with the local version.
        /// If an update is available, the <see cref="SdeUpdateAvailable"/> event will be raised.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CheckForSdeUpdatesAsync(CancellationToken cancellationToken = default);
    }
} 
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Sde;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace EveHelper.Services.Services
{
    public class SdeService : ISdeService
    {
        private const string SdeSourceUrl = "https://www.fuzzwork.co.uk/dump/";
        private const string SqliteLatestFilePattern = "sqlite-latest.zip"; 
        private const string Md5FilePattern = "sqlite-latest.zip.md5";

        private readonly HttpClient _httpClient;
        private readonly ILogger<SdeService> _logger;
        private readonly string _sdeDirectory;
        private readonly string _sdeVersionFile;
        private readonly string _sdeDatabaseFile;

        public event EventHandler<SdeUpdateAvailableEventArgs>? SdeUpdateAvailable;
        public event EventHandler<SdeUpdateProgressEventArgs>? SdeUpdateProgress;

        public SdeService(ILogger<SdeService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("EveHelperApp/1.0");

            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EveHelper");
            _sdeDirectory = Path.Combine(appDataPath, "SDE");
            _sdeVersionFile = Path.Combine(_sdeDirectory, "sde_version.txt");
            _sdeDatabaseFile = Path.Combine(_sdeDirectory, "sde.sqlite"); // Standard name for the SDE database file
            Directory.CreateDirectory(_sdeDirectory);
            _logger.LogInformation("SdeService initialized. SDE directory: {SdeDirectory}", _sdeDirectory);
        }

        public async Task<SdeVersionInfo?> GetLatestSdeVersionInfoAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching latest SDE version information from {SdeSourceUrl}", SdeSourceUrl);
            try
            {
                var response = await _httpClient.GetAsync(SdeSourceUrl, cancellationToken);
                response.EnsureSuccessStatusCode();
                var htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                var sqliteLinkNode = htmlDoc.DocumentNode.SelectSingleNode($"//a[contains(@href, '{SqliteLatestFilePattern}')]");
                var md5LinkNode = htmlDoc.DocumentNode.SelectSingleNode($"//a[contains(@href, '{Md5FilePattern}')]");

                if (sqliteLinkNode == null)
                {
                    _logger.LogWarning("Could not find the link for '{SqliteLatestFilePattern}' on {SdeSourceUrl}", SqliteLatestFilePattern, SdeSourceUrl);
                    return null;
                }

                var downloadUrl = new Uri(new Uri(SdeSourceUrl), sqliteLinkNode.GetAttributeValue("href", string.Empty));
                string version = "unknown";
                string? md5Checksum = null;
                DateTime releaseDate = DateTime.MinValue;

                if (md5LinkNode != null)
                {
                    var md5FileUrl = new Uri(new Uri(SdeSourceUrl), md5LinkNode.GetAttributeValue("href", string.Empty));
                    try
                    {
                        var md5Response = await _httpClient.GetAsync(md5FileUrl, cancellationToken);
                        md5Response.EnsureSuccessStatusCode();
                        var md5Content = await md5Response.Content.ReadAsStringAsync(cancellationToken);
                        md5Checksum = md5Content.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        
                        var md5FileName = Path.GetFileName(md5LinkNode.GetAttributeValue("href", string.Empty)); 
                        var match = Regex.Match(md5FileName, @"(\d{8})|(\d{4}-\d{2}-\d{2})");
                        if (match.Success)
                        {
                            version = match.Groups[1].Success ? match.Groups[1].Value : DateTime.Parse(match.Groups[2].Value).ToString("yyyyMMdd");
                            if (DateTime.TryParseExact(version, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                            {
                                releaseDate = parsedDate;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to fetch or parse MD5 file from {Md5FileUrl}", md5FileUrl);
                    }
                }
                else
                {
                     _logger.LogWarning("Could not find the MD5 link for '{Md5FilePattern}' on {SdeSourceUrl}", Md5FilePattern, SdeSourceUrl);
                }
                
                if (version == "unknown")
                {
                    // Try to find a date in the page text near the sqlite link as a fallback.
                    var parentText = sqliteLinkNode.ParentNode?.InnerText?.Trim() ?? string.Empty;
                    var dateMatch = Regex.Match(parentText, @"(\d{4}-\d{2}-\d{2})\s+(\d{2}:\d{2})");
                    if (dateMatch.Success)
                    {
                         if (DateTime.TryParse(dateMatch.Value, out var parsedDate))
                         {
                            releaseDate = parsedDate;
                            version = releaseDate.ToString("yyyyMMddHHmm"); // More precise version if available
                         }
                    }
                    else // Ultimate fallback if no date found
                    {
                        releaseDate = DateTime.UtcNow; // Less ideal, but better than MinValue
                        version = releaseDate.ToString("yyyyMMdd") + "-latest"; 
                    }
                }
                // Ensure releaseDate has a value if version implies one
                if (releaseDate == DateTime.MinValue && Regex.IsMatch(version, @"^\d{8}"))
                {
                    if(DateTime.TryParseExact(version.Substring(0,8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDateFromVersion))
                    {
                        releaseDate = parsedDateFromVersion;
                    }
                }


                _logger.LogInformation("Latest SDE version found: {Version}, URL: {DownloadUrl}, Checksum: {Checksum}, Released: {ReleaseDate}", 
                                     version, downloadUrl, md5Checksum ?? "N/A", releaseDate.ToShortDateString());
                return new SdeVersionInfo
                {
                    Version = version,
                    DownloadUrl = downloadUrl,
                    Checksum = md5Checksum,
                    ChecksumType = md5Checksum != null ? "MD5" : null,
                    ReleaseDate = releaseDate == DateTime.MinValue ? DateTime.UtcNow : releaseDate // Ensure a valid date
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while fetching SDE version info from {SdeSourceUrl}", SdeSourceUrl);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching or parsing SDE version info from {SdeSourceUrl}", SdeSourceUrl);
                return null;
            }
        }

        public async Task<string?> GetCurrentLocalSdeVersionAsync(CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_sdeVersionFile))
            {
                _logger.LogInformation("Local SDE version file not found: {SdeVersionFile}", _sdeVersionFile);
                return null;
            }
            try
            {
                var version = await File.ReadAllTextAsync(_sdeVersionFile, cancellationToken);
                _logger.LogInformation("Current local SDE version: {Version}", version.Trim());
                return version.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading local SDE version file: {SdeVersionFile}", _sdeVersionFile);
                return null;
            }
        }

        public async Task<bool> DownloadAndApplySdeAsync(SdeVersionInfo sdeVersionToApply, CancellationToken cancellationToken = default)
        {
            if (sdeVersionToApply?.DownloadUrl == null)
            {
                _logger.LogError("SDE download URL is null. Cannot download.");
                return false;
            }

            var downloadFileName = Path.GetFileName(sdeVersionToApply.DownloadUrl.LocalPath);
            var downloadFilePath = Path.Combine(_sdeDirectory, downloadFileName);
            var extractionPath = Path.Combine(_sdeDirectory, "current_sde_temp"); 

            long totalBytesRead = 0;
            long totalBytes = 0;

            try
            {
                ReportProgress("Starting SDE download...", 0, 0, 0, false, false);
                using (var response = await _httpClient.GetAsync(sdeVersionToApply.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1L;

                    using (var streamToReadFrom = await response.Content.ReadAsStreamAsync(cancellationToken))
                    using (var streamToWriteTo = new FileStream(downloadFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await streamToWriteTo.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            totalBytesRead += bytesRead;
                            if (canReportProgress)
                            {
                                ReportProgress("Downloading SDE...", (double)totalBytesRead / totalBytes * 100, totalBytesRead, totalBytes, false, false);
                            }
                            else
                            {
                                ReportProgress($"Downloading SDE ({totalBytesRead / 1024 / 1024} MB)...", 0, totalBytesRead, totalBytes, false, false);
                            }
                        }
                    }
                }
                ReportProgress("SDE Download complete.", 100, totalBytesRead, totalBytes, false, false);
                _logger.LogInformation("SDE downloaded successfully to {DownloadFilePath}", downloadFilePath);

                if (!string.IsNullOrEmpty(sdeVersionToApply.Checksum) && sdeVersionToApply.ChecksumType == "MD5")
                {
                    _logger.LogInformation("Verifying MD5 checksum for {DownloadFilePath}", downloadFilePath);
                    ReportProgress("Verifying checksum...", 0,0,0, true, false);
                    using (var md5 = MD5.Create())
                    using (var stream = File.OpenRead(downloadFilePath))
                    {
                        var hash = await md5.ComputeHashAsync(stream, cancellationToken);
                        var downloadedFileChecksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        if (downloadedFileChecksum != sdeVersionToApply.Checksum.ToLowerInvariant())
                        {
                            _logger.LogError("Checksum mismatch for {DownloadFilePath}. Expected: {ExpectedChecksum}, Actual: {ActualChecksum}", 
                                downloadFilePath, sdeVersionToApply.Checksum, downloadedFileChecksum);
                            File.Delete(downloadFilePath);
                            ReportProgress("Checksum verification failed!", 100,0,0, true, false);
                            return false;
                        }
                    }
                    ReportProgress("Checksum verified.", 100,0,0, true, false);
                    _logger.LogInformation("Checksum verified successfully for {DownloadFilePath}", downloadFilePath);
                }
                else if (!string.IsNullOrEmpty(sdeVersionToApply.Checksum))
                {
                    _logger.LogWarning("Unsupported checksum type '{ChecksumType}' provided for SDE. Skipping verification.", sdeVersionToApply.ChecksumType);
                }

                _logger.LogInformation("Applying SDE: Extracting {DownloadFilePath} to {ExtractionPath}", downloadFilePath, extractionPath);
                ReportProgress("Applying SDE (extracting)...", 0,0,0, false, true);
                
                if (Directory.Exists(extractionPath)) Directory.Delete(extractionPath, true);
                Directory.CreateDirectory(extractionPath);

                ZipFile.ExtractToDirectory(downloadFilePath, extractionPath, true);
                _logger.LogInformation("SDE extracted to {ExtractionPath}", extractionPath);
                ReportProgress("SDE extraction complete.", 50,0,0, false, true);

                var sqliteFileInZip = Directory.GetFiles(extractionPath, "*.sqlite", SearchOption.AllDirectories).FirstOrDefault() ?? 
                                      Directory.GetFiles(extractionPath, "*.db", SearchOption.AllDirectories).FirstOrDefault();

                if (sqliteFileInZip == null)
                {
                    _logger.LogError("No .sqlite or .db file found in extracted SDE at {ExtractionPath}", extractionPath);
                    ReportProgress("SDE application failed: SQLite DB not found.", 100,0,0, false, true);
                    return false;
                }
                
                // Atomically replace the old SDE database file if it exists
                if (File.Exists(_sdeDatabaseFile)) File.Delete(_sdeDatabaseFile);
                File.Move(sqliteFileInZip, _sdeDatabaseFile);
                _logger.LogInformation("SDE SQLite database moved to {SdeDatabaseFile}", _sdeDatabaseFile);

                if (Directory.Exists(extractionPath)) Directory.Delete(extractionPath, true); // Clean up temp extraction folder

                await File.WriteAllTextAsync(_sdeVersionFile, sdeVersionToApply.Version, cancellationToken);
                _logger.LogInformation("Updated local SDE version to {Version}", sdeVersionToApply.Version);
                ReportProgress("SDE applied successfully.", 100,0,0, false, true);
                
                if(File.Exists(downloadFilePath)) File.Delete(downloadFilePath);
                _logger.LogInformation("Cleaned up downloaded SDE archive: {DownloadFilePath}", downloadFilePath);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("SDE download and apply was cancelled.");
                ReportProgress("SDE update cancelled.", 0,0,0, false, false);
                if(File.Exists(downloadFilePath)) File.Delete(downloadFilePath);
                if(Directory.Exists(extractionPath)) Directory.Delete(extractionPath, true);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading or applying SDE version {Version}", sdeVersionToApply.Version);
                ReportProgress($"SDE update failed: {ex.Message}", 100,0,0, false, false);
                if(File.Exists(downloadFilePath)) File.Delete(downloadFilePath);
                if(Directory.Exists(extractionPath)) Directory.Delete(extractionPath, true);
                return false;
            }
        }

        public async Task CheckForSdeUpdatesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Checking for SDE updates...");
            var latestVersionInfo = await GetLatestSdeVersionInfoAsync(cancellationToken);
            var localVersion = await GetCurrentLocalSdeVersionAsync(cancellationToken);

            if (latestVersionInfo != null)
            {
                SdeUpdateAvailable?.Invoke(this, new SdeUpdateAvailableEventArgs(latestVersionInfo, localVersion));
                if (string.IsNullOrEmpty(localVersion) || localVersion != latestVersionInfo.Version)
                {
                    _logger.LogInformation("SDE update available. Latest: {LatestVersion} (Released: {ReleaseDate}), Local: {LocalVersion}", 
                                         latestVersionInfo.Version, latestVersionInfo.ReleaseDate.ToShortDateString(), localVersion ?? "None");
                }
                else
                {
                    _logger.LogInformation("Local SDE is up to date. Version: {LocalVersion}", localVersion);
                }
            }
            else
            {
                _logger.LogWarning("Could not determine the latest SDE version.");
            }
        }

        private void ReportProgress(string message, double percentage, long bytesDownloaded, long totalBytes, bool isVerifying, bool isApplying)
        {
            SdeUpdateProgress?.Invoke(this, new SdeUpdateProgressEventArgs
            {
                StatusMessage = message,
                ProgressPercentage = Math.Clamp(percentage, 0, 100),
                BytesDownloaded = bytesDownloaded,
                TotalBytesToDownload = totalBytes,
                IsVerifying = isVerifying,
                IsApplying = isApplying
            });
        }
    }
} 
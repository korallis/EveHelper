using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EveHelper.App.Commands;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Character;
using EveHelper.Core.ViewModels;
using Microsoft.Extensions.Logging;

namespace EveHelper.App.ViewModels
{
    /// <summary>
    /// ViewModel for the character selection interface
    /// </summary>
    public class CharacterSelectionViewModel : BaseViewModel
    {
        private readonly ICharacterService _characterService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<CharacterSelectionViewModel> _logger;
        
        private ObservableCollection<CharacterInfo> _characters;
        private CharacterInfo? _selectedCharacter;
        private bool _isLoading;
        private string _statusMessage;
        private bool _hasCharacters;

        /// <summary>
        /// Collection of available characters
        /// </summary>
        public ObservableCollection<CharacterInfo> Characters
        {
            get => _characters;
            set => SetProperty(ref _characters, value);
        }

        /// <summary>
        /// Currently selected character
        /// </summary>
        public CharacterInfo? SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (SetProperty(ref _selectedCharacter, value))
                {
                    OnPropertiesChanged(nameof(CanSetAsDefault), nameof(CanRemoveCharacter));
                }
            }
        }

        /// <summary>
        /// Whether the interface is currently loading
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Status message to display to the user
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Whether there are any characters available
        /// </summary>
        public bool HasCharacters
        {
            get => _hasCharacters;
            set => SetProperty(ref _hasCharacters, value);
        }

        /// <summary>
        /// Whether the selected character can be set as default
        /// </summary>
        public bool CanSetAsDefault => SelectedCharacter != null && !SelectedCharacter.IsDefault;

        /// <summary>
        /// Whether the selected character can be removed
        /// </summary>
        public bool CanRemoveCharacter => SelectedCharacter != null;

        /// <summary>
        /// Command to add a new character
        /// </summary>
        public ICommand AddCharacterCommand { get; }

        /// <summary>
        /// Command to remove the selected character
        /// </summary>
        public ICommand RemoveCharacterCommand { get; }

        /// <summary>
        /// Command to set the selected character as default
        /// </summary>
        public ICommand SetAsDefaultCommand { get; }

        /// <summary>
        /// Command to refresh character information
        /// </summary>
        public ICommand RefreshCharacterCommand { get; }

        /// <summary>
        /// Command to refresh all characters
        /// </summary>
        public ICommand RefreshAllCharactersCommand { get; }

        /// <summary>
        /// Command to select a character (double-click)
        /// </summary>
        public ICommand SelectCharacterCommand { get; }

        /// <summary>
        /// Initializes a new instance of CharacterSelectionViewModel
        /// </summary>
        public CharacterSelectionViewModel(
            ICharacterService characterService,
            INavigationService navigationService,
            ILogger<CharacterSelectionViewModel> logger)
        {
            _characterService = characterService;
            _navigationService = navigationService;
            _logger = logger;
            
            _characters = new ObservableCollection<CharacterInfo>();
            _statusMessage = "Loading characters...";

            // Initialize commands
            AddCharacterCommand = new RelayCommand(async () => await ExecuteAddCharacterAsync());
            RemoveCharacterCommand = new RelayCommand(async () => await ExecuteRemoveCharacterAsync(), () => CanRemoveCharacter);
            SetAsDefaultCommand = new RelayCommand(async () => await ExecuteSetAsDefaultAsync(), () => CanSetAsDefault);
            RefreshCharacterCommand = new RelayCommand(async () => await ExecuteRefreshCharacterAsync(), () => SelectedCharacter != null);
            RefreshAllCharactersCommand = new RelayCommand(async () => await ExecuteRefreshAllCharactersAsync());
            SelectCharacterCommand = new RelayCommand<CharacterInfo>(async (character) => await ExecuteSelectCharacterAsync(character));

            // Subscribe to character service events
            _characterService.CharacterListChanged += OnCharacterListChanged;
            _characterService.DefaultCharacterChanged += OnDefaultCharacterChanged;
        }

        /// <summary>
        /// Initialize method called when navigating to this ViewModel
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadCharactersAsync();
        }

        /// <summary>
        /// Loads characters from the character service
        /// </summary>
        private async Task LoadCharactersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading characters...";

                var characters = await _characterService.GetCharactersAsync();
                
                Characters.Clear();
                foreach (var character in characters)
                {
                    Characters.Add(character);
                }

                HasCharacters = Characters.Any();
                
                if (HasCharacters)
                {
                    StatusMessage = $"Loaded {Characters.Count} character(s)";
                    SelectedCharacter = Characters.FirstOrDefault(c => c.IsDefault) ?? Characters.First();
                }
                else
                {
                    StatusMessage = "No characters found. Add a character to get started.";
                }

                _logger.LogInformation("Loaded {Count} characters", Characters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load characters");
                StatusMessage = "Failed to load characters. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Executes the add character command
        /// </summary>
        private async Task ExecuteAddCharacterAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Adding character... Please complete authentication in your browser.";

                var newCharacter = await _characterService.AddCharacterAsync();
                if (newCharacter != null)
                {
                    StatusMessage = $"Character {newCharacter.Name} added successfully!";
                    _logger.LogInformation("Character {CharacterName} added successfully", newCharacter.Name);
                }
                else
                {
                    StatusMessage = "Failed to add character. Please try again.";
                    _logger.LogWarning("Failed to add character");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding character");
                StatusMessage = "Error adding character. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Executes the remove character command
        /// </summary>
        private async Task ExecuteRemoveCharacterAsync()
        {
            if (SelectedCharacter == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Removing character {SelectedCharacter.Name}...";

                var success = await _characterService.RemoveCharacterAsync(SelectedCharacter.CharacterId);
                if (success)
                {
                    StatusMessage = $"Character {SelectedCharacter.Name} removed successfully.";
                    _logger.LogInformation("Character {CharacterName} removed successfully", SelectedCharacter.Name);
                }
                else
                {
                    StatusMessage = "Failed to remove character. Please try again.";
                    _logger.LogWarning("Failed to remove character {CharacterId}", SelectedCharacter.CharacterId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing character");
                StatusMessage = "Error removing character. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Executes the set as default command
        /// </summary>
        private async Task ExecuteSetAsDefaultAsync()
        {
            if (SelectedCharacter == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Setting {SelectedCharacter.Name} as default character...";

                var success = await _characterService.SetDefaultCharacterAsync(SelectedCharacter.CharacterId);
                if (success)
                {
                    StatusMessage = $"{SelectedCharacter.Name} is now the default character.";
                    _logger.LogInformation("Character {CharacterName} set as default", SelectedCharacter.Name);
                }
                else
                {
                    StatusMessage = "Failed to set default character. Please try again.";
                    _logger.LogWarning("Failed to set default character {CharacterId}", SelectedCharacter.CharacterId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default character");
                StatusMessage = "Error setting default character. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Executes the refresh character command
        /// </summary>
        private async Task ExecuteRefreshCharacterAsync()
        {
            if (SelectedCharacter == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Refreshing {SelectedCharacter.Name}...";

                var refreshedCharacter = await _characterService.RefreshCharacterAsync(SelectedCharacter.CharacterId);
                if (refreshedCharacter != null)
                {
                    StatusMessage = $"{SelectedCharacter.Name} refreshed successfully.";
                    _logger.LogInformation("Character {CharacterName} refreshed successfully", SelectedCharacter.Name);
                }
                else
                {
                    StatusMessage = "Failed to refresh character. Please try again.";
                    _logger.LogWarning("Failed to refresh character {CharacterId}", SelectedCharacter.CharacterId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing character");
                StatusMessage = "Error refreshing character. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Executes the refresh all characters command
        /// </summary>
        private async Task ExecuteRefreshAllCharactersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Refreshing all characters...";

                var refreshedCharacters = await _characterService.RefreshAllCharactersAsync();
                StatusMessage = $"Refreshed {refreshedCharacters.Count()} character(s) successfully.";
                _logger.LogInformation("Refreshed {Count} characters", refreshedCharacters.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing all characters");
                StatusMessage = "Error refreshing characters. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Executes the select character command (double-click)
        /// </summary>
        private async Task ExecuteSelectCharacterAsync(CharacterInfo? character)
        {
            if (character == null) return;

            try
            {
                // Set as default and navigate back
                await _characterService.SetDefaultCharacterAsync(character.CharacterId);
                
                // Navigate back to home or previous view
                // _navigationService.NavigateTo<HomeViewModel>();
                StatusMessage = $"{character.Name} selected as active character.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting character");
                StatusMessage = "Error selecting character. Please try again.";
            }
        }

        /// <summary>
        /// Handles character list changed events
        /// </summary>
        private void OnCharacterListChanged(object? sender, CharacterListChangedEventArgs e)
        {
            // Update the UI on the main thread
            App.Current.Dispatcher.Invoke(() =>
            {
                Characters.Clear();
                foreach (var character in e.Characters)
                {
                    Characters.Add(character);
                }

                HasCharacters = Characters.Any();

                // Update selected character if needed
                if (e.ChangeType == CharacterListChangeType.Removed && SelectedCharacter?.CharacterId == e.Character?.CharacterId)
                {
                    SelectedCharacter = Characters.FirstOrDefault();
                }
                else if (e.ChangeType == CharacterListChangeType.Added && e.Character != null)
                {
                    SelectedCharacter = e.Character;
                }

                OnPropertiesChanged(nameof(CanSetAsDefault), nameof(CanRemoveCharacter));
            });
        }

        /// <summary>
        /// Handles default character changed events
        /// </summary>
        private void OnDefaultCharacterChanged(object? sender, DefaultCharacterChangedEventArgs e)
        {
            // Update the UI on the main thread
            App.Current.Dispatcher.Invoke(() =>
            {
                // Refresh the characters to update the IsDefault property
                var currentSelection = SelectedCharacter;
                foreach (var character in Characters)
                {
                    character.IsDefault = character.CharacterId == e.NewDefault?.CharacterId;
                }

                // Trigger property change notifications
                OnPropertiesChanged(nameof(CanSetAsDefault));
            });
        }

        /// <summary>
        /// Cleanup when the ViewModel is disposed
        /// </summary>
        protected override void OnPropertyChanged(string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            
            // Update command states when relevant properties change
            if (propertyName == nameof(SelectedCharacter))
            {
                (RemoveCharacterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (SetAsDefaultCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RefreshCharacterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
} 
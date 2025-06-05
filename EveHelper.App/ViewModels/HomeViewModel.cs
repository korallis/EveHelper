using EveHelper.Core.ViewModels;
using EveHelper.App.Commands;
using EveHelper.Core.Interfaces;
using System.Windows.Input;

namespace EveHelper.App.ViewModels
{
    /// <summary>
    /// ViewModel for the Home view
    /// </summary>
    public class HomeViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private string _welcomeMessage;
        private string _statusMessage;

        /// <summary>
        /// Gets or sets the welcome message
        /// </summary>
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Command to browse ships
        /// </summary>
        public ICommand BrowseShipsCommand { get; }

        /// <summary>
        /// Command to open fitting tool
        /// </summary>
        public ICommand OpenFittingToolCommand { get; }

        /// <summary>
        /// Command to open skill planner
        /// </summary>
        public ICommand OpenSkillPlannerCommand { get; }

        /// <summary>
        /// Initializes a new instance of HomeViewModel
        /// </summary>
        /// <param name="navigationService">Navigation service</param>
        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _welcomeMessage = "Welcome to EVE Helper";
            _statusMessage = "Ready to optimize your ship fittings";

            // Initialize commands
            BrowseShipsCommand = new RelayCommand(ExecuteBrowseShips);
            OpenFittingToolCommand = new RelayCommand(ExecuteOpenFittingTool);
            OpenSkillPlannerCommand = new RelayCommand(ExecuteOpenSkillPlanner);
        }

        /// <summary>
        /// Executes the browse ships command
        /// </summary>
        private void ExecuteBrowseShips()
        {
            StatusMessage = "Navigating to Ship Browser...";
            // TODO: Navigate to ship browser when implemented
            // _navigationService.NavigateTo<ShipBrowserViewModel>();
        }

        /// <summary>
        /// Executes the open fitting tool command
        /// </summary>
        private void ExecuteOpenFittingTool()
        {
            StatusMessage = "Opening Fitting Tool...";
            // TODO: Navigate to fitting tool when implemented
            // _navigationService.NavigateTo<FittingToolViewModel>();
        }

        /// <summary>
        /// Executes the open skill planner command
        /// </summary>
        private void ExecuteOpenSkillPlanner()
        {
            StatusMessage = "Opening Skill Planner...";
            // TODO: Navigate to skill planner when implemented
            // _navigationService.NavigateTo<SkillPlannerViewModel>();
        }

        /// <summary>
        /// Initialize method called when navigating to this ViewModel
        /// </summary>
        /// <param name="parameter">Navigation parameter</param>
        public void Initialize(object? parameter = null)
        {
            // Handle any initialization logic here
            if (parameter is string message)
            {
                StatusMessage = message;
            }
        }
    }
} 
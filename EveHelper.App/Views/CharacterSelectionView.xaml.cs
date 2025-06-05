using System.Windows.Controls;
using EveHelper.App.ViewModels;

namespace EveHelper.App.Views
{
    /// <summary>
    /// Interaction logic for CharacterSelectionView.xaml
    /// </summary>
    public partial class CharacterSelectionView : UserControl
    {
        public CharacterSelectionView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when the view is loaded
        /// </summary>
        private async void CharacterSelectionView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is CharacterSelectionViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }
    }
} 
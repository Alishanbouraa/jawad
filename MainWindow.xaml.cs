using System.Windows;
using JawadContractingApp.ViewModels;
using JawadContractingApp.Services;

namespace JawadContractingApp
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly INavigationService _navigationService;

        public MainWindow(MainViewModel viewModel, INavigationService navigationService)
        {
            InitializeComponent();

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            DataContext = _viewModel;
            _navigationService.SetMainFrame(MainFrame);
        }
    }
}
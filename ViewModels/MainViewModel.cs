using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JawadContractingApp.Services;

namespace JawadContractingApp.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _currentViewTitle = "إدارة العملاء";

        [ObservableProperty]
        private bool _canGoBack;

        private readonly Dictionary<string, string> _viewTitles;

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            _viewTitles = new Dictionary<string, string>
            {
                { "CustomerView", "إدارة العملاء" },
                { "AccountingView", "كشف حساب" }
            };

            NavigateToCustomersCommand.Execute(null);
        }

        [RelayCommand]
        private void NavigateToCustomers()
        {
            Execute(() =>
            {
                _navigationService.NavigateTo("CustomerView");
                CurrentViewTitle = _viewTitles["CustomerView"];
                UpdateNavigationState();
            }, "التنقل إلى العملاء");
        }

        [RelayCommand]
        private void NavigateToAccounting()
        {
            Execute(() =>
            {
                _navigationService.NavigateTo("AccountingView");
                CurrentViewTitle = _viewTitles["AccountingView"];
                UpdateNavigationState();
            }, "التنقل إلى كشف الحساب");
        }

        [RelayCommand]
        private void GoBack()
        {
            if (!_navigationService.CanGoBack) return;

            Execute(() =>
            {
                _navigationService.GoBack();
                UpdateNavigationState();
            }, "العودة");
        }

        [RelayCommand]
        private void RefreshCurrentView()
        {
            Execute(() =>
            {
                StatusMessage = "تم تحديث الصفحة";
            }, "تحديث الصفحة");
        }

        private void UpdateNavigationState()
        {
            CanGoBack = _navigationService.CanGoBack;
        }
    }
}
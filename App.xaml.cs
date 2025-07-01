using System.Globalization;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using JawadContractingApp.Data;
using JawadContractingApp.Services;
using JawadContractingApp.ViewModels;

namespace JawadContractingApp
{
    public partial class App : Application
    {
        private IConfiguration _configuration = null!;
        private ApplicationDbContext _dbContext = null!;
        private readonly Dictionary<Type, object> _serviceRegistry;

        public App()
        {
            _serviceRegistry = new Dictionary<Type, object>();
            InitializeApplication();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigureLocalization();
            InitializeDatabase();
            RegisterServices();

            var mainWindow = CreateMainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _dbContext?.Dispose();
            base.OnExit(e);
        }

        private void InitializeApplication()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = configurationBuilder.Build();
        }

        private void ConfigureLocalization()
        {
            var culture = new CultureInfo("ar-SA");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        private void InitializeDatabase()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Database connection string not configured");

            _dbContext = new ApplicationDbContext(connectionString);

            try
            {
                _dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إعداد قاعدة البيانات: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void RegisterServices()
        {
            _serviceRegistry[typeof(ApplicationDbContext)] = _dbContext;
            _serviceRegistry[typeof(INavigationService)] = new NavigationService();
            _serviceRegistry[typeof(ICustomerService)] = new CustomerService(_dbContext);
            _serviceRegistry[typeof(IAccountingService)] = new AccountingService(_dbContext);

            var navigationService = (INavigationService)_serviceRegistry[typeof(INavigationService)];
            var customerService = (ICustomerService)_serviceRegistry[typeof(ICustomerService)];
            var accountingService = (IAccountingService)_serviceRegistry[typeof(IAccountingService)];

            _serviceRegistry[typeof(CustomerViewModel)] = new CustomerViewModel(customerService);
            _serviceRegistry[typeof(AccountingViewModel)] = new AccountingViewModel(accountingService);
            _serviceRegistry[typeof(MainViewModel)] = new MainViewModel(navigationService);
        }

        private MainWindow CreateMainWindow()
        {
            var mainViewModel = (MainViewModel)_serviceRegistry[typeof(MainViewModel)];
            var navigationService = (INavigationService)_serviceRegistry[typeof(INavigationService)];

            var mainWindow = new MainWindow(mainViewModel, navigationService);

            var customerViewModel = (CustomerViewModel)_serviceRegistry[typeof(CustomerViewModel)];
            var accountingViewModel = (AccountingViewModel)_serviceRegistry[typeof(AccountingViewModel)];

            var customerView = new Views.CustomerView { DataContext = customerViewModel };
            var accountingView = new Views.AccountingView { DataContext = accountingViewModel };

            return mainWindow;
        }

        public T GetService<T>() where T : class
        {
            if (_serviceRegistry.ContainsKey(typeof(T)))
            {
                return (T)_serviceRegistry[typeof(T)];
            }
            throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
        }
    }
}
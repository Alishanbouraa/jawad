using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using JawadContractingApp.Models;
using JawadContractingApp.Services;

namespace JawadContractingApp.ViewModels
{
    public partial class CustomerViewModel : BaseViewModel
    {
        private readonly ICustomerService _customerService;

        [ObservableProperty]
        private Customer _selectedCustomer = new();

        [ObservableProperty]
        private ObservableCollection<Customer> _customers = new();

        [ObservableProperty]
        private ObservableCollection<Transaction> _customerTransactions = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _customerName = string.Empty;

        [ObservableProperty]
        private string _customerPhone = string.Empty;

        [ObservableProperty]
        private string _customerAddress = string.Empty;

        [ObservableProperty]
        private decimal _customerBalance;

        [ObservableProperty]
        private decimal _paymentAmount;

        [ObservableProperty]
        private string _paymentDescription = string.Empty;

        [ObservableProperty]
        private bool _isEditMode;

        public CustomerViewModel(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            LoadCustomersCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadCustomers()
        {
            await ExecuteAsync(async () =>
            {
                var customers = await _customerService.GetAllCustomersAsync();
                Customers.Clear();

                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            }, "تحميل العملاء");
        }

        [RelayCommand]
        private async Task SearchCustomers()
        {
            await ExecuteAsync(async () =>
            {
                var customers = await _customerService.SearchCustomersAsync(SearchText);
                Customers.Clear();

                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            }, "البحث");
        }

        [RelayCommand]
        private async Task SaveCustomer()
        {
            await ExecuteAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(CustomerName))
                {
                    SetError(nameof(CustomerName), "اسم العميل مطلوب");
                    return;
                }

                var customer = new Customer
                {
                    Id = IsEditMode ? SelectedCustomer.Id : 0,
                    Name = CustomerName.Trim(),
                    PhoneNumber = CustomerPhone.Trim(),
                    Address = CustomerAddress.Trim(),
                    Balance = CustomerBalance
                };

                if (IsEditMode)
                {
                    await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    await _customerService.CreateCustomerAsync(customer);
                }

                ClearForm();
                await LoadCustomersCommand.ExecuteAsync(null);
            }, IsEditMode ? "تحديث العميل" : "إضافة العميل");
        }

        [RelayCommand]
        private async Task DeleteCustomer(Customer customer)
        {
            if (customer == null) return;

            await ExecuteAsync(async () =>
            {
                var result = await _customerService.DeleteCustomerAsync(customer.Id);
                if (result)
                {
                    await LoadCustomersCommand.ExecuteAsync(null);
                    ClearForm();
                }
            }, "حذف العميل");
        }

        [RelayCommand]
        private async Task SelectCustomer(Customer customer)
        {
            if (customer == null) return;

            await ExecuteAsync(async () =>
            {
                SelectedCustomer = customer;
                CustomerName = customer.Name;
                CustomerPhone = customer.PhoneNumber;
                CustomerAddress = customer.Address;
                CustomerBalance = customer.Balance;
                IsEditMode = true;

                await LoadCustomerTransactions();
            }, "تحديد العميل");
        }

        [RelayCommand]
        private async Task AddPayment()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id == 0)
            {
                SetError(nameof(SelectedCustomer), "يجب اختيار عميل");
                return;
            }

            await ExecuteAsync(async () =>
            {
                await _customerService.AddPaymentAsync(
                    SelectedCustomer.Id,
                    PaymentAmount,
                    PaymentDescription);

                PaymentAmount = 0;
                PaymentDescription = string.Empty;

                await LoadCustomerTransactions();
                await LoadCustomersCommand.ExecuteAsync(null);

                var updatedCustomer = await _customerService.GetCustomerByIdAsync(SelectedCustomer.Id);
                if (updatedCustomer != null)
                {
                    CustomerBalance = updatedCustomer.Balance;
                    SelectedCustomer = updatedCustomer;
                }
            }, "إضافة دفعة");
        }

        [RelayCommand]
        private void NewCustomer()
        {
            ClearForm();
        }

        [RelayCommand]
        private void ClearForm()
        {
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerAddress = string.Empty;
            CustomerBalance = 0;
            PaymentAmount = 0;
            PaymentDescription = string.Empty;
            IsEditMode = false;
            SelectedCustomer = new Customer();
            CustomerTransactions.Clear();
            ClearAllErrors();
        }

        private async Task LoadCustomerTransactions()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id == 0) return;

            var transactions = await _customerService.GetCustomerTransactionsAsync(SelectedCustomer.Id);
            CustomerTransactions.Clear();

            foreach (var transaction in transactions)
            {
                CustomerTransactions.Add(transaction);
            }
        }
    }
}
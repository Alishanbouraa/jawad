using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using JawadContractingApp.Models;
using JawadContractingApp.Services;

namespace JawadContractingApp.ViewModels
{
    public partial class AccountingViewModel : BaseViewModel
    {
        private readonly IAccountingService _accountingService;

        [ObservableProperty]
        private AccountingEntry _selectedEntry = new();

        [ObservableProperty]
        private ObservableCollection<AccountingEntry> _entries = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTime _entryDate = DateTime.Now;

        [ObservableProperty]
        private string _serialNumber = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private decimal _amount;

        [ObservableProperty]
        private decimal _paid;

        [ObservableProperty]
        private decimal _balance;

        [ObservableProperty]
        private string _statement = string.Empty;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private DateTime _filterStartDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _filterEndDate = DateTime.Now;

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private decimal _totalPaid;

        [ObservableProperty]
        private decimal _totalBalance;

        public AccountingViewModel(IAccountingService accountingService)
        {
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            LoadEntriesCommand.Execute(null);
            LoadSummaryCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadEntries()
        {
            await ExecuteAsync(async () =>
            {
                var entries = await _accountingService.GetAllEntriesAsync();
                Entries.Clear();

                foreach (var entry in entries)
                {
                    Entries.Add(entry);
                }
            }, "تحميل القيود");
        }

        [RelayCommand]
        private async Task SearchEntries()
        {
            await ExecuteAsync(async () =>
            {
                var entries = await _accountingService.SearchEntriesAsync(SearchText);
                Entries.Clear();

                foreach (var entry in entries)
                {
                    Entries.Add(entry);
                }
            }, "البحث");
        }

        [RelayCommand]
        private async Task FilterByDateRange()
        {
            await ExecuteAsync(async () =>
            {
                var entries = await _accountingService.GetEntriesByDateRangeAsync(FilterStartDate, FilterEndDate);
                Entries.Clear();

                foreach (var entry in entries)
                {
                    Entries.Add(entry);
                }
            }, "تصفية حسب التاريخ");
        }

        [RelayCommand]
        private async Task SaveEntry()
        {
            await ExecuteAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    SetError(nameof(Description), "الوصف مطلوب");
                    return;
                }

                var entry = new AccountingEntry
                {
                    Id = IsEditMode ? SelectedEntry.Id : 0,
                    Date = EntryDate,
                    SerialNumber = SerialNumber.Trim(),
                    Description = Description.Trim(),
                    Amount = Amount,
                    Paid = Paid,
                    Statement = Statement.Trim()
                };

                if (IsEditMode)
                {
                    await _accountingService.UpdateEntryAsync(entry);
                }
                else
                {
                    await _accountingService.CreateEntryAsync(entry);
                }

                ClearForm();
                await LoadEntriesCommand.ExecuteAsync(null);
                await LoadSummaryCommand.ExecuteAsync(null);
            }, IsEditMode ? "تحديث القيد" : "إضافة القيد");
        }

        [RelayCommand]
        private async Task DeleteEntry(AccountingEntry entry)
        {
            if (entry == null) return;

            await ExecuteAsync(async () =>
            {
                var result = await _accountingService.DeleteEntryAsync(entry.Id);
                if (result)
                {
                    await LoadEntriesCommand.ExecuteAsync(null);
                    await LoadSummaryCommand.ExecuteAsync(null);
                    ClearForm();
                }
            }, "حذف القيد");
        }

        [RelayCommand]
        private void SelectEntry(AccountingEntry entry)
        {
            if (entry == null) return;

            Execute(() =>
            {
                SelectedEntry = entry;
                EntryDate = entry.Date;
                SerialNumber = entry.SerialNumber;
                Description = entry.Description;
                Amount = entry.Amount;
                Paid = entry.Paid;
                Statement = entry.Statement;
                IsEditMode = true;
                CalculateBalance();
            }, "تحديد القيد");
        }

        [RelayCommand]
        private void NewEntry()
        {
            ClearForm();
        }

        [RelayCommand]
        private void ClearForm()
        {
            EntryDate = DateTime.Now;
            SerialNumber = string.Empty;
            Description = string.Empty;
            Amount = 0;
            Paid = 0;
            Balance = 0;
            Statement = string.Empty;
            IsEditMode = false;
            SelectedEntry = new AccountingEntry();
            ClearAllErrors();
        }

        [RelayCommand]
        private void CalculateBalance()
        {
            Balance = Amount - Paid;
        }

        [RelayCommand]
        private async Task LoadSummary()
        {
            await ExecuteAsync(async () =>
            {
                TotalAmount = await _accountingService.GetTotalAmountAsync();
                TotalPaid = await _accountingService.GetTotalPaidAsync();
                TotalBalance = await _accountingService.GetTotalBalanceAsync();
            }, "تحميل الملخص");
        }

        partial void OnAmountChanged(decimal value)
        {
            CalculateBalance();
        }

        partial void OnPaidChanged(decimal value)
        {
            CalculateBalance();
        }
    }
}
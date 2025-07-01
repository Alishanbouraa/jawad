using Microsoft.EntityFrameworkCore;
using JawadContractingApp.Data;
using JawadContractingApp.Models;

namespace JawadContractingApp.Services
{
    public interface IAccountingService
    {
        Task<List<AccountingEntry>> GetAllEntriesAsync();
        Task<AccountingEntry?> GetEntryByIdAsync(int id);
        Task<AccountingEntry> CreateEntryAsync(AccountingEntry entry);
        Task<AccountingEntry> UpdateEntryAsync(AccountingEntry entry);
        Task<bool> DeleteEntryAsync(int id);
        Task<List<AccountingEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AccountingEntry>> SearchEntriesAsync(string searchTerm);
        Task<decimal> GetTotalAmountAsync();
        Task<decimal> GetTotalPaidAsync();
        Task<decimal> GetTotalBalanceAsync();
    }

    public class AccountingService : IAccountingService
    {
        private readonly ApplicationDbContext _context;
        private readonly object _operationLock;

        public AccountingService(ApplicationDbContext context)
        {
            _context = context;
            _operationLock = new object();
        }

        public async Task<List<AccountingEntry>> GetAllEntriesAsync()
        {
            return await _context.AccountingEntries
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<AccountingEntry?> GetEntryByIdAsync(int id)
        {
            return await _context.AccountingEntries.FindAsync(id);
        }

        public async Task<AccountingEntry> CreateEntryAsync(AccountingEntry entry)
        {
            lock (_operationLock)
            {
                entry.Balance = entry.Amount - entry.Paid;
                entry.CreatedDate = DateTime.Now;

                _context.AccountingEntries.Add(entry);
                _context.SaveChanges();
                return entry;
            }
        }

        public async Task<AccountingEntry> UpdateEntryAsync(AccountingEntry entry)
        {
            lock (_operationLock)
            {
                entry.Balance = entry.Amount - entry.Paid;
                entry.LastModifiedDate = DateTime.Now;

                _context.AccountingEntries.Update(entry);
                _context.SaveChanges();
                return entry;
            }
        }

        public async Task<bool> DeleteEntryAsync(int id)
        {
            lock (_operationLock)
            {
                var entry = _context.AccountingEntries.Find(id);
                if (entry == null) return false;

                _context.AccountingEntries.Remove(entry);
                _context.SaveChanges();
                return true;
            }
        }

        public async Task<List<AccountingEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AccountingEntries
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<AccountingEntry>> SearchEntriesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllEntriesAsync();

            return await _context.AccountingEntries
                .Where(e => e.SerialNumber.Contains(searchTerm) ||
                           e.Description.Contains(searchTerm) ||
                           e.Statement.Contains(searchTerm))
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountAsync()
        {
            return await _context.AccountingEntries.SumAsync(e => e.Amount);
        }

        public async Task<decimal> GetTotalPaidAsync()
        {
            return await _context.AccountingEntries.SumAsync(e => e.Paid);
        }

        public async Task<decimal> GetTotalBalanceAsync()
        {
            return await _context.AccountingEntries.SumAsync(e => e.Balance);
        }
    }
}
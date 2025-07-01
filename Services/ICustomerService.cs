using Microsoft.EntityFrameworkCore;
using JawadContractingApp.Data;
using JawadContractingApp.Models;

namespace JawadContractingApp.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<List<Transaction>> GetCustomerTransactionsAsync(int customerId);
        Task<Transaction> AddPaymentAsync(int customerId, decimal amount, string description);
        Task<decimal> GetCustomerBalanceAsync(int customerId);
        Task<List<Customer>> SearchCustomersAsync(string searchTerm);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly object _transactionLock;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
            _transactionLock = new object();
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            lock (_transactionLock)
            {
                customer.CreatedDate = DateTime.Now;
                _context.Customers.Add(customer);
                _context.SaveChanges();
                return customer;
            }
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            lock (_transactionLock)
            {
                customer.LastModifiedDate = DateTime.Now;
                _context.Customers.Update(customer);
                _context.SaveChanges();
                return customer;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            lock (_transactionLock)
            {
                var customer = _context.Customers.Find(id);
                if (customer == null) return false;

                _context.Customers.Remove(customer);
                _context.SaveChanges();
                return true;
            }
        }

        public async Task<List<Transaction>> GetCustomerTransactionsAsync(int customerId)
        {
            return await _context.Transactions
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction> AddPaymentAsync(int customerId, decimal amount, string description)
        {
            lock (_transactionLock)
            {
                var customer = _context.Customers.Find(customerId);
                if (customer == null)
                    throw new ArgumentException("العميل غير موجود");

                var previousBalance = customer.Balance;
                var newBalance = previousBalance + amount;

                var transaction = new Transaction
                {
                    CustomerId = customerId,
                    Amount = amount,
                    Type = amount >= 0 ? "دفع" : "سحب",
                    Description = description,
                    TransactionDate = DateTime.Now,
                    BalanceAfter = newBalance
                };

                customer.Balance = newBalance;
                customer.LastModifiedDate = DateTime.Now;

                _context.Transactions.Add(transaction);
                _context.Customers.Update(customer);
                _context.SaveChanges();

                return transaction;
            }
        }

        public async Task<decimal> GetCustomerBalanceAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            return customer?.Balance ?? 0;
        }

        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCustomersAsync();

            return await _context.Customers
                .Where(c => c.Name.Contains(searchTerm) ||
                           c.PhoneNumber.Contains(searchTerm) ||
                           c.Address.Contains(searchTerm))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
using System.Linq;
using Lab_1.Models;

namespace Lab_1.Services
{
    public class BankService
    {
        private readonly JsonDataContext _context;

        public BankService(JsonDataContext context)
        {
            _context = context;
        }

        public void ToggleAccountBlock(string accountId, bool isBlocked)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (account != null)
            {
                account.IsBlocked = isBlocked;
                _context.SaveChanges();
            }
        }

        public void ApproveClient(string clientId)
        {
            var client = _context.Users.OfType<Client>().FirstOrDefault(c => c.Id == clientId);
            if (client != null)
            {
                client.IsApproved = true;
                _context.SaveChanges();
            }
        }

        public void SkipMonth()
        {
            var deposits = _context.Accounts.OfType<DepositAccount>().ToList();

            foreach (var deposit in deposits)
            {
                deposit.AccrueInterest();
            }

            _context.SaveChanges();
        }
    }
}
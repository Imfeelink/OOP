using System.Linq;
using Lab_1.Models;

namespace Lab_1.Services
{
    public class AuthService
    {
        private readonly JsonDataContext _context;
        public User CurrentUser { get; private set; } 

        public AuthService(JsonDataContext context)
        {
            _context = context;
        }

        public bool Login(string login, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == password);
            if (user != null)
            {
                if (user is Client client && !client.IsApproved)
                {
                    throw new System.Exception("Ваш аккаунт еще не подтвержден менеджером.");
                }

                CurrentUser = user;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
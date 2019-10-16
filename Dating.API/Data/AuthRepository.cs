using System.Threading.Tasks;
using Dating.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Dating.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Username.Equals(username));
            //throw new System.NotImplementedException();
            if(user == null)
                return null;

            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;

            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
            //throw new System.NotImplementedException();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(System.Security.Cryptography.HMACSHA512 hmac = (System.Security.Cryptography.HMACSHA512)System.Security.Cryptography.HashAlgorithm.Create("HMACSHA512"))
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(System.Security.Cryptography.HMACSHA512 hmac = (System.Security.Cryptography.HMACSHA512)System.Security.Cryptography.HashAlgorithm.Create("HMACSHA512"))
            {
                hmac.Key = passwordSalt;
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for(int i = 0; i < computedHash.Length; i++)
                {
                    if(computedHash[i] != passwordHash[i])
                        return false;
                }
            }

            return true;
        }

        public async Task<bool> UserExists(string username)
        {
            //throw new System.NotImplementedException();
            if(await _context.Users.AnyAsync(u => u.Username.Equals(username)))
                return true;

            return false;
        }
    }
}
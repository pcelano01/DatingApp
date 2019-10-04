using System.Collections.Generic;
using System.Linq;
using Dating.API.Models;
using Newtonsoft.Json;

namespace Dating.API.Data
{
    public class Seed
    {
        public static void SeedUsers(DataContext context)
        {
            if(!context.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                foreach(var item in users)
                {
                    byte[] passwordHash, passwordSalt;

                    CreatePasswordHash("password", out passwordHash, out passwordSalt);
                    item.PasswordHash = passwordHash;
                    item.PasswordSalt = passwordSalt;
                    item.Username = item.Username.ToLower();

                    context.Users.Add(item);                    
                }

                context.SaveChanges();
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(System.Security.Cryptography.HMACSHA512 hmac = (System.Security.Cryptography.HMACSHA512)System.Security.Cryptography.HashAlgorithm.Create("HMACSHA512"))
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
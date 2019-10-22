using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dating.API.Helpers;
using Dating.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Dating.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.Id.Equals(id));

            return user;
        }

        // public async Task<IEnumerable<User>> GetUsers()
        // {
        //     var users = await _context.Users.Include(u => u.Photos).ToListAsync<User>();

        //     return users;
        // }
        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(u => u.Photos);

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.SingleOrDefaultAsync(u => u.Id.Equals(id));

            return photo;
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(p => p.UserId.Equals(userId)).FirstOrDefaultAsync(p => p.IsMain);
        }
    }
}
using System;
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
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id.Equals(id));

            return user;
        }

        // public async Task<IEnumerable<User>> GetUsers()
        // {
        //     var users = await _context.Users.Include(u => u.Photos).ToListAsync<User>();

        //     return users;
        // }
        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Where(u => !u.Id.Equals(userParams.UserId)).OrderByDescending(u => u.LastActive).AsQueryable();

            if(!String.IsNullOrWhiteSpace(userParams.Gender))
            {
                users = users.Where(u => u.Gender.Equals(userParams.Gender));
            }

            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);

                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if(userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, !userParams.Likees);

                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1 );
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if(!String.IsNullOrWhiteSpace(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.Equals(id));

            if(likers)
            {
                return user.Likers.Where(u => u.LikeeId.Equals(id)).Select(l => l.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId.Equals(id)).Select(l => l.LikeeId);
            }
            
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

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(
                l => l.LikerId.Equals(userId) && l.LikeeId.Equals(recipientId));
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.SingleOrDefaultAsync(m => m.Id.Equals(id));
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages.AsQueryable();

            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(m => m.RecipientId.Equals(messageParams.UserId) && m.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(m => m.SenderId.Equals(messageParams.UserId) && m.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(m => m.RecipientId.Equals(messageParams.UserId) && m.IsRead == false && m.RecipientDeleted == false);
                    break;
            }

            messages = messages.OrderByDescending(m => m.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }       

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                            .Where(m => m.RecipientId.Equals(userId) && m.RecipientDeleted == false && m.SenderId.Equals(recipientId) ||
                            m.SenderId.Equals(userId) && m.SenderDeleted == false && m.RecipientId.Equals(recipientId))
                            .OrderByDescending(m => m.MessageSent).ToListAsync();

            return messages;
        }
    }
}
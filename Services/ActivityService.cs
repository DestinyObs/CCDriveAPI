using TheDriveAPI.Models;
using TheDriveAPI.DTOs.Activity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using TheDriveAPI.Data;

namespace TheDriveAPI.Services
{
    public interface IActivityService
    {
        Task<IEnumerable<ActivityDto>> GetActivitiesAsync(string userId, int? page = null, int? pageSize = null);
    }

    public class ActivityService : IActivityService
    {
        private readonly AppDbContext _context;
        public ActivityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ActivityDto>> GetActivitiesAsync(string userId, int? page = null, int? pageSize = null)
        {
            var query = (IOrderedQueryable<Activity>)_context.Activities.AsNoTracking().Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt);
            if (page.HasValue && pageSize.HasValue)
                query = (IOrderedQueryable<Activity>)query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var activities = await query.ToListAsync();
            return activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Action = a.Action,
                FileId = a.FileId,
                Details = a.Details,
                CreatedAt = a.CreatedAt.ToString("o")
            });
        }
    }
}

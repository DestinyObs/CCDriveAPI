using TheDriveAPI.Models;
using TheDriveAPI.DTOs.Pricing;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using TheDriveAPI.Data;

namespace TheDriveAPI.Services
{
    public interface IPricingService
    {
        Task<IEnumerable<PlanDto>> ListPlansAsync();
        Task<(bool success, string redirectUrl)> SubscribeAsync(string userId, string planName);
        Task<bool> CancelSubscriptionAsync(string userId);
    }

    public class PricingService : IPricingService
    {
        private readonly AppDbContext _context;
        public PricingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlanDto>> ListPlansAsync()
        {
            var plans = await _context.Plans.AsNoTracking().ToListAsync();
            return plans.Select(p => new PlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StorageLimit = p.StorageLimit,
                Features = p.Features
            });
        }

        public async Task<(bool success, string redirectUrl)> SubscribeAsync(string userId, string planName)
        {
            var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Name == planName);
            if (plan == null) throw new Exception("Plan not found");
            // TODO: Integrate payment provider (Stripe/PayPal)
            string redirectUrl = "https://payment-provider.com/session";
            // On payment success, update subscription
            var sub = new Subscription
            {
                UserId = userId,
                PlanId = plan.Id,
                Status = "active",
                StartedAt = DateTime.UtcNow
            };
            _context.Subscriptions.Add(sub);
            await _context.SaveChangesAsync();
            return (true, redirectUrl);
        }

        public async Task<bool> CancelSubscriptionAsync(string userId)
        {
            var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active");
            if (sub == null) throw new Exception("Active subscription not found");
            sub.Status = "cancelled";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

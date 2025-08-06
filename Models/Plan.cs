using System;
using System.Collections.Generic;

namespace TheDriveAPI.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public long StorageLimit { get; set; }
        public string Features { get; set; } = string.Empty;
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}

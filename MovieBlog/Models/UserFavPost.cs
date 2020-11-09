using System;

namespace MovieBlog.Models
{
    public class UserFavPost
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string UserId { get; set; }
        public User User { get; set; }
        
        public string PostId { get; set; }
        public Post Post { get; set; }
    }
}
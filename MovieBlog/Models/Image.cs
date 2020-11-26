using System;
using System.IO;
using System.Threading.Tasks;

namespace MovieBlog.Models
{
    public class Image
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Extension { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public byte[] Data { get; set; }
        
        public string GetSource()
        {
            var base64 = Convert.ToBase64String(Data);
            var imgSrc = string.Format($"data:image/gif;base64,{base64}");

            return imgSrc;
        }
    }
}
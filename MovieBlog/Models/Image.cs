using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using TinyPng;

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
            var imgSrc = string.Format($"data:image/png;base64,{base64}");

            return imgSrc;
        }

        public static async Task<byte[]> GetCompressedData(IFormFile image)
        {
            const string apiKey = "W5NLkgty71CbplCKMYpWhgQx9d7gVPNb";
            await using var fileStream = image.OpenReadStream();
            await using var dataStream = new MemoryStream();
            await fileStream.CopyToAsync(dataStream);
            dataStream.Position = 0;
            
            if (dataStream.Length > 1e5)
            {
                var tinyPngClient = new TinyPngClient(apiKey);
                var compressResponse = await tinyPngClient.Compress(dataStream);
                var downloadResponse = await compressResponse.Download();
                return await downloadResponse.GetImageByteData();
            }
            else
            {
                var imageOptimizer = new ImageOptimizer();
                imageOptimizer.Compress(dataStream);
                return dataStream.ToArray();
            }
        }
    }
}
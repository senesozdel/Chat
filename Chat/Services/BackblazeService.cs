using B2Net;
using B2Net.Models;
using Chat.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Chat.Services
{
    public class BackblazeService : IFileStorageService
    {
        private readonly string _bucketId;
        private readonly string _keyId;
        private readonly string _appKey;
        private readonly string _endpoint;
        private readonly B2Client _client;
        private readonly string _bucketName = "chatimages";

        public BackblazeService()
        {
            _bucketId = Environment.GetEnvironmentVariable("B2_BUCKET_ID");
            _appKey = Environment.GetEnvironmentVariable("B2_APPLICATION_KEY");
            _keyId = Environment.GetEnvironmentVariable("B2_KEY_ID");
            _endpoint = Environment.GetEnvironmentVariable("B2_ENDPOINT");

            var options = new B2Options()
            {
                KeyId = _keyId,
                ApplicationKey = _appKey,
                BucketId = _bucketId,
                PersistBucket = true
            };

            _client = new B2Client(options);

        }

        public Task DeleteFileAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] fileData = memoryStream.ToArray();


            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}" ;

            var uploadUrl = await _client.Files.GetUploadUrl(_bucketId);

            var uploadedFile = await _client.Files.Upload(fileData, fileName);



            return $"https://{_bucketName}.{_endpoint}/{fileName}";

        }

        public async Task<B2File> DownloadFileAsync(string fileName)
        {
            var downloadedFile = await _client.Files.DownloadByName(fileName, _bucketName);


            return downloadedFile;

        }
    }
}
using B2Net;
using Chat.Interfaces;
using Chat.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class FileController : ControllerBase
    {

        private readonly string _bucketId;
        private readonly IFileStorageService _fileService;
        private readonly UserService _userService;

        public FileController(IFileStorageService fileStorageService, UserService userService)
        {
            _bucketId = Environment.GetEnvironmentVariable("B2_BUCKET_ID");
            _fileService = fileStorageService;
            _userService = userService;
        }

        [HttpPut("{email}")]
        public async Task<IActionResult> UploadImage(string email, IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return BadRequest(new { message = "Resim dosyası seçilmedi." });

                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                    return NotFound(new { message = "Kullanıcı bulunamadı." });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Sadece .jpg, .jpeg, .png ve .gif uzantılı dosyalar kabul edilir." });


                var imageUrl = await _fileService.UploadFileAsync(image);
                user.Image = imageUrl;


                await _userService.UpdateAsync(user.Id, user);

                return Ok(new
                {
                    message = "Resim başarıyla yüklendi.",
                    imagePath = user.Image
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Bir hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            try
            {
                var downloadedFile = await _fileService.DownloadFileAsync(fileName);

                // Content type'ı belirle
                var contentType = "application/octet-stream"; // varsayılan
                if (Path.HasExtension(fileName))
                {
                    var ext = Path.GetExtension(fileName).ToLowerInvariant();
                    contentType = ext switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".pdf" => "application/pdf",
                        _ => "application/octet-stream"
                    };
                }

                return File(downloadedFile.FileData, contentType);
            }
            catch (Exception ex)
            {
                return NotFound($"Dosya bulunamadı: {fileName}");
            }
        }

    }
}

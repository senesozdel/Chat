using Chat.Entities;
using Chat.Models.ResponseModels;
using Chat.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class UserController : BaseController<User>
    {
        private readonly UserService _userService;
        private readonly UserRelationShipService _userRelationShipService; 

        public UserController(UserService userService, UserRelationShipService userRelationShipService) : base(userService)
        {
            _userService = userService;
            _userRelationShipService = userRelationShipService;

        }

        [HttpPost]
        public override async Task<IActionResult> Create([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest(new { message = "User data cannot be null." });
            }

            if (string.IsNullOrWhiteSpace(user.Name))
            {
                return BadRequest(new { message = "UserName is required." });
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest(new { message = "Password is required." });
            }

            // Sıralı ID oluştur
            user.Id = await _userService.GetNextSequenceValue(nameof(User));

            await _userService.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }


        [HttpGet("{email}")]
        public async Task<IActionResult> GetFriends(string email)
        {
            var user = await _userService.GetByEmailAsync(email);

            var relations = await _userRelationShipService.GetAllAsync();

            var friendIds = relations
                  .Where(x => x.UserId == user.Id || x.RelatedUserId == user.Id)
                  .Select(x => x.UserId == user.Id ? x.RelatedUserId : x.UserId) 
                  .Distinct() 
                  .ToList();

            List<UserResponseModel> friends = new List<UserResponseModel>();

            foreach (var id in friendIds)
            {
                var friend = await _userService.GetByIdAsync(id);

                var friendResponseModel = new UserResponseModel()
                {
                    Email = friend.Email,
                    UserName = friend.Name,
                    Image = friend.Image
                };

                friends.Add(friendResponseModel);
            }


            return Ok(friends);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            return Ok(user);
        }

        [HttpPost("{email}")]
        public async Task<IActionResult> UploadImage(string email, IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return BadRequest(new { message = "Resim dosyası seçilmedi." });

                // Kullanıcıyı bul
                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                    return NotFound(new { message = "Kullanıcı bulunamadı." });

                // Dosya uzantısını kontrol et
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Sadece .jpg, .jpeg, .png ve .gif uzantılı dosyalar kabul edilir." });

                // Benzersiz dosya adı oluştur
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                // Dosya kayıt yolu
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Dosyayı kaydet
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Kullanıcının resim yolunu güncelle
                user.Image = $"/uploads/{fileName}";
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


    }
}

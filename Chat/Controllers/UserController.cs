using Chat.Entities;
using Chat.Models.ResponseModels;
using Chat.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    [Route("api/[controller]")]
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


        [HttpGet("friends/{email}")]
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
                    UserName = friend.Name
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

    }
}

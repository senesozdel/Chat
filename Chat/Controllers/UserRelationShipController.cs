using Chat.Entities;
using Chat.Models;
using Chat.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRelationShipController : BaseController<UserRelationShip>
    {
        private readonly UserRelationShipService _userRelationShipService;
        private readonly UserService _userService;


        public UserRelationShipController(UserRelationShipService userRelationShipService, UserService userService)
            : base(userRelationShipService) // BaseController'a UserRelationShipService'i gönderiyoruz
        {
            _userRelationShipService = userRelationShipService;
            _userService = userService;
        }

        [HttpPost]
        [Route("/CreateUserRelationship")]
        public  async Task<IActionResult> CreateCustom([FromBody] UserRelationRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid entity data." });

            var mainUser = await _userService.GetByEmailAsync(request.MainUserMail);
            var relatedUser = await _userService.GetByEmailAsync(request.RelatedUserMail);

            var userRelation = new UserRelationShip()
            {
                UserId = mainUser.Id,
                RelatedUserId = relatedUser.Id
            };

            // Sıralı ID oluştur
            userRelation.Id = await _userRelationShipService.GetNextSequenceValue(nameof(UserRelationShip));

            await _userRelationShipService.CreateAsync(userRelation);
            return CreatedAtAction(nameof(GetById), new { id = userRelation.Id }, userRelation);
        }


    }
}

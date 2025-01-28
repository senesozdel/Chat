using Chat.Entities;
using Chat.Models;
using Chat.Models.ResponseModels;
using Chat.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRelationRequestsController : ControllerBase
    {
        private readonly UserRelationRequestsService _userRelationRequestsService;
        private readonly UserService _userService;


        public UserRelationRequestsController(UserRelationRequestsService userRelationRequestsService, UserService userService)
        {
            _userRelationRequestsService = userRelationRequestsService;
            _userService = userService;

        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserRelationRequest request)
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

            userRelation.Id = await _userRelationRequestsService.GetNextSequenceValue(nameof(UserRelationShip));

            await _userRelationRequestsService.CreateAsync(userRelation);
            return CreatedAtAction(nameof(GetById), new { id = userRelation.Id }, userRelation);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var relation = await _userRelationRequestsService.GetByIdAsync(id);

            if (relation == null)
                return NotFound(new { message = $"Relation with ID {id} not found." });

            return Ok(relation);
        }

        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    var relations = await _userRelationRequestsService.GetAllAsync();
        //    return Ok(relations);
        //}

        [HttpGet]
        public async Task<IActionResult> GetFriendRequests([FromQuery]string email)
        {
            var user = await _userService.GetByEmailAsync(email);

            var relations = await _userRelationRequestsService.GetAllAsync();

            var friendIds = relations
                  .Where(x =>  x.RelatedUserId == user.Id)
                  .Select(x => x.UserId)
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

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] UserRelationRequest request)
        {

            var sender = await _userService.GetByEmailAsync(request.MainUserMail);
            var receiver = await _userService.GetByEmailAsync(request.RelatedUserMail);

            var relations = await _userRelationRequestsService.GetAllAsync();

            var friendIds = relations
                  .Where(x => (x.UserId == sender.Id && x.RelatedUserId == receiver.Id ) || (x.UserId == receiver.Id && x.RelatedUserId == sender.Id))
                  .Select(x =>x.Id)
                  .Distinct()
                  .ToList();

            foreach (var id in friendIds) {

                var result = await _userRelationRequestsService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = $"Relation with ID {id} not found or already deleted." });

            }

            return Ok("Başarıyla Silindi");
        }
    }
}

using Chat.Data;
using Chat.Entities;
using Chat.Models;
using Chat.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;

namespace Chat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        protected readonly IMongoCollection<User> UserCollection;
        private readonly TokenService _tokenService;
        public AuthController(MongoDbService mongoDbService,TokenService tokenService)
        {
            var database = mongoDbService.Database;
            UserCollection = database.GetCollection<User>("Users");
            _tokenService = tokenService;

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(user => user.Name, loginRequest.Username),
                Builders<User>.Filter.Eq(user => user.Password, loginRequest.Password)
            );
            var user = UserCollection.Find(filter).FirstOrDefault();


            if (user != null)
            {
                var token = _tokenService.GenerateToken(loginRequest.Username);
                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        Username = user.Name,
                        Email = user.Email
                    }
                });
            }

            return Unauthorized("Geçersiz kullanıcı adı veya şifre.");
        }



    }
}

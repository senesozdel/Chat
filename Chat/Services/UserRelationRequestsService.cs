using Chat.Data;
using Chat.Entities;

namespace Chat.Services
{
    public class UserRelationRequestsService : BaseService<UserRelationShip>
    {
        public UserRelationRequestsService(MongoDbService mongoDbService) : base(mongoDbService, "UserRelationRequests")
        {
        }
    }
}

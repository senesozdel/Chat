using Chat.Data;
using Chat.Entities;

namespace Chat.Services
{
    public class UserRelationShipService : BaseService<UserRelationShip>
    {
        public UserRelationShipService(MongoDbService mongoDbService) : base(mongoDbService, "UserRelations")
        {
        }

    }
}

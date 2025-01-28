using Chat.Data;
using Chat.Entities;

namespace Chat.Services
{
    public class MessageService : BaseService<Message>
    {
        public MessageService(MongoDbService mongoDbService) : base(mongoDbService, "Messages")
        {
        }
    }
}

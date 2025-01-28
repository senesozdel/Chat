using Chat.Cache;
using Chat.Entities;
using Chat.Interfaces;

namespace Chat.Services
{
    public class BackgroundService
    {
        private readonly CookieService _cookieService;
        private readonly MessageService _messageService;
        private readonly ICacheService _cacheService;

        public BackgroundService(CookieService cookieService, MessageService messageService, ICacheService cacheService)
        {
            _cookieService = cookieService;
            _cacheService = cacheService;
            _messageService = messageService;

        }
        public async Task TakeBackup()
        {
            var messages = _cacheService.Get<List<Message>>("messages");

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    await _messageService.CreateAsync(message);

                }
            }

  

        }
    }
}

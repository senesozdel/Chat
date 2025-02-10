using Chat.Cache;
using Chat.Entities;
using Chat.Helpers;
using Chat.Hub;
using Chat.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Services
{
    public class BackgroundService
    {
        private readonly CookieService _cookieService;
        private readonly MessageService _messageService;
        private readonly ICacheService _cacheService;
        private readonly IHubContext<ChatHub> _hubContext;
        public BackgroundService(CookieService cookieService, MessageService messageService, ICacheService cacheService, IHubContext<ChatHub> hubContext)
        {
            _cookieService = cookieService;
            _cacheService = cacheService;
            _messageService = messageService;
            _hubContext = hubContext;
        }
        public async Task TakeBackup()
        {
            var (receiver, sender) = ConnectedUsersHelper.GetMessageUsers();

            if(receiver != null && sender != null)
            {

                var senderConnectionId = ConnectionManager.GetConnectionId(sender.Name);

                try
                {
                    await _hubContext.Clients.Client(senderConnectionId).SendAsync("takeBackup", true);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Backup işlemi başarısız: {ex.Message}");
                }
            }

        }
    }
}

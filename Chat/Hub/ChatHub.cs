

using Chat.Data;
using Chat.Interfaces;
using Chat.Entities;
using Chat.Models;
using Chat.Models.ResponseModels;
using Chat.Services;
using Microsoft.AspNetCore.SignalR;
using Chat.Cache;

namespace Chat.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly UserService _userService;
        private readonly ICacheService _cacheService;
        public ChatHub(UserService userService, ICacheService cacheService ) { 
        
            _userService = userService;
            _cacheService = cacheService;
        }
        public async Task OnConnected (string userName)
        {
            Client client = new Client
            {
                ConnectionId = Context.ConnectionId,
                Name = userName
            };
            ClientData.Clients.Add(client);

            ConnectionManager.AddConnection(userName, Context.ConnectionId);

            await Clients.All.SendAsync("clientJoined",userName);
            await Clients.All.SendAsync("clientList", ClientData.Clients);
        }

        public async Task SendMessageAsync(Message message)
        {
            //var senderClient =  ClientData.Clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);   
            var receiverUser = await _userService.GetByEmailAsync(message.Receivers.FirstOrDefault().Email);
            var senderUser = await _userService.GetByEmailAsync(message.Sender);

            var receiverConnectionId = ConnectionManager.GetConnectionId(receiverUser.Name);
            var senderConnectionId = ConnectionManager.GetConnectionId(senderUser.Name);

            Clients.Client(receiverConnectionId).SendAsync("receiveMessage", message);
            Clients.Client(senderConnectionId).SendAsync("receiveMessage", message);

            //var messages = _cacheService.Get<List<Message>>("messages") ?? new List<Message>();

            //// Yeni mesajı ekle
            //messages.Add(message);

            //// Cache'i güncelle
            //_cacheService.Set("messages", messages);



            //if (clientName == "Tümü")
            //{
            //    Clients.All.SendAsync("receiveMessage",message,senderClient.Name);
            //}

            //var desiredClient = ClientData.Clients.FirstOrDefault(c => c.Name == clientName);

            //Clients.Client(desiredClient.ConnectionId).SendAsync("receiveMessage",message,senderClient.Name);
            //var zaman = DateTime.Now;
            //var receiverList = new List<string>() { "baska"};
            //var modelMessage = new Message { SenderName=clientName,ReceiverNames =receiverList,  Content = message, Time = $"{zaman.Hour}:{zaman.Minute}" };

            //Clients.Client(Context.ConnectionId).SendAsync("receiveMessage", modelMessage);

        }

        public async Task SendAddFriendRequest(string senderEmail,string receiverEmail)
        {
          var receiverRequestUser = await _userService.GetByEmailAsync(receiverEmail);
          var senderRequestUser = await _userService.GetByEmailAsync(senderEmail);

           if(receiverRequestUser != null && senderRequestUser != null )
            {

                var receiverConnectionId = ConnectionManager.GetConnectionId(receiverRequestUser.Name);

                if(receiverConnectionId != null)
                {
                    Clients.Client(receiverConnectionId).SendAsync("receiveAddFriendReuqest", new UserResponseModel { Email= senderRequestUser.Email, UserName = senderRequestUser.Name });

                }
               

            }


        }





        public async Task AddGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            GroupData.Groups.Add(new Group
            {
                GroupName = groupName
            });

            await Clients.All.SendAsync("groups", groupName);

        }
    }
}


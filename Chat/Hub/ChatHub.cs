

using Chat.Data;
using Chat.Interfaces;
using Chat.Entities;
using Chat.Models;
using Chat.Models.ResponseModels;
using Chat.Services;
using Microsoft.AspNetCore.SignalR;
using Chat.Cache;
using Chat.Helpers;

namespace Chat.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly UserService _userService;
        public ChatHub(UserService userService ) { 
        
            _userService = userService;
        }
        public async Task OnConnected (string userName)
        {
            ConnectionManager.AddConnection(userName, Context.ConnectionId);
            await Clients.All.SendAsync("clientJoined",userName);
        }



        public async Task SendMessageAsync(Message message)
        {
            var receiverUser = await _userService.GetByEmailAsync(message.Receivers.FirstOrDefault()?.Email);
            var senderUser = await _userService.GetByEmailAsync(message.Sender);

            var senderConnectionId = ConnectionManager.GetConnectionId(senderUser.Name);
            var receiverConnectionId = ConnectionManager.GetConnectionId(receiverUser.Name);

            if (receiverConnectionId == null)
            {
                    message.Status = false;
                    Clients.Client(senderConnectionId).SendAsync("receiveMessage", message);

            }
            else 
            {

                ConnectedUsersHelper.SetMessageUsers(receiverUser, senderUser);

                try
                {
                    Clients.Client(receiverConnectionId).SendAsync("receiveMessage", message);
                    Clients.Client(senderConnectionId).SendAsync("receiveMessage", message);
                }
                catch (Exception ex)
                {
                    throw new HubException($"Mesaj gönderilemedi: {ex.Message}");
                }
            }

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

            await Clients.All.SendAsync("groups", groupName);

        }
    }
}


using Chat.Entities;
using Chat.Models;
using Chat.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class MessageController : BaseController<Message>
    {
        private readonly MessageService _messageService;
        public MessageController(MessageService messageService) : base(messageService)
        {
            _messageService = messageService;
        }
        [HttpGet]

        public async Task<IActionResult> GetDesiredMessages([FromQuery] ChatOwner chatOwner)
        {
            var allmessages = await _messageService.GetAllAsync();

            var messageList = allmessages
             .Where(x =>
                 (x.Sender == chatOwner.SenderMail && x.Receivers.Any(r => r.Email == chatOwner.ReceiverMail)) ||
                 (x.Sender == chatOwner.ReceiverMail && x.Receivers.Any(r => r.Email == chatOwner.SenderMail))
             )
             .ToList();

            return Ok(messageList);

        }


        [HttpPost]
        public  async Task<IActionResult> CreateMulti([FromBody] List<Message> messages)
        {
            var allMessages = await _messageService.GetAllAsync();

            var willAddedMessages = !allMessages.Any()
                ? messages.ToList()
                : messages.Where(newMessage =>
                      !allMessages.Select(m => m.Key).Contains(newMessage.Key))
                      .ToList();


            foreach (var message in willAddedMessages)
            {

                message.Id = await _messageService.GetNextSequenceValue(nameof(Message));

                await _messageService.CreateAsync(message);
            }
           return Ok(messages);
        }

    }
}

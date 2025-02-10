using Chat.Entities;
using Chat.Models;
using Chat.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : BaseController<Message>
    {
        private readonly MessageService _messageService;
        public MessageController(MessageService messageService) : base(messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        [Route("/GetMessages")]

        public async Task<IActionResult> GetDesiredMessages([FromQuery] ChatOwner chatOwner)
        {
            var allmessages = await _messageService.GetAllAsync();

            var messageList = allmessages
             .Where(x =>
                 (x.Sender == chatOwner.senderMail && x.Receivers.Any(r => r.Email == chatOwner.receiverMail)) ||
                 (x.Sender == chatOwner.receiverMail && x.Receivers.Any(r => r.Email == chatOwner.senderMail))
             )
             .ToList();

            return Ok(messageList);

        }


        [HttpPost]
        [Route("/CreateMulti")]
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

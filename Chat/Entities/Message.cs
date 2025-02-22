﻿using Chat.Models.ResponseModels;

namespace Chat.Entities
{
    public class Message : BaseEntity
    {
        public string? Key { get; set; }
        public string Content { get; set; }

        public string Sender { get; set; }

        public DateTime SendTime { get; set; }

        public List<UserResponseModel> Receivers { get; set; }

        public bool Status { get; set; }
    }
}

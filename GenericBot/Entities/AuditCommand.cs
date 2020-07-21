using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.Entities
{
    [BsonIgnoreExtraElements]
    public class AuditCommand
    {
        public ulong MessageId { get; set; }
        public ulong UserId { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Message { get; set; }

        public AuditCommand()
        {

        }

        public AuditCommand(ParsedCommand command)
        {
            this.MessageId = command.Message.Id;
            this.UserId = command.Author.Id;
            this.Time = DateTimeOffset.UtcNow;
            this.Message = command.Message.Content;
        }
    }
}

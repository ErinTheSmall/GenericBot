using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace GenericBot.Entities
{
    [BsonIgnoreExtraElements]
    public class CustomCommand
    {
        public string Name { get; set; }
        public bool Delete { get; set; }
        public string Response { get; set; }

        public CustomCommand()
        {

        }
        public CustomCommand(string name, string response)
        {
            this.Name = name;
            this.Delete = false;
            this.Response = response;
        }
    }
}

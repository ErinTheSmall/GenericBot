using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.CommandModules 
{
    class QuoteModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command addQuote = new Command("addQuote");
            addQuote.SendTyping = false;
            addQuote.Description = "Add a quote to the server's list";
            addQuote.ToExecute += async (context) =>
            {
                if (string.IsNullOrEmpty(context.ParameterString))
                {
                    await context.Message.ReplyAsync("You can't add an empty quote");
                    return;
                }
                var q = Core.AddQuote(context.ParameterString, context.Guild.Id);
                await context.Message.ReplyAsync($"Added {q.ToString()}");
            };
            commands.Add(addQuote);

            Command removeQuote = new Command("removeQuote");
            removeQuote.Description = "Remove a quote from the server's list";
            removeQuote.SendTyping = false;
            removeQuote.RequiredPermission = Command.PermissionLevels.Moderator;
            removeQuote.ToExecute += async (context) =>
            {
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync("You must supply a number");
                    return;
                }

                if (int.TryParse(context.Parameters[0], out int quid))
                {
                    if (Core.RemoveQuote(quid, context.Guild.Id))
                    {
                        await context.Message.ReplyAsync($"Succefully removed quote #{quid}");
                    }
                    else
                    {
                        await context.Message.ReplyAsync($"The number was greater than the number of quotes saved");
                        return;
                    }
                }
                else
                {
                    await context.Message.ReplyAsync("You must pass in a number");
                }
            };
            commands.Add(removeQuote);

            Command quote = new Command("quote");
            quote.SendTyping = false;
            quote.Description = "Get a random quote from the server's list";
            quote.ToExecute += async (context) =>
            {
                await context.Message.ReplyAsync(Core.GetQuote(context.ParameterString, context.Guild.Id));
            };
            commands.Add(quote);

            return commands;
        }
    }
}

using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericBot.CommandModules
{
    class ClearModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command clear = new Command("clear");
            clear.Description = "Clear a number of messages from a channel";
            clear.Usage = "clear <number> <user>";
            clear.RequiredPermission = Command.PermissionLevels.Moderator;
            clear.ToExecute += async (context) =>
            {
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync("You gotta tell me how many messages to delete!");
                    return;
                }

                ulong count;


                if (ulong.TryParse(context.Parameters[0], out count))
                {
                    int messagesToDownloadCount = (int)Math.Min(1000, count);
                    List<IMessage> msgs = (context.Message.Channel as SocketTextChannel).GetManyMessages(messagesToDownloadCount);
                    if (context.Message.MentionedUsers.Any())
                    {
                        var users = context.Message.MentionedUsers;
                        msgs = msgs.Where(m => users.Select(u => u.Id).Contains(m.Author.Id)).ToList();
                        try
                        {
                            await context.Message.DeleteAsync();
                        }
                        catch (Exception ex)
                        {
                            await Core.Logger.LogErrorMessage(ex, context);
                        }
                    }
                    if (context.Parameters.Count > 1 && !context.Message.MentionedUsers.Any())
                    {
                        await context.Message.ReplyAsync($"It looks like you're trying to mention someone but failed.");
                        return;
                    }
                    if (count > 1000) // If the input number was probably an ID
                    {
                        msgs = msgs.Where(m => m.Id >= count).ToList(); // Only keep messages sent after that ID
                    }
                    msgs = msgs.Where(m => DateTime.Now - m.CreatedAt < TimeSpan.FromDays(14)).ToList(); // Only keep last 2 weeks of messages (API Limits)
                    var logChannelId = Core.GetGuildConfig(context.Guild.Id).LoggingChannelId;
                    if (context.Guild.Channels.Any(c => c.Id == logChannelId) && !Core.GetGuildConfig(context.Guild.Id).MessageLoggingIgnoreChannels.Contains(context.Channel.Id))
                    {
                        string fileName = $"files/{context.Message.Channel.Name}-cleared-{context.Message.Id.ToString().Substring(14, 4)}.txt";
                        File.WriteAllText(fileName, JsonConvert.SerializeObject(new
                        {
                            Guild = new
                            {
                                context.Guild.Id,
                                context.Guild.Name,
                                Channel = new
                                {
                                    context.Message.Channel.Id,
                                    context.Message.Channel.Name
                                }
                            },
                            Messages = msgs.OrderBy(m => m.Id).Select(m => new
                            {
                                m.Id,
                                Author = new
                                {
                                    m.Author.Id,
                                    Username = $"{m.Author.Username}#{m.Author.Discriminator}"
                                },
                                m.Content,
                                Attatchments = m.Attachments,
                                Timestamp = m.CreatedAt
                            })
                        }, Formatting.Indented));
                        context.Guild.GetTextChannel(logChannelId).SendFileAsync(fileName, "");
                        File.Delete(fileName);
                    }
                    msgs.ForEach(m => GenericBot.ClearedMessageIds.Add(m.Id));

                    await (context.Message.Channel as ITextChannel).DeleteMessagesAsync(msgs);

                    var messagesSent = new List<IMessage>();

                    messagesSent.Add(context.Message.ReplyAsync($"{context.Author.Mention}, done deleting those messages!").Result);
                    if (msgs.Any(m => DateTime.Now - m.CreatedAt > TimeSpan.FromDays(14)))
                    {
                        messagesSent.Add(context.Message.ReplyAsync($"I couldn't delete all of them, some were older than 2 weeks old :frowning:").Result);
                    }

                    await Task.Delay(5000);
                    await (context.Message.Channel as ITextChannel).DeleteMessagesAsync(messagesSent);
                }
                else
                {
                    await context.Message.ReplyAsync("That's not a valid number");
                }
            };
            commands.Add(clear);

            return commands;
        }
    }
}

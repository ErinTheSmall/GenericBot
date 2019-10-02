﻿using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericBot.CommandModules
{
    class WarningModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command addwarning = new Command("addwarning");
            addwarning.Description += "Add a warning to the database";
            addwarning.Usage = "addwarning <user> <warning>";
            addwarning.RequiredPermission = Command.PermissionLevels.Moderator;
            addwarning.ToExecute += async (context) =>
            {
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync("You must specify a user");
                    return;
                }
                ulong uid;
                if (ulong.TryParse(context.Parameters[0].TrimStart('<', '@', '!').TrimEnd('>'), out uid))
                {
                    context.Parameters.RemoveAt(0);
                    string warning = context.Parameters.Rejoin();
                    warning += $" (Added By `{context.Author}` At `{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT`)";
                    var finalUser = Core.MongoEngine.GetUserFromGuild(uid, context.Guild.Id).AddWarning(warning);
                    Core.MongoEngine.SaveUserToGuild(finalUser, context.Guild.Id);
                    var builder = new EmbedBuilder()
                        .WithTitle("Warning Added")
                        .WithDescription(warning)
                        .WithColor(new Color(0xFFFF00))
                        .WithFooter(footer =>
                        {
                            footer
                                .WithText($"By {context.Author} at {DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");
                        });


                    EmbedFieldBuilder warningCountField = new EmbedFieldBuilder().WithName("Warning Count").WithValue(finalUser.Warnings.Count).WithIsInline(true);
                    builder.AddField(warningCountField);

                    try
                    {
                        var user = Core.DiscordClient.GetUser(uid);
                        builder.Author = new EmbedAuthorBuilder().WithName(user.ToString()).WithIconUrl(user.GetAvatarUrl());
                    }
                    catch
                    {
                        builder.Author = new EmbedAuthorBuilder().WithName(uid.ToString());
                    }


                    await context.Message.Channel.SendMessageAsync("", embed: builder.Build());
                    if (Core.GetGuildConfig(context.Guild.Id).LoggingChannelId != 0)
                    {
                        await ((SocketTextChannel)Core.DiscordClient.GetChannel(Core.GetGuildConfig(context.Guild.Id).LoggingChannelId))
                            .SendMessageAsync("", embed: builder.Build());
                    }
                }
                else
                {
                    await context.Message.ReplyAsync("Could not find that user");
                }
            };
            commands.Add(addwarning);

            Command issuewarning = new Command("issuewarning");
            issuewarning.Description += "Add a warning to the database and send it to the user";
            issuewarning.Usage = "issuewarning <user> <warning>";
            issuewarning.RequiredPermission = Command.PermissionLevels.Moderator;
            issuewarning.ToExecute += async (context) =>
            {
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync("You must specify a user");
                    return;
                }
                if (context.Message.GetMentionedUsers().Any())
                {
                    var user = context.Message.GetMentionedUsers().First();
                    if (!context.Guild.Users.Any(u => u.Id.Equals(user.Id)))
                    {
                        await context.Message.ReplyAsync("Could not find that user");
                        return;
                    }
                    context.Parameters.RemoveAt(0);
                    string warning = context.Parameters.Rejoin();
                    warning += $" (Issued By `{context.Author}` At `{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT`)";

                    var finalUser = Core.MongoEngine.GetUserFromGuild(user.Id, context.Guild.Id).AddWarning(warning);
                    Core.MongoEngine.SaveUserToGuild(finalUser, context.Guild.Id);

                    try
                    {
                        await user.GetOrCreateDMChannelAsync().Result.SendMessageAsync(
                            $"The Moderator team of **{context.Guild.Name}** has issued you the following warning:\n{context.Parameters.Rejoin()}");
                    }
                    catch
                    {
                        warning += $"\nCould not message {user}";
                    }

                    var builder = new EmbedBuilder()
                        .WithTitle("Warning Issued")
                        .WithDescription(warning)
                        .WithColor(new Color(0xFFFF00))
                        .WithFooter(footer =>
                        {
                            footer
                                .WithText($"By {context.Author} at {DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");
                        });

                    EmbedFieldBuilder warningCountField = new EmbedFieldBuilder().WithName("Warning Count").WithValue(finalUser.Warnings.Count).WithIsInline(true);
                    builder.AddField(warningCountField);

                    builder.Author = new EmbedAuthorBuilder().WithName(user.ToString()).WithIconUrl(user.GetAvatarUrl());

                    await context.Message.Channel.SendMessageAsync("", embed: builder.Build());
                    if (Core.GetGuildConfig(context.Guild.Id).LoggingChannelId != 0)
                    {
                        await ((SocketTextChannel)Core.DiscordClient.GetChannel(Core.GetGuildConfig(context.Guild.Id).LoggingChannelId))
                            .SendMessageAsync("", embed: builder.Build());
                    }
                }
                else
                {
                    await context.Message.ReplyAsync("Could not find that user");
                }
            };
            commands.Add(issuewarning);

            return commands;
        }
    }
}

﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot
{
    public static class MessageEventHandler
    {
        public static async Task MessageRecieved(SocketMessage parameterMessage, bool edited = false)
        {
            // Don't do stuff if the user is blacklisted
            if (Core.CheckBlacklisted(parameterMessage.Author.Id))
                return;
            // Ignore self
            if (parameterMessage.Author.Id == Core.GetCurrentUserId())
                return;
            try
            {
                ulong guildId = parameterMessage.GetGuild().Id;
                var command = new Command("t").ParseMessage(parameterMessage);

                if (Core.GetCustomCommands(guildId).Result.HasElement(c => c.Name == command.Name, 
                    out CustomCommand customCommand))
                {
                    if (customCommand.Delete)
                        await parameterMessage.DeleteAsync();
                    await parameterMessage.ReplyAsync(customCommand.Response);
                }

                if(command != null && command.RawCommand != null)
                    await command.Execute();
            }
            catch (Exception ex)
            {
                if (parameterMessage.Author.Id == Core.GetOwnerId())
                {
                    await parameterMessage.ReplyAsync("```\n" + $"{ex.Message}\n{ex.StackTrace}".SafeSubstring(1000) +
                                                      "\n```");
                }
                await Core.Logger.LogErrorMessage(ex.Message);
                Console.WriteLine($"{ex.StackTrace}");
            }
        }

        public static async Task MessageRecieved(SocketMessage arg)
        {
            await MessageRecieved(arg, edited: false);
        }

    }
}
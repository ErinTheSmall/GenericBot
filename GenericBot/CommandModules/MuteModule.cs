using Discord;
using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericBot.CommandModules
{
    class MuteModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command mute = new Command("mute");
            mute.RequiredPermission = Command.PermissionLevels.Moderator;
            mute.Usage = "mute <user>";
            mute.Description = "Mute a user";
            mute.ToExecute += async (context) =>
            {
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync($"You need to specify a user!");
                    return;
                }
                var gc = Core.GetGuildConfig(context.Guild.Id);
                if (!context.Guild.Roles.Any(r => r.Id == gc.MutedRoleId))
                {
                    await context.Message.ReplyAsync("The Muted Role Id is configured incorrectly. Please talk to your server admin");
                    return;
                }
                var mutedRole = context.Guild.Roles.First(r => r.Id == gc.MutedRoleId);
                List<IUser> mutedUsers = new List<IUser>();
                if (gc.MutedUsers == null)
                    gc.MutedUsers = new List<ulong>();
                foreach (var user in context.Message.GetMentionedUsers().Select(u => u.Id))
                {
                    try
                    {
                        gc.MutedUsers.Add(user);
                        await (context.Guild.GetUser(user)).AddRolesAsync(new List<IRole> { mutedRole });
                        mutedUsers.Add(context.Guild.GetUser(user));
                    }
                    catch(Exception ex)
                    {
                        await Core.Logger.LogErrorMessage(ex, context);
                    }
                    Core.SaveGuildConfig(gc);
                }
                string result;
                if (mutedUsers.Count > 0)
                {
                    result = "Succesfully muted " + mutedUsers.Select(u => u.Mention).ToList().SumAnd();
                }
                else
                {
                    result = "Could not find that user";
                }

                await context.Message.ReplyAsync(result);
            };
            commands.Add(mute);

            Command unmute = new Command("unmute");
            unmute.RequiredPermission = Command.PermissionLevels.Moderator;
            unmute.Usage = "unmute <user>";
            unmute.Description = "Unmute a user";
            unmute.ToExecute += async (context) =>
            {
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync($"You need to specify a user!");
                    return;
                }
                var gc = Core.GetGuildConfig(context.Guild.Id);
                if (!context.Guild.Roles.Any(r => r.Id == gc.MutedRoleId))
                {
                    await context.Message.ReplyAsync("The Muted Role Id is configured incorrectly. Please talk to your server admin");
                    return;
                }
                var mutedRole = context.Guild.Roles.First(r => r.Id == gc.MutedRoleId);
                List<IUser> mutedUsers = new List<IUser>();
                foreach (var user in context.Message.GetMentionedUsers().Select(u => u.Id))
                {
                    try
                    {
                        gc.MutedUsers.Remove(user);
                        await (context.Guild.GetUser(user)).RemoveRoleAsync(mutedRole);
                        mutedUsers.Add(context.Guild.GetUser(user));
                    }
                    catch (Exception ex)
                    {
                        await Core.Logger.LogErrorMessage(ex, context);
                    }
                }
                Core.SaveGuildConfig(gc);

                string res = "Succesfully unmuted ";
                for (int i = 0; i < mutedUsers.Count; i++)
                {
                    if (i == mutedUsers.Count - 1 && mutedUsers.Count > 1)
                    {
                        res += $"and {mutedUsers.ElementAt(i).Mention}";
                    }
                    else
                    {
                        res += $"{mutedUsers.ElementAt(i).Mention}, ";
                    }
                }

                await context.Message.ReplyAsync(res.TrimEnd(',', ' '));
            };
            commands.Add(unmute);

            return commands;
        }
    }
}

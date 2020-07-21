using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericBot.CommandModules
{
    class GiveawayModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command giveaway = new Command("giveaway");
            giveaway.Usage = "giveaway [create <description>|join <id>|close <id>|roll <id>|list]";
            giveaway.ToExecute += async (context) =>
            {
                if (context.Parameters.Count == 0)
                {
                    await context.Message.ReplyAsync("Please choose one of the following options: `create`, `join`, `close`, `roll`, or `list`");
                }
                else if (context.Parameters[0].ToLower().Equals("create") || context.Parameters[0].ToLower().Equals("open") || context.Parameters[0].ToLower().Equals("start"))
                {
                    string desc = context.Parameters.Count > 1 ? context.ParameterString.Substring("create".Length).Trim() : string.Empty;
                    Giveaway newGiveaway = new Giveaway(context, desc);
                    newGiveaway = Core.CreateGiveaway(newGiveaway, context.Guild.Id);

                    await context.Message.ReplyAsync($"New giveaway with id `{newGiveaway.Id}` created!");
                }
                else if (context.Parameters[0].ToLower().Equals("join"))
                {
                    List<Giveaway> giveaways = Core.GetGiveaways(context.Guild.Id).Where(g => g.IsActive).ToList();

                    if (giveaways.Count == 0)
                    {
                        await context.Message.ReplyAsync("No giveaways found");
                    }
                    else if (giveaways.Count == 1)
                    {
                        Giveaway g = giveaways.First();
                        if (g.EnteredUsers.Contains(context.Author.Id))
                            await context.Message.ReplyAsync("You're already in that giveaway");
                        else
                        {
                            g.EnteredUsers.Add(context.Author.Id);
                            Core.UpdateOrCreateGiveaway(g, context.Guild.Id);
                            await context.Message.ReplyAsync($"You've joined the giveaway! Good luck!");
                        }
                    }
                    else
                    {
                        if (context.Parameters.Count < 2)
                            await context.Message.ReplyAsync("There are multiple giveaways running. Please provide an Id");
                        else
                        {
                            Giveaway g = giveaways.Find(x => x.Id.ToLower().Equals(context.Parameters[1]));
                            if (g == null)
                                await context.Message.ReplyAsync("Invalid Id");
                            else if (g.EnteredUsers.Contains(context.Author.Id))
                                await context.Message.ReplyAsync("You're already in that giveaway");
                            else
                            {
                                g.EnteredUsers.Add(context.Author.Id);
                                Core.UpdateOrCreateGiveaway(g, context.Guild.Id);
                                await context.Message.ReplyAsync($"You've joined the giveaway! Good luck!");
                            }
                        }
                    }
                }
                else if (context.Parameters[0].ToLower().Equals("close"))
                {
                    List<Giveaway> giveaways = Core.GetGiveaways(context.Guild.Id).Where(g => g.IsActive).ToList();

                    if (giveaways.Count == 0)
                    {
                        await context.Message.ReplyAsync("No giveaways found");
                    }
                    else if (giveaways.Count == 1)
                    {
                        Giveaway g = giveaways.First();
                        if (g.OwnerId != context.Author.Id && giveaway.GetPermissions(context) < Command.PermissionLevels.Moderator)
                            await context.Message.ReplyAsync("You do not have permissions to do that.");
                        else
                        {
                            g.IsActive = false;
                            Core.UpdateOrCreateGiveaway(g, context.Guild.Id);
                            await context.Message.ReplyAsync($"Giveaway closed with {g.EnteredUsers.Count} participants!");
                        }
                    }
                    else
                    {
                        if (context.Parameters.Count < 2)
                            await context.Message.ReplyAsync("There are multiple giveaways running. Please provide an Id");
                        else
                        {
                            Giveaway g = giveaways.Find(x => x.Id.ToLower().Equals(context.Parameters[1]));
                            if (g == null)
                                await context.Message.ReplyAsync("Invalid Id");
                            else if (g.OwnerId != context.Author.Id && giveaway.GetPermissions(context) < Command.PermissionLevels.Moderator)
                                await context.Message.ReplyAsync("You do not have permissions to do that.");
                            else
                            {
                                g.IsActive = false;
                                Core.UpdateOrCreateGiveaway(g, context.Guild.Id);
                                await context.Message.ReplyAsync($"Giveaway closed with {g.EnteredUsers.Count} participants!");
                            }
                        }
                    }
                }
                else if (context.Parameters[0].ToLower().Equals("roll"))
                {
                    List<Giveaway> giveaways = Core.GetGiveaways(context.Guild.Id).Where(g => !g.IsActive).ToList();

                    if (giveaways.Count == 0)
                    {
                        await context.Message.ReplyAsync("No giveaways found. Make sure the giveaway is closed before rolling.");
                    }
                    else if (giveaways.Count == 1)
                    {
                        Giveaway g = giveaways.First();
                        if (g.OwnerId != context.Author.Id && giveaway.GetPermissions(context) < Command.PermissionLevels.Moderator)
                            await context.Message.ReplyAsync("You do not have permissions to do that.");
                        else
                            await context.Message.ReplyAsync($"<@{g.EnteredUsers.GetRandomItem()}> has won {g.Description}!");
                    }
                    else
                    {
                        if (context.Parameters.Count < 2)
                            await context.Message.ReplyAsync("There are multiple giveaways running. Please provide an Id");
                        else
                        {
                            Giveaway g = giveaways.Find(x => x.Id.ToLower().Equals(context.Parameters[1]));
                            if (g == null)
                                await context.Message.ReplyAsync("Invalid Id");
                            else if (g.OwnerId != context.Author.Id && giveaway.GetPermissions(context) < Command.PermissionLevels.Moderator)
                                await context.Message.ReplyAsync("You do not have permissions to do that.");
                            else
                                await context.Message.ReplyAsync($"<@{g.EnteredUsers.GetRandomItem()}> has won {g.Description}!");
                        }
                    }
                }
                else if (context.Parameters[0].ToLower().Equals("delete"))
                {
                    List<Giveaway> giveaways = Core.GetGiveaways(context.Guild.Id);

                    if (giveaways.Count == 0)
                    {
                        await context.Message.ReplyAsync("No giveaways found");
                    }
                    else
                    {
                        if (context.Parameters.Count < 2)
                            await context.Message.ReplyAsync("Please provide an Id");
                        else
                        {
                            Giveaway g = giveaways.Find(x => x.Id.ToLower().Equals(context.Parameters[1]));
                            if (g == null)
                                await context.Message.ReplyAsync("Invalid Id");
                            else if (g.OwnerId != context.Author.Id && giveaway.GetPermissions(context) < Command.PermissionLevels.Moderator)
                                await context.Message.ReplyAsync("You do not have permissions to do that.");
                            else
                            {
                                Core.DeleteGiveaway(g, context.Guild.Id);
                                await context.Message.ReplyAsync($"Deleted giveaway.");
                            }
                        }
                    }
                }
                else if (context.Parameters[0].ToLower().Equals("list"))
                {
                    List<Giveaway> giveaways = Core.GetGiveaways(context.Guild.Id);
                    string resp = "";
                    if (giveaways.Any(g => g.IsActive))
                        resp += "Open Giveaways:";
                    foreach (Giveaway _giveaway in giveaways.Where(g => g.IsActive))
                    {
                        resp += $"\n  `{_giveaway.Id}`: {_giveaway.Description} (By {Core.DiscordClient.GetUser(_giveaway.OwnerId)}, {_giveaway.EnteredUsers.Count} entered)";
                    }
                    if (giveaways.Any(g => !g.IsActive))
                        resp += "\nClosed Giveaways:";
                    foreach (Giveaway _giveaway in giveaways.Where(g => !g.IsActive))
                    {
                        resp += $"\n  `{_giveaway.Id}`: {_giveaway.Description} (By {Core.DiscordClient.GetUser(_giveaway.OwnerId)}, {_giveaway.EnteredUsers.Count} entered)";
                    }

                    if (string.IsNullOrEmpty(resp))
                        await context.Message.ReplyAsync("No running giveaways found");
                    else
                        await context.Message.ReplyAsync(resp);
                }
            };
            commands.Add(giveaway);

            return commands;
        } 
    }
}

using Discord;
using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace GenericBot.CommandModules
{
    class SocialModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command roll = new Command("roll");
            roll.Aliases.Add("dice");
            roll.Description = "Roll a specified number of dices. Defaults to 1d20 if no parameters";
            roll.Usage = "roll [count]d[sides]";
            roll.WorksInDms = true;
            roll.ToExecute += async (context) =>
            {
                uint count = 1;
                uint sides = 20;
                int add = 0;
                if (!context.Parameters.IsEmpty()) // If there are any parameters
                {
                    string param = context.ParameterString.ToLower();
                    if (!param.Contains("d")) // it's just a number
                    {
                        if (!uint.TryParse(param, out count))
                        {
                            await context.Message.ReplyAsync("Input improperly formatted");
                            return;
                        }
                    }
                    else
                    {
                        if (!(param.Contains("+") || param.Contains("-"))) // There's no modifier
                        {
                            var list = param.Split('d');
                            if (!uint.TryParse(list[1], out sides))
                            {
                                await context.Message.ReplyAsync("Input improperly formatted");
                                return;
                            }
                            uint.TryParse(list[0], out count);
                            count = (count <= 1 ? 1 : count);
                        }
                        else
                        {
                            string c = param.Split('d')[0];
                            string second = param.Split('d')[1];
                            string s = "";
                            string a = "";
                            if (param.Contains("+"))
                            {
                                s = second.Split('+')[0];
                                a = second.Split('+')[1];
                            }
                            else if (param.Contains("-"))
                            {
                                s = second.Split('-')[0];
                                a = second.Replace(s, "");
                            }

                            if (!(uint.TryParse(c, out count) && uint.TryParse(s, out sides) && int.TryParse(a, out add)))
                            {
                                await context.Message.ReplyAsync("Input improperly formatted");
                                return;
                            }
                        }
                    }
                }

                //if (count > 100)
                //{
                //    await context.Message.ReplyAsync("I don't think you can hold that many!");
                //    return;
                //}
                //if (sides < 2)
                //{
                //    await context.Message.ReplyAsync("You can't have a 1-sided dice!");
                //    return;
                //}
                //if (sides > 100)
                //{
                //    await context.Message.ReplyAsync("That's an awefully large dice, I can't hold that");
                //    return;
                //}
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                List<int> results = new List<int>();
                while (results.Count < count)
                {
                    byte[] bytes = new byte[4];
                    crypto.GetNonZeroBytes(bytes);
                    long rand = Math.Abs(BitConverter.ToInt32(bytes, 0)) % sides;
                    results.Add((int)rand + 1 + add);
                }

                string res = $"{context.Author.Mention}, you rolled ";
                results.Sort();
                res += results.SumAnd();
                if (count > 1) res += $" with a total of {results.Sum()}";
                await context.Message.ReplyAsync(res);
            };
            commands.Add(roll);



            Command say = new Command("say");
            say.Delete = true;
            say.Aliases = new List<string> { "echo" };
            say.Description = "Say something a contributor said";
            say.SendTyping = false;
            say.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            say.Usage = "say <phrase>";
            say.ToExecute += async (context) =>
            {
                ulong channelid = context.Channel.Id;
                if (ulong.TryParse(context.Parameters[0], out channelid))
                {
                    await ((ITextChannel)Core.DiscordClient.GetChannel(channelid)).SendMessageAsync(context.ParameterString.Substring(context.ParameterString.IndexOf(' ')));
                    return;
                }
                else
                {
                    await context.Message.ReplyAsync(context.ParameterString.Substring(context.ParameterString.IndexOf(' ')));
                }
            };
            commands.Add(say);
            
            return commands;
        }
    }
}

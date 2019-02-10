﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TweetSharp;
using Timer = System.Timers.Timer;

namespace GenericBot
{
    class GenericBot
    {
        public static DiscordShardedClient DiscordClient;
        public static Logger Logger;
        public static GlobalConfiguration GlobalConfiguration;
        public static Dictionary<ulong, GuildConfig> GuildConfigs;
        public static List<Command> Commands = new List<Command>();
        public static string BuildNumber = "Unknown";
        public static bool DebugMode = false;
        public static Animols Animols = new Animols();
        public static string DBPassword;

        public static Timer StatusPollingTimer = new Timer();
        //public static int MessageCounter = 0;
        //public static int CommandCounter = 0;
        //public static int Latency = 0;

        public static ConcurrentDictionary<ulong, DBGuild> LoadedGuildDbs = new ConcurrentDictionary<ulong, DBGuild>();
        public static ConcurrentDictionary<ulong, GuildMessageStats> LoadedGuildMessageStats = new ConcurrentDictionary<ulong, GuildMessageStats>();
        public static LiteDB.LiteDatabase GlobalDatabase;

        public static Timer Updater = new Timer();

        public static Dictionary<ulong, List<IMessage>> MessageDeleteQueue = new Dictionary<ulong, List<IMessage>>();
        public static Timer MessageDeleteTimer = new Timer();
        public static bool Test = true;
        public static Stopwatch QuickWatch = new Stopwatch();
        public static ParsedCommand LastCommand;

        public static List<string> LockedFiles = new List<string>();
        public static HashSet<ulong> ClearedMessageIds = new HashSet<ulong>();
        public static DateTimeOffset LastMessageRecieved = DateTimeOffset.UtcNow;

        public static int Disconnects = 0;

        static void Main(string[] args)
        {
            Logger = new Logger(GetStringSha256Hash(DateTime.UtcNow.ToString()));
            if (File.Exists("version.txt"))
            {
                Logger.LogGenericMessage($"Build {File.ReadAllText("version.txt").Trim()}");
                BuildNumber = File.ReadAllText("version.txt").Trim();
            }
            GlobalConfiguration = new GlobalConfiguration().Load();
            DBPassword = GlobalConfiguration.DatabasePassword;
            GlobalDatabase = new LiteDB.LiteDatabase(@"Filename=files/guildDatabase.db; Mode=Shared; Async=true; Password=" + DBPassword);
            GuildConfigs = new Dictionary<ulong, GuildConfig>();

            #region Timers

            Updater.AutoReset = true;
            Updater.Interval = 5 * 1000;
            Updater.Elapsed += CheckMuteRemoval;

            if (!string.IsNullOrEmpty(GlobalConfiguration.StatusAuthKey))
            {
                StatusPollingTimer.AutoReset = true;
                StatusPollingTimer.Interval = 1 * 1000;
                StatusPollingTimer.Elapsed += StatusPollingTimerOnElapsed;
                StatusPollingTimer.Start();
            }

            #endregion Timers

            while (true)
            {
                new GenericBot().Start().GetAwaiter().GetResult();
            }
        }

        public async Task Start()
        {
            DiscordClient = new DiscordShardedClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
            });

            DiscordClient.Log += Logger.LogClientMessage;
            DiscordClient.GuildAvailable += OnGuildConnected;

            try
            {
                await DiscordClient.LoginAsync(TokenType.Bot, GlobalConfiguration.Token);
                await DiscordClient.StartAsync();
            }
            catch (Exception e)
            {
                await Logger.LogErrorMessage($"{e.Message}\n{e.StackTrace.Split('\n').First(s => s.Contains("line"))}");
                return;
            }

            if (GlobalConfiguration.OwnerId == 0)
            {
                GlobalConfiguration.OwnerId = DiscordClient.GetApplicationInfoAsync().Result.Owner.Id;
                GlobalConfiguration.Save();
            }

            foreach (var shard in DiscordClient.Shards)
            {
                shard.Ready += OnReady;
                shard.SetGameAsync($"v. {BuildNumber}").FireAndForget();
            }

            DiscordClient.MessageReceived += AsyncEventHandler.MessageRecieved;
            DiscordClient.MessageUpdated += AsyncEventHandler.MessageUpdated;
            DiscordClient.MessageDeleted += AsyncEventHandler.MessageDeleted;
            DiscordClient.UserJoined += AsyncEventHandler.UserJoinedGuild;
            DiscordClient.UserLeft += AsyncEventHandler.UserLeftGuild;
            DiscordClient.GuildMemberUpdated += AsyncEventHandler.UserUpdated;
            DiscordClient.JoinedGuild += AsyncEventHandler.BotJoinedGuild;
            DiscordClient.LeftGuild += AsyncEventHandler.BotLeftGuild;

            var serviceProvider = ConfigureServices();

            var _handler = new CommandHandler();
            await _handler.Install(serviceProvider);

            Updater.Start();

            // Block this program until it is closed.
            await Task.Delay(-1);
        }


        private async Task OnReady()
        {
            foreach (var guild in DiscordClient.Guilds.Where(g => !File.Exists($"files/guildConfigs/{g.Id}.json")))
            {
                new GuildConfig(guild.Id).Save();
            }
            foreach (var guild in DiscordClient.Guilds)
            {
                var config = JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText($"files/guildConfigs/{guild.Id}.json"));
                GuildConfigs.Add(guild.Id, config); 
                await Logger.LogGenericMessage($"Loaded config for {guild.Name} ({guild.Id}) Prefix: \"{config.Prefix}\"");
            }
            await Task.Delay(100);
            await Logger.LogGenericMessage($"Loaded {GuildConfigs.Count} Configs on Startup");
        }

        private async Task OnGuildConnected(SocketGuild guild)
        {
            await Logger.LogGenericMessage($"Connected to {guild.Name} ({guild.Id})");
            bool f = LoadedGuildDbs.TryAdd(guild.Id, new DBGuild(guild.Id));
            await Logger.LogGenericMessage($"Loaded DB for {guild.Name} ({guild.Id}): {f}");
            if (!File.Exists($"files/guildConfigs/{guild.Id}.json"))
            {
                new GuildConfig(guild.Id).Save();
            }
            else
            {
                GuildConfigs.Add(guild.Id, JsonConvert.DeserializeObject<GuildConfig>(
                    File.ReadAllText($"files/guildConfigs/{guild.Id}.json")));
            }
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(DiscordClient);
            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
            return provider;
        }

        private static void StatusPollingTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (GenericBot.DiscordClient.GetShard(0).ConnectionState == ConnectionState.Disconnecting)
            {
                if (StatusPollingTimer.Interval == 15 * 1000)
                {
                    Logger.LogErrorMessage("Disconnecting timed out, forcing exit");
                    Environment.Exit(1);
                }
                else
                    StatusPollingTimer.Interval = 15 * 1000;
                return;
            }

            //try
            //{
            //    using (var httpClient = new WebClient())
            //    {
            //        httpClient.Headers["Authorization"] = GlobalConfiguration.StatusAuthKey;
            //        httpClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            //        var result = httpClient.UploadString("https://mastrchef.rocks/programs/genericbot/status/update/", JsonConvert.SerializeObject(new BotStatus()));
            //    }
            //    StatusPollingTimer.Interval = 5 * 1000;
            //}
            //catch (Exception ex)
            //{
            //    StatusPollingTimer.Interval = 15 * 1000;
            //}
        }

        private static void CheckMuteRemoval(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var mute in GuildConfigs.SelectMany(g => g.Value.ChannelMutes))
            {
                if (mute.RemovealTime < DateTime.UtcNow)
                {
                    (DiscordClient.GetChannel(mute.ChannelId) as SocketTextChannel)
                        .RemovePermissionOverwriteAsync(DiscordClient.GetUser(mute.UserId));
                    GuildConfigs[((SocketGuildChannel)DiscordClient.GetChannel(mute.ChannelId)).Guild.Id].ChannelMutes
                        .Remove(mute);
                    GuildConfigs[((SocketGuildChannel)DiscordClient.GetChannel(mute.ChannelId)).Guild.Id].Save();
                    GenericBot.Logger.LogGenericMessage($"Unmuted {mute.UserId} from {mute.ChannelId}");
                }
            }

            foreach (var gc in GuildConfigs.Select(l => l.Value))
            {
                foreach (var ban in gc.Bans)
                {
                    if (ban.BannedUntil < DateTime.UtcNow)
                    {
                        gc.Bans.Remove(ban);
                        var user = DiscordClient.GetGuild(ban.GuildId).GetBansAsync().Result
                            .First(b => b.User.Id == ban.Id).User;
                        DiscordClient.GetGuild(ban.GuildId).RemoveBanAsync(ban.Id);
                        var builder = new EmbedBuilder()
                            .WithTitle("User Unbanned")
                            .WithDescription($"Banned for: {ban.Reason}")
                            .WithColor(new Color(0xFFFF00))
                            .WithFooter(footer => {
                                footer
                                    .WithText($"{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");
                            })
                            .WithAuthor(author => {
                                author
                                    .WithName(user.ToString())
                                    .WithIconUrl(user.GetAvatarUrl());
                            })
                            .AddField(new EmbedFieldBuilder().WithName("All Warnings").WithValue(
                                new DBGuild(ban.GuildId).GetUser(ban.Id).Warnings.SumAnd()));
                        ((SocketTextChannel)DiscordClient.GetChannel(gc.UserLogChannelId))
                            .SendMessageAsync("", embed: builder.Build());
                    }
                }
            }
        }

        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }

        public static void QueueMessagesForDelete(List<IMessage> messages)
        {
            try
            {
                GenericBot.MessageDeleteQueue.Add(messages.First().Channel.Id, messages);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                GenericBot.MessageDeleteQueue[messages.First().Channel.Id].AddRange(messages);
            }
        }
    }
}

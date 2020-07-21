using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenericBot
{
    public static class GuildEventHandler
    {
        public static async Task GuildLoaded(SocketGuild guild)
        {
            guild.DownloadUsersAsync();
        }
    }
}

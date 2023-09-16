using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MattBot.Utilities;
using System.Diagnostics;

namespace MattBot.Modules.General
{
    public class PingModule : InteractionModuleBase<SocketInteractionContext>
    {
        // ping
        [EnabledInDm(false)]
        [SlashCommand("ping", "Checks the bot's latency")]
        public async Task PingAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await RespondAsync("Ping: ...");
            stopwatch.Stop();
            await ModifyOriginalResponseAsync(msg => msg.Content = $"Ping: {stopwatch.ElapsedMilliseconds}ms | Websocket: {Context.Client.Latency}ms");
        }
    }
}

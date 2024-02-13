using Discord.Interactions;
using System.Diagnostics;

namespace mattbot.modules.general
{
    [EnabledInDm(false)]
    public class PingModule : InteractionModuleBase<SocketInteractionContext>
    {
        // ping
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

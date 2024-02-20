using Discord.Commands;

namespace mattbot.modules.general
{
    public class ThanksModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Random random = new Random();

        [Command("thanks mattbot")]
        [Alias("thank you mattbot", "thanks automatt", "thank you automatt")]
        public async Task ThanksAsync()
        {
            string[] responses =
            {
                "coolio",
                "glad to help",
                "Glad to help!",
                "happy to help",
                "I have no emotion but I appreciate the thanks nonetheless.",
                "just doin my job!",
                "mhm",
                "mhmm",
                "no problem",
                "No problem!",
                "np",
                "o",
                "ur welcome",
                "you don't need to thank me I'm just a bot",
                "You're very welcome!",
                "you're welcome",
                "You're welcome.",
                "\uD83D\uDE04",
                "\uD83D\uDE47"
            };
            await ReplyAsync(responses[random.Next(responses.Length)]);
        }
    }
}

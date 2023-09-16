using Discord.Interactions;

namespace MattBot.Modules.General
{
    public class HelpModule : InteractionModuleBase<SocketInteractionContext>
    {
        // help
        [EnabledInDm(false)]
        [SlashCommand("help", "Sends help")]
        public async Task HelpAsync()
        {
            try
            {
                await Context.User.SendMessageAsync("help");
                await RespondAsync($"{Context.User.Mention}, help has been sent to your Direct Messages!");
                await Task.Delay(15000);
                await Context.User.SendMessageAsync("no seriously can you help me");
                await Task.Delay(5000);
                await Context.User.SendMessageAsync("please");
            }
            catch (Exception)
            {
                await RespondAsync("I couldn't send you a direct message... are you blocking them... or me... :(");
            }
        }
    }
}

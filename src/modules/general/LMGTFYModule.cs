using Discord.Interactions;
using Discord.WebSocket;

namespace mattbot.modules.general;

public class LMGTFYModule : InteractionModuleBase<SocketInteractionContext>
{
    [EnabledInDm(false)]
    [MessageCommand("LMGTFY")]
    public async Task LMGTFYCommand(IMessage message)
    {
        string url = "https://lmgt.org/?q=";
        string[] words = message.Content.Split(' ');
        string query = string.Join("+", words);
        string lmgtfyurl = url + query;

        SocketRole noContextCommands = Context.Guild.Roles.FirstOrDefault(role => role.Name == "No Context Commands");
        if (Context.User is IGuildUser guildUser && guildUser.RoleIds.Contains(noContextCommands.Id))
        {
            await RespondAsync("You are prohibited from using this command!", ephemeral: true);
            return;
        }

        if (message.Author.IsBot || message.Author.IsWebhook)
        {
            await RespondAsync("Bots know everything already!", ephemeral: true);
            return;
        }

        if (message is not IUserMessage userMessage || query.Length == 0)
        {
            await RespondAsync("There is nothing to Google!", ephemeral: true);
            return;
        }

        AllowedMentions allowedMentions = new AllowedMentions { UserIds = new List<ulong> { message.Author.Id } };
        await RespondAsync($"{message.Author.Mention}, this might help:\n<{lmgtfyurl}>", allowedMentions: allowedMentions);
    }
}

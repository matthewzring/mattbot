using Discord.Interactions;
using Discord.WebSocket;

namespace mattbot.modules.general
{
    [CyberPatriot]
    public class LinksModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const string RULESBOOK = "https://www.uscyberpatriot.org/competition/rules-book";
        private const string SCHEDULE = "https://www.uscyberpatriot.org/competition/current-competition/competition-schedule";
        private const string CHALLENGES = "https://www.uscyberpatriot.org/competition/current-competition/challenges-by-round";
        private const string ADVANCEMENT = "https://www.uscyberpatriot.org/competition/Competition-Overview/tiers-and-advancement";
        private const string CONTACT = "https://www.uscyberpatriot.org/Pages/About/Contact-Us.aspx";

        [UserCommand("Rules Book")]
        public async Task HandleRulesBookCommand(IUser user)
        {
            SocketRole noContextCommands = Context.Guild.Roles.FirstOrDefault(role => role.Name == "No Context Commands");
            if (Context.User is IGuildUser guildUser && guildUser.RoleIds.Contains(noContextCommands.Id))
            {
                await RespondAsync("You are prohibited from using this command!", ephemeral: true);
                return;
            }
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(RULESBOOK, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, please read the rules book:\n{RULESBOOK}");
        }

        [UserCommand("Competition Schedule")]
        public async Task HandleCompetitionScheduleCommand(IUser user)
        {
            SocketRole noContextCommands = Context.Guild.Roles.FirstOrDefault(role => role.Name == "No Context Commands");
            if (Context.User is IGuildUser guildUser && guildUser.RoleIds.Contains(noContextCommands.Id))
            {
                await RespondAsync("You are prohibited from using this command!", ephemeral: true);
                return;
            }
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(SCHEDULE, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, please see the CyberPatriot website:\n{SCHEDULE}");
        }

        [UserCommand("Challenges by Round")]
        public async Task HandleChallengesByRoundCommand(IUser user)
        {
            SocketRole noContextCommands = Context.Guild.Roles.FirstOrDefault(role => role.Name == "No Context Commands");
            if (Context.User is IGuildUser guildUser && guildUser.RoleIds.Contains(noContextCommands.Id))
            {
                await RespondAsync("You are prohibited from using this command!", ephemeral: true);
                return;
            }
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(CHALLENGES, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, please see the CyberPatriot website:\n{CHALLENGES}");
        }

        [UserCommand("Tiers & Advancement")]
        public async Task HandleTiersAndAdvancementCommand(IUser user)
        {
            SocketRole noContextCommands = Context.Guild.Roles.FirstOrDefault(role => role.Name == "No Context Commands");
            if (Context.User is IGuildUser guildUser && guildUser.RoleIds.Contains(noContextCommands.Id))
            {
                await RespondAsync("You are prohibited from using this command!", ephemeral: true);
                return;
            }
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(ADVANCEMENT, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, please see the CyberPatriot website:\n{ADVANCEMENT}");
        }

        [UserCommand("Contact")]
        public async Task HandleContactCommand(IUser user)
        {
            SocketRole noContextCommands = Context.Guild.Roles.FirstOrDefault(role => role.Name == "No Context Commands");
            if (Context.User is IGuildUser guildUser && guildUser.RoleIds.Contains(noContextCommands.Id))
            {
                await RespondAsync("You are prohibited from using this command!", ephemeral: true);
                return;
            }
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(CONTACT, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, you can contact CyberPatriot here:\n{CONTACT}");
        }
    }
}

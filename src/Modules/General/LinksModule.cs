using Discord.Interactions;

namespace MattBot.Modules.General
{
    [CyberPatriot]
    public class LinksModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const string RULESBOOK = "https://www.uscyberpatriot.org/competition/rules-book";
        private const string SCHEDULE = "https://www.uscyberpatriot.org/competition/current-competition/competition-schedule";
        private const string CHALLENGES = "https://www.uscyberpatriot.org/competition/current-competition/challenges-by-round";
        private const string ADVANCEMENT = "https://www.uscyberpatriot.org/competition/Competition-Overview/tiers-and-advancement";
        private const string CONTACT = "https://www.uscyberpatriot.org/Pages/About/Contact-Us.aspx";

        [EnabledInDm(false)]
        [UserCommand("Rules Book")]
        public async Task HandleRulesBookCommand(IUser user)
        {
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(RULESBOOK, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, please read the rules book:\n{RULESBOOK}");
        }

        [EnabledInDm(false)]
        [UserCommand("Competition Schedule")]
        public async Task HandleCompetitionScheduleCommand(IUser user)
        {
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(SCHEDULE, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, please see the CyberPatriot website:\n{SCHEDULE}");
        }

        [EnabledInDm(false)]
        [UserCommand("Challenges by Round")]
        public async Task HandleChallengesByRoundCommand(IUser user)
        {
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(CHALLENGES, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, please see the CyberPatriot website:\n{CHALLENGES}");
        }

        [EnabledInDm(false)]
        [UserCommand("Tiers & Advancement")]
        public async Task HandleTiersAndAdvancementCommand(IUser user)
        {
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(ADVANCEMENT, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, please see the CyberPatriot website:\n{ADVANCEMENT}");
        }

        [EnabledInDm(false)]
        [UserCommand("Contact")]
        public async Task HandleContactCommand(IUser user)
        {
            if (user.IsBot || user.Id == Context.User.Id)
            {
                await RespondAsync(CONTACT, ephemeral: true);
                return;
            }
            await RespondAsync($"{user.Mention}, you can contact CyberPatriot here:\n{CONTACT}");
        }
    }
}

using Discord.Interactions;
using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.modules.owner
{
    [EnabledInDm(false)]
    [Group("admin", "admin commands")]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        // admin shutdown
        [SlashCommand("shutdown", "Terminates the bot instance")]
        public async Task ShutdownAsync()
        {
            var owner = Context.Guild.GetUser(OWNER_ID);
            if (Context.User.Id != owner.Id)
            {
                await RespondAsync($"{X} Sorry, this command can only be used by {FormatUtil.formatUser(owner)}!", ephemeral: true);
                return;
            }

            await RespondAsync("Shutting down...", ephemeral: true).ConfigureAwait(false);
            Environment.Exit(0);
        }

        // admin set
        [Ignore]
        [Group("set", "Set bot status")]
        public class StatusModule : InteractionModuleBase<SocketInteractionContext>
        {
            private DiscordSocketClient _client;

            public StatusModule(DiscordSocketClient client)
            {
                _client = client;
            }

            // admin set activity
            [SlashCommand("activity", "Set the bot's activity")]
            public async Task SetActivityAsync([Choice("Playing", 0),
                                            Choice("Listening to", 2),
                                            Choice("Watching", 3),
                                            Choice("Competing in", 5)]
                                            [Summary("activity", "The type of the activity")] int activityTypeValue,
                                            [Summary("name", "The name of the activity")] string name)
            {
                var owner = Context.Guild.GetUser(OWNER_ID);
                if (Context.User.Id != owner.Id)
                {
                    await RespondAsync($"{X} Sorry, this command can only be used by {FormatUtil.formatUser(owner)}!", ephemeral: true);
                    return;
                }

                try
                {
                    ActivityType activityType;
                    Enum.TryParse(activityTypeValue.ToString(), out activityType);
                    await _client.SetGameAsync(name, type: activityType).ConfigureAwait(false);
                    await RespondAsync($"Successfully changed activity to `{activityType} {name}`", ephemeral: true);
                }
                catch (Exception e)
                {
                    await RespondAsync($"Error setting activity:\n```{e}```", ephemeral: true);
                }
            }

            // admin set status
            [SlashCommand("status", "Set the bot's status")]
            public async Task SetStatusAsync([Choice("Online", 1),
                                          Choice("Idle", 2),
                                          Choice("Do Not Disturb", 4)]
                                          [Summary("status", "The status of the bot")] int userStatusValue)
            {
                var owner = Context.Guild.GetUser(OWNER_ID);
                if (Context.User.Id != owner.Id)
                {
                    await RespondAsync($"{X} Sorry, this command can only be used by {FormatUtil.formatUser(owner)}!", ephemeral: true);
                    return;
                }

                try
                {
                    UserStatus userStatus;
                    Enum.TryParse(userStatusValue.ToString(), out userStatus);
                    await _client.SetStatusAsync(userStatus).ConfigureAwait(false);
                    await RespondAsync($"Successfully changed status to `{userStatus}`", ephemeral: true);
                }
                catch (Exception e)
                {
                    await RespondAsync($"Error setting status:\n```{e}```", ephemeral: true);
                }
            }
        }
    }
}

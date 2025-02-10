/*
 * Copyright 2023-2025 Matthew Ring
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Discord.Interactions;
using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.modules.owner;

[Group("admin", "admin commands")]
[CommandContextType(InteractionContextType.Guild)]
[DefaultMemberPermissions(GuildPermission.Administrator)]
[RequireTeam]
public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
    // admin shutdown
    [SlashCommand("shutdown", "Terminates the bot instance")]
    public async Task ShutdownAsync()
    {
        SocketGuildUser owner = Context.Guild.GetUser(OWNER_ID);
        if (Context.User.Id != owner.Id)
        {
            await RespondAsync($"{ERROR} Sorry, this command can only be used by {FormatUtil.formatUser(owner)}!", ephemeral: true);
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
            SocketGuildUser owner = Context.Guild.GetUser(OWNER_ID);
            if (Context.User.Id != owner.Id)
            {
                await RespondAsync($"{ERROR} Sorry, this command can only be used by {FormatUtil.formatUser(owner)}!", ephemeral: true);
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
            SocketGuildUser owner = Context.Guild.GetUser(OWNER_ID);
            if (Context.User.Id != owner.Id)
            {
                await RespondAsync($"{ERROR} Sorry, this command can only be used by {FormatUtil.formatUser(owner)}!", ephemeral: true);
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

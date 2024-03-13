/*
 * Copyright 2023-2024 Matthew Ring
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

using Discord.Commands;
using Discord.WebSocket;

namespace mattbot.modules.moderation;

[RequireContext(ContextType.Guild)]
public class CommandModule : ModuleBase<SocketCommandContext>
{
    [Command("!scoreboard")]
    [Alias("!leaderboard", "!help", "!team", "!archive", "!datasource", "!listarchives", "!listdatasources", "!ping")]
    public async Task CyberScoresCommandsAsync([Remainder] string text = null)
    {
        SocketGuildUser user = Context.User as SocketGuildUser;

        // CyberPatriot only
        if (user.Guild.Id != CYBERPATRIOT_ID)
            return;

        // Ignore moderators
        if (user.GuildPermissions.Has(GuildPermission.BanMembers))
            return;

        // Get channels
        SocketTextChannel vip_commands = Context.Guild.GetTextChannel(1030664143528792084);
        SocketTextChannel comp_results = Context.Guild.GetTextChannel(473981770791124992);
        if (vip_commands == null || comp_results == null)
            return;

        // Warn user
        if (!Context.Channel.Equals(vip_commands) && !Context.Channel.Equals(comp_results))
        {
            StringBuilder builder = new StringBuilder("Please use ");
            if (user.Roles.FirstOrDefault(x => x.Id == 591801562922483712) != null)
            {
                builder.Append(vip_commands.Mention).Append(" or ");
            }
            builder.Append(comp_results.Mention).Append(" for commands.");
            await ReplyAsync(builder.ToString());
        }
    }
}

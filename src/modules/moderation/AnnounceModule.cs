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

using Discord.Interactions;

namespace mattbot.modules.moderation;

[DefaultMemberPermissions(GuildPermission.BanMembers)]
public class AnnounceModule : InteractionModuleBase<SocketInteractionContext>
{
    // announce
    [EnabledInDm(false)]
    [SlashCommand("announce", "Announce a message")]
    public async Task AnnounceAsync([Summary("message", "Announcement message")] string message,
                                    [Summary("channel", "Channel to send the message in. Leave blank for current channel")] ITextChannel channel = null)
    {
        // Channel is current channel
        if (channel == null)
            channel = Context.Channel as ITextChannel;

        // Bot perms
        IGuildUser gUser = await channel.Guild.GetUserAsync(Context.Client.CurrentUser.Id).ConfigureAwait(false);
        ChannelPermissions botPerms = gUser.GetPermissions(channel);
        if (!botPerms.Has(ChannelPermission.SendMessages))
            await RespondAsync($"I do not have permissions to speak in {channel.Mention}!", ephemeral: true);

        try
        {
            await channel.SendMessageAsync(message);
            await RespondAsync($"Announcement published to {channel.Mention}!", ephemeral: true);
        }
        catch (Exception e)
        {
            await RespondAsync($"Error sending announcement:\n```{e}```", ephemeral: true);
        }
    }
}

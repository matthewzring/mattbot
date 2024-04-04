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

using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.automod;

/*
 * Modelled after jagrosh's AutoMod in Vortex
 */
public class AutoMod
{
    public static async Task userJoin(IGuildUser user)
    {
        // completely ignore bots
        if(user.IsBot)
            return;
    }

    public static async Task userLeft(IGuild guild, IUser user)
    {
        // completely ignore bots
        if (user.IsBot)
            return;
    }

    private static bool shouldPerformAutomod(SocketGuildUser member, ISocketMessageChannel channel)
    {
        // ignore users not in the guild
        if(member==null)
            return false;

        // ignore broken guilds
        if(member.Guild.Owner==null)
            return false;

        // ignore bots
        if(member.IsBot)
            return false;

        // ignore users the bot cant interact with
        if(!PermissionUtil.canInteract(member.Guild.CurrentUser, member))
            return false;

        // ignore users that can kick
        if(member.GuildPermissions.Has(GuildPermission.KickMembers))
            return false;

        // ignore users that can ban
        if(member.GuildPermissions.Has(GuildPermission.BanMembers))
            return false;

        // ignore users that can manage server
        if(member.GuildPermissions.Has(GuildPermission.ManageGuild))
            return false;

        // if a channel is specified, ignore users that can manange messages in that channel
        if(channel!=null && member.GetPermissions(channel as IGuildChannel).Has(ChannelPermission.ManageMessages))
            return false;

        return true;
    }
    public static async Task performAutomod(SocketMessage message)
    {
        // ignore users with Manage Messages, Kick Members, Ban Members, Manage Server, or anyone the bot can't interact with
        if (!shouldPerformAutomod((SocketGuildUser)message.Author, message.Channel))
            return;
    }
}

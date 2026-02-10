/*
 * Copyright 2023-2026 Matthew Ring
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
using Color = Discord.Color;

namespace mattbot.modules.general;

[CCDC]
[CyberPatriot]
public class ColorMeModule : InteractionModuleBase<SocketInteractionContext>
{
    // colorme
    [SlashCommand("colorme", "Set the color of your role")]
    public async Task ColorMeAsync([Summary("color", "#hexcode")] string hex)
    {
        SocketGuildUser user = Context.User as SocketGuildUser;
        SocketRole colorRole = null;
        if (user.Roles.FirstOrDefault(x => x.Name == "Nitro Booster") != null || user.GuildPermissions.Has(GuildPermission.BanMembers))
        {
            colorRole = user.Roles.Where(x => x.Name.Equals(user.Username)).FirstOrDefault();
        }
        else if (user.Roles.FirstOrDefault(x => x.Id == 1042648973950849065) != null) // Event Winner
        {
            colorRole = user.Roles.Where(x => x.Id == 1042648973950849065).FirstOrDefault();
        }
        if (colorRole != null)
        {
            await SetRoleColor(colorRole.Id, hex);
        }
        else
        {
            // TODO: better logging and error handling
            await RespondAsync(ERROR_MESSAGE, ephemeral: true);
        }
    }

    private async Task SetRoleColor(ulong roleID, string hex)
    {
        if (hex[0].Equals('#'))
            hex = hex.Remove(0, 1);
        try
        {
            Color dColor = new(Convert.ToUInt32(hex, 16));
            await Context.Guild.GetRole(roleID).ModifyAsync(x => x.Color = dColor);
            await RespondAsync($"The role *{Context.Guild.GetRole(roleID).Name}*'s color was successfully changed to `{dColor}`", ephemeral: true);
        }
        catch (Exception)
        {
            await RespondAsync(ERROR_MESSAGE, ephemeral: true);
        }
    }
}

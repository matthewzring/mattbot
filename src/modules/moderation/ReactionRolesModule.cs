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

namespace mattbot.modules.moderation;

[RequireContext(ContextType.Guild)]
public class ReactionRolesModule : InteractionModuleBase<SocketInteractionContext>
{
    // Handle reaction roles
    [ComponentInteraction("role:*")]
    public async Task HandleReactionRole(ulong roleID)
    {
        string name = Context.Guild.GetRole(roleID).Name;
        SocketGuildUser user = Context.User as SocketGuildUser;
        if (user.Roles.FirstOrDefault(x => x.Id == roleID) == null)
        {
            await user.AddRoleAsync(roleID);
            await RespondAsync($"You will receive the **{name}** role shortly!", ephemeral: true);
        }
        else if (user.Roles.FirstOrDefault(x => x.Id == roleID) != null)
        {
            await user.RemoveRoleAsync(roleID);
            await RespondAsync($"The role **{name}** will be removed shortly!", ephemeral: true);
        }
    }
}

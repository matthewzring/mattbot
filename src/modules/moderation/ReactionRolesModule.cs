using Discord.Interactions;
using Discord.WebSocket;

namespace mattbot.modules.moderation
{
    public class ReactionRolesModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Handle reaction roles
        [RequireContext(ContextType.Guild)]
        [ComponentInteraction("role:*")]
        public async Task HandleReactionRole(ulong roleID)
        {
            var name = Context.Guild.GetRole(roleID).Name;
            var user = Context.User as SocketGuildUser;
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
}

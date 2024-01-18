using Discord.Interactions;
using Discord.WebSocket;
using Color = Discord.Color;

namespace mattbot.modules.general
{
    [CyberPatriot]
    [CCDC]
    public class ColorMeModule : InteractionModuleBase<SocketInteractionContext>
    {
        // colorme
        [EnabledInDm(false)]
        [SlashCommand("colorme", "Set the color of your role")]
        public async Task ColorMeAsync([Summary("color", "#hexcode")] string hex)
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            SocketRole colorRole = null;
            if (user.Roles.FirstOrDefault(x => x.Name == "Nitro Booster") != null || user.GuildPermissions.Has(GuildPermission.BanMembers))
            {
                colorRole = user.Roles.Where(x => x.Name.Equals(user.Username)).FirstOrDefault();
            }
            else if (user.Roles.FirstOrDefault(x => x.Name == "Event Winner") != null)
            {
                colorRole = user.Roles.Where(x => x.Name == "Event Winner").FirstOrDefault();
            }
            if (colorRole != null)
                await SetRoleColor(colorRole.Id, hex);
        }

        public async Task SetRoleColor(ulong roleID, string hex)
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
}

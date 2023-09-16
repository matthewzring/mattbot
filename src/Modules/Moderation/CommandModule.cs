using Discord.Commands;
using Discord.WebSocket;

namespace MattBot.Modules.Moderation
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        [RequireContext(ContextType.Guild)]
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

            // Warn user
            if (Context.Channel.Name != "vip_commands" && Context.Channel.Name != "comp_results")
            {
                StringBuilder builder = new StringBuilder("Please use ");
                if (user.Roles.FirstOrDefault(x => x.Name == "Nitro Booster") != null)
                {
                    SocketGuildChannel vip_commands = Context.Guild.Channels.FirstOrDefault(x => x.Name == "vip_commands");
                    if (vip_commands == null)
                        return;
                    builder.Append("<#").Append(vip_commands.Id).Append("> or ");
                }
                SocketGuildChannel comp_results = Context.Guild.Channels.FirstOrDefault(x => x.Name == "comp_results");
                if (comp_results == null)
                    return;
                builder.Append("<#").Append(comp_results.Id).Append("> for commands.");

                await ReplyAsync(builder.ToString());
            }
        }
    }
}

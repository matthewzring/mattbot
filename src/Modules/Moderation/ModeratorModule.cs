using Discord.Interactions;

namespace MattBot.Modules.Moderation
{
    [EnabledInDm(false)]
    [Group("mod", "Global moderator commands")]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public class ModeratorModule : InteractionModuleBase<SocketInteractionContext>
    {
        // announce
        [SlashCommand("announce", "Announce a message")]
        public async Task AnnounceAsync([Summary("message", "Announcement message")] string message,
                                        [Summary("channel", "Channel to send the message in. Leave blank for current channel")] ITextChannel channel = null)
        {
            // Channel is current channel
            if (channel == null)
                channel = Context.Channel as ITextChannel;

            // Bot perms
            var gUser = await channel.Guild.GetUserAsync(Context.Client.CurrentUser.Id).ConfigureAwait(false);
            var botPerms = gUser.GetPermissions(channel);
            if (!botPerms.Has(ChannelPermission.SendMessages))
                await RespondAsync($"I do not have permissions to speak in {channel.Mention}!", ephemeral: true);

            try
            {
                await channel.SendMessageAsync(message);
                await RespondAsync($"Announcement sent to {channel.Mention}!", ephemeral: true);
            }
            catch (Exception e)
            {
                await RespondAsync($"Error sending announcement:\n```{e}```", ephemeral: true);
            }
        }
    }
}

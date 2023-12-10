using Discord.Interactions;
using Discord.WebSocket;

namespace mattbot.modules.moderation
{
    [CyberPatriot]
    [EnabledInDm(false)]
    [Group("mod", "Global moderator commands")]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public class ModeratorModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const string LOCK = "\uD83D\uDD12"; // 🔒
        private const string UNLOCK = "\uD83D\uDD13"; // 🔓

        private const string LOADING = "<a:loading:1110293618222178324>";

        // mod announce
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

        // mod lockdown
        [Group("lockdown", "Set the server lockdown status")]
        public class LockdownModule : InteractionModuleBase<SocketInteractionContext>
        {
            // mod lockdown toggle
            [SlashCommand("toggle", "Toggle the server lockdown status")]
            public async Task ToggleLockdownAsync([Choice("OFF", 0), 
                                                Choice("ON", 1)]
                                                [Summary("setting", "The setting of lockdown mode")] int lockdownStatusValue)
            {
                switch (lockdownStatusValue)
                {
                    case 0:
                        StringBuilder sb = new StringBuilder($"{SUCCESS} Disabling lockdown mode!\n");
                        await RespondAsync($"{sb} {LOADING} Unhiding channels...");
                        try
                        {
                            SocketRole everyone = Context.Guild.GetRole(301768361136750592);
                            await everyone.ModifyAsync(role => role.Permissions = everyone.Permissions.Modify(viewChannel: true));
                            sb.Append($"{SUCCESS} Channels unhidden!\n");
                            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb} {LOADING} Hoisting roles...\n");
                            SocketRole windowsHelper = Context.Guild.GetRole(514289040242114561);
                            await windowsHelper.ModifyAsync(role => role.Hoist = true);
                            SocketRole linuxHelper = Context.Guild.GetRole(514289361177673728);
                            await linuxHelper.ModifyAsync(role => role.Hoist = true);
                            SocketRole ciscoHelper = Context.Guild.GetRole(514289524612923393);
                            await ciscoHelper.ModifyAsync(role => role.Hoist = true);
                            SocketRole eventWinner = Context.Guild.GetRole(1042648973950849065);
                            await eventWinner.ModifyAsync(role => role.Hoist = true);
                            SocketRole star = Context.Guild.GetRole(1031999100033450014);
                            await star.ModifyAsync(role => role.Hoist = true);
                            SocketRole formerStaff = Context.Guild.GetRole(847962114337275915);
                            await formerStaff.ModifyAsync(role => role.Hoist = true);
                            sb.Append($"{SUCCESS} Roles hoisted!\n");
                            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb} {LOADING} Restoring default permissions...\n");
                            await everyone.ModifyAsync(role => role.Permissions = everyone.Permissions.Modify(sendMessagesInThreads: true,
                                                                                                            createPublicThreads: true,
                                                                                                            createPrivateThreads: true));
                            await windowsHelper.ModifyAsync(role => role.Mentionable = true);
                            await linuxHelper.ModifyAsync(role => role.Mentionable = true);
                            await ciscoHelper.ModifyAsync(role => role.Mentionable = true);
                            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb}{SUCCESS} Default permissions restored!\n\n{UNLOCK} Lockdown mode disabled!");
                        }
                        catch (Exception)
                        {
                            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb} {ERROR} {ERROR_MESSAGE}");
                        }
                        break;
                    case 1:
                        StringBuilder sb2 = new StringBuilder($"{SUCCESS} Enabling lockdown mode!\n");
                        await RespondAsync($"{sb2} {LOADING} Hiding channels...");
                        try
                        {
                            SocketRole everyone = Context.Guild.GetRole(301768361136750592);
                            await everyone.ModifyAsync(role => role.Permissions = everyone.Permissions.Modify(viewChannel: false));
                            sb2.Append($"{SUCCESS} Channels hidden!\n");
                            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb2} {LOADING} Dehoisting roles...\n");
                            SocketRole windowsHelper = Context.Guild.GetRole(514289040242114561);
                            await windowsHelper.ModifyAsync(role => role.Hoist = false);
                            SocketRole linuxHelper = Context.Guild.GetRole(514289361177673728);
                            await linuxHelper.ModifyAsync(role => role.Hoist = false);
                            SocketRole ciscoHelper = Context.Guild.GetRole(514289524612923393);
                            await ciscoHelper.ModifyAsync(role => role.Hoist = false);
                            SocketRole eventWinner = Context.Guild.GetRole(1042648973950849065);
                            await eventWinner.ModifyAsync(role => role.Hoist = false);
                            SocketRole star = Context.Guild.GetRole(1031999100033450014);
                            await star.ModifyAsync(role => role.Hoist = false);
                            SocketRole formerStaff = Context.Guild.GetRole(847962114337275915);
                            await formerStaff.ModifyAsync(role => role.Hoist = false);
                            sb2.Append($"{SUCCESS} Roles dehoisted!\n");
                            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb2} {LOADING} Removing default permissions...\n");
                            await everyone.ModifyAsync(role => role.Permissions = everyone.Permissions.Modify(sendMessagesInThreads: false,
                                                                                                            createPublicThreads: false,
                                                                                                            createPrivateThreads: false));
                            await windowsHelper.ModifyAsync(role => role.Mentionable = false);
                            await linuxHelper.ModifyAsync(role => role.Mentionable = false);
                            await ciscoHelper.ModifyAsync(role => role.Mentionable = false);
                            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb2}{SUCCESS} Default permissions removed!\n\n{LOCK} Lockdown mode enabled!");
                        } 
                        catch (Exception)
                        {
                            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb2} {ERROR} {ERROR_MESSAGE}");
                        }
                        break;
                }
            }
        }
    }
}

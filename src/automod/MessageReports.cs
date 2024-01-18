using mattbot.services;
using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.automod
{
    public class MessageReports
    {
        private readonly DiscordSocketClient _client;
        private readonly Listener _listener;
        private readonly IConfiguration _configuration;

        public MessageReports(DiscordSocketClient client, Listener listener, IConfiguration configuration)
        {
            _client = client;
            _listener = listener;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            _listener.ReactionAdded += OnReactionAddedAsync;
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            int.TryParse(_configuration["message_report_threshold"], out int MESSAGE_REPORT_THRESHOLD);
            int.TryParse(_configuration["message_report_duration"], out int MESSAGE_REPORT_DURATION);
            string MESSAGE_REPORT_EMOJI = _configuration["message_report_emoji"];

            if (!reaction.User.IsSpecified
                || reaction.User.Value.IsBot
                || !channel.HasValue
                || channel.Value is not ITextChannel textChannel
                || MESSAGE_REPORT_THRESHOLD == 0)
            {
                return;
            }

            IGuild guild = textChannel.Guild;

            // Look for a channel called modlog
            ITextChannel tc = (await guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == "modlog");
            if (tc == null)
                return;

            IUserMessage newMessage;
            if (!message.HasValue)
                newMessage = await message.GetOrDownloadAsync().ConfigureAwait(false);
            else
                newMessage = message.Value;

            // Reaction emoji
            if (!Equals(reaction.Emote, new Emoji(MESSAGE_REPORT_EMOJI)))
                return;

            // Ignore system messages
            if (newMessage == null)
                return;

            // Bot perms
            IGuildUser gUser = await guild.GetUserAsync(_client.CurrentUser.Id).ConfigureAwait(false);
            ChannelPermissions botPerms = gUser.GetPermissions(textChannel);
            if (!botPerms.Has(ChannelPermission.ManageMessages))
                return;

            // Ignore moderators & bots
            if ((newMessage.Author is IGuildUser user && user.GuildPermissions.Has(GuildPermission.BanMembers)) || newMessage.Author.IsBot)
                return;

            // Message is within the allowed duration
            if ((newMessage.Timestamp - DateTimeOffset.UtcNow).TotalHours <= -MESSAGE_REPORT_DURATION)
                return;

            // Check if message is replying to someone
            StringBuilder builder = new StringBuilder();
            if (newMessage.Reference is not null)
                builder.Append(newMessage.ReferencedMessage.Author.Mention).Append("\n");

            // Get the contents of the message
            string content;
            string imageurl;

            builder.Append(newMessage.Content);
            imageurl = newMessage.Attachments?.FirstOrDefault()?.ProxyUrl;

            content = builder.ToString();
            if (content is null && imageurl is null)
                return;

            // Look for a role called "No Reports"
            IRole noReports = guild.Roles.FirstOrDefault(role => role.Name == "No Reports");

            // Count all valid reactions
            // A reaction is considered "valid" if the user who reacted is not a bot, the message author, or prohibited from reacting
            int count = 0;
            IAsyncEnumerable<IReadOnlyCollection<IUser>> users = newMessage.GetReactionUsersAsync(reaction.Emote, int.MaxValue);
            StringBuilder rlist = new StringBuilder();
            await foreach (IReadOnlyCollection<IUser> chunk in users)
            {
                foreach (IUser reactuser in chunk)
                {
                    IGuildUser guilduser = await guild.GetUserAsync(reactuser.Id);
                    if ((noReports is null || !guilduser.RoleIds.Contains(noReports.Id)) && !reactuser.IsBot && reactuser.Id != newMessage.Author.Id)
                    {
                        count++;
                        rlist.Append(FormatUtil.formatFullUser(reactuser)).Append(", ");
                    }
                }
            }
            rlist.Length -= 2;
            if (count < MESSAGE_REPORT_THRESHOLD)
                return;

            EmbedBuilder eb = new EmbedBuilder().WithColor(0xFF0000).WithDescription(content + $"\n\n[Jump Link]({newMessage.GetJumpUrl()})");

            if (imageurl is not null)
                eb.WithImageUrl(imageurl);

            try
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                await Logger.Log(now, tc, MESSAGE_REPORT_EMOJI, $"{FormatUtil.formatFullUser(newMessage.Author)}'s message was reported in {textChannel.Mention} by:\n\n{rlist}", eb.Build());

                // Delete the message
                await newMessage.DeleteAsync();
            }
            catch (Exception) { }
        }
    }
}

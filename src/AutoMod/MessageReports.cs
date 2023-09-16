using MattBot.Services;
using Discord.WebSocket;
using MattBot.Utilities;

namespace MattBot.AutoMod
{
    public class MessageReports
    {
        private readonly DiscordSocketClient _client;
        private readonly Listener _listener;

        public MessageReports(DiscordSocketClient client, Listener listener)
        {
            _client = client;
            _listener = listener;
        }

        public async Task InitializeAsync()
        {
            _listener.ReactionAdded += OnReactionAddedAsync;
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (!reaction.User.IsSpecified
                || reaction.User.Value.IsBot
                || !channel.HasValue
                || channel.Value is not ITextChannel textChannel
                || MESSAGE_REPORT_THRESHOLD == 0)
            {
                return;
            }

            // Look for a channel called botlog
            ITextChannel tc = (await textChannel.Guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == "botlog");
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
            var gUser = await textChannel.Guild.GetUserAsync(_client.CurrentUser.Id).ConfigureAwait(false);
            var botPerms = gUser.GetPermissions(textChannel);
            if (!botPerms.Has(ChannelPermission.ManageMessages))
                return;

            // Ignore moderators & bots
            if ((newMessage.Author is IGuildUser user && user.GuildPermissions.Has(GuildPermission.BanMembers)) || newMessage.Author.IsBot)
                return;

            // Message is within the allowed duration
            if ((newMessage.Timestamp - DateTimeOffset.UtcNow).TotalHours <= -MESSAGE_REPORT_DURATION)
                return;

            // Get the contents of the message
            string content;
            string imageurl;

            content = newMessage.Content;
            imageurl = newMessage.Attachments?.FirstOrDefault()?.ProxyUrl;

            if (content is null && imageurl is null)
                return;

            // Count reactions
            var emoteCount = await newMessage.GetReactionUsersAsync(reaction.Emote, int.MaxValue).FlattenAsync().ConfigureAwait(false);
            var count = emoteCount.Where(x => !x.IsBot).Where(x => x.Id != newMessage.Author.Id);
            var enumerable = count as IUser[] ?? count.ToArray();
            if (enumerable.Length < MESSAGE_REPORT_THRESHOLD)
                return;

            // Get a list of users that reacted
            IAsyncEnumerable<IReadOnlyCollection<IUser>> users = newMessage.GetReactionUsersAsync(reaction.Emote, int.MaxValue);
            StringBuilder rlist = new StringBuilder();
            await foreach (IReadOnlyCollection<IUser> chunk in users)
                foreach (IUser reactuser in chunk)
                    if (reactuser.Id != newMessage.Author.Id)
                        rlist.Append("- ").Append(FormatUtil.formatFullUser(reactuser)).Append('\n');

            var eb = new EmbedBuilder().WithColor(0xFF0000).WithDescription(content + $"\n\n[Jump Link]({newMessage.GetJumpUrl()})");

            if (imageurl is not null)
                eb.WithImageUrl(imageurl);

            var now = DateTimeOffset.UtcNow;
            await Logger.Log(now, tc, MESSAGE_REPORT_EMOJI, $"{FormatUtil.formatFullUser(newMessage.Author)}'s message was reported in {textChannel.Mention} by:\n\n{rlist}", eb.Build());

            // Delete the message
            await newMessage.DeleteAsync();
        }
    }
}

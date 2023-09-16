using mattbot.services;
using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.automod
{
    public class CrowdMute
    {
        private readonly DiscordSocketClient _client;
        private readonly Listener _listener;

        public CrowdMute(DiscordSocketClient client, Listener listener)
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
                || CROWD_MUTE_THRESHOLD == 0)
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

            // Reaction emote
            if (!Equals(reaction.Emote, Emote.Parse(CROWD_MUTE_EMOJI)))
                return;

            // Ignore system messages
            if (newMessage == null)
                return;

            // Bot perms
            var gUser = await textChannel.Guild.GetUserAsync(_client.CurrentUser.Id).ConfigureAwait(false);
            if (!gUser.GuildPermissions.ModerateMembers)
                return;

            // Ignore moderators & bots
            if ((newMessage.Author is IGuildUser user && user.GuildPermissions.Has(GuildPermission.BanMembers)) || newMessage.Author.IsBot)
                return;

            // Message is within the allowed duration
            if ((newMessage.Timestamp - DateTimeOffset.UtcNow).TotalMinutes <= -CROWD_MUTE_DURATION)
                return;

            // User is already timed out
            if (((newMessage.Author as IGuildUser).TimedOutUntil != null) && !(((newMessage.Author as IGuildUser).TimedOutUntil - DateTimeOffset.UtcNow).ToString()[0].Equals("-")))
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
            if (enumerable.Length < CROWD_MUTE_THRESHOLD)
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
            await Logger.Log(now, tc, CROWD_MUTE_EMOJI, $"{FormatUtil.formatFullUser(newMessage.Author)} has been crowd muted for {CROWD_MUTE_DURATION} minutes in {textChannel.Mention} by:\n\n{rlist}", eb.Build());

            // Timeout the user
            TimeSpan interval = new TimeSpan(0, CROWD_MUTE_DURATION, 0);
            await (newMessage.Author as IGuildUser).SetTimeOutAsync(span: interval);

            // Let everyone know the user has been timed out
            await newMessage.ReplyAsync($"This user has been crowd muted for {CROWD_MUTE_DURATION} minutes.");
        }
    }
}

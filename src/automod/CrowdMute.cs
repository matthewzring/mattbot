using mattbot.services;
using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.automod
{
    public class CrowdMute
    {
        private readonly DiscordSocketClient _client;
        private readonly Listener _listener;
        private readonly IConfiguration _configuration;

        public CrowdMute(DiscordSocketClient client, Listener listener, IConfiguration configuration)
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
            int.TryParse(_configuration["crowd_mute_threshold"], out int CROWD_MUTE_THRESHOLD);
            int.TryParse(_configuration["crowd_mute_duration"], out int CROWD_MUTE_DURATION);
            string CROWD_MUTE_EMOJI = _configuration["crowd_mute_emoji"];

            if (!reaction.User.IsSpecified
                || reaction.User.Value.IsBot
                || !channel.HasValue
                || channel.Value is not ITextChannel textChannel
                || CROWD_MUTE_THRESHOLD == 0)
            {
                return;
            }

            // CyberPatriot only
            IGuild guild = textChannel.Guild;
            if (guild.Id is not CYBERPATRIOT_ID)
                return;

            // Look for a channel called modlog
            ITextChannel tc = (await guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == "modlog");
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
            IGuildUser gUser = await guild.GetUserAsync(_client.CurrentUser.Id).ConfigureAwait(false);
            if (!gUser.GuildPermissions.ModerateMembers)
                return;

            // Ignore moderators & bots
            if ((newMessage.Author is IGuildUser user && user.GuildPermissions.Has(GuildPermission.BanMembers)) || newMessage.Author.IsBot)
                return;

            // Message is within the allowed duration
            if ((newMessage.Timestamp - DateTimeOffset.UtcNow).TotalMinutes <= -CROWD_MUTE_DURATION)
                return;

            // User is already timed out
            if (((newMessage.Author as IGuildUser).TimedOutUntil != null) && !((newMessage.Author as IGuildUser).TimedOutUntil - DateTimeOffset.UtcNow).ToString()[0].Equals('-'))
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

            // Look for a role called "No Crowdmute"
            IRole noCrowdmute = guild.Roles.FirstOrDefault(role => role.Name == "No Crowdmute");

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
                    if ((noCrowdmute is null || !guilduser.RoleIds.Contains(noCrowdmute.Id)) && !reactuser.IsBot && reactuser.Id != newMessage.Author.Id)
                    {
                        count++;
                        rlist.Append(FormatUtil.formatFullUser(reactuser)).Append(", ");
                    }
                }
            }
            rlist.Length -= 2;
            if (count < CROWD_MUTE_THRESHOLD)
                return;

            EmbedBuilder eb = new EmbedBuilder().WithColor(0xFF0000).WithDescription(content + $"\n\n[Jump Link]({newMessage.GetJumpUrl()})");

            if (imageurl is not null)
                eb.WithImageUrl(imageurl);

            try
            {
                // Timeout the user
                TimeSpan interval = new TimeSpan(0, CROWD_MUTE_DURATION, 0);
                RequestOptions reason = new RequestOptions { AuditLogReason = $"Crowd muted" };
                await (newMessage.Author as IGuildUser).SetTimeOutAsync(interval, reason);

                DateTimeOffset now = DateTimeOffset.UtcNow;
                await Logger.Log(now, tc, CROWD_MUTE_EMOJI, $"{FormatUtil.formatFullUser(newMessage.Author)} was crowd muted for {CROWD_MUTE_DURATION} minutes in {textChannel.Mention} by:\n\n{rlist}", eb.Build());

                // Let everyone know the user has been timed out
                await newMessage.ReplyAsync($"This user has been crowd muted for {CROWD_MUTE_DURATION} minutes.");
            }
            catch (Exception) { } 
        }
    }
}

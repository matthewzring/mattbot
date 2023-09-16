using Discord.WebSocket;
using MattBot.Utilities;

namespace MattBot.Logging
{
    public class Logger
    {
        private const string NAME = "\uD83D\uDCDB"; // 📛
        private const string JOIN = "\uD83D\uDCE5"; // 📥
        private const string NEW = "\uD83C\uDD95"; // 🆕
        private const string LEAVE = "\uD83D\uDCE4"; // 📤

        public static async Task Log(DateTimeOffset now, ITextChannel tc, string emote, string message, Embed embed)
        {
            try
            {
                await tc.SendMessageAsync(FormatUtil.filterEveryone(LogUtil.LogFormat(now, emote, message)), embed: embed);
            }
            catch (Exception) { }
        }

        public static async Task LogNameChange(SocketUser arg1, SocketUser arg2)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (var guild in arg2.MutualGuilds)
            {
                ITextChannel tc = guild.TextChannels.FirstOrDefault(x => x.Name == "serverlog");
                if (tc != null)
                {
                    await Log(now, tc, NAME, $"{FormatUtil.formatFullUser(arg1)} has changed names to {FormatUtil.formatUser(arg2)}", null);
                }
            }
        }

        public static async Task LogGuildJoin(IGuildUser arg)
        {
            ITextChannel tc = (await arg.Guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == "serverlog");
            if (tc == null)
                return;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            long seconds = (long)(now - arg.CreatedAt).TotalSeconds;
            await Log(now, tc, JOIN, $"{FormatUtil.formatFullUser(arg)} joined the server. "
                    +(seconds < 16*60 ? NEW : "")
                    +$"\nCreation: {arg.CreatedAt:R} ({FormatUtil.secondsToTimeCompact(seconds)} ago)", null);
        }

        public static async Task LogGuildLeave(IGuild arg1, IUser arg2)
        {
            ITextChannel tc = (await arg1.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == "serverlog");
            if (tc == null)
                return;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string msg = $"{FormatUtil.formatFullUser(arg2)} left or was kicked from the server.";
            SocketGuildUser user = arg2 as SocketGuildUser;
            if (user != null)
            {
                long seconds = (long)(now - user.JoinedAt).Value.TotalSeconds;
                StringBuilder rlist;
                if (!user.Roles.Skip(1).Any()) // skip @everyone
                    rlist = new StringBuilder();
                else
                {
                    rlist = new StringBuilder($"\nRoles: `{user.Roles.Skip(1).First()}");
                    foreach (var role in user.Roles.Skip(2))
                        rlist.Append("`, `").Append(role.Name);
                    rlist.Append('`');
                }
                msg += $"\nJoined: {user.JoinedAt.Value:R} ({FormatUtil.secondsToTimeCompact(seconds)} ago) {rlist}";
            }
            await Log(now, tc, LEAVE, msg, null);
        }

        public static async Task LogVoiceJoin(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            ITextChannel tc = arg3.VoiceChannel.Guild.Channels.FirstOrDefault(x => x.Name == "serverlog") as ITextChannel;
            if (tc == null)
                return;
            await Log(DateTimeOffset.UtcNow, tc, VOICE_JOIN, $"{FormatUtil.formatFullUser(arg1)} has joined voice channel _{arg3.VoiceChannel.Name}_", null);
        }

        public static async Task LogVoiceMove(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            ITextChannel tc = arg3.VoiceChannel.Guild.Channels.FirstOrDefault(x => x.Name == "serverlog") as ITextChannel;
            if (tc == null)
                return;
            await Log(DateTimeOffset.UtcNow, tc, VOICE_CHANGE, $"{FormatUtil.formatFullUser(arg1)} has moved voice channels from _{arg2.VoiceChannel.Name}_ to _{arg3.VoiceChannel.Name}_", null);
        }

        public static async Task LogVoiceLeave(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            ITextChannel tc = arg2.VoiceChannel.Guild.Channels.FirstOrDefault(x => x.Name == "serverlog") as ITextChannel;
            if (tc == null)
                return;
            await Log(DateTimeOffset.UtcNow, tc, VOICE_LEAVE, $"{FormatUtil.formatFullUser(arg1)} has left voice channel _{arg2.VoiceChannel.Name}_", null);
        }

        public static async Task LogModmailReceived(DiscordSocketClient client, SocketMessage message)
        {
            string content = message.Content;
            string imageurl = message.Attachments?.FirstOrDefault()?.ProxyUrl;
            if (content is null && imageurl is null)
                return;
            StringBuilder builder = new StringBuilder();
            builder.Append(message.Author.Username);
            if (message.Author.Discriminator != "0000")
            {
                builder.Append("#").Append(message.Author.Discriminator);
            }
            string author = builder.ToString();
            var eb = new EmbedBuilder().WithAuthor(author, message.Author.GetAvatarUrl()).WithColor(0xFF0000).WithDescription(content).WithTimestamp(message.Timestamp);
            if (imageurl is not null)
                eb.WithImageUrl(imageurl);
            ITextChannel tc = client.GetGuild(TESTING_ID)?.TextChannels.FirstOrDefault(x => x.Name == "botlog");
            if (tc == null)
                return;
            await tc.SendMessageAsync(embed: eb.Build());
        }
    }
}

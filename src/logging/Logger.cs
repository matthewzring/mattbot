/*
 * Copyright 2023-2024 Matthew Ring
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.logging;

/**
 * Modelled after jagrosh's BasicLogger in Vortex
 */
public class Logger
{
    private readonly string NAME = "\uD83D\uDCDB"; // 📛
    private readonly string JOIN = "\uD83D\uDCE5"; // 📥
    private readonly string NEW = "\uD83C\uDD95"; // 🆕
    private readonly string LEAVE = "\uD83D\uDCE4"; // 📤

    private readonly string VOICE_JOIN = "<:voicejoin:1110632369414742046>";
    private readonly string VOICE_LEAVE = "<:voiceleave:1110632368156463246>";
    private readonly string VOICE_CHANGE = "<:voicechange:1110632371495129098>";

    private readonly MattBot mattbot;

    public Logger (MattBot mattbot)
    {
        this.mattbot = mattbot;
    }

    // todo
    public static async Task Log(DateTimeOffset now, ITextChannel tc, string emote, string message, Embed embed)
    {
        try
        {
            await tc.SendMessageAsync(FormatUtil.filterEveryone(LogUtil.LogFormat(now, emote, message)), embed: embed);
        }
        catch (Exception) { }
    }

    public async Task LogNameChange(SocketUser arg1, SocketUser arg2)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (SocketGuild guild in arg2.MutualGuilds)
        {
            ITextChannel tc = guild.TextChannels.FirstOrDefault(x => x.Name == "serverlog");
            if (tc != null)
            {
                await Log(now, tc, NAME, $"{FormatUtil.formatFullUser(arg1)} has changed names to {FormatUtil.formatUser(arg2)}", null);
            }
        }
    }

    public async Task LogGuildJoin(IGuildUser arg)
    {
        ITextChannel tc = arg.Guild.GetTextChannelsAsync().Result.FirstOrDefault(x => x.Name == "serverlog");
        if (tc == null)
            return;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        long seconds = (long)(now - arg.CreatedAt).TotalSeconds;
        await Log(now, tc, JOIN, $"{FormatUtil.formatFullUser(arg)} joined the server. "
                +(seconds < 16*60 ? NEW : "")
                +$"\nCreation: {arg.CreatedAt:R} ({FormatUtil.secondsToTimeCompact(seconds)} ago)", null);
    }

    public async Task LogGuildLeave(IGuild arg1, IUser arg2)
    {
        ITextChannel tc = arg1.GetTextChannelsAsync().Result.FirstOrDefault(x => x.Name == "serverlog");
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
                foreach (SocketRole role in user.Roles.Skip(2))
                    rlist.Append("`, `").Append(role.Name);
                rlist.Append('`');
            }
            msg += $"\nJoined: {user.JoinedAt.Value:R} ({FormatUtil.secondsToTimeCompact(seconds)} ago) {rlist}";
        }
        await Log(now, tc, LEAVE, msg, null);
    }

    public async Task LogVoiceJoin(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
    {
        ITextChannel tc = arg3.VoiceChannel.Guild.Channels.FirstOrDefault(x => x.Name == "serverlog") as ITextChannel;
        if (tc == null)
            return;
        await Log(DateTimeOffset.UtcNow, tc, VOICE_JOIN, $"{FormatUtil.formatFullUser(arg1)} has joined voice channel _{arg3.VoiceChannel.Name}_", null);
    }

    public async Task LogVoiceMove(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
    {
        ITextChannel tc = arg3.VoiceChannel.Guild.Channels.FirstOrDefault(x => x.Name == "serverlog") as ITextChannel;
        if (tc == null)
            return;
        await Log(DateTimeOffset.UtcNow, tc, VOICE_CHANGE, $"{FormatUtil.formatFullUser(arg1)} has moved voice channels from _{arg2.VoiceChannel.Name}_ to _{arg3.VoiceChannel.Name}_", null);
    }

    public async Task LogVoiceLeave(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
    {
        ITextChannel tc = arg2.VoiceChannel.Guild.Channels.FirstOrDefault(x => x.Name == "serverlog") as ITextChannel;
        if (tc == null)
            return;
        await Log(DateTimeOffset.UtcNow, tc, VOICE_LEAVE, $"{FormatUtil.formatFullUser(arg1)} has left voice channel _{arg2.VoiceChannel.Name}_", null);
    }

    public async Task LogMessageReceived(DiscordSocketClient client, SocketMessage message)
    {
        string content = message.Content;
        string imageurl = message.Attachments?.FirstOrDefault()?.ProxyUrl;
        if (content is null && imageurl is null)
            return;
        StringBuilder builder = new StringBuilder();
        builder.Append(message.Author.Username);
        if (message.Author.Discriminator != "0000")
            builder.Append("#").Append(message.Author.Discriminator);
        string author = builder.ToString();
        EmbedBuilder eb = new EmbedBuilder().WithAuthor(author, message.Author.GetAvatarUrl()).WithColor(0xFF0000).WithDescription(content).WithTimestamp(message.Timestamp);
        if (imageurl is not null)
            eb.WithImageUrl(imageurl);
        ITextChannel tc = client.GetGuild(MATTLOUNGE_ID)?.GetTextChannel(1094113944714608680);
        if (tc == null)
            return;
        await tc.SendMessageAsync(embed: eb.Build());
    }
}

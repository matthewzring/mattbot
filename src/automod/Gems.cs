using Discord.WebSocket;
using mattbot.services;
using Color = Discord.Color;

namespace mattbot.automod;

public class Gems
{
    private readonly DiscordSocketClient _client;
    private readonly Listener _listener;
    private readonly IConfiguration _configuration;

    public Gems(DiscordSocketClient client, Listener listener, IConfiguration configuration)
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
        int.TryParse(_configuration["gem_threshold"], out int GEM_THRESHOLD);
        int.TryParse(_configuration["gem_duration"], out int GEM_DURATION);

        if (!reaction.User.IsSpecified
            || reaction.User.Value.IsBot
            || !channel.HasValue
            || channel.Value is not ITextChannel textChannel
            || GEM_THRESHOLD == 0)
        {
            return;
        }

        IGuild guild = textChannel.Guild;

        // Look for a channel called gems
        ITextChannel gemChannel = (await guild.GetTextChannelsAsync())?.FirstOrDefault(x => x.Name == "gems");
        if (gemChannel == null)
            return;

        IUserMessage newMessage;
        if (!message.HasValue)
            newMessage = await message.GetOrDownloadAsync().ConfigureAwait(false);
        else
            newMessage = message.Value;

        // Reaction emoji
        Emoji gem = new("\uD83D\uDC8E"); // 💎
        if (!Equals(reaction.Emote, gem))
            return;

        // Ignore system messages
        if (newMessage == null)
            return;

        // Blacklisted channels
        if (newMessage.Channel.Name.Contains("gems") || newMessage.Channel.Name.Contains("announcements"))
            return;

        // Bot perms
        IGuildUser gUser = await guild.GetUserAsync(_client.CurrentUser.Id).ConfigureAwait(false);
        ChannelPermissions botPerms = gUser.GetPermissions(gemChannel);
        if (!botPerms.Has(ChannelPermission.SendMessages))
            return;

        // Message is within the allowed duration
        if ((newMessage.Timestamp - DateTimeOffset.UtcNow).TotalHours <= -GEM_DURATION)
            return;

        // Check if message is replying to someone
        StringBuilder builder = new StringBuilder();
        if (newMessage.Reference is not null)
            builder.Append(newMessage.ReferencedMessage.Author.Mention).Append(" ");

        // Get the contents of the message
        string content;
        string imageurl;
        if (newMessage.Author.IsBot)
        {
            builder.Append(newMessage.Embeds.Count > 0 ? newMessage.Embeds.Select(x => x.Description).FirstOrDefault() : newMessage.Content);
            imageurl = newMessage.Attachments.Count > 0
                ? newMessage.Attachments.FirstOrDefault().ProxyUrl
                : newMessage.Embeds?.Select(x => x.Image).FirstOrDefault()?.ProxyUrl;
        }
        else
        {
            builder.Append(newMessage.Content);
            imageurl = newMessage.Attachments?.FirstOrDefault()?.ProxyUrl;
        }
        content = builder.ToString();
        if (content is null && imageurl is null)
            return;

        // Look for a role called "No Gems"
        IRole noGems = guild.Roles.FirstOrDefault(role => role.Name == "No Gems");

        // Make sure the author does not have the "No Gems" role
        if (newMessage.Author is IGuildUser user && user.RoleIds.Contains(noGems.Id))
            return;

        // Count all valid reactions
        // A reaction is considered "valid" if the user who reacted is not a bot, the message author, or prohibited from reacting
        int count = 0;
        IAsyncEnumerable<IReadOnlyCollection<IUser>> users = newMessage.GetReactionUsersAsync(reaction.Emote, int.MaxValue);
        await foreach (IReadOnlyCollection<IUser> chunk in users)
        {
            foreach (IUser reactuser in chunk)
            {
                IGuildUser guilduser = await guild.GetUserAsync(reactuser.Id);
                if ((noGems is null || !guilduser.RoleIds.Contains(noGems.Id)) && !reactuser.IsBot && reactuser.Id != newMessage.Author.Id)
                    count++;
            }
        }
        if (count < GEM_THRESHOLD)
            return;

        // Grabs the highest role and the color for it
        Color userHighestRoleColor = Color.Default;
        if (newMessage.Author is SocketGuildUser guildUser)
        {
            // Filters out any roles that are default color
            IEnumerable<SocketRole> filterOutDefault = guildUser.Roles.Where(r => r.Color != Color.Default);

            if (!filterOutDefault.Count().Equals(0))
                userHighestRoleColor = filterOutDefault.MaxBy(r => r.Position).Color;
        }

        // Workaround for new username system
        StringBuilder author = new StringBuilder();
        author.Append(newMessage.Author.Username);
        if (newMessage.Author.Discriminator != "0000")
            author.Append("#").Append(newMessage.Author.Discriminator);

        EmbedBuilder eb = new EmbedBuilder().WithAuthor(author.ToString(), newMessage.Author.GetAvatarUrl())
                                .WithColor(userHighestRoleColor)
                                .WithDescription(content + $"\n\n[Jump Link]({newMessage.GetJumpUrl()})")
                                .WithTimestamp(newMessage.Timestamp);

        if (imageurl is not null)
            eb.WithImageUrl(imageurl);

        // Check if the message has already been gemmed
        IEnumerable<IMessage> gemposts = await gemChannel.GetMessagesAsync(10).FlattenAsync().ConfigureAwait(false);
        foreach (IMessage gempost in gemposts)
            if (gempost.Content.Contains(message.Id.ToString()))
                return;

        // Post the message
        IUserMessage msg = await gemChannel.SendMessageAsync($"{gem} {textChannel.Mention} `{message.Id}` {gem}", embed: eb.Build()).ConfigureAwait(false);
        await msg.AddReactionAsync(reaction.Emote);
    }
}

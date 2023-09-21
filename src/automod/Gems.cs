﻿using Discord.WebSocket;
using mattbot.services;
using Color = Discord.Color;

namespace mattbot.automod
{
    public class Gems
    {
        private readonly DiscordSocketClient _client;
        private readonly Listener _listener;

        public Gems(DiscordSocketClient client, Listener listener)
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
                || GEM_THRESHOLD == 0)
            {
                return;
            }

            // Look for a channel called gems
            var gemChannel = (await textChannel.Guild.GetTextChannelsAsync())?.FirstOrDefault(x => x.Name == "gems");
            if (gemChannel == null)
                return;

            IUserMessage newMessage;
            if (!message.HasValue)
                newMessage = await message.GetOrDownloadAsync().ConfigureAwait(false);
            else
                newMessage = message.Value;

            // Reaction emoji
            if (!Equals(reaction.Emote, new Emoji(GEM_EMOJI)))
                return;

            // Ignore system messages
            if (newMessage == null)
                return;

            // Blacklisted channels
            if (newMessage.Channel.Name.Contains("gems") || newMessage.Channel.Name.Contains("announcements"))
                return;

            // Bot perms
            var gUser = await textChannel.Guild.GetUserAsync(_client.CurrentUser.Id).ConfigureAwait(false);
            var botPerms = gUser.GetPermissions(gemChannel);
            if (!botPerms.Has(ChannelPermission.SendMessages))
                return;

            // Message is within the allowed duration
            if ((newMessage.Timestamp - DateTimeOffset.UtcNow).TotalHours <= -GEM_DURATION)
                return;

            // Message has already been gemmed
            IEnumerable<IMessage> gemposts = await gemChannel.GetMessagesAsync(10).FlattenAsync().ConfigureAwait(false);
            foreach (IMessage gempost in gemposts)
                if (gempost.Content.Contains(message.Id.ToString()))
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

            // Count reactions
            var emoteCount = await newMessage.GetReactionUsersAsync(reaction.Emote, int.MaxValue).FlattenAsync().ConfigureAwait(false);
            var count = emoteCount.Where(x => !x.IsBot).Where(x => x.Id != newMessage.Author.Id);
            var enumerable = count as IUser[] ?? count.ToArray();
            if (enumerable.Length < GEM_THRESHOLD)
                return;

            // Grabs the highest role and the color for it
            Color userHighestRoleColor = Color.Default;
            if (newMessage.Author is SocketGuildUser guildUser)
            {
                // Filters out any roles that are default color
                var filterOutDefault = guildUser.Roles.Where(r => r.Color != Color.Default);

                if (!filterOutDefault.Count().Equals(0))
                    userHighestRoleColor = filterOutDefault.MaxBy(r => r.Position).Color;
            }

            // Workaround for new username system
            StringBuilder author = new StringBuilder();
            author.Append(newMessage.Author.Username);
            if (newMessage.Author.Discriminator != "0000")
                author.Append("#").Append(newMessage.Author.Discriminator);

            var eb = new EmbedBuilder().WithAuthor(author.ToString(), newMessage.Author.GetAvatarUrl())
                                    .WithColor(userHighestRoleColor)
                                    .WithDescription(content + $"\n\n[Jump Link]({newMessage.GetJumpUrl()})")
                                    .WithTimestamp(newMessage.Timestamp);

            if (imageurl is not null)
                eb.WithImageUrl(imageurl);

            var msg = await gemChannel.SendMessageAsync($"{GEM_EMOJI} {textChannel.Mention} `{message.Id}` {GEM_EMOJI}", embed: eb.Build()).ConfigureAwait(false);
            await msg.AddReactionAsync(reaction.Emote);
        }
    }
}
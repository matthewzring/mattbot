using Discord.WebSocket;

namespace MattBot.Services
{
    public class Listener
    {
        private readonly DiscordSocketClient _client;

        public delegate Task AsyncListener<in TEventArgs>(TEventArgs args);

        public delegate Task AsyncListener<in TEventArgs, in TArgs>(TEventArgs args, TArgs arsg2);

        public delegate Task AsyncListener<in TEventArgs, in TArgs, in TEvent>(TEventArgs args, TArgs args2, TEvent args3);
        public delegate Task AsyncListener<in TEventArgs, in TArgs, in TEvent, in TArgs2>(TEventArgs args, TArgs args2, TEvent args3, TArgs2 args4);

        public event AsyncListener<SocketMessage>? MessageReceived;
        public event AsyncListener<IGuildUser>? UserJoined;
        public event AsyncListener<IGuild, IUser>? UserLeft;
        public event AsyncListener<SocketUser, SocketUser>? UserUpdated;
        public event AsyncListener<Cacheable<SocketGuildUser, ulong>, SocketGuildUser>? GuildMemberUpdated;
        public event AsyncListener<SocketUser, SocketVoiceState, SocketVoiceState>? UserVoiceStateUpdated;
        public event AsyncListener<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction>? ReactionAdded;

        public Listener(DiscordSocketClient client)
        {
            _client = client;
            client.MessageReceived += ClientOnMessageReceived;
            client.UserJoined += ClientOnUserJoined;
            client.UserLeft += ClientOnUserLeft;
            client.UserUpdated += ClientOnUserUpdated;
            client.GuildMemberUpdated += ClientOnGuildMemberUpdated;
            client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;
            client.ReactionAdded += ClientOnReactionAdded;
        }

        private async Task ClientOnMessageReceived(SocketMessage arg)
        {
            if (MessageReceived is not null)
                _ = MessageReceived(arg);

            if (!arg.Author.IsBot) // ignore bot messages
            {
                if (arg.Channel is IPrivateChannel)
                {
                    // Log the modmail received
                    await Logger.LogModmailReceived(_client, arg);
                }
            }
        }

        private async Task ClientOnUserJoined(IGuildUser arg)
        {
            if (UserJoined is not null)
                _ = UserJoined(arg);

            // Log the join
            await Logger.LogGuildJoin(arg);
        }

        private async Task ClientOnUserLeft(IGuild arg1, IUser arg2)
        {
            if (UserLeft is not null)
                _ = UserLeft(arg1, arg2);

            // Log the member leaving
            await Logger.LogGuildLeave(arg1, arg2);
        }

        private async Task ClientOnUserUpdated(SocketUser arg1, SocketUser arg2)
        {
            if (UserUpdated is not null)
                _ = UserUpdated(arg1, arg2);

            if (arg1.Username != arg2.Username || arg1.Discriminator != arg2.Discriminator)
            {
                // Log the name change
                await Logger.LogNameChange(arg1, arg2);
            }
        }

        private async Task ClientOnGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser arg2)
        {
            if (GuildMemberUpdated is not null)
                _ = GuildMemberUpdated(arg1, arg2);
        }

        private async Task ClientOnUserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (UserVoiceStateUpdated is not null)
                _ = UserVoiceStateUpdated(arg1, arg2, arg3);

            if (arg2.VoiceChannel == null && arg3.VoiceChannel != null)
            {
                // Log the voice join
                await Logger.LogVoiceJoin(arg1, arg2, arg3);
            }
            else if (arg2.VoiceChannel != null && arg3.VoiceChannel != null && arg2.VoiceChannel != arg3.VoiceChannel)
            {
                // Log the voice move
                await Logger.LogVoiceMove(arg1, arg2, arg3);
            }
            else if (arg2.VoiceChannel != null && arg3.VoiceChannel == null)
            {
                // Log the voice leave
                await Logger.LogVoiceLeave(arg1, arg2, arg3);
            }
        }

        private async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            if (ReactionAdded is not null)
                _ = ReactionAdded(arg1, arg2, arg3);
        }
    }
}

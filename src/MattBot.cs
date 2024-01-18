using mattbot.services;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using mattbot.automod;

namespace mattbot
{
    public class MattBot
    {
        public DiscordSocketClient _client { get; }

        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;

        public MattBot()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessageReactions | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100
            });

            _services = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionService>()
                .AddSingleton<InteractionHandlingService>()
                .AddSingleton(new Listener(_client))
                .AddSingleton<Gems>()
                .AddSingleton<MessageReports>()
                .AddSingleton<Users>()
                .AddSingleton<CrowdMute>()
                .BuildServiceProvider();
        }

        static void Main(string[] args)
            => new MattBot().RunAsync().GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            DiscordSocketClient client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;

            await _services.GetRequiredService<CommandHandlingService>().InitializeAsync();
            await _services.GetRequiredService<InteractionHandlingService>().InitializeAsync();
            await _services.GetRequiredService<Gems>().InitializeAsync();
            await _services.GetRequiredService<MessageReports>().InitializeAsync();
            await _services.GetRequiredService<Users>().InitializeAsync();
            await _services.GetRequiredService<CrowdMute>().InitializeAsync();

            Enum.TryParse(_configuration["activity"], out ActivityType activityType);
            await client.SetGameAsync(_configuration["status"], type: activityType);

            await client.LoginAsync(TokenType.Bot, _configuration["token"]);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task LogAsync(LogMessage message)
            => Console.WriteLine(message.ToString());
    }
}

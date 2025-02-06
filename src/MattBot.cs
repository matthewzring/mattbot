/*
 * Copyright 2023-2025 Matthew Ring
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

using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using mattbot.automod;
using mattbot.services;

namespace mattbot;

public class MattBot
{
    public DiscordSocketClient _client { get; }

    public Logger Logger { get; }
    public AutoMod AutoMod { get; }

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
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
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
            .AddSingleton<IRestClientProvider>(p => p.GetRequiredService<DiscordSocketClient>())
            .AddSingleton<InteractionHandlingService>()
            .AddSingleton(new Listener(this, _client))
            .AddSingleton(new Logger(this))
            .AddSingleton<CrowdMute>()
            .AddSingleton<Gems>()
            .AddSingleton<MessageReports>()
            .BuildServiceProvider();

        // todo
        Logger = new Logger(this);
        AutoMod = new AutoMod(this);
    }

    static void Main(string[] args)
        => new MattBot().RunAsync().GetAwaiter().GetResult();

    public async Task RunAsync()
    {
        DiscordSocketClient client = _services.GetRequiredService<DiscordSocketClient>();

        client.Log += LogAsync;

        await _services.GetRequiredService<CommandHandlingService>().InitializeAsync();
        await _services.GetRequiredService<InteractionHandlingService>().InitializeAsync();
        await _services.GetRequiredService<CrowdMute>().InitializeAsync();
        await _services.GetRequiredService<Gems>().InitializeAsync();
        await _services.GetRequiredService<MessageReports>().InitializeAsync();

        Enum.TryParse(_configuration["activity"], out ActivityType activityType);
        await client.SetGameAsync(_configuration["status"], type: activityType);

        await client.LoginAsync(TokenType.Bot, _configuration["token"]);
        await client.StartAsync();

        await Task.Delay(-1);
    }

    private async Task LogAsync(LogMessage message)
        => Console.WriteLine(message.ToString());
}

using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

namespace mattbot.services
{
    public class InteractionHandlingService
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public InteractionHandlingService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Process when the client is ready, so we can register our commands.
            _client.Ready += ReadyAsync;
            _commands.Log += LogAsync;

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;
        }

        private async Task LogAsync(LogMessage log)
            => Console.WriteLine(log);

        private async Task ReadyAsync()
        {
            // await _client.GetGuild(CYBERPATRIOT_ID).DeleteApplicationCommandsAsync();
            // await _client.GetGuild(CCDC_ID).DeleteApplicationCommandsAsync();
            // await _client.GetGuild(CYBERDISCORD_ID).DeleteApplicationCommandsAsync();
            // await _client.GetGuild(TESTING_ID).DeleteApplicationCommandsAsync();
            // await _client.Rest.DeleteAllGlobalCommandsAsync();

            ModuleInfo[] cyberpatriotCommands = _commands.Modules
                .Where(x => x.Attributes
                    .Any(y => y is CyberPatriotAttribute))
                .ToArray();
            RestGuild cyberpatriot = await _client.Rest.GetGuildAsync(CYBERPATRIOT_ID);
            await _commands.AddModulesToGuildAsync(cyberpatriot, true, cyberpatriotCommands);

            ModuleInfo[] ccdcCommands = _commands.Modules
                .Where(x => x.Attributes
                    .Any(y => y is CCDCAttribute))
                .ToArray();
            RestGuild ccdc = await _client.Rest.GetGuildAsync(CCDC_ID);
            await _commands.AddModulesToGuildAsync(ccdc, true, ccdcCommands);

            ModuleInfo[] cyberdiscordCommands = _commands.Modules
                .Where(x => x.Attributes
                    .Any(y => y is CyberDiscordAttribute))
                .ToArray();
            RestGuild cyberdiscord = await _client.Rest.GetGuildAsync(CYBERDISCORD_ID);
            await _commands.AddModulesToGuildAsync(cyberdiscord, true, cyberdiscordCommands);

            ModuleInfo[] debugCommands = _commands.Modules
                .Where(x => x.Attributes
                    .Any(y => y is DebugAttribute))
                .ToArray();
            RestGuild testing = await _client.Rest.GetGuildAsync(TESTING_ID);
            await _commands.AddModulesToGuildAsync(testing, true, debugCommands);

            ModuleInfo[] ignoredCommands = _commands.Modules
                .Where(x => x.Attributes
                    .Any(y => y is IgnoreAttribute))
                .ToArray();

            ModuleInfo[] globalCommands = _commands.Modules
                .Where(x => !cyberpatriotCommands.Contains(x))
                .Where(x => !ccdcCommands.Contains(x))
                .Where(x => !cyberdiscordCommands.Contains(x))
                .Where(x => !debugCommands.Contains(x))
                .Where(x => !ignoredCommands.Contains(x))
                .ToArray();
            await _commands.AddModulesGloballyAsync(true, globalCommands);
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                SocketInteractionContext ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

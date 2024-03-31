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

using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

namespace mattbot.services;

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
        // await _client.GetGuild(CCDC_ID).DeleteApplicationCommandsAsync();
        // await _client.GetGuild(CYBERDISCORD_ID).DeleteApplicationCommandsAsync();
        // await _client.GetGuild(CYBERPATRIOT_ID).DeleteApplicationCommandsAsync();
        // await _client.GetGuild(MATTLOUNGE_ID).DeleteApplicationCommandsAsync();
        // await _client.GetGuild(FINALISTS_ID).DeleteApplicationCommandsAsync();
        // await _client.Rest.DeleteAllGlobalCommandsAsync();

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

        ModuleInfo[] cyberpatriotCommands = _commands.Modules
            .Where(x => x.Attributes
                .Any(y => y is CyberPatriotAttribute))
            .ToArray();
        RestGuild cyberpatriot = await _client.Rest.GetGuildAsync(CYBERPATRIOT_ID);
        await _commands.AddModulesToGuildAsync(cyberpatriot, true, cyberpatriotCommands);

        ModuleInfo[] debugCommands = _commands.Modules
            .Where(x => x.Attributes
                .Any(y => y is DebugAttribute))
            .ToArray();
        RestGuild mattlounge = await _client.Rest.GetGuildAsync(MATTLOUNGE_ID);
        await _commands.AddModulesToGuildAsync(mattlounge, true, debugCommands);

        ModuleInfo[] finalistsCommands = _commands.Modules
            .Where(x => x.Attributes
                .Any(y => y is FinalistsAttribute))
            .ToArray();
        RestGuild finalists = await _client.Rest.GetGuildAsync(FINALISTS_ID);
        await _commands.AddModulesToGuildAsync(finalists, true, finalistsCommands);

        ModuleInfo[] ignoredCommands = _commands.Modules
            .Where(x => x.Attributes
                .Any(y => y is IgnoreAttribute))
            .ToArray();

        ModuleInfo[] globalCommands = _commands.Modules
            .Where(x => !ccdcCommands.Contains(x))
            .Where(x => !cyberdiscordCommands.Contains(x))
            .Where(x => !cyberpatriotCommands.Contains(x))
            .Where(x => !debugCommands.Contains(x))
            .Where(x => !finalistsCommands.Contains(x))
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

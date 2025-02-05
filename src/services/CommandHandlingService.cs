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
using Discord.WebSocket;

namespace mattbot.services;

public class CommandHandlingService
{
    private readonly DiscordSocketClient _discord;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _services = services;

        // Hook MessageReceived so we can process each message to see if it qualifies as a command.
        _discord.MessageReceived += MessageReceivedAsync;
    }

    public async Task InitializeAsync()
    {
        // Register modules that are public and inherit ModuleBase<T>.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (rawMessage is not SocketUserMessage message)
            return;
        if (message.Source != MessageSource.User)
            return;

        // This value holds the offset where the prefix ends
        int argPos = 0;

        // Create a WebSocket-based command context based on the message
        SocketCommandContext context = new SocketCommandContext(_discord, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await _commands.ExecuteAsync(context, argPos, _services);
    }
}

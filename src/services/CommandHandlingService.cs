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

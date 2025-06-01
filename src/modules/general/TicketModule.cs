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

using Discord.Interactions;
using Discord.WebSocket;
using System.Text.Json;

namespace mattbot.modules.general;

[eCitadel]
[CommandContextType(InteractionContextType.Guild)]
public class TicketModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IConfiguration _configuration;
    public TicketModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // [DefaultMemberPermissions(GuildPermission.BanMembers)]
    // [SlashCommand("ticket", "Send ticket message")]
    // public async Task TicketMessageAsync()
    // {
    //     var ticket = new ComponentBuilder()
    //        .WithButton("Create Ticket", customId: "open-ticket", ButtonStyle.Success, new Emoji("\u2709\uFE0F"));
    //     await ReplyAsync("# Request help or technical assistance\nClick the button below to open a private conversation with competition organizers.\nUse this if you have a technical or administrative-related issue.", components: ticket.Build());
    //     await RespondAsync("OK", ephemeral: true);
    // }

    [ComponentInteraction("open-ticket")]
    public async Task HandleTicketButton()
    {
        var confirm = new ModalBuilder()
            .WithTitle("\u2709\uFE0F Create Ticket")
            .WithCustomId("submit-ticket")
            .AddTextInput("Your Issue", "issue", TextInputStyle.Paragraph, placeholder: "Provide as much detail as possible. You can add more info (screenshots, etc.) after submitting.");
        await RespondWithModalAsync(confirm.Build());
    }

    [ModalInteraction("submit-ticket")]
    public async Task CreateNewTicket(TicketModal modal)
    {
        await DeferAsync();
        var issue = modal.Issue;
        string teamId = null;
        List<string> teammates = null;
        try
        {
            HttpClient httpClient = new HttpClient();
            string apiUrl = "https://portal.ecitadel.org/auth/competitor/data/";
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["token"]);
            var response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(content).RootElement.GetProperty("data");
                foreach (var team in data.EnumerateArray())
                {
                    var competitors = team.GetProperty("competitors")
                        .EnumerateArray()
                        .Select(c => c.GetString())
                        .ToList();

                    if (!competitors.Contains(Context.User.Id.ToString()))
                        continue;

                    teamId = team.GetProperty("id").GetInt32().ToString();
                    teammates = competitors.Except([Context.User.Id.ToString()]).ToList();
                    break;
                }
            }
        }
        catch (Exception) { }

        var channel = Context.Channel as SocketTextChannel;
        var name = teamId ?? Context.User.Username;
        var thread = (Context.Channel as SocketTextChannel).Threads.FirstOrDefault(t => t.Name == name && !t.IsArchived && !t.IsLocked);

        if (thread != null)
        {
            await FollowupAsync($"You already have an existing ticket open: {thread.Mention}", ephemeral: true);
            return;
        }

        thread = await (Context.Channel as SocketTextChannel).CreateThreadAsync(
            name: name,
            type: ThreadType.PrivateThread
        );

        await thread.SendMessageAsync($"{Context.User.Mention} opened a ticket:\n\n**Issue**\n{issue}");

        if (teammates != null && teammates.Count > 0)
        {
            foreach (var id in teammates)
            {
                var user = Context.Guild.GetUser(ulong.Parse(id));
                if (user != null)
                {
                    await thread.AddUserAsync(user);
                }
            }
        }
        await thread.AddUserAsync(Context.Guild.GetUser(349007194768801792)); // matt
        await thread.AddUserAsync(Context.Guild.GetUser(332967486540611585)); // kali
        await thread.SendMessageAsync("**\uD83D\uDC4B Welcome to your new ticket!**\n\n" +
                                        "**\u23F2\uFE0F We'll be here soon!** Typically we respond in a few minutes, but sometimes we might take a bit longer if the server is busy or if you have a particularly tricky issue.\n\n" +
                                        "**\u23F1\uFE0F We close idle tickets,** which makes them read-only. Once a ticket is closed it won't be reopened, but you can always create a new ticket if you have another issue.\n\n" +
                                        "**\uD83D\uDCDD Have more to share?** If you have not fully explained your issue, please do so now. You may add more details, screenshots, videos, etc. below.");
        await FollowupAsync($"New ticket created: {thread.Mention}", ephemeral: true);
    }
}

public class TicketModal : IModal
{
    public string Title => "\u2709\uFE0F Create Ticket";

    [ModalTextInput("issue")]
    public string Issue { get; set; }
}
﻿using Discord.Interactions;
using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.modules.general
{
    [CyberPatriot]
    public class MinecraftModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const string PICK = "\u26CF\uFE0F"; // ⛏️

        private readonly IConfiguration _configuration;

        public MinecraftModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // register
        [EnabledInDm(false)]
        [SlashCommand("register", "Register your Minecraft account")]
        public async Task RegisterMCAsync([Summary("username", "Your Minecraft username")] string username)
        {
            var user = Context.User as SocketGuildUser;

            ITextChannel tc = Context.Guild.TextChannels.FirstOrDefault(x => x.Name == "minecraft_log");
            if (tc == null)
                return;
            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (!Regex.IsMatch(username, "^[a-zA-Z0-9_]{3,16}$"))
            {
                await Logger.Log(now, tc, PICK, $"{FormatUtil.formatFullUser(Context.User)} submitted an invalid Minecraft username: `{username}`", null);
                await RespondAsync("Invalid Minecraft username!", ephemeral: true);
                return;
            }

            var mc_registered = user.Guild.Roles.FirstOrDefault(x => x.Name == "MC Registered");
            if (mc_registered == null)
                return;

            if (user.Roles.Contains(mc_registered))
            {
                await Logger.Log(now, tc, ERROR, $"{FormatUtil.formatFullUser(Context.User)} attempted to re-register with username: `{username}`", null);
                await RespondAsync($"This account is already registered!", ephemeral: true);
                return;
            }

            // Create HttpClient instance
            HttpClient httpClient = new HttpClient();

            // Set the base URL of the API
            string apiUrl = "https://api.matthewzring.dev/whitelist?username=";

            // Set the Authorization header
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["bearerToken"]);

            await DeferAsync(ephemeral: true);

            try
            {
                apiUrl += username;
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                // Check if response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Read response content as string
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Give MC registered role
                    await user.AddRoleAsync(mc_registered);
                    await Logger.Log(now, tc, PICK, $"{FormatUtil.formatFullUser(Context.User)} registered with username: `{username}`", null);
                    await FollowupAsync($"Your account has been registered with **{username}**!", ephemeral: true);
                    return;
                }
                else
                {
                    await Logger.Log(now, tc, ERROR, $"{FormatUtil.formatFullUser(Context.User)} failed registration with username: `{username}`"
                            + $"\nRequest failed with status code: `{response.StatusCode}`", null);
                    await FollowupAsync(ERROR_MESSAGE, ephemeral: true);
                    return;
                }
            }
            catch (Exception ex)
            {
                await Logger.Log(now, tc, ERROR, $"{FormatUtil.formatFullUser(Context.User)} failed registration with username: `{username}`"
                            + $"\nError: `{ex.Message}`", null);
                await FollowupAsync(ERROR_MESSAGE, ephemeral: true);
            }
            finally
            {
                // Dispose HttpClient instance
                httpClient.Dispose();
            }
        }

        // restart
        [EnabledInDm(false)]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("restart", "Restart the minecraft server")]
        public async Task RestartMCAsync()
        {
            var owner = Context.Guild.GetUser(OWNER_ID);
            if (Context.User.Id != OWNER_ID)
            {
                await RespondAsync($"{ERROR} Sorry, this command can only be used by {FormatUtil.formatUser(owner)}!", ephemeral: true);
                return;
            }

            // Create HttpClient instance
            HttpClient httpClient = new HttpClient();

            // Set the base URL of the API
            string apiUrl = "https://api.matthewzring.dev/restart";

            // Set the Authorization header
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["bearerToken"]);

            await DeferAsync(ephemeral: true);

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                // Check if response is successful
                if (response.IsSuccessStatusCode)
                {
                    await FollowupAsync($"Server restarting!", ephemeral: true);
                    return;
                }
                else
                {
                    await FollowupAsync($"Request failed with status code: `{response.StatusCode}`", ephemeral: true);
                    return;
                }
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Error: `{ex.Message}`", ephemeral: true);
            }
            finally
            {
                // Dispose HttpClient instance
                httpClient.Dispose();
            }
        }
    }
}

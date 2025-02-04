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
using System.Diagnostics;

namespace mattbot.modules.general;

[CommandContextType(InteractionContextType.Guild)]
public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    // ping
    [SlashCommand("ping", "Checks the bot's latency")]
    public async Task PingAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        await RespondAsync("Ping: ...");
        stopwatch.Stop();
        await ModifyOriginalResponseAsync(msg => msg.Content = $"Ping: {stopwatch.ElapsedMilliseconds}ms | Websocket: {Context.Client.Latency}ms");
    }
}

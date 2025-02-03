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

using Discord.Commands;

namespace mattbot.modules.general;

[RequireContext(ContextType.Guild)]
public class ThanksModule : ModuleBase<SocketCommandContext>
{
    private static readonly Random random = new Random();

    [Command("thanks mattbot")]
    [Alias("thank you mattbot", "thanks automatt", "thank you automatt", "love you mattbot", "i love you mattbot", "thanks john cyberpatriot")]
    public async Task ThanksAsync()
    {
        string[] responses =
        {
            "coolio",
            "glad to help",
            "Glad to help!",
            "happy to help",
            "I have no emotion but I appreciate the thanks nonetheless.",
            "just doin my job!",
            "mhm",
            "mhmm",
            "no problem",
            "No problem!",
            "np",
            "o",
            "ur welcome",
            "you don't need to thank me I'm just a bot",
            "You're very welcome!",
            "you're welcome",
            "You're welcome.",
            "\uD83D\uDE04",
            "\uD83D\uDE47"
        };
        await ReplyAsync(responses[random.Next(responses.Length)]);
    }
}

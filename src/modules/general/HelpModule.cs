/*
 * Copyright 2023-2026 Matthew Ring
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

namespace mattbot.modules.general;

[CommandContextType(InteractionContextType.Guild)]
public class HelpModule : InteractionModuleBase<SocketInteractionContext>
{
    // help
    [SlashCommand("help", "Sends help")]
    public async Task HelpAsync()
    {
        try
        {
            await Context.User.SendMessageAsync("help");
            await RespondAsync($"{Context.User.Mention}, help has been sent to your Direct Messages!");
            await Task.Delay(15000);
            await Context.User.SendMessageAsync("no seriously can you help me");
            await Task.Delay(5000);
            await Context.User.SendMessageAsync("please");
        }
        catch (Exception)
        {
            await RespondAsync("I couldn't send you a direct message... are you blocking them... or me... :(");
        }
    }
}

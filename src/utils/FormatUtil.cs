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

namespace mattbot.utils;

/**
 * Modelled after jagrosh's FormatUtil in Vortex
 */
public class FormatUtil
{
    public static string filterEveryone(string input)
    {
        return input.Replace("\u202E", "") // RTL override
                .Replace("@everyone", "@\u0435veryone") // cyrillic e
                .Replace("@here", "@h\u0435re") // cyrillic e
                .Replace("discord.gg/", "discord\u2024gg/") // one dot leader
                .Replace("@&", "\u0DB8&"); // role failsafe
    }

    public static string formatUser (IUser user)
    {
        if (user.Discriminator == "0000")
            return filterEveryone($"**{user.Username}**");
        return filterEveryone($"**{user.Username}**#{user.Discriminator}");
    }

    public static string formatFullUser(IUser user)
    {
        if (user.Discriminator == "0000")
            return filterEveryone($"**{user.Username}** (ID:{user.Id})");
        return filterEveryone($"**{user.Username}**#{user.Discriminator} (ID:{user.Id})");
    }

    public static string secondsToTimeCompact (long timeseconds)
    {
        StringBuilder builder = new StringBuilder();
        int years = (int)(timeseconds / (60*60*24*365));
        if (years > 0)
        {
            builder.Append("**").Append(years).Append("**y ");
            timeseconds = timeseconds % (60*60*24*365);
        }
        int weeks = (int)(timeseconds / (60*60*24*7));
        if (weeks > 0)
        {
            builder.Append("**").Append(weeks).Append("**w ");
            timeseconds = timeseconds % (60*60*24*7);
        }
        int days = (int)(timeseconds / (60*60*24));
        if (days > 0)
        {
            builder.Append("**").Append(days).Append("**d ");
            timeseconds = timeseconds % (60*60*24);
        }
        int hours = (int)(timeseconds / (60*60));
        if (hours > 0)
        {
            builder.Append("**").Append(hours).Append("**h ");
            timeseconds = timeseconds % (60*60);
        }
        int minutes = (int)(timeseconds / (60));
        if (minutes > 0)
        {
            builder.Append("**").Append(minutes).Append("**m ");
            timeseconds = timeseconds % (60);
        }
        if (timeseconds > 0)
            builder.Append("**").Append(timeseconds).Append("**s");
        string str = builder.ToString();
        if (str.EndsWith(", "))
            str = str.Substring(0, str.Length-2);
        if (string.IsNullOrEmpty(str))
            str = "**No time**";
        return str;
    }
}

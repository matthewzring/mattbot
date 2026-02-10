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

using Discord.WebSocket;

namespace mattbot.utils;

public class PermissionUtil
{
    /// <summary>
    ///     Checks if one given Member can interact with a 2nd given Member - in a permission sense (kick/ban/modify perms).
    ///     This only checks the Role-Position and does not check the actual permission (kick/ban/manage_role/...)
    /// </summary>
    /// <param name="issuer">The member that tries to interact with 2nd member</param>
    /// <param name="target">The member that is the target of the interaction</param>
    /// <returns>
    ///     True, if issuer can interact with target in guild
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if any of the provided parameters is {@code null}
    ///     or the provided entities are not from the same guild
    /// </exception>
    public static bool CanInteract(SocketGuildUser issuer, SocketGuildUser target)
    {
        if (issuer == null)
            throw new ArgumentException("Issuer Member may not be null");
        if (target == null)
            throw new ArgumentException("Target Member may not be null");

        SocketGuild guild = issuer.Guild;
        if (!guild.Equals(target.Guild))
            throw new ArgumentException("Provided members must both be Member objects of the same Guild!");
        if (issuer.Id == guild.Owner.Id)
            return true;
        if (target.Id == guild.Owner.Id)
            return false;
        List<SocketRole> issuerRoles = issuer.Roles.ToList();
        List<SocketRole> targetRoles = target.Roles.ToList();
        return issuerRoles.Any() && (!targetRoles.Any() || CanInteract(issuerRoles.MaxBy(r => r.Position), targetRoles.MaxBy(r => r.Position)));
    }

    /// <summary>
    ///     Checks if a given Member can interact with a given Role - in a permission sense (kick/ban/modify perms).
    ///     This only checks the Role-Position and does not check the actual permission (kick/ban/manage_role/...)
    /// </summary>
    /// <param name="issuer">The member that tries to interact with the role</param>
    /// <param name="target">The role that is the target of the interaction</param>
    /// <returns>
    ///     True, if issuer can interact with target
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if any of the provided parameters is {@code null}
    ///     or the provided entities are not from the same guild
    /// </exception>
    public static bool CanInteract(SocketGuildUser issuer, SocketRole target)
    {
        if (issuer == null)
            throw new ArgumentException("Issuer Member may not be null");
        if (target == null)
            throw new ArgumentException("Target Role may not be null");

        SocketGuild guild = issuer.Guild;
        if (!guild.Equals(target.Guild))
            throw new ArgumentException("Provided Member issuer and Role target must be from the same Guild!");
        if (issuer.Id == guild.Owner.Id)
            return true;
        List<SocketRole> issuerRoles = issuer.Roles.ToList();
        return issuerRoles.Any() && CanInteract(issuerRoles.MaxBy(r => r.Position), target);
    }

    /// <summary>
    ///     Checks if one given Role can interact with a 2nd given Role - in a permission sense (kick/ban/modify perms).
    ///     This only checks the Role-Position and does not check the actual permission (kick/ban/manage_role/...)
    /// </summary>
    /// <param name="issuer">The role that tries to interact with 2nd role</param>
    /// <param name="target">The role that is the target of the interaction</param>
    /// <returns>
    ///     True, if issuer can interact with target
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if any of the provided parameters is {@code null}
    ///     or the provided entities are not from the same guild
    /// </exception>
    public static bool CanInteract(SocketRole issuer, SocketRole target)
    {
        if (issuer == null)
            throw new ArgumentException("Issuer Role may not be null");
        if (target == null)
            throw new ArgumentException("Target Role may not be null");

        if (!issuer.Guild.Equals(target.Guild))
            throw new ArgumentException("The 2 Roles are not from same Guild!");
        return target.CompareTo(issuer) < 0;
    }
}

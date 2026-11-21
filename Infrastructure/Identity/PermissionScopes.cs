namespace Infrastructure.Identity;

public static class PermissionScopes
{
    public const string ReservationsRead = "reservations.read";
    public const string ReservationsWrite = "reservations.write";

    public static IReadOnlyList<string> ForRoles(IEnumerable<string> roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var scopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (roleSet.Contains(Roles.Owner)
            || roleSet.Contains(Roles.Manager)
            || roleSet.Contains(Roles.Receptionist))
        {
            scopes.Add(ReservationsRead);
        }

        if (roleSet.Contains(Roles.Owner)
            || roleSet.Contains(Roles.Manager)
            || roleSet.Contains(Roles.Receptionist))
        {
            scopes.Add(ReservationsWrite);
        }

        return scopes.ToArray();
    }
}

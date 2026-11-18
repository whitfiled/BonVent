namespace Infrastructure.Identity;

public static class Roles
{
    public const string Owner = "Owner";
    public const string Manager = "Manager";
    public const string Receptionist = "Receptionist";
    public const string Guest = "Guest";
    public static readonly string[] All = [Owner, Manager, Receptionist, Guest];
}
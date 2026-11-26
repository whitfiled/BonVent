using Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Guests;

internal class Guest : Entity, ISoftDelete, IAggregateRoot
{
    [Required]
    public string FirstName { get; private set; } = default!;
    [Required]
    public string LastName { get; private set; } = default!;
    [Required]
    public string Email { get; private set; } = default!;
    [Required]
    public string Phone { get; private set; } = default!;
    [Required]
    public string Address { get; private set; } = default!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    private Guest() { }

    public Guest(string firstName, string lastName, string email, string phone, string address)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Address = address;
    }

    public void Update(string firstName, string lastName, string email, string phone, string address)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Guest name and email are required.");
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
        Address = address?.Trim() ?? string.Empty;
    }

    public void Deactivate()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
    }
}

using OrderPulse.Domain.Common;
using OrderPulse.Domain.ValueObjects;

namespace OrderPulse.Domain.Entities;

public sealed class Customer : Entity<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Address? DefaultShippingAddress { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Customer() { } // EF Core required parameterless constructor

    public Customer(Guid id, string email, string firstName, string lastName, Address? defaultShippingAddress = null)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));

        Email = email.Trim().ToLowerInvariant();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DefaultShippingAddress = defaultShippingAddress;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName, Address? shippingAddress)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DefaultShippingAddress = shippingAddress;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

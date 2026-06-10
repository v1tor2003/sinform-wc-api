using System;

namespace SinformWcApi.Contexts;

public class UserContext : IUserContext
{
    public Guid? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}

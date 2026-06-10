using System;

namespace SinformWcApi.Contexts;

public interface IUserContext
{
    Guid? UserId { get; }
    string? Name { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

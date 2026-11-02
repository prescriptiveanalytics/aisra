using JetBrains.Annotations;

namespace AIsra.Web.Dtos;

public sealed record UserDto
{
    [UsedImplicitly]
    public required bool AuthenticationEnabled { get; init; }

    [UsedImplicitly]
    public string? Name { get; init; }

    [UsedImplicitly]
    public string? Email { get; init; }
}

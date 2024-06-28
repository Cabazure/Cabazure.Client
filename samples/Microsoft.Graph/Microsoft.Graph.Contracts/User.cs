namespace Microsoft.Graph.Contracts;

public record User(
    string? Id,
    string? DisplayName,
    string? GivenName,
    string? Surname,
    string? Mail,
    string? MobilePhone,
    string? OfficeLocation,
    string? JobTitle,
    string? UserPrincipalName,
    string? PreferredLanguage,
    IEnumerable<string>? BusinessPhones);
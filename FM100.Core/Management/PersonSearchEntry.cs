namespace FM100.Core.Management;

public sealed record PersonSearchEntry(
    Guid PersonId,
    PersonCategory Category,
    string PersonType,
    string FullName,
    string Role,
    string ClubName,
    string Division,
    int Age,
    string Nationality,
    int Reputation,
    string Status);

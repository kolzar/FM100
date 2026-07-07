namespace FM100.Core.Management;

public sealed record PersonDetail(
    Guid PersonId,
    string FullName,
    string Subtitle,
    string ClubName,
    IReadOnlyList<PersonPropertyEntry> Properties);

using FM100.Domain.Base;

namespace FM100.Domain.Personnel;

public sealed class ClubPerson : Person
{
    public Guid ClubId { get; set; }
    public PersonnelRole Role { get; set; }
    public int Ability { get; set; }
    public int Potential { get; set; }
    public int Reputation { get; set; }
    public int Leadership { get; set; }
    public int Negotiation { get; set; }
    public int TacticalKnowledge { get; set; }
    public int JudgingPlayers { get; set; }
    public int JudgingPotential { get; set; }
    public int YouthDevelopment { get; set; }
    public int MedicalKnowledge { get; set; }
    public int FitnessKnowledge { get; set; }
    public int WageInMillions { get; set; }
    public int ContractExpiresSeason { get; set; } = 3;
    public bool IsHumanManager { get; set; }
}

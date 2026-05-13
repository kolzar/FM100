using FM100.Domain.Base;

namespace FM100.Domain.Staff;
public class StaffMember : Person
{
    public StaffRole Role { get; set; }

    public int TacticalSkill { get; set; }
    public int Motivation { get; set; }
    public int Discipline { get; set; }
    public int PlayerDevelopment { get; set; }
    public int Leadership { get; set; }
    public int ManManagement { get; set; }
    public int JudgingAbility { get; set; }
    public int PotentialJudging { get; set; }
}

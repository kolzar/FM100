namespace FM100.Domain.Base;

public abstract class Person : IPerson
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string Description {  get; set; } = string.Empty;
    public int Height { get; set; }
    public int Weight { get; set; }
    public DynamicState CurrentState { get; set; } = new DynamicState();
    public MentalAttributes MentalAttributes { get; set; } = new MentalAttributes();
}


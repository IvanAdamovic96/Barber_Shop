namespace Hair.Domain.Entities;

public class Haircuts
{
    public Haircuts(int duration, string haircutType)
    {
     
        Duration = duration;
        HaircutType = haircutType;
    }

    public Guid Id { get; set; }
    public int Duration { get; set; }
    public string HaircutType { get; set; }
    
    Barber Barber { get; set; }
    
}
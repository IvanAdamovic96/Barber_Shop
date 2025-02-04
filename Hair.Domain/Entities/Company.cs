using System.Data;

namespace Hair.Domain.Entities;

public class Company
{
    public Company( string companyName)
    {
        CompanyName = companyName;
    }

    public Guid Id { get; set; }
    public string CompanyName { get; set; }
    
    public IList<Barber> Barbers { get; set; } = new List<Barber>();
    
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hair.Domain.Entities;

public class Barber
{
    public Barber(string barberName, string phoneNumber, string email, TimeSpan? individualStartTime, TimeSpan? individualEndTime)
    {
        
        BarberName = barberName;
        PhoneNumber = phoneNumber;
        Email = email;
        IndividualStartTime = individualStartTime;
        IndividualEndTime = individualEndTime;
        
        BarberId = Guid.NewGuid();
    }

    public Barber(){}
    public Guid BarberId { get; private set; }
    public string BarberName { get; private set; }
    public string PhoneNumber { get; private set; }
    
    public string Email { get; private set; }
    
    public string? Code { get; private set; }
    
    public bool? Verified { get; set; } 
    
    public DateTime? CodeExpiry { get; set; }
    public Company Company { get; private set; }
    public TimeSpan? IndividualStartTime { get; private set; } 
    public TimeSpan? IndividualEndTime { get; private set; }   
    
    

    public Barber AddBarberCompany(Company company)
    {
        Company = company;
        return this;
    }


}
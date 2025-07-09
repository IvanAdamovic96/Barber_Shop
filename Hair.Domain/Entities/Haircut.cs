namespace Hair.Domain.Entities;

public class Haircut
{
    public Haircut(int duration, string haircutType, decimal price)
    {
        Duration = duration;
        HaircutType = haircutType;
        Price = price;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public int Duration { get; private set; }
    public Guid CompanyId { get; private set; }
    public string HaircutType { get; private set; }
    public decimal Price { get; private set; }
    public Company Company { get; set; }

    public Haircut AddCompanyCompany(Company company)
    {
        Company = company;
        return this;
    }
    
}
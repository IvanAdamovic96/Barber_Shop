using Microsoft.EntityFrameworkCore;

namespace Hair.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Time { get; set; }
    public Guid Barberid { get; set; }

    public Appointment(Guid id, DateTime time, Guid barberid)
    {
        Id = id;
        Time = time;
        Barberid = barberid;
    }

    public Appointment()
    {
        
    }

    /*
       var startTime = new DateTime(2024, 12, 20, 09, 0, 0);
        var endTime = new DateTime(2024, 12,20,17,0,0);
        var helpTime = startTime;
        List<DateTime> dates = new List<DateTime>();
        
        while (helpTime <= endTime)
        {
            helpTime = startTime.AddMinutes(30);
            dates.Add(helpTime);
        }
     */
}
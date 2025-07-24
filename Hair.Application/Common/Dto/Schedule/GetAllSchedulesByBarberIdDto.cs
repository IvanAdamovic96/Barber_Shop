namespace Hair.Application.Common.Dto.Schedule;

public record GetAllSchedulesByBarberIdDto
{
    public GetAllSchedulesByBarberIdDto(Guid AppointmentId ,Guid BarberId,DateTime Time, string FirstName,
        string LastName, string Email, string PhoneNumber)
    {
        appointmentId = AppointmentId;
        barberId = BarberId;
        time = Time;
        firstName = FirstName;
        lastName = LastName;
        email = Email;
        phoneNumber = PhoneNumber;
    }

    public GetAllSchedulesByBarberIdDto()
    {
        
    }

    public Guid appointmentId { get; init; }
    public Guid barberId { get; init; }
    public DateTime time { get; init; }
    public string firstName { get; init; }
    public string lastName { get; init; }
    public string email { get; init; }
    public string phoneNumber { get; init; }

    public void Deconstruct(out Guid appointmentId, out Guid barberId, out DateTime time, out string firstName, 
        out string lastName, out string email, out string phoneNumber)
    {
        appointmentId = this.appointmentId;
        barberId = this.barberId;
        time = this.time;
        firstName = this.firstName;
        lastName = this.lastName;
        email = this.email;
        phoneNumber = this.phoneNumber;
        
    }
}
namespace Hair.Application.Common.Dto.Barber;

public record BarberDetailsDto(
    Guid BarberId, 
    string BarberName, 
    string CompanyName,
    string PhoneNumber,
    string Email,
    Guid CompanyId,
    TimeSpan IndividualStartTime,
    TimeSpan IndividualEndTime
    );
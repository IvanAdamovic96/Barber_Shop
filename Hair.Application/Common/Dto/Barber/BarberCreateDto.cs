namespace Hair.Application.Common.Dto.Barber;

public record BarberCreateDto(Guid companyId,string barberName,int phoneNumber,string email,TimeSpan? individualStartTime,TimeSpan? individualEndTime);
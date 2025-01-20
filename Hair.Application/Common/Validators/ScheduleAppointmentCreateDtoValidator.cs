using FluentValidation;
using Hair.Application.Common.Dto.Schedule;

namespace Hair.Application.Common.Validators;

public class ScheduleAppointmentCreateDtoValidator : AbstractValidator<ScheduleAppointmentCreateDto>
{
    public ScheduleAppointmentCreateDtoValidator()
    {
        RuleFor(x => x.phoneNumber).MinimumLength(7);
        RuleFor(x => x.phoneNumber).MaximumLength(15);
        RuleFor(x=>x.firstName).MinimumLength(3);
        RuleFor(x=>x.lastName).MinimumLength(3);
    }
    
}
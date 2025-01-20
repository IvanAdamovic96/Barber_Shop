using FluentValidation;
using Hair.Application.Common.Validators;

namespace Hair.Application.Schedules.Commands;

public class ScheduleAppointmentCommandValidator : AbstractValidator<ScheduleAppointmentCommand>
{
    public ScheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.Schedule).NotNull();
        RuleFor(x=>x.Schedule).SetValidator(new ScheduleAppointmentCreateDtoValidator());
    }
}
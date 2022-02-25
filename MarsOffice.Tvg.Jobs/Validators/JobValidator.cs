using FluentValidation;
using MarsOffice.Tvg.Jobs.Abstractions;

namespace MarsOffice.Tvg.Jobs.Validators
{
    public class JobValidator : AbstractValidator<Job>
    {
        public JobValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
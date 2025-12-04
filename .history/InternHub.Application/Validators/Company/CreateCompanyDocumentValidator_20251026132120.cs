using FluentValidation;
using InternHub.Application.DTO.Company;

namespace InternHub.Application.Validators.Company
{
    public class CreateCompanyDocumentValidator : AbstractValidator<CompanyDocumentUploadDto>
    {
        public CreateCompanyDocumentValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.FileUrl)
                .NotEmpty()
                .MaximumLength(250);
        }
    }
}

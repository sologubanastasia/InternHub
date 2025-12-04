using FluentValidation.TestHelper;
using InternHub.Application.DTO.Job;
using InternHub.Application.Validators;
using Xunit;

namespace InternHub.UnitTests.Validators.Job;

public class JobValidatorsTests
{
    private readonly ApplyJobValidator _validator;

    public JobValidatorsTests()
    {
        _validator = new ApplyJobValidator();
    }

    [Fact]
    public void ApplyJobValidator_Should_HaveError_When_MessageTooLong()
    {
        var dto = new JobApplicationCreateDto { Message = new string('a', 501) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void ApplyJobValidator_Should_NotHaveError_When_MessageValid()
    {
        var dto = new JobApplicationCreateDto { Message = "Hello" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }
}

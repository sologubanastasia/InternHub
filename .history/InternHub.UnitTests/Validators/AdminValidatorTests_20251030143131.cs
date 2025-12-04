using FluentValidation.TestHelper;
using InternHub.Application.DTO.Admin;
using InternHub.Application.Validators;
using Xunit;

namespace InternHub.UnitTests.Validators.Admin;

public class AdminValidatorsTests
{
    private readonly ApproveCompanyValidator _approveCompanyValidator;

    public AdminValidatorsTests()
    {
        _approveCompanyValidator = new ApproveCompanyValidator();
    }

    [Fact]
    public void ApproveCompanyValidator_Should_HaveError_When_ApproveIsNull()
    {
        var dto = new ApproveCompanyDto { Approve = null };
        var result = _approveCompanyValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Approve);
    }

    [Fact]
    public void ApproveCompanyValidator_Should_NotHaveError_When_ApproveIsNotNull()
    {
        var dto = new ApproveCompanyDto { Approve = true };
        var result = _approveCompanyValidator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Approve);
    }
}

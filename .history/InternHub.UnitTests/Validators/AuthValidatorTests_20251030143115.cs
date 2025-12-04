using FluentValidation.TestHelper;
using InternHub.Application.DTO.Auth;
using InternHub.Application.Validators;
using InternHub.Application.Validators; // правильний namespace

using Xunit;

namespace InternHub.UnitTests.Validators.Auth;

public class AuthValidatorsTests
{
    private readonly LoginValidator _loginValidator;
    private readonly RegisterCompanyValidator _registerCompanyValidator;

    public AuthValidatorsTests()
    {
        _loginValidator = new LoginValidator();
        _registerCompanyValidator = new RegisterCompanyValidator();
    }

    [Fact]
    public void LoginValidator_Should_HaveError_When_EmailIsEmpty()
    {
        var dto = new LoginDto { Email = "", Password = "123" };
        var result = _loginValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void RegisterCompanyValidator_Should_HaveError_When_PasswordTooShort()
    {
        var dto = new RegisterCompanyDto { Email = "a@b.com", Password = "123", CompanyName = "Test", Name = "Name" };
        var result = _registerCompanyValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

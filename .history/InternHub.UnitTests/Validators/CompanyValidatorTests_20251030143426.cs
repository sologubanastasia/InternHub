using FluentValidation.TestHelper;
using InternHub.Application.DTO.Company;
using InternHub.Application.Validators.Company;
using Xunit;

namespace InternHub.UnitTests.Validators.Company;

public class CompanyValidatorsTests
{
    private readonly CreateCompanyDocumentValidator _docValidator;
    private readonly UpdateCompanyValidator _updateValidator;

    public CompanyValidatorsTests()
    {
        _docValidator = new CreateCompanyDocumentValidator();
        _updateValidator = new UpdateCompanyValidator();
    }

    [Fact]
    public void CreateCompanyDocumentValidator_Should_HaveError_When_FileNameEmpty()
    {
        var dto = new CompanyDocumentUploadDto { FileName = "", FileUrl = "url" };
        var result = _docValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void UpdateCompanyValidator_Should_HaveError_When_EmailInvalid()
    {
        var dto = new UpdateCompanyDto { CompanyName = "Test", Email = "wrong", Description = "", Website = "" };
        var result = _updateValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}

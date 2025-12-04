using FluentValidation.TestHelper;
using InternHub.Application.DTO.Candidate;
using InternHub.Application.Validators;
using Xunit;

namespace InternHub.UnitTests.Validators.Candidate;

public class CandidateValidatorsTests
{
    private readonly UpdateCandidateValidator _validator;

    public CandidateValidatorsTests()
    {
        _validator = new UpdateCandidateValidator();
    }

    [Fact]
    public void UpdateCandidateValidator_Should_HaveError_When_GitHubUrlInvalid()
    {
        var dto = new UpdateCandidateDto { GitHubUrl = "http://wrong.com" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.GitHubUrl);
    }

    [Fact]
    public void UpdateCandidateValidator_Should_NotHaveError_When_GitHubUrlCorrect()
    {
        var dto = new UpdateCandidateDto { GitHubUrl = "https://github.com/user" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.GitHubUrl);
    }
}

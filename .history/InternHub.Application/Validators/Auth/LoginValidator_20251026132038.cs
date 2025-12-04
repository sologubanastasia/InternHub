namespace InternHub.Application.Validators.Auth;
using InternHub.Application.DTO.Auth;
using FluentValidation;

public class LoginValidator : AbstractValidator<LoginDto>
{
   public LoginValidator()
   {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty(); 
   }   
}
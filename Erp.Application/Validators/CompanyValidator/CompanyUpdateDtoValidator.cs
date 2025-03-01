using Erp.Domain.DTOs.Company;
using FluentValidation;

namespace Erp.Application.Validators.CompanyValidator;

public class CompanyUpdateDtoValidator : AbstractValidator<CompanyUpdateDto>
{
    public CompanyUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Şirket adı boş olamaz")
            .MaximumLength(100).WithMessage("Şirket adı en fazla 100 karakter olabilir");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("Vergi numarası boş olamaz")
            .Length(10).WithMessage("Vergi numarası 10 haneli olmalıdır")
            .Matches("^[0-9]*$").WithMessage("Vergi numarası sadece rakamlardan oluşmalıdır");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası boş olamaz")
            .Length(10).WithMessage("Telefon numarası 10 haneli olmalıdır")
            .Matches("^[0-9]*$").WithMessage("Telefon numarası sadece rakamlardan oluşmalıdır");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres boş olamaz")
            .MaximumLength(250).WithMessage("Adres en fazla 250 karakter olabilir");

        RuleFor(x => x.Website)
            .MaximumLength(100).WithMessage("Website adresi en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
} 
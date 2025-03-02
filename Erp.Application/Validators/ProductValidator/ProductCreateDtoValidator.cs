using Erp.Domain.DTOs.Product;
using FluentValidation;

namespace Erp.Application.Validators.ProductValidator;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz")
            .MaximumLength(100).WithMessage("Ürün adı en fazla 100 karakter olabilir");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU boş olamaz")
            .MaximumLength(50).WithMessage("SKU en fazla 50 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");

        RuleFor(x => x.Barcode)
            .MaximumLength(50).WithMessage("Barkod en fazla 50 karakter olabilir");
    }
} 
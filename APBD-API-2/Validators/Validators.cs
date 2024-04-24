using FluentValidation;

namespace APBD_API_2.Validators;

public static class Validators
{
    public static void RegisterValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AnimalCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<AnimalReplaceRequestValidator>();
    }
}
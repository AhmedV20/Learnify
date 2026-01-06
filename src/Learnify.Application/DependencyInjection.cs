using FluentValidation;
using Learnify.Api.Mappings;
using Learnify.Application.Common.Behaviors;
using Learnify.Application.Email;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Learnify.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(assembly);
        });

        services.AddAutoMapper(assembly);

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register validation pipeline behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}


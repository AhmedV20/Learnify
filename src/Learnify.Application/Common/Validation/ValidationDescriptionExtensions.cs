using FluentValidation;

namespace Learnify.Application.Common.Validation;

/// <summary>
/// Extension methods for adding user-friendly descriptions to FluentValidation rules
/// </summary>
public static class ValidationDescriptionExtensions
{
    /// <summary>
    /// Adds a user-friendly description that will be shown in error responses.
    /// The description can include context about the attempted value.
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <typeparam name="TProperty">The property type being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <param name="description">Static description text</param>
    public static IRuleBuilderOptions<T, TProperty> WithDescription<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> ruleBuilder,
        string description)
    {
        return ruleBuilder.WithState(_ => new ValidationDescriptionState(description));
    }

    /// <summary>
    /// Adds a user-friendly description with access to the attempted value.
    /// Use this when you want to include the actual value in the description.
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <typeparam name="TProperty">The property type being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <param name="descriptionFactory">Factory function that receives the object and property value</param>
    public static IRuleBuilderOptions<T, TProperty> WithDescription<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> ruleBuilder,
        Func<T, TProperty, string> descriptionFactory)
    {
        return ruleBuilder.WithState(x => new ValidationDescriptionState<T, TProperty>(descriptionFactory, x));
    }
}

/// <summary>
/// Holds a static description for validation failures
/// </summary>
public class ValidationDescriptionState
{
    public string Description { get; }

    public ValidationDescriptionState(string description)
    {
        Description = description;
    }

    public virtual string GetDescription(object? attemptedValue) => Description;
}

/// <summary>
/// Holds a dynamic description factory that can use the attempted value
/// </summary>
public class ValidationDescriptionState<T, TProperty> : ValidationDescriptionState
{
    private readonly Func<T, TProperty, string> _descriptionFactory;
    private readonly T _instance;

    public ValidationDescriptionState(Func<T, TProperty, string> descriptionFactory, T instance)
        : base(string.Empty)
    {
        _descriptionFactory = descriptionFactory;
        _instance = instance;
    }

    public override string GetDescription(object? attemptedValue)
    {
        try
        {
            return _descriptionFactory(_instance, (TProperty)attemptedValue!);
        }
        catch
        {
            return "Validation failed for this field";
        }
    }
}

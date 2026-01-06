using FluentResults;
using Learnify.Domain.Entities;
using MediatR;

namespace Learnify.Application.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Slug) : IRequest<Category>;

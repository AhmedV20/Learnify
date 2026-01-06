using Learnify.Domain.Entities;
using MediatR;

namespace Learnify.Application.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(int Id, string Name, string Slug) : IRequest<Category>; 
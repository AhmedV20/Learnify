using Learnify.Domain.Entities;
using MediatR;

namespace Learnify.Application.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(int Id) : IRequest<Category>; 
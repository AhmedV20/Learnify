using Learnify.Domain.Entities;
using MediatR;

namespace Learnify.Application.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery() : IRequest<List<Category>>; 
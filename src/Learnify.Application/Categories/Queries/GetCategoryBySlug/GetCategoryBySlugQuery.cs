using Learnify.Domain.Entities;
using MediatR;

namespace Learnify.Application.Categories.Queries.GetCategoryBySlug;

public record GetCategoryBySlugQuery(string Slug) : IRequest<Category>; 
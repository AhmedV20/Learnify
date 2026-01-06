using AutoMapper;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Sections.DTOs;
using Learnify.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Sections.Queries.GetSectionById;

public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, SectionResponse>
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IMapper _mapper;

    public GetSectionByIdQueryHandler(ISectionRepository sectionRepository, IMapper mapper)
    {
        _sectionRepository = sectionRepository;
        _mapper = mapper;
    }

    public async Task<SectionResponse> Handle(GetSectionByIdQuery request, CancellationToken cancellationToken)
    {
        var section = await _sectionRepository.GetByIdAsync(request.Id);

        if (section == null)
        {
            throw new NotFoundException(nameof(Section), request.Id);
        }

        return _mapper.Map<SectionResponse>(section);
    }
} 
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Common.Exceptions;
using Learnify.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Sections.Commands.UpdateSection;

public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand>
{
    private readonly ISectionRepository _sectionRepository;

    public UpdateSectionCommandHandler(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _sectionRepository.GetByIdAsync(request.Id);

        if (section == null)
        {
            throw new NotFoundException(nameof(Section), request.Id);
        }

        section.Title = request.Title;
        section.Order = request.Order;

        await _sectionRepository.UpdateAsync(section);
    }
} 
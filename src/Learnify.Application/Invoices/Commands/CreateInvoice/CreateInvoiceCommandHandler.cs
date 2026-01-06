using AutoMapper;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Invoice>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IMapper _mapper;


    public CreateInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IMapper mapper)
    {
        _invoiceRepository = invoiceRepository;
        _mapper = mapper;
    }

    public async Task<Invoice> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Create invoice
        var invoice = _mapper.Map<Invoice>(request);

        // Add it to the db
        await _invoiceRepository.AddInvoiceAsync(invoice);
        // Return invoice

        return invoice;
    }
}

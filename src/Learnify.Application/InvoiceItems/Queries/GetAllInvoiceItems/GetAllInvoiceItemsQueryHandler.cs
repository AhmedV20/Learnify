using Learnify.Application.Common.Interfaces;
using Learnify.Application.Invoices.Queries.GetAllInvoices;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.InvoiceItems.Queries.GetAllInvoiceItems;
 
public class GetAllInvoiceItemsQueryHandler : IRequestHandler<GetAllInvoiceItemsQuery, (IEnumerable<InvoiceItem>, int count)>
{
    private readonly IInvoiceItemRepository _invoiceItemRepository;

    public GetAllInvoiceItemsQueryHandler(IInvoiceItemRepository invoiceItemRepository)
    {
        _invoiceItemRepository = invoiceItemRepository;
    }

    public async Task<(IEnumerable<InvoiceItem>, int count)> Handle(GetAllInvoiceItemsQuery request, CancellationToken cancellationToken)
    {
        var (invoiceItems, totalCount) = await _invoiceItemRepository.GetAllInvoiceItemsAsync(request.PageNumber, request.PageSize);
        return (invoiceItems, totalCount);
    }
}
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.InvoiceItems.Queries.GetInvoiceItemById;

public record GetInvoiceItemByIdQuery(int Id) : IRequest<InvoiceItem>;

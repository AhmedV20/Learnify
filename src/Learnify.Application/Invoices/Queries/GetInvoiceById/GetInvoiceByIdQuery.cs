using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Invoices.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(int Id) : IRequest<Invoice>;
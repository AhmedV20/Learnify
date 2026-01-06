using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.InvoiceItems.Commands.CreateInvoiceItem;
public record CreateInvoiceItemCommand(int InvoiceId, int CourseId, string Description,  decimal OriginalUnitPrice, decimal DiscountAppliedOnItem, decimal UnitPrice) : IRequest<InvoiceItem>;

using AutoMapper;
using Learnify.Application.Enrollments.Commands.CreateEnrollment;
using Learnify.Application.Enrollments.Commands.UpdateEnrollment;
using Learnify.Application.Enrollments.DTOs.Requests;
using Learnify.Application.Enrollments.DTOs.Responses;
using Learnify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Mappings;

class EnrollmentMappings : Profile
{
    public EnrollmentMappings()
    {

        // Domain to Response DTO
        CreateMap<Enrollment, EnrollmentResponse>();

        // Command to Domain
        CreateMap<CreateEnrollmentCommand, Enrollment>();
        CreateMap<UpdateEnrollmentCommand, Enrollment>();

        // Request DTO to Command
        CreateMap<CreateEnrollmentRequest, CreateEnrollmentCommand>();


    // For UpdateEnrollmentCommand, we need to handle the Id parameter
    CreateMap<(UpdateEnrollmentRequest Request, int Id), UpdateEnrollmentCommand>()
        .ConstructUsing(src => new UpdateEnrollmentCommand(src.Id,src.Request.UserId, src.Request.CourseId, src.Request.EnrollmentDate, src.Request.PricePaid));
    }
}

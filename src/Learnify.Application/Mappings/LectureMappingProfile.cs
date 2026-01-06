using AutoMapper;
using Learnify.Application.Lectures.Commands.CreateLecture;
using Learnify.Application.Lectures.Commands.UpdateLecture;
using Learnify.Application.Lectures.Commands.UpdateLectureOrder;
using Learnify.Application.Lectures.DTOs;
using Learnify.Application.Lectures.DTOs.Requests;
using Learnify.Domain.Entities;

namespace Learnify.Application.Mappings
{
    public class LectureMappingProfile : Profile
    {
        public LectureMappingProfile()
        {
            CreateMap<Lecture, LectureResponse>()
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom<VideoUrlResolver>());
            CreateMap<CreateLectureCommand, Lecture>();
            CreateMap<CreateLectureRequest, CreateLectureCommand>();

            CreateMap<UpdateLectureRequest, UpdateLectureCommand>();
            CreateMap<UpdateLectureCommand, Lecture>();

            CreateMap<(UpdateLectureOrderRequest, int Id), UpdateLectureOrderCommand>();
        }
    }
}
 
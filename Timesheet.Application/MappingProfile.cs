using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Application.DTOs;
using Timesheet.Application.DTOs.Timesheet;
using Timesheet.Domain.Entities;
using Timesheet.Domain.Enums;

namespace Timesheet.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProjectCode, ProjectCodeResponseDto>().ReverseMap();
            CreateMap<ProjectCodeCreateDto, ProjectCode>();
            CreateMap<User, UserResponseDto>();
            CreateMap<ProjectAssignmentCreateDto,ProjectAssignment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectCode, opt => opt.Ignore());

            CreateMap< ProjectAssignment, ProjectAssignmentResponseDto>().ReverseMap();
            //CreateMap<Timesheet, TimesheetDto>().ReverseMap();
            // Timesheet entry hour <-> DTO
            CreateMap<TimesheetEntryHour, TimesheetEntryHourDto>()
                .ForMember(d => d.Date, opt => opt.MapFrom(s => s.WorkDate))
                .ForMember(d => d.Hours, opt => opt.MapFrom(s => s.HoursWorked))
                .ReverseMap()
                .ForMember(d => d.WorkDate, opt => opt.MapFrom(s => s.Date))
                .ForMember(d => d.HoursWorked, opt => opt.MapFrom(s => s.Hours));

            // Timesheet entry <-> DTO (Hours collection will map using the TimesheetEntryHour mapping)
            CreateMap<TimesheetEntryDto,TimesheetEntry>().ReverseMap()
                .ForMember(d=> d.ProjectName, opt=> opt.MapFrom(s=> s.ProjectCode.ProjectName))
                .ForMember(d => d.Hours, opt => opt.MapFrom(s => s.Hours))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => (int)s.Status))
                .ForMember(d => d.Comment, opt => opt.MapFrom(s => s.Comment))
                .ReverseMap()
                .ForMember(d => d.Hours, opt => opt.MapFrom(s => s.Hours));

            // Timesheet data <-> DTO
            // Map enum -> string for outgoing DTO, and parse string -> enum when mapping back.
            CreateMap<TimesheetData, TimesheetDto>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name))
                .ForMember(d => d.Entries, opt => opt.MapFrom(s => s.Entries))
                .ReverseMap()
                .ForMember(d => d.Entries, opt => opt.Ignore()); ;
                

            // Create DTOs -> Entities (used when creating new timesheets)
            CreateMap<CreateTimesheetDto, TimesheetData>()
                .ForMember(d => d.Entries, opt => opt.MapFrom(s => s.Entries));

            CreateMap<CreateTimesheetEntryDto, TimesheetEntry>()
                .ForMember(d => d.Hours, opt => opt.MapFrom(s => s.Hours));
        }
    }
}

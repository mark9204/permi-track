using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using PermiTrack.DataContext.Entites;
using PermiTrack.DataContext.DTOs;

namespace PermiTrack.DataContext.Mappings
{
   public class PermiTrackProfile : Profile
    {
        public PermiTrackProfile() 
        {
            //--Users
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>()
                .ForMember(d => d.PasswordHash, o => o.Ignore());

            //--Roles
            CreateMap<Role, RoleDTO>().ReverseMap();

            //--Permissions 
            CreateMap<Permission, PermissionDTO>().ReverseMap();

            //--Workflows & Steps 
            CreateMap<ApprovalWorkflowsDTO, ApprovalWorkflowsDTO>().ReverseMap();
            CreateMap<ApprovalStep, ApprovalStepDTO>().ReverseMap();

            // AccessRequests
            CreateMap<AccessRequest, AccessRequestDTO>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status));

            CreateMap<AccessRequestDTO, AccessRequest>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
                .ForMember(d => d.RequestedPermissions, o => o.Condition(s => s.RequestedPermissions != null));




        }
    }
}

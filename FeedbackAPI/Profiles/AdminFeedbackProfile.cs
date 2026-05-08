using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;

namespace FeedbackAPI.Profiles
{
    public class AdminFeedbackProfile : Profile
    {
        public AdminFeedbackProfile()
        {
            CreateMap<ProductFeedback, AdminFeedbackItemDto>()
                .ForMember(dest => dest.ReplyContent,
                           opt => opt.MapFrom(src => src.Reply))
                .ForMember(dest => dest.HasReply,
                           opt => opt.MapFrom(src =>
                               !string.IsNullOrEmpty(src.Reply)));
        }
    }
}

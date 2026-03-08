using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;

namespace FeedbackAPI.Profiles
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            CreateMap<CreateFeedbackDTO, ProductFeedback>();

            CreateMap<ProductFeedback, FeedbackResponseDTO>();
        }
    }
}
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Application.ExternalServices;
using TravelSwipe.Core.Features.Activities;

namespace TravelSwipe.Core.Features.Cities
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repository;
        private readonly IMapper _mapper;
        private readonly SerpApiService serpApiService;
        public async Task<IEnumerable<ActivityDto>> GetActivitiesByCity(string city)
        {
            var activityList = await _repository.GetActivitiesByCity(city);
            return _mapper.Map<IEnumerable<ActivityDto>>(activityList);
        }
        public async Task<IEnumerable<ActivityDto>> ScrapePhotosForActivity(string city)
        {
            var activitiesWithoutImages = await _repository.GetActivitiesWithoutImages(city);
            if (activitiesWithoutImages == null || activitiesWithoutImages.Count() > 0)
                return [];
            foreach (var activiti in activitiesWithoutImages)
            {
                var externalImages = await serpApiService.GetImages(activiti.Name, city);
                await _repository.AddImage(activiti.Name, city, externalImages);
            }
            return [];
        }


    }
}

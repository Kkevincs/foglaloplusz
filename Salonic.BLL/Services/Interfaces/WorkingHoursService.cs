using AutoMapper;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services.Interfaces
{
    public class WorkingHoursService : IWorkingHoursService
    {

        private readonly IWorkingHoursRepository _workingHoursRepository;
        private readonly IMapper _mapper;

        public WorkingHoursService(IWorkingHoursRepository workingHoursRepository, IMapper mapper)
        {
            _workingHoursRepository = workingHoursRepository;
            _mapper = mapper;
        }
        public async Task Set(WorkingHoursDTO dto)
        {
            var workDate = dto.Date.Date;

            var existing = await _workingHoursRepository.GetByArtistAndDate(dto.ArtistId, workDate);
            if (existing != null)
            {
                existing.Date = workDate;
                existing.StartTime = TimeSpan.Parse(dto.StartTime);
                existing.EndTime = TimeSpan.Parse(dto.EndTime);
                await _workingHoursRepository.Update(existing);
            }
            else
            {
                var workingHours = new WorkingHours
                {
                    ArtistId = dto.ArtistId,
                    Date = workDate,
                    StartTime = TimeSpan.Parse(dto.StartTime),
                    EndTime = TimeSpan.Parse(dto.EndTime)
                };
                await _workingHoursRepository.Add(workingHours);
            }
        }
    }
    
}

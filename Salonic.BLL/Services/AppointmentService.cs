using AutoMapper;
using Salonic.BLL.DTOs.AppointmentDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;

        public AppointmentService(IAppointmentRepository repo, IMapper mapper)
        {
            _appointmentRepository = repo;
            _mapper = mapper;
        }

        public async Task<AddAppointment> Create(AddAppointment app) {
            Appointment appointment = _mapper.Map<Appointment>(app);
            await _appointmentRepository.Create(appointment);
            return app;
        }

        public async Task DeleteById(int id) { 
            await _appointmentRepository.DeleteById(id);       
        }

        public async Task<List<GetAppointment>> GetAll() {
            var appointments = await _appointmentRepository.GetAll();
            var AppointmentDTOs = _mapper.Map<List<GetAppointment>>(appointments);
            return AppointmentDTOs;
        }

        public async Task<GetAppointment> GetById(int id) { 
            var appointment = await _appointmentRepository.GetById(id);
            var appointmentDTO = _mapper.Map<GetAppointment>(appointment);
            return appointmentDTO;
        }
        public async Task Update(GetAppointment app) {
            var appointment = _mapper.Map<Appointment>(app);
            await _appointmentRepository.Update(appointment);
        }
    }
}

using AutoMapper;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Models;
using Salonic.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salonic.BLL.Services
{
    public class ServiceService : IServiceService
    {

        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;



        public ServiceService(IServiceRepository repo, IMapper mapper)
        {
            _serviceRepository = repo;
            _mapper = mapper;
        }

        public async Task<ServiceWithIdDTO> Create(ServiceWithIdDTO service)
        {
            var Service = _mapper.Map<Service>(service);
            Service newService = await _serviceRepository.Create(Service);
            return service;
        }

        public async Task<ServiceWithoutIdDTO> Create(ServiceWithoutIdDTO service)
        {
            var serviceEntity = _mapper.Map<Service>(service);
            Service newService = await _serviceRepository.Create(serviceEntity);
            return service;
        }

        public async Task DeleteById(int id)
        {
            await _serviceRepository.DeleteById(id);
        }

        public async Task<List<ServiceWithIdDTO>> GetAll()
        {
            var services = await _serviceRepository.GetAll();
            var serviceDTO = _mapper.Map<List<ServiceWithIdDTO>>(services);
            return serviceDTO;
        }

        public async Task<ServiceWithIdDTO> GetById(int id)
        {
            var service = await _serviceRepository.GetById(id);
            var serviceDTO = _mapper.Map<ServiceWithIdDTO>(service);
            return serviceDTO;
        }

        public async Task Update(ServiceWithIdDTO service)
        {
            var serviceEntity = _mapper.Map<Service>(service);
            await _serviceRepository.Update(serviceEntity);
        }
    }
}

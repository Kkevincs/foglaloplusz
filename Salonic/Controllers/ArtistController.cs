using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services.Interfaces;
using Salonic.DAL.Data;
using Salonic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salonic.presentation.Controllers
{
    [Route("api/Artist")]
    [Authorize(Roles ="artist,admin")]
    [ApiController]
    public class ArtistController : ControllerBase
    {


        private readonly IArtistService _artistService;

        public ArtistController(IArtistService artist)
        {
            _artistService = artist;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArtistWithIdDTO>> GetById(int id)
        {
            return await _artistService.GetById(id);
        }
        [HttpGet]

        public async Task<ActionResult<List<ArtistWithIdDTO>>> GetAll()
        {
            return await _artistService.GetAll();
        }
        [HttpPost]
        public async Task<ActionResult<ArtistWithoutIdDTO>> Create(ArtistWithoutIdDTO artist)
        {
            return await _artistService.Create(artist);
        }

        [HttpPut]

        public async Task Update(ArtistWithIdDTO artist)
        {
            await _artistService.Update(artist);
        }

        [HttpDelete]

        public async Task DeleteById(int id)
        {
            await _artistService.DeleteById(id);
        }
    }
}

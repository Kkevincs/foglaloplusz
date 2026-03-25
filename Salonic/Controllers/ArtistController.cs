using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salonic.BLL.DTOs.ArtistDTOs;
using Salonic.BLL.DTOs.GuestDTOs;
using Salonic.BLL.DTOs.ServiceDTOs;
using Salonic.BLL.Services;
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
    [ApiController]
    public class ArtistController : ControllerBase
    {


        private readonly IArtistService _artistService;

        public ArtistController(IArtistService artist)
        {
            _artistService = artist;
        }

        [HttpGet("me")]
        [Authorize(Roles ="artist")]
        public async Task<ActionResult<ArtistWithIdDTO>> GetById()
        {
            var subClaim = User.FindFirst("sub")?.Value;
            if (subClaim == null) return Unauthorized();

            int Id = Convert.ToInt32(subClaim);
            return await _artistService.GetById(Id);
        }


        [HttpGet]
        [Authorize(Roles ="admin,owner,guest,artist")]
        public async Task<ActionResult<List<ArtistWithIdDTO>>> GetAll()
        {
            return await _artistService.GetAll();
        }
        [HttpPut]
        [Authorize(Roles ="admin,owner,artist")]

        public async Task Update(ArtistWithIdDTO artist)
        {
            await _artistService.Update(artist);
        }

        [HttpDelete]
        [Authorize(Roles = "admin,owner,artist")]

        public async Task DeleteById(int id)
        {
            await _artistService.DeleteById(id);
        }
    }
}

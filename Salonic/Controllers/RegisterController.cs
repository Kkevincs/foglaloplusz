using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Salonic.BLL.DTOs.UserDTOs;
using Salonic.BLL.Services.Interfaces;

namespace Salonic.presentation.Controllers
{
    [Route("api/register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IRegisterService _registerService;

        public RegisterController(IRegisterService reg)
        {
            _registerService = reg;
        }

        [HttpPost]
        public async Task<UserDTO> Register(UserDTO user) {
            return await _registerService.Register(user);
            
        } 
    }
}

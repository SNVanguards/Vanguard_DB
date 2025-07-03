using Microsoft.AspNetCore.Mvc;

using VanguardDev_API.Service;

namespace VanguardDev_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IUserService _userService;
        public WeatherForecastController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            var res = await _userService.GetUserEntities();
            return Ok(res);
        }
    }
}

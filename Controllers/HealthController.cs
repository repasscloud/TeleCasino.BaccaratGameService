using Microsoft.AspNetCore.Mvc;
namespace TeleCasino.BaccaratGameApi.Controllers;
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Healthy");
    }
}
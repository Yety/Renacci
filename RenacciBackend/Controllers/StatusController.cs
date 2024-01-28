using Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Renacci;
using Renacci.UseCases;

namespace RenacciBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController
{
    [HttpGet("GetStatus")]
    [ProducesResponseType<InverterStatus>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus([FromServices] GetStatusUseCase useCase)
    {
        var result = await useCase.GetStatus();
        return new OkObjectResult(result);
    }
}
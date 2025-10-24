using Microsoft.AspNetCore.Mvc;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Controllers;

[ApiController]
[Route("api/compiler")]
public sealed class CompilerController : ControllerBase
{
    private readonly ICompilerAnalysisService _compilerAnalysisService;

    public CompilerController(ICompilerAnalysisService compilerAnalysisService)
    {
        _compilerAnalysisService = compilerAnalysisService;
    }

    [HttpPost("analyze")]
    [ProducesResponseType(typeof(CompilerAnalysisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Analyze([FromBody] CompilerAnalysisRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _compilerAnalysisService.AnalyzeAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

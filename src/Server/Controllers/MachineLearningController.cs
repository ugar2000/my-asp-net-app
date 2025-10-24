using Microsoft.AspNetCore.Mvc;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Controllers;

[ApiController]
[Route("api/ml")]
public sealed class MachineLearningController : ControllerBase
{
    private readonly IDigitPredictionService _digitPredictionService;

    public MachineLearningController(IDigitPredictionService digitPredictionService)
    {
        _digitPredictionService = digitPredictionService;
    }

    [HttpPost("digit")]
    [ProducesResponseType(typeof(DigitPredictionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PredictDigit([FromBody] DigitPredictionRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _digitPredictionService.PredictAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return Problem("Prediction was cancelled.", statusCode: StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

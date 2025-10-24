using System.Threading;
using System.Threading.Tasks;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Abstractions;

/// <summary>
/// Exposes a consistent server-side inference contract so API controllers can remain thin.
/// </summary>
public interface IDigitPredictionService
{
    /// <summary>
    /// Runs inference over the supplied digit payload and enriches it with timing metadata.
    /// </summary>
    Task<DigitPredictionResponseDto> PredictAsync(
        DigitPredictionRequestDto request,
        CancellationToken cancellationToken);
}

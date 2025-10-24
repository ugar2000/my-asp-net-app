// Response object pairing the server ML.NET inference with the optional client-side ONNX result.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Describes the outcome of a digit prediction request so the UI can compare runtimes and accuracy.
/// </summary>
/// <param name="SessionId">Logical correlation identifier provided by the caller.</param>
/// <param name="PredictedDigit">Digit predicted by the server-side ML.NET model.</param>
/// <param name="Confidence">Server probability score for the winning digit (0-1).</param>
/// <param name="ClientPredictedDigit">Digit predicted in the browser via ONNX Runtime Web.</param>
/// <param name="ClientConfidence">Client confidence score for the ONNX prediction.</param>
/// <param name="ServerElapsedMilliseconds">Execution latency for the ML.NET inference path.</param>
/// <param name="ClientElapsedMilliseconds">Execution latency for the ONNX runtime in the browser.</param>
public sealed record DigitPredictionResponseDto(
    string SessionId,
    int PredictedDigit,
    float Confidence,
    int? ClientPredictedDigit,
    float? ClientConfidence,
    double ServerElapsedMilliseconds,
    double? ClientElapsedMilliseconds);

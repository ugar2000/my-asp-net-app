// Shared DTO describing the incoming MNIST-style digit payload so both the ML endpoint and
// WebAssembly ONNX client agree on the format without duplicating shape metadata.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Captures a digit canvas submission from the browser for inference on the ML.NET backend.
/// </summary>
/// <param name="SessionId">Logical correlation identifier for comparing server and client inference.</param>
/// <param name="Pixels">Flattened grayscale pixel intensities (0-1 range) read from the canvas.</param>
/// <param name="Width">Number of columns in the original drawing surface.</param>
/// <param name="Height">Number of rows in the original drawing surface.</param>
public sealed record DigitPredictionRequestDto(
    string SessionId,
    float[] Pixels,
    int Width,
    int Height);

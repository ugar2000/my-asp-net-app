// Options POCO describing where the ML assets live and how to warm them for inference.
namespace NetAppForVika.Server.Configuration;

/// <summary>
/// Configuration values bound from appsettings for the ML.NET and ONNX runtime assets.
/// </summary>
public sealed class MLModelOptions
{
    /// <summary>
    /// Path to the ML.NET zip model used for server-side inference.
    /// </summary>
    public string MlNetModelPath { get; set; } = "Resources/digit-mnist.zip";

    /// <summary>
    /// Path to the ONNX model mirrored on the server for parity checks and CDN distribution.
    /// </summary>
    public string OnnxModelPath { get; set; } = "Resources/digit.onnx";

    /// <summary>
    /// When true the hosted service pre-warms the prediction engine on application start.
    /// </summary>
    public bool WarmupOnStartup { get; set; } = true;
}

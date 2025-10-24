// Service wrapping ML.NET to run digit classification and compare with client-side ONNX inference.
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Server.Configuration;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Services;

/// <summary>
/// Provides server-side digit inference using ML.NET with a fallback heuristic for missing models.
/// </summary>
public sealed class MlDigitPredictionService : IDigitPredictionService
{
    private readonly MLContext _mlContext = new(seed: 23);
    private readonly ILogger<MlDigitPredictionService> _logger;
    private readonly IOptions<MLModelOptions> _options;
    private readonly Lazy<ITransformer?> _model;

    /// <summary>
    /// Initializes the service and defers model loading until the first prediction request arrives.
    /// </summary>
    public MlDigitPredictionService(
        ILogger<MlDigitPredictionService> logger,
        IOptions<MLModelOptions> options)
    {
        _logger = logger;
        _options = options;
        _model = new Lazy<ITransformer?>(LoadModel, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <inheritdoc />
    public async Task<DigitPredictionResponseDto> PredictAsync(
        DigitPredictionRequestDto request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var stopwatch = Stopwatch.StartNew();
        var prediction = await Task.Run(() => Score(request), cancellationToken);
        stopwatch.Stop();

        var client = ExtractClientMetrics(request);

        return new DigitPredictionResponseDto(
            request.SessionId,
            prediction.PredictedDigit,
            prediction.Confidence,
            client.Digit,
            client.Confidence,
            stopwatch.Elapsed.TotalMilliseconds,
            client.ElapsedMilliseconds);
    }

    /// <summary>
    /// Runs the ML.NET prediction engine or falls back to heuristics.
    /// </summary>
    private DigitPrediction Score(DigitPredictionRequestDto request)
    {
        var model = _model.Value;
        if (model is null)
        {
            _logger.LogWarning("ML.NET model not found at {Path}. Falling back to heuristic.", _options.Value.MlNetModelPath);
            return HeuristicPredict(request);
        }

        using var engine = _mlContext.Model.CreatePredictionEngine<DigitInput, DigitOutput>(model);
        var input = new DigitInput
        {
            Pixels = request.Pixels
        };

        var output = engine.Predict(input);
        var label = (int)output.PredictedLabel;
        var confidence = output.Score is { Length: > 0 } scores ? scores[label] : 0.5f;

        return new DigitPrediction(label, confidence);
    }

    /// <summary>
    /// Loads the ML.NET model from disk if it exists.
    /// </summary>
    private ITransformer? LoadModel()
    {
        var path = _options.Value.MlNetModelPath;
        if (!File.Exists(path))
        {
            return null;
        }

        using var stream = File.OpenRead(path);
        return _mlContext.Model.Load(stream, out _);
    }

    /// <summary>
    /// Generates a coarse prediction when the trained model is unavailable.
    /// </summary>
    private static DigitPrediction HeuristicPredict(DigitPredictionRequestDto request)
    {
        if (request.Pixels.Length == 0)
        {
            return new DigitPrediction(0, 0);
        }

        var bucketSize = Math.Max(request.Pixels.Length / 10, 1);
        var aggregates = new float[10];
        for (var i = 0; i < request.Pixels.Length; i++)
        {
            var bucket = Math.Min(i / bucketSize, aggregates.Length - 1);
            aggregates[bucket] += request.Pixels[i];
        }

        var max = aggregates
            .Select((value, index) => (value, index))
            .OrderByDescending(x => x.value)
            .FirstOrDefault();

        var total = aggregates.Sum();
        var confidence = total > 0 ? max.value / total : 0f;

        return new DigitPrediction(max.index, confidence);
    }

    /// <summary>
    /// Placeholder for future client-side telemetry integration.
    /// </summary>
    private static (int? Digit, float? Confidence, double? ElapsedMilliseconds) ExtractClientMetrics(
        DigitPredictionRequestDto request)
    {
        // The WebAssembly client attaches its inference metrics via SignalR, so this HTTP payload remains clean.
        return (null, null, null);
    }

    private sealed record DigitPrediction(int PredictedDigit, float Confidence);

    private sealed class DigitInput
    {
        [VectorType(28 * 28)]
        [ColumnName("Pixels")]
        public float[] Pixels { get; set; } = Array.Empty<float>();
    }

    private sealed class DigitOutput
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedLabel { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; } = Array.Empty<float>();
    }
}

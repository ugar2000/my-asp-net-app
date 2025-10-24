// Hosted service that proactively loads ML assets into memory so the first request is fast.
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Server.Configuration;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.ML;

/// <summary>
/// Performs a lightweight inference on startup to hydrate the ML.NET prediction engine.
/// </summary>
public sealed class ModelWarmupHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ModelWarmupHostedService> _logger;
    private readonly IOptions<MLModelOptions> _options;

    /// <summary>
    /// Creates the hosted service with access to scoped services for warmup.
    /// </summary>
    public ModelWarmupHostedService(
        IServiceProvider serviceProvider,
        ILogger<ModelWarmupHostedService> logger,
        IOptions<MLModelOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Value.WarmupOnStartup)
        {
            _logger.LogInformation("Skipping ML warmup because configuration disabled it.");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var predictor = scope.ServiceProvider.GetRequiredService<IDigitPredictionService>();

            var fakePixels = Enumerable.Repeat(0.0f, 28 * 28).ToArray();
            var request = new DigitPredictionRequestDto("warmup", fakePixels, 28, 28);

            await predictor.PredictAsync(request, cancellationToken);

            _logger.LogInformation("ML.NET model successfully warmed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ML.NET warmup failed; the application will continue and lazily load the model.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

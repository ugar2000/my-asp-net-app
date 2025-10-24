// SignalR hub that streams algorithm simulation steps to connected Blazor clients.
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Shared.Dtos;
using NetAppForVika.Shared.Enums;

namespace NetAppForVika.Server.Hubs;

/// <summary>
/// Broadcasts algorithm steps so multiple viewers can watch the same visualisation in sync.
/// </summary>
public sealed class AlgorithmHub : Hub
{
    private readonly IAlgorithmVisualizer _visualizer;
    private readonly ILogger<AlgorithmHub> _logger;

    /// <summary>
    /// Injects the algorithm visualiser that generates step-by-step animations.
    /// </summary>
    public AlgorithmHub(IAlgorithmVisualizer visualizer, ILogger<AlgorithmHub> logger)
    {
        _visualizer = visualizer;
        _logger = logger;
    }

    /// <summary>
    /// Streams algorithm steps to the caller as an async enumerable so Blazor can render them in real time.
    /// </summary>
    /// <param name="family">Family of algorithms being requested.</param>
    /// <param name="pathfinding">Optional pathfinding algorithm selection.</param>
    /// <param name="sorting">Optional sorting algorithm selection.</param>
    /// <param name="seed">Seed used to keep simulations reproducible.</param>
    /// <param name="cancellationToken">Propagated cancellation to stop simulation when the client disconnects.</param>
    /// <returns>Asynchronous stream of algorithm steps.</returns>
    public async IAsyncEnumerable<AlgorithmStepDto> StreamAlgorithmAsync(
        AlgorithmFamily family,
        PathfindingAlgorithmType? pathfinding,
        SortingAlgorithmType? sorting,
        int seed,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation("Client {ConnectionId} requested {Family} simulation with seed {Seed}.",
            Context.ConnectionId,
            family,
            seed);

        await foreach (var step in _visualizer.VisualizeAsync(
                           family == AlgorithmFamily.Pathfinding ? pathfinding : null,
                           family == AlgorithmFamily.Sorting ? sorting : null,
                           seed,
                           cancellationToken))
        {
            yield return step;
        }
    }
}

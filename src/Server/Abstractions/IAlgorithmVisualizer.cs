using System.Collections.Generic;
using System.Threading;
using NetAppForVika.Shared.Dtos;
using NetAppForVika.Shared.Enums;

namespace NetAppForVika.Server.Abstractions;

/// <summary>
/// Abstraction that generates a unified stream of visualisation steps for the Algorithm Theatre.
/// </summary>
public interface IAlgorithmVisualizer
{
    IAsyncEnumerable<AlgorithmStepDto> VisualizeAsync(
        PathfindingAlgorithmType? pathfindingAlgorithm,
        SortingAlgorithmType? sortingAlgorithm,
        int graphSeed,
        CancellationToken cancellationToken);
}

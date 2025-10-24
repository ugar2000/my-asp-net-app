// This DTO sits in the shared library because both the server SignalR hub and the Blazor client
// need to agree on how an algorithm step is represented when streamed in real time.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Represents a single snapshot in an algorithm visualisation stream so the frontend can render
/// synchronous state transitions without reinterpreting raw solver data.
/// </summary>
/// <param name="StepIndex">Zero-based order of the step within the current run.</param>
/// <param name="StateSnapshot">Compressed JSON payload describing the front-end drawing state.</param>
/// <param name="Cost">Running total cost used for algorithms such as Dijkstra or A*.</param>
/// <param name="Heuristic">Heuristic value recorded for transparency when visualising informed search.</param>
public sealed record AlgorithmStepDto(
    int StepIndex,
    string StateSnapshot,
    double Cost,
    double Heuristic);

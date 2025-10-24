// Service generating deterministic algorithm step sequences that the SignalR hub streams to clients.
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Shared.Dtos;
using NetAppForVika.Shared.Enums;

namespace NetAppForVika.Server.Services;

/// <summary>
/// Produces visualisation payloads for the Algorithm Theatre so the UI can animate server-driven logic.
/// </summary>
public sealed class AlgorithmVisualizer : IAlgorithmVisualizer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async IAsyncEnumerable<AlgorithmStepDto> VisualizeAsync(
        PathfindingAlgorithmType? pathfindingAlgorithm,
        SortingAlgorithmType? sortingAlgorithm,
        int graphSeed,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var random = new Random(graphSeed);
        var stepIndex = 0;

        if (pathfindingAlgorithm is { } pathAlgo)
        {
            foreach (var step in SimulatePathfinding(pathAlgo, random))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new AlgorithmStepDto(
                    stepIndex++,
                    JsonSerializer.Serialize(step.VisualState, SerializerOptions),
                    step.Cost,
                    step.Heuristic);

                // Short delay keeps the stream digestible for the front-end animation loop.
                await Task.Delay(TimeSpan.FromMilliseconds(18), cancellationToken);
            }
        }

        if (sortingAlgorithm is { } sortAlgo)
        {
            foreach (var step in SimulateSorting(sortAlgo, random))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new AlgorithmStepDto(
                    stepIndex++,
                    JsonSerializer.Serialize(step.VisualState, SerializerOptions),
                    step.Cost,
                    step.Heuristic);

                await Task.Delay(TimeSpan.FromMilliseconds(12), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Generates pseudo pathfinding steps for the requested algorithm type.
    /// </summary>
    private static IEnumerable<PathfindingStep> SimulatePathfinding(PathfindingAlgorithmType algorithm, Random random)
    {
        const int gridSize = 12;
        var start = new GridPoint(0, 0);
        var goal = new GridPoint(gridSize - 1, gridSize - 1);

        var open = new List<GridPoint> { start };
        var closed = new HashSet<GridPoint>();
        var walls = GenerateWalls(gridSize, random);

        double cost = 0;

        while (open.Count > 0)
        {
            var current = algorithm == PathfindingAlgorithmType.AStar
                ? ExtractLowestHeuristic(open, goal)
                : open[0];

            open.Remove(current);
            if (!closed.Add(current))
            {
                continue;
            }

            var heuristic = algorithm == PathfindingAlgorithmType.AStar
                ? Heuristic(current, goal)
                : cost;

            yield return new PathfindingStep(
                new
                {
                    gridSize,
                    current,
                    goal,
                    walls,
                    open = open.ToArray(),
                    closed = closed.ToArray()
                },
                cost,
                heuristic);

            if (current == goal)
            {
                yield break;
            }

            foreach (var neighbor in ExpandNeighbors(current, gridSize))
            {
                if (walls.Contains(neighbor) || closed.Contains(neighbor))
                {
                    continue;
                }

                if (!open.Contains(neighbor))
                {
                    open.Add(neighbor);
                }
            }

            cost += random.NextDouble() + 0.1;
        }
    }

    /// <summary>
    /// Dispatches to the algorithm-specific sorting simulation.
    /// </summary>
    private static IEnumerable<SortingStep> SimulateSorting(SortingAlgorithmType algorithm, Random random)
    {
        var data = Enumerable.Range(0, 24).Select(_ => random.Next(5, 99)).ToArray();
        return algorithm switch
        {
            SortingAlgorithmType.MergeSort => MergeSort(data),
            SortingAlgorithmType.QuickSort => QuickSort(data),
            SortingAlgorithmType.HeapSort => HeapSort(data),
            _ => MergeSort(data)
        };
    }

    /// <summary>
    /// Yields snapshots for an iterative merge sort.
    /// </summary>
    private static IEnumerable<SortingStep> MergeSort(int[] data)
    {
        double cost = 0;

        var buffer = new int[data.Length];
        for (var size = 1; size < data.Length; size *= 2)
        {
            for (var leftStart = 0; leftStart < data.Length - 1; leftStart += 2 * size)
            {
                var mid = Math.Min(leftStart + size - 1, data.Length - 1);
                var rightEnd = Math.Min(leftStart + 2 * size - 1, data.Length - 1);
                Merge(leftStart, mid, rightEnd);
                yield return Snapshot(data, cost += rightEnd - leftStart, heuristic: size);
            }
        }

        yield return Snapshot(data, cost, heuristic: 0);

        void Merge(int left, int mid, int right)
        {
            var i = left;
            var j = mid + 1;
            var k = left;
            while (i <= mid && j <= right)
            {
                buffer[k++] = data[i] <= data[j] ? data[i++] : data[j++];
            }

            while (i <= mid)
            {
                buffer[k++] = data[i++];
            }

            while (j <= right)
            {
                buffer[k++] = data[j++];
            }

            for (var index = left; index <= right; index++)
            {
                data[index] = buffer[index];
            }
        }
    }

    /// <summary>
    /// Yields snapshots for a quick sort partitioning routine.
    /// </summary>
    private static IEnumerable<SortingStep> QuickSort(int[] data)
    {
        double cost = 0;

        foreach (var step in QuickSortInternal(0, data.Length - 1))
        {
            yield return step;
        }

        yield return Snapshot(data, cost, heuristic: 0);

        IEnumerable<SortingStep> QuickSortInternal(int left, int right)
        {
            if (left >= right)
            {
                yield break;
            }

            var pivot = data[right];
            var partition = left;
            for (var i = left; i < right; i++)
            {
                if (data[i] <= pivot)
                {
                    (data[i], data[partition]) = (data[partition], data[i]);
                    cost++;
                    yield return Snapshot(data, cost, heuristic: right - left);
                    partition++;
                }
            }

            (data[partition], data[right]) = (data[right], data[partition]);
            cost++;
            yield return Snapshot(data, cost, heuristic: right - left);

            foreach (var snapshot in QuickSortInternal(left, partition - 1))
            {
                yield return snapshot;
            }

            foreach (var snapshot in QuickSortInternal(partition + 1, right))
            {
                yield return snapshot;
            }
        }
    }

    /// <summary>
    /// Yields snapshots for heap sort using a max-heap structure.
    /// </summary>
    private static IEnumerable<SortingStep> HeapSort(int[] data)
    {
        double cost = 0;

        var length = data.Length;
        for (var i = length / 2 - 1; i >= 0; i--)
        {
            Heapify(length, i);
        }

        for (var i = length - 1; i > 0; i--)
        {
            (data[0], data[i]) = (data[i], data[0]);
            cost++;
            yield return Snapshot(data, cost, heuristic: i);
            Heapify(i, 0);
        }

        yield return Snapshot(data, cost, heuristic: 0);

        void Heapify(int heapSize, int root)
        {
            while (true)
            {
                var largest = root;
                var left = 2 * root + 1;
                var right = 2 * root + 2;

                if (left < heapSize && data[left] > data[largest])
                {
                    largest = left;
                }

                if (right < heapSize && data[right] > data[largest])
                {
                    largest = right;
                }

                if (largest == root)
                {
                    return;
                }

                (data[root], data[largest]) = (data[largest], data[root]);
                cost++;
                root = largest;
            }
        }
    }

    /// <summary>
    /// Builds a serialisable payload for charting the array state.
    /// </summary>
    private static SortingStep Snapshot(int[] data, double cost, double heuristic)
    {
        var payload = new
        {
            bars = data.ToImmutableArray(),
            markers = ImmutableArray<int>.Empty
        };

        return new SortingStep(payload, cost, heuristic);
    }

    /// <summary>
    /// Generates deterministic wall placements to keep pathfinding interesting.
    /// </summary>
    private static HashSet<GridPoint> GenerateWalls(int gridSize, Random random)
    {
        var walls = new HashSet<GridPoint>();
        for (var i = 0; i < gridSize * 2; i++)
        {
            var wall = new GridPoint(random.Next(0, gridSize), random.Next(0, gridSize));
            var isStart = wall.X == 0 && wall.Y == 0;
            var isGoal = wall.X == gridSize - 1 && wall.Y == gridSize - 1;
            if (isStart || isGoal)
            {
                continue;
            }

            walls.Add(wall);
        }

        return walls;
    }

    /// <summary>
    /// Returns the node with the best heuristic score for A*.
    /// </summary>
    private static GridPoint ExtractLowestHeuristic(List<GridPoint> open, GridPoint goal)
    {
        open.Sort((a, b) => Heuristic(a, goal).CompareTo(Heuristic(b, goal)));
        return open[0];
    }

    /// <summary>
    /// Manhattan distance heuristic for grid-based navigation.
    /// </summary>
    private static double Heuristic(GridPoint a, GridPoint b)
        => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    /// <summary>
    /// Enumerates valid neighbour nodes for the current point.
    /// </summary>
    private static IEnumerable<GridPoint> ExpandNeighbors(GridPoint point, int gridSize)
    {
        var offsets = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        foreach (var (dx, dy) in offsets)
        {
            var x = point.X + dx;
            var y = point.Y + dy;
            if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            {
                yield return new GridPoint(x, y);
            }
        }
    }

    private readonly record struct GridPoint(int X, int Y);

    private sealed record PathfindingStep(object VisualState, double Cost, double Heuristic);

    private sealed record SortingStep(object VisualState, double Cost, double Heuristic);
}

using System.Threading;
using System.Threading.Tasks;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Abstractions;

/// <summary>
/// Provides Roslyn syntax analysis and optional script execution.
/// </summary>
public interface ICompilerAnalysisService
{
    Task<CompilerAnalysisResponseDto> AnalyzeAsync(CompilerAnalysisRequestDto request, CancellationToken cancellationToken);
}

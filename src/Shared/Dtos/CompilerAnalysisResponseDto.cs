// Response DTO returned by the Roslyn analysis endpoint for the Compiler Playground module.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Summarises syntax tree output, diagnostics, and optional script execution results.
/// </summary>
public sealed record CompilerAnalysisResponseDto(
    string SyntaxTree,
    string[] Diagnostics,
    string ExecutionOutput);

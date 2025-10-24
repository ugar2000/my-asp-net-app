// Request DTO used by the Compiler Playground to run Roslyn analysis server-side.
namespace NetAppForVika.Shared.Dtos;

/// <summary>
/// Captures the code and options submitted for Roslyn-based analysis.
/// </summary>
/// <param name="Code">Source code entered in the collaborative playground editor.</param>
/// <param name="RunScript">When true, the server runs the snippet using Roslyn scripting.</param>
public sealed record CompilerAnalysisRequestDto(string Code, bool RunScript);

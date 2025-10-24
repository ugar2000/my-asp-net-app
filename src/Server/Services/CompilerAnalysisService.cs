// Service leveraging Roslyn to analyse code and optionally execute it via scripting.
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Shared.Dtos;

namespace NetAppForVika.Server.Services;

/// <summary>
/// Provides Roslyn syntax analysis and optional script execution for the Compiler Playground module.
/// </summary>
public sealed class CompilerAnalysisService : ICompilerAnalysisService
{
    private readonly ILogger<CompilerAnalysisService> _logger;

    /// <summary>
    /// Stores logging to report compilation diagnostics server-side.
    /// </summary>
    public CompilerAnalysisService(ILogger<CompilerAnalysisService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CompilerAnalysisResponseDto> AnalyzeAsync(CompilerAnalysisRequestDto request, CancellationToken cancellationToken)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(request.Code, cancellationToken: cancellationToken);
        var root = await syntaxTree.GetRootAsync(cancellationToken);
        var diagnostics = syntaxTree.GetDiagnostics(cancellationToken).Select(d => d.ToString()).ToArray();

        string output = string.Empty;
        if (request.RunScript)
        {
            output = await ExecuteScriptAsync(request.Code, cancellationToken);
        }

        var treeBuilder = new StringBuilder();
        BuildSyntaxTreeString(root, treeBuilder, indent: 0);

        return new CompilerAnalysisResponseDto(treeBuilder.ToString(), diagnostics, output);
    }

    /// <summary>
    /// Runs the provided code through Roslyn scripting and captures stdout output.
    /// </summary>
    private async Task<string> ExecuteScriptAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            var options = ScriptOptions.Default
                .WithReferences(typeof(object).Assembly, typeof(Enumerable).Assembly)
                .WithImports("System", "System.Linq");

            var stringWriter = new StringWriter();
            var previousOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                await CSharpScript.RunAsync(code, options, cancellationToken: cancellationToken);
                return stringWriter.ToString();
            }
            finally
            {
                Console.SetOut(previousOut);
            }
        }
        catch (CompilationErrorException ex)
        {
            _logger.LogWarning(ex, "Roslyn scripting failed.");
            return string.Join(Environment.NewLine, ex.Diagnostics.Select(d => d.ToString()));
        }
    }

    /// <summary>
    /// Recursively renders the syntax tree into a human-readable outline.
    /// </summary>
    private static void BuildSyntaxTreeString(SyntaxNode node, StringBuilder builder, int indent)
    {
        builder.Append(' ', indent * 2);
        builder.AppendLine(node.Kind().ToString());

        foreach (var child in node.ChildNodesAndTokens())
        {
            if (child.IsNode)
            {
                BuildSyntaxTreeString(child.AsNode()!, builder, indent + 1);
            }
            else
            {
                builder.Append(' ', (indent + 1) * 2);
                builder.AppendLine(child.Kind().ToString());
            }
        }
    }
}

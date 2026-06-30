using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Services.CPL
{
    public record DiagnosticMessage(string Severity, string Message, int Line, int Column);

    // A single member-completion entry: the name to insert and a readable signature.
    public sealed record MemberCompletion(string Name, string Description);

    // One overload shown in the parameter-info popup. The rendered signature is split
    // into Prefix + Parameters (comma-joined) + Suffix so the editor can highlight the
    // parameter the caret is currently on.
    public sealed record SignatureHelpSignature(string Prefix, IReadOnlyList<string> Parameters, string Suffix);

    // Parameter-info payload: every candidate overload, the overload to show first, and
    // the index of the parameter the caret is currently positioned on.
    public sealed record SignatureHelpResult(IReadOnlyList<SignatureHelpSignature> Signatures, int ActiveSignature, int ActiveParameter);

    public class CompileResult
    {
        public bool IsSuccess { get; set; }
        public byte[]? Assembly { get; set; }
        public List<DiagnosticMessage> Diagnostics { get; set; } = [];

        public static CompileResult Failure(List<DiagnosticMessage> d) =>
            new() { IsSuccess = false, Assembly = null, Diagnostics = d };

        public static CompileResult Success(byte[] asm, List<DiagnosticMessage> d)
            => new() { IsSuccess = true, Assembly = asm, Diagnostics = d };
    }
}

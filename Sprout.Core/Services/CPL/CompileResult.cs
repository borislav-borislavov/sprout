using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Services.CPL
{
    public record DiagnosticMessage(string Severity, string Message, int Line, int Column);

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

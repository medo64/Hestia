namespace Hestia;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

internal class OutputStore : IDisposable {

    private OutputStore() {
    }

    public Guid Id { get; } = Guid.NewGuid();

    private readonly StringBuilder TextBuilder = new();


    public OutputCopy Attach(Process process) {
        var sbStdOut = new StringBuilder();
        var sbStdErr = new StringBuilder();
        var outCopy = new OutputCopy(sbStdOut, sbStdErr);

        TextBuilder.AppendLine($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");

        process.OutputDataReceived += (sender, args) => {
            if (args.Data != null) {
                var text = args.Data;
                foreach (var line in text.Split(['\n'], StringSplitOptions.None)) {
                    sbStdOut.AppendLine(text);
                    TextBuilder.AppendLine("  " + line);
                }
            }
        };
        process.ErrorDataReceived += (sender, args) => {
            if (args.Data != null) {
                var text = args.Data;
                foreach (var line in text.Split(['\n'], StringSplitOptions.None)) {
                    sbStdErr.AppendLine(text);
                    TextBuilder.AppendLine("  " + line);
                }
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return outCopy;
    }

    public string GetText() {
        return TextBuilder.ToString();
    }

    public string GetHtmlText() {
        return WebUtility.HtmlEncode(TextBuilder.ToString());
    }

    public void Dispose() {
        Outputs.Remove(Id);  // use dispose to remove from static store
    }


    #region Static

    private static readonly Dictionary<Guid, OutputStore> Outputs = [];

    public static OutputStore GetNew() {
        var output = new OutputStore();
        Outputs.Add(output.Id, output);
        return output;
    }

    public static OutputStore? GetOutput(Guid id) {
        if (Outputs.TryGetValue(id, out var output)) {
            return output;
        }
        return null;
    }

    #endregion Static

    internal sealed class OutputCopy {

        internal OutputCopy(StringBuilder stdOutBuilder, StringBuilder stdErrBuilder) {
            StdOutBuilder = stdOutBuilder;
            StdErrBuilder = stdErrBuilder;
        }

        private readonly StringBuilder StdOutBuilder = new();
        private readonly StringBuilder StdErrBuilder = new();

        public string GetStdOut() {
            return StdOutBuilder.ToString();
        }

        public string GetStdErr() {
            return StdErrBuilder.ToString();
        }

    }


    internal static string[] SplitStdOutIntoLines(Process process, OutputCopy? copy) {
        return Helpers.SplitOutIntoLines((copy != null) ? copy.GetStdOut() : process.StandardOutput.ReadToEnd());
    }

    internal static string[] SplitStdErrIntoLines(Process process, OutputCopy? copy) {
        return Helpers.SplitOutIntoLines((copy != null) ? copy.GetStdErr() : process.StandardError.ReadToEnd());
    }

}

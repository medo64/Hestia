namespace Hestia;

using System;
using System.Collections.Generic;

internal static class Helpers {

    public static string[] SplitOutIntoLines(string output) {
        var lines = output.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        var outLines = new List<string>();
        foreach (var line in lines) {
            var outline = line.TrimEnd();
            if (outline.Length > 0) {
                outLines.Add(outline);
            }
        }
        return outLines.ToArray();
    }

}

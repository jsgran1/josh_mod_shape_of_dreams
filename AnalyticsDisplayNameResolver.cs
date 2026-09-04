using System;
using System.Text;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal static class AnalyticsDisplayNameResolver
    {
        internal static string ToDisplayName(string rawName, string fallback)
        {
            if (string.IsNullOrEmpty(rawName))
            {
                return fallback;
            }

            var normalized = rawName.Replace('_', ' ').Replace('-', ' ').Trim();
            if (normalized.Length == 0)
            {
                return fallback;
            }

            var tokens = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var start = 0;
            while (start < tokens.Length && IsInternalPrefix(tokens[start]))
            {
                start++;
            }

            if (start >= tokens.Length)
            {
                start = 0;
            }

            if (start == 0 && tokens.Length > 1)
            {
                return normalized;
            }

            var builder = new StringBuilder();
            for (var i = start; i < tokens.Length; i++)
            {
                AppendFriendlyToken(builder, tokens[i]);
            }

            return builder.Length > 0 ? builder.ToString() : fallback;
        }

        private static bool IsInternalPrefix(string token)
        {
            return string.Equals(token, "St", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "R", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "Skill", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "Mem", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "Gem", StringComparison.OrdinalIgnoreCase);
        }

        private static void AppendFriendlyToken(StringBuilder builder, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            for (var i = 0; i < token.Length; i++)
            {
                var ch = token[i];
                var previous = i > 0 ? token[i - 1] : '\0';
                var next = i + 1 < token.Length ? token[i + 1] : '\0';
                var splitBefore = i > 0
                    && char.IsUpper(ch)
                    && (char.IsLower(previous) || (char.IsUpper(previous) && next != '\0' && char.IsLower(next)));

                if (builder.Length > 0 && (i == 0 || splitBefore))
                {
                    builder.Append(' ');
                }

                builder.Append(ch);
            }
        }
    }
}

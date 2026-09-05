using System;
using System.Collections.Generic;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum ComparisonPresentationMetricState
    {
        Known,
        Estimated,
        Utility,
        NotApplicable,
        Unknown,
        Unsupported,
        Empty
    }

    internal enum ComparisonRecommendationState
    {
        Empty,
        Recommended,
        Suppressed
    }

    internal sealed class ComparisonPresentationView
    {
        internal ComparisonPresentationView(
            string candidateLabel,
            IReadOnlyList<ComparisonOptionPresentation> rankedDamageOptions,
            IReadOnlyList<ComparisonOptionPresentation> unrankedOptions,
            ComparisonOptionPresentation recommendedOption,
            ComparisonRecommendationState recommendationState,
            string recommendationText,
            string damageDimensionLabel,
            ComparisonPresentationMetricRow emptyRow)
        {
            CandidateLabel = candidateLabel ?? "";
            RankedDamageOptions = rankedDamageOptions ?? Array.Empty<ComparisonOptionPresentation>();
            UnrankedOptions = unrankedOptions ?? Array.Empty<ComparisonOptionPresentation>();
            RecommendedOption = recommendedOption;
            RecommendationState = recommendationState;
            RecommendationText = recommendationText ?? "";
            DamageDimensionLabel = damageDimensionLabel ?? "";
            EmptyRow = emptyRow;
        }

        internal string CandidateLabel { get; }
        internal IReadOnlyList<ComparisonOptionPresentation> RankedDamageOptions { get; }
        internal IReadOnlyList<ComparisonOptionPresentation> UnrankedOptions { get; }
        internal ComparisonOptionPresentation RecommendedOption { get; }
        internal ComparisonRecommendationState RecommendationState { get; }
        internal string RecommendationText { get; }
        internal string DamageDimensionLabel { get; }
        internal ComparisonPresentationMetricRow EmptyRow { get; }
        internal bool HasOptions
        {
            get { return RankedDamageOptions.Count > 0 || UnrankedOptions.Count > 0; }
        }
    }

    internal sealed class ComparisonOptionPresentation
    {
        internal ComparisonOptionPresentation(
            BuildOptionComparison comparison,
            string replacementLabel,
            int originalIndex,
            bool isDamageRanked,
            ComparisonPresentationMetricRow primaryDamageRow,
            IReadOnlyList<ComparisonPresentationMetricRow> projectedDamageRows,
            IReadOnlyList<ComparisonPresentationMetricRow> utilityRows,
            IReadOnlyList<ComparisonObservedContextRow> observedContextRows,
            IReadOnlyList<string> limitations)
        {
            Comparison = comparison;
            ReplacementLabel = replacementLabel ?? "";
            OriginalIndex = originalIndex;
            IsDamageRanked = isDamageRanked;
            PrimaryDamageRow = primaryDamageRow;
            ProjectedDamageRows = projectedDamageRows ?? Array.Empty<ComparisonPresentationMetricRow>();
            UtilityRows = utilityRows ?? Array.Empty<ComparisonPresentationMetricRow>();
            ObservedContextRows = observedContextRows ?? Array.Empty<ComparisonObservedContextRow>();
            Limitations = limitations ?? Array.Empty<string>();
        }

        internal BuildOptionComparison Comparison { get; }
        internal string ReplacementLabel { get; }
        internal int OriginalIndex { get; }
        internal bool IsDamageRanked { get; }
        internal ComparisonPresentationMetricRow PrimaryDamageRow { get; }
        internal IReadOnlyList<ComparisonPresentationMetricRow> ProjectedDamageRows { get; }
        internal IReadOnlyList<ComparisonPresentationMetricRow> UtilityRows { get; }
        internal IReadOnlyList<ComparisonObservedContextRow> ObservedContextRows { get; }
        internal IReadOnlyList<string> Limitations { get; }
    }

    internal sealed class ComparisonPresentationMetricRow
    {
        internal ComparisonPresentationMetricRow(
            ComparisonPresentationMetricState state,
            string metricId,
            string label,
            string beforeText,
            string afterText,
            string deltaText,
            string valueText,
            string detailText,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IReadOnlyList<string> limitations)
        {
            State = state;
            MetricId = metricId ?? "";
            Label = label ?? "";
            BeforeText = beforeText ?? "";
            AfterText = afterText ?? "";
            DeltaText = deltaText ?? "";
            ValueText = valueText ?? "";
            DetailText = detailText ?? "";
            ResultClass = resultClass;
            Confidence = confidence;
            Limitations = limitations ?? Array.Empty<string>();
        }

        internal ComparisonPresentationMetricState State { get; }
        internal string MetricId { get; }
        internal string Label { get; }
        internal string BeforeText { get; }
        internal string AfterText { get; }
        internal string DeltaText { get; }
        internal string ValueText { get; }
        internal string DetailText { get; }
        internal ComparisonResultClass ResultClass { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
    }

    internal sealed class ComparisonObservedContextRow
    {
        internal ComparisonObservedContextRow(string metricId, string label, string valueText, string scope, string sourceReference)
        {
            MetricId = metricId ?? "";
            Label = label ?? "";
            ValueText = valueText ?? "";
            Scope = scope ?? "";
            SourceReference = sourceReference ?? "";
        }

        internal string MetricId { get; }
        internal string Label { get; }
        internal string ValueText { get; }
        internal string Scope { get; }
        internal string SourceReference { get; }
    }

    internal static class ComparisonPresentationShell
    {
        private const float ZeroTolerance = 0.0001f;

        internal static ComparisonPresentationView BuildView(IEnumerable<BuildOptionComparison> comparisons)
        {
            var options = CopyComparisons(comparisons);
            if (options.Count == 0)
            {
                return new ComparisonPresentationView(
                    "No candidate",
                    Array.Empty<ComparisonOptionPresentation>(),
                    Array.Empty<ComparisonOptionPresentation>(),
                    null,
                    ComparisonRecommendationState.Empty,
                    "No comparison options available",
                    "",
                    CreateEmptyRow("No comparison options available"));
            }

            var optionViews = new List<ComparisonOptionPresentation>();
            for (var i = 0; i < options.Count; i++)
            {
                optionViews.Add(BuildOption(options[i], i));
            }

            var comparableMetricId = ResolveComparableDamageMetricId(optionViews);
            var ranked = new List<ComparisonOptionPresentation>();
            var unranked = new List<ComparisonOptionPresentation>();
            for (var i = 0; i < optionViews.Count; i++)
            {
                var option = optionViews[i];
                if (IsComparableDamageOption(option, comparableMetricId))
                {
                    ranked.Add(MarkRanked(option));
                }
                else
                {
                    unranked.Add(option);
                }
            }

            ranked.Sort(CompareRankedDamageOptions);
            var recommendationState = ComparisonRecommendationState.Suppressed;
            ComparisonOptionPresentation recommended = null;
            var recommendationText = ResolveSuppressionText(optionViews, comparableMetricId);
            if (ranked.Count > 0 && IsRecommendationSafe(optionViews, comparableMetricId, out recommendationText))
            {
                recommendationState = ComparisonRecommendationState.Recommended;
                recommended = ranked[0];
                recommendationText = "Best known damage improvement";
            }

            return new ComparisonPresentationView(
                ResolveCandidateLabel(options[0]),
                ranked.AsReadOnly(),
                unranked.AsReadOnly(),
                recommended,
                recommendationState,
                recommendationText,
                ranked.Count > 0 ? ranked[0].PrimaryDamageRow.Label : "",
                null);
        }

        internal static string FormatMetricValue(float? value, string unit)
        {
            if (!value.HasValue)
            {
                return "-";
            }

            var suffix = string.IsNullOrEmpty(unit) ? "" : " " + unit;
            if (string.Equals(unit, "%", StringComparison.Ordinal))
            {
                return value.Value.ToString("0.##") + "%";
            }

            return value.Value.ToString("0.##") + suffix;
        }

        internal static string FormatDelta(float? deltaValue, float? deltaPercent, string unit)
        {
            if (!deltaValue.HasValue && !deltaPercent.HasValue)
            {
                return "-";
            }

            var result = "";
            if (deltaValue.HasValue)
            {
                result = FormatSignedMetricValue(deltaValue.Value, unit);
            }

            if (deltaPercent.HasValue)
            {
                var percent = FormatSignedRatio(deltaPercent.Value);
                result = string.IsNullOrEmpty(result) || result == "-"
                    ? percent
                    : result + " (" + percent + ")";
            }

            return result;
        }

        private static ComparisonOptionPresentation BuildOption(BuildOptionComparison comparison, int originalIndex)
        {
            var projectedRows = new List<ComparisonPresentationMetricRow>();
            var utilityRows = new List<ComparisonPresentationMetricRow>();
            var observedRows = new List<ComparisonObservedContextRow>();
            var limitations = ComparisonSemantics.CopyLimitations(comparison != null ? comparison.Limitations : null);

            if (comparison == null)
            {
                return new ComparisonOptionPresentation(
                    null,
                    "Unknown replacement",
                    originalIndex,
                    false,
                    CreateUnsupportedRow("Comparison unavailable", "comparison object was missing"),
                    projectedRows.AsReadOnly(),
                    utilityRows.AsReadOnly(),
                    observedRows.AsReadOnly(),
                    limitations.AsReadOnly());
            }

            var primaryDamageRow = CreateMetricRow(comparison.PrimaryDamageDelta);
            if (primaryDamageRow != null)
            {
                projectedRows.Add(primaryDamageRow);
            }

            for (var i = 0; i < comparison.Metrics.Count; i++)
            {
                var row = CreateMetricRow(comparison.Metrics[i]);
                if (row == null)
                {
                    continue;
                }

                if (primaryDamageRow != null && string.Equals(row.MetricId, primaryDamageRow.MetricId, StringComparison.Ordinal))
                {
                    continue;
                }

                projectedRows.Add(row);
            }

            for (var i = 0; i < comparison.UtilityChanges.Count; i++)
            {
                utilityRows.Add(CreateUtilityRow(comparison.UtilityChanges[i]));
            }

            for (var i = 0; i < comparison.ObservedContext.Count; i++)
            {
                observedRows.Add(CreateObservedRow(comparison.ObservedContext[i]));
            }

            if (projectedRows.Count == 0)
            {
                projectedRows.Add(CreateEmptyRow("No projected damage metrics"));
            }

            if (utilityRows.Count == 0)
            {
                utilityRows.Add(CreateEmptyRow("No utility changes"));
            }

            return new ComparisonOptionPresentation(
                comparison,
                ResolveReplacementLabel(comparison),
                originalIndex,
                false,
                primaryDamageRow,
                projectedRows.AsReadOnly(),
                utilityRows.AsReadOnly(),
                observedRows.AsReadOnly(),
                limitations.AsReadOnly());
        }

        private static ComparisonPresentationMetricRow CreateMetricRow(ComparisonMetric metric)
        {
            if (metric == null)
            {
                return null;
            }

            var state = ResolveMetricState(metric.ResultClass, metric.Confidence);
            var valueText = state == ComparisonPresentationMetricState.Unknown
                ? "Unknown"
                : state == ComparisonPresentationMetricState.Unsupported
                    ? "Unsupported"
                    : state == ComparisonPresentationMetricState.NotApplicable ? "N/A" : "";
            return new ComparisonPresentationMetricRow(
                state,
                metric.MetricId,
                metric.Label,
                state == ComparisonPresentationMetricState.Known || state == ComparisonPresentationMetricState.Estimated
                    ? FormatMetricValue(metric.BeforeValue, metric.Unit)
                    : "",
                state == ComparisonPresentationMetricState.Known || state == ComparisonPresentationMetricState.Estimated
                    ? FormatMetricValue(metric.AfterValue, metric.Unit)
                    : "",
                state == ComparisonPresentationMetricState.Known || state == ComparisonPresentationMetricState.Estimated
                    ? FormatDelta(metric.DeltaValue, metric.DeltaPercent, metric.Unit)
                    : "",
                valueText,
                GetResultDetail(metric.ResultClass, metric.Confidence),
                metric.ResultClass,
                metric.Confidence,
                metric.Limitations);
        }

        private static ComparisonPresentationMetricRow CreateUtilityRow(ComparisonUtilityChange utility)
        {
            if (utility == null)
            {
                return CreateUnsupportedRow("Utility unavailable", "utility object was missing");
            }

            var state = ResolveUtilityState(utility);
            var hasNumbers = utility.BeforeValue.HasValue || utility.AfterValue.HasValue || utility.DeltaValue.HasValue;
            var valueText = state == ComparisonPresentationMetricState.NotApplicable
                ? "N/A"
                : hasNumbers ? "" : utility.Description;
            return new ComparisonPresentationMetricRow(
                state,
                utility.UtilityId,
                string.IsNullOrEmpty(utility.Description) ? utility.UtilityId : utility.Description,
                hasNumbers ? FormatMetricValue(utility.BeforeValue, utility.Unit) : "",
                hasNumbers ? FormatMetricValue(utility.AfterValue, utility.Unit) : "",
                hasNumbers ? FormatDelta(utility.DeltaValue, null, utility.Unit) : "",
                valueText,
                GetResultDetail(utility.ResultClass, utility.Confidence),
                utility.ResultClass,
                utility.Confidence,
                utility.Limitations);
        }

        private static ComparisonObservedContextRow CreateObservedRow(ObservedContextMetric metric)
        {
            if (metric == null)
            {
                return new ComparisonObservedContextRow("unknown-observed-context", "Observed context unavailable", "Unknown", "", "");
            }

            return new ComparisonObservedContextRow(
                metric.MetricId,
                metric.Label,
                !string.IsNullOrEmpty(metric.TextValue) ? metric.TextValue : FormatMetricValue(metric.NumericValue, metric.Unit),
                metric.Scope,
                metric.SourceReference);
        }

        private static ComparisonPresentationMetricRow CreateEmptyRow(string label)
        {
            return new ComparisonPresentationMetricRow(
                ComparisonPresentationMetricState.Empty,
                "empty",
                label,
                "",
                "",
                "",
                label,
                "",
                ComparisonResultClass.Unknown,
                ComparisonConfidence.Unknown,
                Array.Empty<string>());
        }

        private static ComparisonPresentationMetricRow CreateUnsupportedRow(string label, string limitation)
        {
            return new ComparisonPresentationMetricRow(
                ComparisonPresentationMetricState.Unsupported,
                "unsupported",
                label,
                "",
                "",
                "",
                "Unsupported",
                "Unsupported",
                ComparisonResultClass.Unsupported,
                ComparisonConfidence.Unsupported,
                new[] { limitation });
        }

        private static ComparisonPresentationMetricState ResolveMetricState(
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence)
        {
            if (resultClass == ComparisonResultClass.Unsupported || confidence == ComparisonConfidence.Unsupported)
            {
                return ComparisonPresentationMetricState.Unsupported;
            }

            if (resultClass == ComparisonResultClass.NotApplicable)
            {
                return ComparisonPresentationMetricState.NotApplicable;
            }

            if (resultClass == ComparisonResultClass.Unknown || confidence == ComparisonConfidence.Unknown)
            {
                return ComparisonPresentationMetricState.Unknown;
            }

            if (resultClass == ComparisonResultClass.Estimated || confidence == ComparisonConfidence.Estimated)
            {
                return ComparisonPresentationMetricState.Estimated;
            }

            return ComparisonPresentationMetricState.Known;
        }

        private static ComparisonPresentationMetricState ResolveUtilityState(ComparisonUtilityChange utility)
        {
            if (utility.Confidence == ComparisonConfidence.Unsupported)
            {
                return ComparisonPresentationMetricState.Unsupported;
            }

            if (utility.ResultClass == ComparisonResultClass.NotApplicable)
            {
                return ComparisonPresentationMetricState.NotApplicable;
            }

            if (utility.Confidence == ComparisonConfidence.Unknown)
            {
                return ComparisonPresentationMetricState.Unknown;
            }

            return ComparisonPresentationMetricState.Utility;
        }

        private static string ResolveComparableDamageMetricId(IReadOnlyList<ComparisonOptionPresentation> options)
        {
            string metricId = null;
            for (var i = 0; i < options.Count; i++)
            {
                var row = options[i].PrimaryDamageRow;
                if (!IsKnownDamageRow(row))
                {
                    continue;
                }

                if (metricId == null)
                {
                    metricId = row.MetricId;
                    continue;
                }

                if (!string.Equals(metricId, row.MetricId, StringComparison.Ordinal))
                {
                    return "";
                }
            }

            return metricId ?? "";
        }

        private static bool IsComparableDamageOption(ComparisonOptionPresentation option, string comparableMetricId)
        {
            return !string.IsNullOrEmpty(comparableMetricId)
                && IsKnownDamageRow(option != null ? option.PrimaryDamageRow : null)
                && string.Equals(option.PrimaryDamageRow.MetricId, comparableMetricId, StringComparison.Ordinal);
        }

        private static bool IsKnownDamageRow(ComparisonPresentationMetricRow row)
        {
            return row != null
                && (row.State == ComparisonPresentationMetricState.Known || row.State == ComparisonPresentationMetricState.Estimated)
                && (row.ResultClass == ComparisonResultClass.Exact
                    || row.ResultClass == ComparisonResultClass.Derived
                    || row.ResultClass == ComparisonResultClass.Estimated
                    || row.ResultClass == ComparisonResultClass.StronglyInferred)
                && !string.IsNullOrEmpty(row.MetricId)
                && row.DeltaText != "-"
                && IsDamageMetric(row);
        }

        private static bool IsDamageMetric(ComparisonPresentationMetricRow row)
        {
            return ContainsIgnoreCase(row.MetricId, "damage")
                || ContainsIgnoreCase(row.MetricId, "dps")
                || ContainsIgnoreCase(row.Label, "damage")
                || ContainsIgnoreCase(row.Label, "dps")
                || ContainsIgnoreCase(row.ValueText, "damage");
        }

        private static bool IsRecommendationSafe(
            IReadOnlyList<ComparisonOptionPresentation> options,
            string comparableMetricId,
            out string suppressionText)
        {
            suppressionText = "";
            if (string.IsNullOrEmpty(comparableMetricId))
            {
                suppressionText = "Damage metrics are not comparable";
                return false;
            }

            for (var i = 0; i < options.Count; i++)
            {
                var row = options[i].PrimaryDamageRow;
                if (!IsComparableDamageOption(options[i], comparableMetricId))
                {
                    suppressionText = "At least one replacement has unknown or unsupported damage impact";
                    return false;
                }

                if (!IsRecommendationSafeConfidence(row.Confidence) || row.State == ComparisonPresentationMetricState.Estimated)
                {
                    suppressionText = "Damage confidence is too weak for a recommendation";
                    return false;
                }

                if (HasUnknownOrUnsupportedRows(options[i]))
                {
                    suppressionText = "Unsupported or unknown rows are present";
                    return false;
                }
            }

            return true;
        }

        private static bool IsRecommendationSafeConfidence(ComparisonConfidence confidence)
        {
            return confidence == ComparisonConfidence.Verified
                || confidence == ComparisonConfidence.HighConfidence
                || confidence == ComparisonConfidence.StronglyInferred;
        }

        private static bool HasUnknownOrUnsupportedRows(ComparisonOptionPresentation option)
        {
            if (option == null)
            {
                return true;
            }

            for (var i = 0; i < option.ProjectedDamageRows.Count; i++)
            {
                if (IsUnknownOrUnsupportedState(option.ProjectedDamageRows[i].State))
                {
                    return true;
                }
            }

            for (var i = 0; i < option.UtilityRows.Count; i++)
            {
                if (IsUnknownOrUnsupportedState(option.UtilityRows[i].State))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsUnknownOrUnsupportedState(ComparisonPresentationMetricState state)
        {
            return state == ComparisonPresentationMetricState.Unknown
                || state == ComparisonPresentationMetricState.Unsupported
                || state == ComparisonPresentationMetricState.NotApplicable;
        }

        private static string ResolveSuppressionText(IReadOnlyList<ComparisonOptionPresentation> options, string comparableMetricId)
        {
            if (options == null || options.Count == 0)
            {
                return "No comparison options available";
            }

            if (string.IsNullOrEmpty(comparableMetricId))
            {
                return "Damage metrics are not comparable";
            }

            return "Recommendation suppressed until all replacement damage impacts are comparable and safe";
        }

        private static ComparisonOptionPresentation MarkRanked(ComparisonOptionPresentation option)
        {
            return new ComparisonOptionPresentation(
                option.Comparison,
                option.ReplacementLabel,
                option.OriginalIndex,
                true,
                option.PrimaryDamageRow,
                option.ProjectedDamageRows,
                option.UtilityRows,
                option.ObservedContextRows,
                option.Limitations);
        }

        private static int CompareRankedDamageOptions(ComparisonOptionPresentation left, ComparisonOptionPresentation right)
        {
            var leftDelta = GetDelta(left);
            var rightDelta = GetDelta(right);
            var damageComparison = rightDelta.CompareTo(leftDelta);
            if (damageComparison != 0)
            {
                return damageComparison;
            }

            var labelComparison = string.Compare(left.ReplacementLabel, right.ReplacementLabel, StringComparison.Ordinal);
            if (labelComparison != 0)
            {
                return labelComparison;
            }

            return left.OriginalIndex.CompareTo(right.OriginalIndex);
        }

        private static float GetDelta(ComparisonOptionPresentation option)
        {
            return option != null && option.Comparison != null && option.Comparison.PrimaryDamageDelta != null && option.Comparison.PrimaryDamageDelta.DeltaValue.HasValue
                ? option.Comparison.PrimaryDamageDelta.DeltaValue.Value
                : float.MinValue;
        }

        private static IReadOnlyList<BuildOptionComparison> CopyComparisons(IEnumerable<BuildOptionComparison> comparisons)
        {
            if (comparisons == null)
            {
                return Array.Empty<BuildOptionComparison>();
            }

            return new List<BuildOptionComparison>(comparisons).AsReadOnly();
        }

        private static string ResolveCandidateLabel(BuildOptionComparison comparison)
        {
            if (comparison == null)
            {
                return "Unknown candidate";
            }

            return string.IsNullOrEmpty(comparison.Candidate.DisplayName)
                ? comparison.Candidate.StableId
                : comparison.Candidate.DisplayName;
        }

        private static string ResolveReplacementLabel(BuildOptionComparison comparison)
        {
            if (comparison == null || !comparison.ReplacementTarget.HasValue)
            {
                if (comparison != null && comparison.ActionKind == CandidateEquipActionKind.EquipIntoEmptySlot)
                {
                    return !string.IsNullOrEmpty(comparison.ActionLabel) ? comparison.ActionLabel : "Empty slot";
                }

                return "No replacement";
            }

            var target = comparison.ReplacementTarget.Value;
            return string.IsNullOrEmpty(target.DisplayName) ? target.StableId : target.DisplayName;
        }

        private static string GetResultDetail(ComparisonResultClass resultClass, ComparisonConfidence confidence)
        {
            return resultClass + " / " + confidence;
        }

        private static string FormatSignedMetricValue(float value, string unit)
        {
            if (Math.Abs(value) <= ZeroTolerance)
            {
                return "0" + (string.IsNullOrEmpty(unit) ? "" : string.Equals(unit, "%", StringComparison.Ordinal) ? "%" : " " + unit);
            }

            var sign = value > 0f ? "+" : "";
            if (string.Equals(unit, "%", StringComparison.Ordinal))
            {
                return sign + value.ToString("0.##") + "%";
            }

            return sign + value.ToString("0.##") + (string.IsNullOrEmpty(unit) ? "" : " " + unit);
        }

        private static string FormatSignedRatio(float ratio)
        {
            if (Math.Abs(ratio) <= ZeroTolerance)
            {
                return "0%";
            }

            return (ratio > 0f ? "+" : "") + (ratio * 100f).ToString("0.#") + "%";
        }

        private static bool ContainsIgnoreCase(string value, string pattern)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

using System;
using System.Collections.Generic;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum ContextualTooltipPresentationState
    {
        Known,
        Estimated,
        Unknown,
        Unsupported,
        Mixed,
        NativeFallback,
        Empty
    }

    internal enum ContextualTooltipIntegrationScope
    {
        ModelOnly
    }

    internal sealed class ContextualTooltipPresentationView
    {
        internal ContextualTooltipPresentationView(
            string subjectLabel,
            string memoryLabel,
            IReadOnlyList<string> attachedGemLabels,
            IReadOnlyList<ContextualTooltipContributionRow> contributionRows,
            EffectiveMemoryElementIndicatorPresentation effectiveElementIndicator,
            ContextualTooltipIntegrationScope integrationScope)
        {
            SubjectLabel = subjectLabel ?? "";
            MemoryLabel = memoryLabel ?? "";
            AttachedGemLabels = attachedGemLabels ?? Array.Empty<string>();
            ContributionRows = contributionRows ?? Array.Empty<ContextualTooltipContributionRow>();
            EffectiveElementIndicator = effectiveElementIndicator;
            IntegrationScope = integrationScope;
        }

        internal string SubjectLabel { get; }
        internal string MemoryLabel { get; }
        internal IReadOnlyList<string> AttachedGemLabels { get; }
        internal IReadOnlyList<ContextualTooltipContributionRow> ContributionRows { get; }
        internal EffectiveMemoryElementIndicatorPresentation EffectiveElementIndicator { get; }
        internal ContextualTooltipIntegrationScope IntegrationScope { get; }
        internal bool HasContributionRows
        {
            get { return ContributionRows.Count > 0 && ContributionRows[0].State != ContextualTooltipPresentationState.Empty; }
        }
    }

    internal sealed class ContextualTooltipContributionRow
    {
        internal ContextualTooltipContributionRow(
            ContextualTooltipPresentationState state,
            string effectId,
            string label,
            string beforeText,
            string afterText,
            string deltaText,
            string valueText,
            string effectiveElement,
            string detailText,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IReadOnlyList<string> limitations)
        {
            State = state;
            EffectId = effectId ?? "";
            Label = label ?? "";
            BeforeText = beforeText ?? "";
            AfterText = afterText ?? "";
            DeltaText = deltaText ?? "";
            ValueText = valueText ?? "";
            EffectiveElement = effectiveElement ?? "";
            DetailText = detailText ?? "";
            ResultClass = resultClass;
            Confidence = confidence;
            Limitations = limitations ?? Array.Empty<string>();
        }

        internal ContextualTooltipPresentationState State { get; }
        internal string EffectId { get; }
        internal string Label { get; }
        internal string BeforeText { get; }
        internal string AfterText { get; }
        internal string DeltaText { get; }
        internal string ValueText { get; }
        internal string EffectiveElement { get; }
        internal string DetailText { get; }
        internal ComparisonResultClass ResultClass { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
    }

    internal sealed class EffectiveMemoryElementIndicatorPresentation
    {
        internal EffectiveMemoryElementIndicatorPresentation(
            ContextualTooltipPresentationState state,
            string element,
            string nativeElement,
            string displayText,
            bool hasIndicator,
            bool usesNativeElement,
            bool isNeutral,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IReadOnlyList<string> limitations)
        {
            State = state;
            Element = element ?? "";
            NativeElement = nativeElement ?? "";
            DisplayText = displayText ?? "";
            HasIndicator = hasIndicator;
            UsesNativeElement = usesNativeElement;
            IsNeutral = isNeutral;
            ResultClass = resultClass;
            Confidence = confidence;
            Limitations = limitations ?? Array.Empty<string>();
        }

        internal ContextualTooltipPresentationState State { get; }
        internal string Element { get; }
        internal string NativeElement { get; }
        internal string DisplayText { get; }
        internal bool HasIndicator { get; }
        internal bool UsesNativeElement { get; }
        internal bool IsNeutral { get; }
        internal ComparisonResultClass ResultClass { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
    }

    internal static class ContextualTooltipPresentationModels
    {
        internal static ContextualTooltipPresentationView BuildGemContributionView(
            ContextualEffectEvaluation evaluation,
            string nativeMemoryElement)
        {
            var rows = new List<ContextualTooltipContributionRow>();
            if (evaluation == null)
            {
                rows.Add(CreateEmptyRow("Contextual evaluation unavailable"));
                return new ContextualTooltipPresentationView(
                    "Unknown context",
                    "",
                    Array.Empty<string>(),
                    rows.AsReadOnly(),
                    BuildEffectiveMemoryElementIndicator(null, nativeMemoryElement),
                    ContextualTooltipIntegrationScope.ModelOnly);
            }

            for (var i = 0; i < evaluation.EffectiveEffects.Count; i++)
            {
                rows.Add(CreateContributionRow(evaluation.EffectiveEffects[i]));
            }

            if (rows.Count == 0)
            {
                rows.Add(CreateEmptyRow("No contextual Gem contributions"));
            }

            return new ContextualTooltipPresentationView(
                ResolveSubjectLabel(evaluation.Subject),
                ResolveMemoryLabel(evaluation.MemoryContext),
                ResolveAttachedGemLabels(evaluation.AttachedGemContext),
                rows.AsReadOnly(),
                BuildEffectiveMemoryElementIndicator(evaluation.EffectiveElement, nativeMemoryElement),
                ContextualTooltipIntegrationScope.ModelOnly);
        }

        internal static EffectiveMemoryElementIndicatorPresentation BuildEffectiveMemoryElementIndicator(
            EffectiveElementResult effectiveElement,
            string nativeMemoryElement)
        {
            var nativeElement = nativeMemoryElement ?? "";
            if (effectiveElement == null)
            {
                return new EffectiveMemoryElementIndicatorPresentation(
                    ContextualTooltipPresentationState.Empty,
                    "",
                    nativeElement,
                    "",
                    false,
                    false,
                    true,
                    ComparisonResultClass.Unknown,
                    ComparisonConfidence.Unknown,
                    Array.Empty<string>());
            }

            var limitations = ComparisonSemantics.CopyLimitations(effectiveElement.Limitations);
            if (effectiveElement.Kind == EffectiveElementResultKind.Known && !string.IsNullOrEmpty(effectiveElement.Element))
            {
                var usesNative = !string.IsNullOrEmpty(nativeElement)
                    && string.Equals(effectiveElement.Element, nativeElement, StringComparison.OrdinalIgnoreCase);
                return new EffectiveMemoryElementIndicatorPresentation(
                    usesNative ? ContextualTooltipPresentationState.NativeFallback : ContextualTooltipPresentationState.Known,
                    effectiveElement.Element,
                    nativeElement,
                    effectiveElement.Element,
                    true,
                    usesNative,
                    false,
                    effectiveElement.ResultClass,
                    effectiveElement.Confidence,
                    limitations.AsReadOnly());
            }

            if (effectiveElement.Kind == EffectiveElementResultKind.Mixed)
            {
                return new EffectiveMemoryElementIndicatorPresentation(
                    ContextualTooltipPresentationState.Mixed,
                    "",
                    nativeElement,
                    "Mixed",
                    false,
                    false,
                    true,
                    effectiveElement.ResultClass,
                    effectiveElement.Confidence,
                    limitations.AsReadOnly());
            }

            var state = ComparisonSemantics.IsUnknownOrUnsupported(effectiveElement.ResultClass, effectiveElement.Confidence)
                && (effectiveElement.ResultClass == ComparisonResultClass.Unsupported || effectiveElement.Confidence == ComparisonConfidence.Unsupported)
                    ? ContextualTooltipPresentationState.Unsupported
                    : ContextualTooltipPresentationState.Unknown;
            return new EffectiveMemoryElementIndicatorPresentation(
                state,
                "",
                nativeElement,
                state == ContextualTooltipPresentationState.Unsupported ? "Unsupported" : "Unknown",
                false,
                false,
                true,
                effectiveElement.ResultClass,
                effectiveElement.Confidence,
                limitations.AsReadOnly());
        }

        private static ContextualTooltipContributionRow CreateContributionRow(EffectiveEffect effect)
        {
            if (effect == null)
            {
                return new ContextualTooltipContributionRow(
                    ContextualTooltipPresentationState.Unsupported,
                    "contextual-effect",
                    "Contextual effect unavailable",
                    "",
                    "",
                    "",
                    "Unsupported",
                    "",
                    "Unsupported",
                    ComparisonResultClass.Unsupported,
                    ComparisonConfidence.Unsupported,
                    new[] { "effective effect object was missing" });
            }

            var state = ResolveEffectState(effect.ResultClass, effect.Confidence);
            var canShowNumbers = state == ContextualTooltipPresentationState.Known
                || state == ContextualTooltipPresentationState.Estimated;
            var beforeText = canShowNumbers && effect.BeforeValue.HasValue
                ? ComparisonPresentationShell.FormatMetricValue(effect.BeforeValue, effect.Unit)
                : "";
            var afterText = canShowNumbers && effect.AfterValue.HasValue
                ? ComparisonPresentationShell.FormatMetricValue(effect.AfterValue, effect.Unit)
                : "";
            var deltaText = canShowNumbers && (effect.DeltaValue.HasValue || effect.DeltaPercent.HasValue)
                ? ComparisonPresentationShell.FormatDelta(effect.DeltaValue, effect.DeltaPercent, effect.Unit)
                : "";
            var valueText = ResolveValueText(effect, state, beforeText, afterText, deltaText);

            return new ContextualTooltipContributionRow(
                state,
                effect.EffectId,
                ResolveEffectLabel(effect),
                beforeText,
                afterText,
                deltaText,
                valueText,
                effect.EffectiveElement,
                effect.ResultClass + " / " + effect.Confidence,
                effect.ResultClass,
                effect.Confidence,
                effect.Limitations);
        }

        private static ContextualTooltipContributionRow CreateEmptyRow(string label)
        {
            return new ContextualTooltipContributionRow(
                ContextualTooltipPresentationState.Empty,
                "empty",
                label,
                "",
                "",
                "",
                label,
                "",
                "",
                ComparisonResultClass.Unknown,
                ComparisonConfidence.Unknown,
                Array.Empty<string>());
        }

        private static ContextualTooltipPresentationState ResolveEffectState(
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence)
        {
            if (resultClass == ComparisonResultClass.Unsupported || confidence == ComparisonConfidence.Unsupported)
            {
                return ContextualTooltipPresentationState.Unsupported;
            }

            if (resultClass == ComparisonResultClass.Unknown || confidence == ComparisonConfidence.Unknown)
            {
                return ContextualTooltipPresentationState.Unknown;
            }

            if (resultClass == ComparisonResultClass.Estimated || confidence == ComparisonConfidence.Estimated)
            {
                return ContextualTooltipPresentationState.Estimated;
            }

            return ContextualTooltipPresentationState.Known;
        }

        private static string ResolveValueText(
            EffectiveEffect effect,
            ContextualTooltipPresentationState state,
            string beforeText,
            string afterText,
            string deltaText)
        {
            if (state == ContextualTooltipPresentationState.Unknown)
            {
                return "Unknown";
            }

            if (state == ContextualTooltipPresentationState.Unsupported)
            {
                return "Unsupported";
            }

            if (!string.IsNullOrEmpty(deltaText) && deltaText != "-")
            {
                return deltaText;
            }

            if (!string.IsNullOrEmpty(afterText) && afterText != "-")
            {
                return afterText;
            }

            if (!string.IsNullOrEmpty(beforeText) && beforeText != "-")
            {
                return beforeText;
            }

            return !string.IsNullOrEmpty(effect.EffectiveDescription)
                ? effect.EffectiveDescription
                : effect.GenericDescription;
        }

        private static string ResolveEffectLabel(EffectiveEffect effect)
        {
            if (!string.IsNullOrEmpty(effect.EffectiveDescription))
            {
                return effect.EffectiveDescription;
            }

            if (!string.IsNullOrEmpty(effect.GenericDescription))
            {
                return effect.GenericDescription;
            }

            return effect.EffectId;
        }

        private static string ResolveSubjectLabel(ComparisonSubject subject)
        {
            return string.IsNullOrEmpty(subject.DisplayName) ? subject.StableId : subject.DisplayName;
        }

        private static string ResolveMemoryLabel(MemoryState memoryContext)
        {
            if (memoryContext == null)
            {
                return "";
            }

            return string.IsNullOrEmpty(memoryContext.MemoryKey.DisplayName)
                ? memoryContext.ContentId
                : memoryContext.MemoryKey.DisplayName;
        }

        private static IReadOnlyList<string> ResolveAttachedGemLabels(IReadOnlyList<GemState> attachedGemContext)
        {
            if (attachedGemContext == null || attachedGemContext.Count == 0)
            {
                return Array.Empty<string>();
            }

            var labels = new List<string>();
            for (var i = 0; i < attachedGemContext.Count; i++)
            {
                var gem = attachedGemContext[i];
                if (gem == null)
                {
                    continue;
                }

                labels.Add(string.IsNullOrEmpty(gem.GemKey.DisplayName) ? gem.ContentId : gem.GemKey.DisplayName);
            }

            return labels.AsReadOnly();
        }
    }
}

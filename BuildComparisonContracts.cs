using System;
using System.Collections.Generic;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum ComparisonResultClass
    {
        Exact,
        Derived,
        Estimated,
        StronglyInferred,
        Utility,
        Unknown,
        Unsupported
    }

    internal enum ComparisonConfidence
    {
        Verified,
        HighConfidence,
        StronglyInferred,
        Partial,
        Estimated,
        Unknown,
        Unsupported
    }

    internal enum ComparisonSubjectKind
    {
        Unknown,
        Build,
        Memory,
        Gem,
        ChaosReward,
        Stat,
        Effect
    }

    internal enum BuildChangeKind
    {
        None,
        Equip,
        Replace,
        Upgrade,
        Merge,
        Remove
    }

    internal readonly struct ComparisonSubject : IEquatable<ComparisonSubject>
    {
        internal ComparisonSubject(ComparisonSubjectKind kind, string stableId, string contentId, string displayName)
        {
            Kind = kind;
            StableId = string.IsNullOrEmpty(stableId) ? kind.ToString() : stableId;
            ContentId = contentId ?? "";
            DisplayName = string.IsNullOrEmpty(displayName) ? StableId : displayName;
        }

        internal ComparisonSubjectKind Kind { get; }
        internal string StableId { get; }
        internal string ContentId { get; }
        internal string DisplayName { get; }

        public bool Equals(ComparisonSubject other)
        {
            return Kind == other.Kind && string.Equals(StableId, other.StableId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is ComparisonSubject other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397) ^ (StableId != null ? StableId.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return Kind + ":" + StableId;
        }
    }

    internal sealed class ComparisonStructuredValue
    {
        internal ComparisonStructuredValue(
            string valueId,
            string label,
            float? numericValue,
            string unit,
            string textValue,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            ValueId = string.IsNullOrEmpty(valueId) ? "UNKNOWN_VALUE" : valueId;
            Label = string.IsNullOrEmpty(label) ? ValueId : label;
            NumericValue = numericValue;
            Unit = unit ?? "";
            TextValue = textValue ?? "";
            ResultClass = resultClass;
            Confidence = confidence;
            Limitations = ComparisonContractLists.Copy(limitations);
        }

        internal string ValueId { get; }
        internal string Label { get; }
        internal float? NumericValue { get; }
        internal string Unit { get; }
        internal string TextValue { get; }
        internal ComparisonResultClass ResultClass { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
        internal bool IsUnknownOrUnsupported
        {
            get { return ComparisonSemantics.IsUnknownOrUnsupported(ResultClass, Confidence); }
        }
    }

    internal sealed class BuildStatValue
    {
        internal BuildStatValue(string statId, float value, string unit, ComparisonConfidence confidence)
        {
            StatId = string.IsNullOrEmpty(statId) ? "UNKNOWN_STAT" : statId;
            Value = value;
            Unit = unit ?? "";
            Confidence = confidence;
        }

        internal string StatId { get; }
        internal float Value { get; }
        internal string Unit { get; }
        internal ComparisonConfidence Confidence { get; }
    }

    internal sealed class BuildModifierState
    {
        internal BuildModifierState(string modifierId, string sourceId, IEnumerable<ComparisonStructuredValue> values)
        {
            ModifierId = string.IsNullOrEmpty(modifierId) ? "UNKNOWN_MODIFIER" : modifierId;
            SourceId = sourceId ?? "";
            Values = ComparisonContractLists.Copy(values);
        }

        internal string ModifierId { get; }
        internal string SourceId { get; }
        internal IReadOnlyList<ComparisonStructuredValue> Values { get; }
    }

    internal sealed class MemoryState
    {
        internal MemoryState(
            MemoryKey memoryKey,
            string contentId,
            int slot,
            int? rank,
            int? level,
            float? quality,
            IEnumerable<GemKey> attachedGemKeys,
            IEnumerable<ComparisonStructuredValue> structuredValues)
        {
            MemoryKey = memoryKey;
            ContentId = string.IsNullOrEmpty(contentId) ? memoryKey.ContentId : contentId;
            Slot = slot;
            Rank = rank;
            Level = level;
            Quality = quality;
            AttachedGemKeys = ComparisonContractLists.Copy(attachedGemKeys);
            StructuredValues = ComparisonContractLists.Copy(structuredValues);
        }

        internal MemoryKey MemoryKey { get; }
        internal string ContentId { get; }
        internal int Slot { get; }
        internal int? Rank { get; }
        internal int? Level { get; }
        internal float? Quality { get; }
        internal IReadOnlyList<GemKey> AttachedGemKeys { get; }
        internal IReadOnlyList<ComparisonStructuredValue> StructuredValues { get; }
    }

    internal sealed class GemState
    {
        internal GemState(
            GemKey gemKey,
            string contentId,
            float? quality,
            MemoryKey? attachedMemoryKey,
            IEnumerable<ComparisonStructuredValue> structuredValues)
        {
            GemKey = gemKey;
            ContentId = string.IsNullOrEmpty(contentId) ? gemKey.ContentId : contentId;
            Quality = quality;
            AttachedMemoryKey = attachedMemoryKey;
            StructuredValues = ComparisonContractLists.Copy(structuredValues);
        }

        internal GemKey GemKey { get; }
        internal string ContentId { get; }
        internal float? Quality { get; }
        internal MemoryKey? AttachedMemoryKey { get; }
        internal IReadOnlyList<ComparisonStructuredValue> StructuredValues { get; }
    }

    internal sealed class BuildStateSnapshot
    {
        internal BuildStateSnapshot(
            PlayerKey playerKey,
            string characterId,
            IEnumerable<BuildStatValue> finalStats,
            IEnumerable<MemoryState> memories,
            IEnumerable<GemState> gems,
            IEnumerable<BuildModifierState> persistentModifiers,
            float capturedAt)
        {
            PlayerKey = playerKey;
            CharacterId = characterId ?? "";
            FinalStats = ComparisonContractLists.Copy(finalStats);
            Memories = ComparisonContractLists.Copy(memories);
            Gems = ComparisonContractLists.Copy(gems);
            PersistentModifiers = ComparisonContractLists.Copy(persistentModifiers);
            CapturedAt = capturedAt;
        }

        internal PlayerKey PlayerKey { get; }
        internal string CharacterId { get; }
        internal IReadOnlyList<BuildStatValue> FinalStats { get; }
        internal IReadOnlyList<MemoryState> Memories { get; }
        internal IReadOnlyList<GemState> Gems { get; }
        internal IReadOnlyList<BuildModifierState> PersistentModifiers { get; }
        internal float CapturedAt { get; }
    }

    internal sealed class CandidateChange
    {
        internal CandidateChange(BuildChangeKind changeKind, ComparisonSubject candidate, ComparisonSubject? replacementTarget)
        {
            ChangeKind = changeKind;
            Candidate = candidate;
            ReplacementTarget = replacementTarget;
        }

        internal BuildChangeKind ChangeKind { get; }
        internal ComparisonSubject Candidate { get; }
        internal ComparisonSubject? ReplacementTarget { get; }
        internal bool HasChange
        {
            get { return ChangeKind != BuildChangeKind.None; }
        }

        internal static CandidateChange None(ComparisonSubject subject)
        {
            return new CandidateChange(BuildChangeKind.None, subject, null);
        }
    }

    internal enum EffectiveElementResultKind
    {
        Unknown,
        Known,
        Mixed
    }

    internal sealed class EffectiveElementResult
    {
        internal EffectiveElementResult(
            EffectiveElementResultKind kind,
            string element,
            IEnumerable<string> elements,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            Kind = kind;
            Element = element ?? "";
            Elements = ComparisonContractLists.Copy(elements);
            ResultClass = resultClass;
            Confidence = confidence;
            Limitations = ComparisonContractLists.Copy(limitations);
        }

        internal EffectiveElementResultKind Kind { get; }
        internal string Element { get; }
        internal IReadOnlyList<string> Elements { get; }
        internal ComparisonResultClass ResultClass { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }

        internal static EffectiveElementResult Unknown(IEnumerable<string> limitations)
        {
            return new EffectiveElementResult(
                EffectiveElementResultKind.Unknown,
                "",
                null,
                ComparisonResultClass.Unknown,
                ComparisonConfidence.Unknown,
                limitations);
        }

        internal static EffectiveElementResult Known(string element, ComparisonConfidence confidence, IEnumerable<string> limitations)
        {
            return new EffectiveElementResult(
                EffectiveElementResultKind.Known,
                element,
                new[] { element },
                ComparisonResultClass.Derived,
                confidence,
                limitations);
        }

        internal static EffectiveElementResult Mixed(IEnumerable<string> elements, ComparisonConfidence confidence, IEnumerable<string> limitations)
        {
            return new EffectiveElementResult(
                EffectiveElementResultKind.Mixed,
                "Mixed",
                elements,
                ComparisonResultClass.Derived,
                confidence,
                limitations);
        }

        internal static EffectiveElementResult FromEffects(
            IEnumerable<EffectiveEffect> effects,
            ComparisonConfidence baseConfidence,
            IEnumerable<string> limitations)
        {
            var knownElements = new List<string>();
            var allLimitations = ComparisonSemantics.CopyLimitations(limitations);
            var confidence = baseConfidence;
            var hasUnknownElement = false;

            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    if (effect == null)
                    {
                        continue;
                    }

                    confidence = ComparisonSemantics.MostConservative(confidence, effect.Confidence);
                    ComparisonSemantics.AppendLimitations(allLimitations, effect.Limitations);

                    if (string.IsNullOrEmpty(effect.EffectiveElement))
                    {
                        if (effect.IsUnknownOrUnsupported)
                        {
                            hasUnknownElement = true;
                        }

                        continue;
                    }

                    if (!ContainsOrdinal(knownElements, effect.EffectiveElement))
                    {
                        knownElements.Add(effect.EffectiveElement);
                    }
                }
            }

            if (knownElements.Count == 0 || hasUnknownElement)
            {
                if (hasUnknownElement)
                {
                    allLimitations.Add("one or more effective elements are unknown");
                }

                return Unknown(allLimitations);
            }

            if (knownElements.Count == 1)
            {
                return Known(knownElements[0], confidence, allLimitations);
            }

            knownElements.Sort(StringComparer.Ordinal);
            return Mixed(knownElements, confidence, allLimitations);
        }

        private static bool ContainsOrdinal(List<string> values, string value)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal sealed class EffectiveEffect
    {
        internal EffectiveEffect(
            string effectId,
            string genericDescription,
            string effectiveDescription,
            float? beforeValue,
            float? afterValue,
            float? deltaValue,
            float? deltaPercent,
            string unit,
            string effectiveElement,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            EffectId = string.IsNullOrEmpty(effectId) ? "UNKNOWN_EFFECT" : effectId;
            GenericDescription = genericDescription ?? "";
            EffectiveDescription = effectiveDescription ?? "";
            BeforeValue = beforeValue;
            AfterValue = afterValue;
            DeltaValue = deltaValue;
            DeltaPercent = deltaPercent;
            Unit = unit ?? "";
            EffectiveElement = effectiveElement ?? "";
            ResultClass = resultClass;
            Confidence = confidence;
            Limitations = ComparisonContractLists.Copy(limitations);
        }

        internal string EffectId { get; }
        internal string GenericDescription { get; }
        internal string EffectiveDescription { get; }
        internal float? BeforeValue { get; }
        internal float? AfterValue { get; }
        internal float? DeltaValue { get; }
        internal float? DeltaPercent { get; }
        internal string Unit { get; }
        internal string EffectiveElement { get; }
        internal ComparisonResultClass ResultClass { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
        internal bool IsUnknownOrUnsupported
        {
            get { return ComparisonSemantics.IsUnknownOrUnsupported(ResultClass, Confidence); }
        }
        internal bool HasNumericProjection
        {
            get { return BeforeValue.HasValue || AfterValue.HasValue || DeltaValue.HasValue || DeltaPercent.HasValue; }
        }

        internal static EffectiveEffect Unknown(string effectId, string genericDescription, IEnumerable<string> limitations)
        {
            return new EffectiveEffect(
                effectId,
                genericDescription,
                "",
                null,
                null,
                null,
                null,
                "",
                "",
                ComparisonResultClass.Unknown,
                ComparisonConfidence.Unknown,
                limitations);
        }

        internal static EffectiveEffect Unsupported(string effectId, string genericDescription, IEnumerable<string> limitations)
        {
            return new EffectiveEffect(
                effectId,
                genericDescription,
                "",
                null,
                null,
                null,
                null,
                "",
                "",
                ComparisonResultClass.Unsupported,
                ComparisonConfidence.Unsupported,
                limitations);
        }
    }

    internal sealed class ContextualEffectEvaluation
    {
        internal ContextualEffectEvaluation(
            ComparisonSubject subject,
            MemoryState memoryContext,
            IEnumerable<GemState> attachedGemContext,
            CandidateChange candidateChange,
            IEnumerable<EffectiveEffect> effectiveEffects,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            Subject = subject;
            MemoryContext = memoryContext;
            AttachedGemContext = ComparisonContractLists.Copy(attachedGemContext);
            CandidateChange = candidateChange;
            EffectiveEffects = ComparisonContractLists.Copy(effectiveEffects);
            Confidence = confidence;
            Limitations = ComparisonContractLists.Copy(limitations);
            EffectiveElement = EffectiveElementResult.FromEffects(EffectiveEffects, Confidence, Limitations);
        }

        internal ComparisonSubject Subject { get; }
        internal MemoryState MemoryContext { get; }
        internal IReadOnlyList<GemState> AttachedGemContext { get; }
        internal CandidateChange CandidateChange { get; }
        internal IReadOnlyList<EffectiveEffect> EffectiveEffects { get; }
        internal EffectiveElementResult EffectiveElement { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
    }

    internal sealed class ComparisonMetric
    {
        internal ComparisonMetric(
            string metricId,
            string label,
            float? beforeValue,
            float? afterValue,
            float? deltaValue,
            float? deltaPercent,
            string unit,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            MetricId = string.IsNullOrEmpty(metricId) ? "UNKNOWN_METRIC" : metricId;
            Label = string.IsNullOrEmpty(label) ? MetricId : label;
            BeforeValue = beforeValue;
            AfterValue = afterValue;
            DeltaValue = deltaValue;
            DeltaPercent = deltaPercent;
            Unit = unit ?? "";
            ResultClass = resultClass;
            Confidence = confidence;
            Limitations = ComparisonContractLists.Copy(limitations);
        }

        internal string MetricId { get; }
        internal string Label { get; }
        internal float? BeforeValue { get; }
        internal float? AfterValue { get; }
        internal float? DeltaValue { get; }
        internal float? DeltaPercent { get; }
        internal string Unit { get; }
        internal ComparisonResultClass ResultClass { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
        internal bool IsUnknownOrUnsupported
        {
            get { return ComparisonSemantics.IsUnknownOrUnsupported(ResultClass, Confidence); }
        }
        internal bool HasNumericProjection
        {
            get { return BeforeValue.HasValue || AfterValue.HasValue || DeltaValue.HasValue || DeltaPercent.HasValue; }
        }
        internal bool HasKnownNumericDelta
        {
            get { return !IsUnknownOrUnsupported && (DeltaValue.HasValue || DeltaPercent.HasValue); }
        }

        internal static ComparisonMetric Unknown(string metricId, string label, IEnumerable<string> limitations)
        {
            return new ComparisonMetric(
                metricId,
                label,
                null,
                null,
                null,
                null,
                "",
                ComparisonResultClass.Unknown,
                ComparisonConfidence.Unknown,
                limitations);
        }

        internal static ComparisonMetric Unsupported(string metricId, string label, IEnumerable<string> limitations)
        {
            return new ComparisonMetric(
                metricId,
                label,
                null,
                null,
                null,
                null,
                "",
                ComparisonResultClass.Unsupported,
                ComparisonConfidence.Unsupported,
                limitations);
        }
    }

    internal sealed class ComparisonUtilityChange
    {
        internal ComparisonUtilityChange(
            string utilityId,
            string description,
            float? beforeValue,
            float? afterValue,
            float? deltaValue,
            string unit,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            UtilityId = string.IsNullOrEmpty(utilityId) ? "UNKNOWN_UTILITY" : utilityId;
            Description = description ?? "";
            BeforeValue = beforeValue;
            AfterValue = afterValue;
            DeltaValue = deltaValue;
            Unit = unit ?? "";
            Confidence = confidence;
            Limitations = ComparisonContractLists.Copy(limitations);
        }

        internal string UtilityId { get; }
        internal string Description { get; }
        internal float? BeforeValue { get; }
        internal float? AfterValue { get; }
        internal float? DeltaValue { get; }
        internal string Unit { get; }
        internal ComparisonResultClass ResultClass
        {
            get { return ComparisonResultClass.Utility; }
        }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
    }

    internal sealed class ObservedContextMetric
    {
        internal ObservedContextMetric(
            string metricId,
            string label,
            float? numericValue,
            string unit,
            string textValue,
            string scope,
            string sourceReference)
        {
            MetricId = string.IsNullOrEmpty(metricId) ? "UNKNOWN_OBSERVED_CONTEXT" : metricId;
            Label = string.IsNullOrEmpty(label) ? MetricId : label;
            NumericValue = numericValue;
            Unit = unit ?? "";
            TextValue = textValue ?? "";
            Scope = scope ?? "";
            SourceReference = sourceReference ?? "";
        }

        internal string MetricId { get; }
        internal string Label { get; }
        internal float? NumericValue { get; }
        internal string Unit { get; }
        internal string TextValue { get; }
        internal string Scope { get; }
        internal string SourceReference { get; }
    }

    internal sealed class BuildOptionComparison
    {
        internal BuildOptionComparison(
            ComparisonSubject candidate,
            ComparisonSubject? replacementTarget,
            IEnumerable<ComparisonMetric> metrics,
            IEnumerable<ComparisonUtilityChange> utilityChanges,
            IEnumerable<ObservedContextMetric> observedContext,
            ComparisonMetric primaryDamageDelta,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            Candidate = candidate;
            ReplacementTarget = replacementTarget;
            Metrics = ComparisonContractLists.Copy(metrics);
            UtilityChanges = ComparisonContractLists.Copy(utilityChanges);
            ObservedContext = ComparisonContractLists.Copy(observedContext);
            PrimaryDamageDelta = primaryDamageDelta;
            Confidence = confidence;
            Limitations = ComparisonContractLists.Copy(limitations);
        }

        internal ComparisonSubject Candidate { get; }
        internal ComparisonSubject? ReplacementTarget { get; }
        internal IReadOnlyList<ComparisonMetric> Metrics { get; }
        internal IReadOnlyList<ComparisonUtilityChange> UtilityChanges { get; }
        internal IReadOnlyList<ObservedContextMetric> ObservedContext { get; }
        internal ComparisonMetric PrimaryDamageDelta { get; }
        internal ComparisonConfidence Confidence { get; }
        internal IReadOnlyList<string> Limitations { get; }
    }

    internal static class ComparisonSemantics
    {
        internal static bool IsUnknownOrUnsupported(ComparisonResultClass resultClass, ComparisonConfidence confidence)
        {
            return resultClass == ComparisonResultClass.Unknown
                || resultClass == ComparisonResultClass.Unsupported
                || confidence == ComparisonConfidence.Unknown
                || confidence == ComparisonConfidence.Unsupported;
        }

        internal static ComparisonConfidence MostConservative(ComparisonConfidence left, ComparisonConfidence right)
        {
            return (int)left >= (int)right ? left : right;
        }

        internal static List<string> CopyLimitations(IEnumerable<string> limitations)
        {
            var result = new List<string>();
            AppendLimitations(result, limitations);
            return result;
        }

        internal static void AppendLimitations(List<string> target, IEnumerable<string> limitations)
        {
            if (target == null || limitations == null)
            {
                return;
            }

            foreach (var limitation in limitations)
            {
                if (string.IsNullOrEmpty(limitation))
                {
                    continue;
                }

                if (!ContainsOrdinal(target, limitation))
                {
                    target.Add(limitation);
                }
            }
        }

        private static bool ContainsOrdinal(List<string> values, string value)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal static class ComparisonContractLists
    {
        internal static IReadOnlyList<T> Copy<T>(IEnumerable<T> values)
        {
            if (values == null)
            {
                return new List<T>().AsReadOnly();
            }

            return new List<T>(values).AsReadOnly();
        }
    }
}

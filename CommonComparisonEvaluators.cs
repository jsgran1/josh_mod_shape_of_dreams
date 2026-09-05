using System;
using System.Collections.Generic;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal static class CommonComparisonValueIds
    {
        internal const string DirectDamage = "direct-damage";
        internal const string DamagePerHit = "damage-per-hit";
        internal const string ContextualMemoryDamage = "contextual-memory-damage";
        internal const string DamageModifier = "damage-modifier";
        internal const string BaseDamage = "base-damage";
        internal const string HitCount = "hit-count";
        internal const string Cooldown = "cooldown";
        internal const string Charges = "charges";
        internal const string Radius = "radius";
        internal const string Area = "area";
        internal const string Range = "range";
        internal const string AttackDamageCoefficient = "ad-coefficient";
        internal const string AbilityPowerCoefficient = "ap-coefficient";
        internal const string AttackDamage = "attack-damage";
        internal const string AbilityPower = "ability-power";
        internal const string WholeBuildDps = "whole-build-dps";
        internal const string StatPrefix = "stat:";
        internal const string MaterialUtilityPrefix = "utility:";
    }

    internal static class CommonComparisonEvaluators
    {
        private const float ZeroTolerance = 0.0001f;

        internal static ComparisonMetric EvaluateDirectDamage(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            return EvaluateNumericMetric(
                "direct-damage",
                "Direct damage",
                "damage",
                before,
                after,
                ComparisonResultClass.Exact,
                new[] { CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.DamagePerHit, CommonComparisonValueIds.BaseDamage });
        }

        internal static ComparisonMetric EvaluateContextualMemoryDamage(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            return EvaluateNumericMetric(
                "contextual-memory-damage",
                "Contextual Memory damage",
                "damage",
                before,
                after,
                ComparisonResultClass.Derived,
                new[] { CommonComparisonValueIds.ContextualMemoryDamage });
        }

        internal static ComparisonMetric EvaluateDamageModifier(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            return EvaluateNumericMetric(
                "damage-modifier",
                "Damage modifier",
                "multiplier",
                before,
                after,
                ComparisonResultClass.Derived,
                new[] { CommonComparisonValueIds.DamageModifier });
        }

        internal static ComparisonMetric EvaluateBaseDamage(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            return EvaluateNumericMetric(
                "base-damage",
                "Base damage",
                "damage",
                before,
                after,
                ComparisonResultClass.Exact,
                new[] { CommonComparisonValueIds.BaseDamage });
        }

        internal static ComparisonMetric EvaluateHitCount(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            return EvaluateNumericMetric(
                "hit-count",
                "Hit count",
                "hits",
                before,
                after,
                ComparisonResultClass.Exact,
                new[] { CommonComparisonValueIds.HitCount });
        }

        internal static ComparisonMetric EvaluateActivationDamage(
            ComparisonStructuredValue beforeDamagePerHit,
            ComparisonStructuredValue afterDamagePerHit,
            ComparisonStructuredValue beforeHitCount,
            ComparisonStructuredValue afterHitCount)
        {
            var limitations = new List<string>();
            var beforeKnown = IsKnownNumeric(beforeDamagePerHit, limitations, "before damage per hit")
                && IsKnownNumeric(beforeHitCount, limitations, "before hit count");
            var afterKnown = IsKnownNumeric(afterDamagePerHit, limitations, "after damage per hit")
                && IsKnownNumeric(afterHitCount, limitations, "after hit count");

            if (!beforeKnown || !afterKnown)
            {
                return ComparisonMetric.Unknown("damage-per-activation", "Damage per activation", limitations);
            }

            var beforeValue = beforeDamagePerHit.NumericValue.Value * beforeHitCount.NumericValue.Value;
            var afterValue = afterDamagePerHit.NumericValue.Value * afterHitCount.NumericValue.Value;
            var confidence = MostConservative(beforeDamagePerHit, afterDamagePerHit, beforeHitCount, afterHitCount);
            ComparisonSemantics.AppendLimitations(limitations, beforeDamagePerHit.Limitations);
            ComparisonSemantics.AppendLimitations(limitations, afterDamagePerHit.Limitations);
            ComparisonSemantics.AppendLimitations(limitations, beforeHitCount.Limitations);
            ComparisonSemantics.AppendLimitations(limitations, afterHitCount.Limitations);

            return CreateMetric(
                "damage-per-activation",
                "Damage per activation",
                beforeValue,
                afterValue,
                "damage",
                ComparisonResultClass.Derived,
                confidence,
                limitations);
        }

        internal static ComparisonMetric EvaluateAdScalingDamage(
            BuildStatValue beforeAttackDamage,
            BuildStatValue afterAttackDamage,
            ComparisonStructuredValue coefficient)
        {
            return EvaluateScaledDamage(
                "ad-scaling-damage",
                "AD scaling damage",
                CommonComparisonValueIds.AttackDamage,
                CommonComparisonValueIds.AttackDamageCoefficient,
                beforeAttackDamage,
                afterAttackDamage,
                coefficient);
        }

        internal static ComparisonMetric EvaluateApScalingDamage(
            BuildStatValue beforeAbilityPower,
            BuildStatValue afterAbilityPower,
            ComparisonStructuredValue coefficient)
        {
            return EvaluateScaledDamage(
                "ap-scaling-damage",
                "AP scaling damage",
                CommonComparisonValueIds.AbilityPower,
                CommonComparisonValueIds.AbilityPowerCoefficient,
                beforeAbilityPower,
                afterAbilityPower,
                coefficient);
        }

        internal static ComparisonMetric EvaluateCooldown(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            return EvaluateNumericMetric(
                "cooldown",
                "Cooldown",
                "s",
                before,
                after,
                ComparisonResultClass.Exact,
                new[] { CommonComparisonValueIds.Cooldown });
        }

        internal static ComparisonUtilityChange EvaluateCharges(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            return EvaluateUtilityChange(
                "charges",
                "Charges",
                "charges",
                before,
                after,
                new[] { CommonComparisonValueIds.Charges });
        }

        internal static ComparisonUtilityChange EvaluateRadiusAreaOrRange(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            var valueId = ResolveComparableValueId(before, after);
            if (IsValueId(valueId, CommonComparisonValueIds.Radius))
            {
                return EvaluateUtilityChange("radius", "Radius", "m", before, after, new[] { CommonComparisonValueIds.Radius });
            }

            if (IsValueId(valueId, CommonComparisonValueIds.Area))
            {
                return EvaluateUtilityChange("area", "Area", "m^2", before, after, new[] { CommonComparisonValueIds.Area });
            }

            if (IsValueId(valueId, CommonComparisonValueIds.Range))
            {
                return EvaluateUtilityChange("range", "Range", "m", before, after, new[] { CommonComparisonValueIds.Range });
            }

            return new ComparisonUtilityChange(
                "range-area",
                "Range/area change is unsupported",
                null,
                null,
                null,
                "",
                ComparisonConfidence.Unsupported,
                new[] { "structured radius, area, or range value was not provided" });
        }

        internal static ComparisonMetric EvaluateStatBonus(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            var valueId = ResolveComparableValueId(before, after);
            if (!IsStatValueId(valueId))
            {
                return ComparisonMetric.Unsupported(
                    "stat-bonus",
                    "Stat bonus",
                    new[] { "structured stat value id must use stat:<stat-id> or a known stat id" });
            }

            var statId = ExtractStatId(valueId);
            var label = !string.IsNullOrEmpty(after != null ? after.Label : null)
                ? after.Label
                : !string.IsNullOrEmpty(before != null ? before.Label : null) ? before.Label : statId;

            return EvaluateNumericMetric(
                "stat." + statId,
                label,
                ResolveUnit(before, after),
                before,
                after,
                ComparisonResultClass.Exact,
                new[] { valueId });
        }

        internal static ComparisonMetric EvaluateFinalStat(BuildStatValue before, BuildStatValue after, string label)
        {
            var limitations = new List<string>();
            if (before == null || after == null)
            {
                limitations.Add("final stat before and after values are required");
                return ComparisonMetric.Unknown("stat.final", string.IsNullOrEmpty(label) ? "Final stat" : label, limitations);
            }

            if (!string.Equals(before.StatId, after.StatId, StringComparison.Ordinal))
            {
                limitations.Add("before and after final stat ids did not match");
                return ComparisonMetric.Unsupported("stat." + before.StatId, string.IsNullOrEmpty(label) ? before.StatId : label, limitations);
            }

            return CreateMetric(
                "stat." + before.StatId,
                string.IsNullOrEmpty(label) ? before.StatId : label,
                before.Value,
                after.Value,
                string.IsNullOrEmpty(after.Unit) ? before.Unit : after.Unit,
                ComparisonResultClass.Exact,
                ComparisonSemantics.MostConservative(before.Confidence, after.Confidence),
                limitations);
        }

        internal static ComparisonMetric WholeBuildDpsUnknown(IEnumerable<string> limitations)
        {
            var copied = ComparisonSemantics.CopyLimitations(limitations);
            if (copied.Count == 0)
            {
                copied.Add("whole-build DPS requires mapped scaling, timing, and source contribution dependencies");
            }

            return ComparisonMetric.Unknown(CommonComparisonValueIds.WholeBuildDps, "Whole-build DPS", copied);
        }

        private static ComparisonMetric EvaluateScaledDamage(
            string metricId,
            string label,
            string requiredStatId,
            string requiredCoefficientId,
            BuildStatValue beforeStat,
            BuildStatValue afterStat,
            ComparisonStructuredValue coefficient)
        {
            var limitations = new List<string>();
            if (beforeStat == null || afterStat == null)
            {
                limitations.Add(requiredStatId + " before and after final stat values are required");
                return ComparisonMetric.Unknown(metricId, label, limitations);
            }

            if (!string.Equals(beforeStat.StatId, requiredStatId, StringComparison.Ordinal)
                || !string.Equals(afterStat.StatId, requiredStatId, StringComparison.Ordinal))
            {
                limitations.Add("scaling stat did not match " + requiredStatId);
                return ComparisonMetric.Unsupported(metricId, label, limitations);
            }

            if (coefficient == null)
            {
                limitations.Add(requiredCoefficientId + " is required");
                return ComparisonMetric.Unknown(metricId, label, limitations);
            }

            if (!IsValueId(coefficient.ValueId, requiredCoefficientId))
            {
                limitations.Add("coefficient value id did not match " + requiredCoefficientId);
                return ComparisonMetric.Unsupported(metricId, label, limitations);
            }

            if (!IsKnownNumeric(coefficient, limitations, "scaling coefficient"))
            {
                return ComparisonMetric.Unknown(metricId, label, limitations);
            }

            var beforeValue = beforeStat.Value * coefficient.NumericValue.Value;
            var afterValue = afterStat.Value * coefficient.NumericValue.Value;
            var confidence = ComparisonSemantics.MostConservative(
                ComparisonSemantics.MostConservative(beforeStat.Confidence, afterStat.Confidence),
                coefficient.Confidence);
            ComparisonSemantics.AppendLimitations(limitations, coefficient.Limitations);
            limitations.Add("per-effect scaling contribution only; whole-build DPS was not evaluated");

            return CreateMetric(
                metricId,
                label,
                beforeValue,
                afterValue,
                "damage",
                ComparisonResultClass.Derived,
                confidence,
                limitations);
        }

        private static ComparisonMetric EvaluateNumericMetric(
            string metricId,
            string label,
            string expectedUnit,
            ComparisonStructuredValue before,
            ComparisonStructuredValue after,
            ComparisonResultClass resultClass,
            IEnumerable<string> acceptedValueIds)
        {
            var limitations = new List<string>();
            if (before == null || after == null)
            {
                limitations.Add("before and after " + label + " values are required");
                return ComparisonMetric.Unknown(metricId, label, limitations);
            }

            if (!HasAcceptedValueId(before, acceptedValueIds) || !HasAcceptedValueId(after, acceptedValueIds))
            {
                limitations.Add("structured value id was not supported for " + label);
                return ComparisonMetric.Unsupported(metricId, label, limitations);
            }

            if (IsNotApplicable(before) || IsNotApplicable(after))
            {
                ComparisonSemantics.AppendLimitations(limitations, before.Limitations);
                ComparisonSemantics.AppendLimitations(limitations, after.Limitations);
                limitations.Add(label + " is not applicable for this option");
                return ComparisonMetric.NotApplicable(metricId, label, limitations);
            }

            if (!IsKnownNumeric(before, limitations, "before " + label) || !IsKnownNumeric(after, limitations, "after " + label))
            {
                return ComparisonMetric.Unknown(metricId, label, limitations);
            }

            ComparisonSemantics.AppendLimitations(limitations, before.Limitations);
            ComparisonSemantics.AppendLimitations(limitations, after.Limitations);

            return CreateMetric(
                metricId,
                label,
                before.NumericValue.Value,
                after.NumericValue.Value,
                string.IsNullOrEmpty(expectedUnit) ? ResolveUnit(before, after) : expectedUnit,
                resultClass,
                ComparisonSemantics.MostConservative(before.Confidence, after.Confidence),
                limitations);
        }

        private static ComparisonUtilityChange EvaluateUtilityChange(
            string utilityId,
            string description,
            string expectedUnit,
            ComparisonStructuredValue before,
            ComparisonStructuredValue after,
            IEnumerable<string> acceptedValueIds)
        {
            var limitations = new List<string>();
            if (before == null || after == null)
            {
                return new ComparisonUtilityChange(
                    utilityId,
                    description + " is unknown",
                    null,
                    null,
                    null,
                    "",
                    ComparisonConfidence.Unknown,
                    new[] { "before and after " + description + " values are required" });
            }

            if (!HasAcceptedValueId(before, acceptedValueIds) || !HasAcceptedValueId(after, acceptedValueIds))
            {
                return new ComparisonUtilityChange(
                    utilityId,
                    description + " is unsupported",
                    null,
                    null,
                    null,
                    "",
                    ComparisonConfidence.Unsupported,
                    new[] { "structured value id was not supported for " + description });
            }

            if (IsNotApplicable(before) || IsNotApplicable(after))
            {
                ComparisonSemantics.AppendLimitations(limitations, before.Limitations);
                ComparisonSemantics.AppendLimitations(limitations, after.Limitations);
                limitations.Add(description + " is not applicable for this option");
                return new ComparisonUtilityChange(
                    utilityId,
                    description + " is not applicable",
                    null,
                    null,
                    null,
                    "",
                    ComparisonResultClass.NotApplicable,
                    ComparisonConfidence.Verified,
                    limitations);
            }

            if (!IsKnownNumeric(before, limitations, "before " + description) || !IsKnownNumeric(after, limitations, "after " + description))
            {
                return new ComparisonUtilityChange(
                    utilityId,
                    description + " is unknown",
                    null,
                    null,
                    null,
                    "",
                    ComparisonConfidence.Unknown,
                    limitations);
            }

            ComparisonSemantics.AppendLimitations(limitations, before.Limitations);
            ComparisonSemantics.AppendLimitations(limitations, after.Limitations);

            return new ComparisonUtilityChange(
                utilityId,
                description,
                before.NumericValue.Value,
                after.NumericValue.Value,
                after.NumericValue.Value - before.NumericValue.Value,
                string.IsNullOrEmpty(expectedUnit) ? ResolveUnit(before, after) : expectedUnit,
                ComparisonSemantics.MostConservative(before.Confidence, after.Confidence),
                limitations);
        }

        private static ComparisonMetric CreateMetric(
            string metricId,
            string label,
            float beforeValue,
            float afterValue,
            string unit,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            var copied = ComparisonSemantics.CopyLimitations(limitations);
            float? deltaPercent = null;
            if (Math.Abs(beforeValue) > ZeroTolerance)
            {
                deltaPercent = (afterValue - beforeValue) / beforeValue;
            }
            else
            {
                copied.Add("delta percent unavailable because before value is zero");
            }

            return new ComparisonMetric(
                metricId,
                label,
                beforeValue,
                afterValue,
                afterValue - beforeValue,
                deltaPercent,
                unit,
                resultClass,
                confidence,
                copied);
        }

        private static bool IsKnownNumeric(ComparisonStructuredValue value, List<string> limitations, string label)
        {
            if (value == null)
            {
                limitations.Add(label + " value was missing");
                return false;
            }

            if (value.IsUnknownOrUnsupported)
            {
                ComparisonSemantics.AppendLimitations(limitations, value.Limitations);
                limitations.Add(label + " value was unknown or unsupported");
                return false;
            }

            if (!value.NumericValue.HasValue)
            {
                ComparisonSemantics.AppendLimitations(limitations, value.Limitations);
                limitations.Add(label + " numeric value was missing");
                return false;
            }

            return true;
        }

        private static bool IsNotApplicable(ComparisonStructuredValue value)
        {
            return value != null && value.ResultClass == ComparisonResultClass.NotApplicable;
        }

        private static ComparisonConfidence MostConservative(params ComparisonStructuredValue[] values)
        {
            var confidence = ComparisonConfidence.Verified;
            if (values == null)
            {
                return confidence;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    confidence = ComparisonSemantics.MostConservative(confidence, values[i].Confidence);
                }
            }

            return confidence;
        }

        private static bool HasAcceptedValueId(ComparisonStructuredValue value, IEnumerable<string> acceptedValueIds)
        {
            if (value == null || acceptedValueIds == null)
            {
                return false;
            }

            foreach (var acceptedValueId in acceptedValueIds)
            {
                if (IsValueId(value.ValueId, acceptedValueId))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolveComparableValueId(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            if (after != null && !string.IsNullOrEmpty(after.ValueId))
            {
                return after.ValueId;
            }

            return before != null ? before.ValueId : "";
        }

        private static string ResolveUnit(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            if (after != null && !string.IsNullOrEmpty(after.Unit))
            {
                return after.Unit;
            }

            return before != null ? before.Unit : "";
        }

        private static bool IsStatValueId(string valueId)
        {
            if (string.IsNullOrEmpty(valueId))
            {
                return false;
            }

            return valueId.StartsWith(CommonComparisonValueIds.StatPrefix, StringComparison.Ordinal)
                || IsValueId(valueId, CommonComparisonValueIds.AttackDamage)
                || IsValueId(valueId, CommonComparisonValueIds.AbilityPower);
        }

        private static string ExtractStatId(string valueId)
        {
            if (string.IsNullOrEmpty(valueId))
            {
                return "UNKNOWN_STAT";
            }

            if (valueId.StartsWith(CommonComparisonValueIds.StatPrefix, StringComparison.Ordinal))
            {
                return valueId.Substring(CommonComparisonValueIds.StatPrefix.Length);
            }

            return valueId;
        }

        private static bool IsValueId(string actual, string expected)
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}

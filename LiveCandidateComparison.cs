using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum LiveCandidateComparisonStatus
    {
        Empty,
        Ready,
        Unsupported,
        Error
    }

    internal enum LiveCandidateComparisonCandidateKind
    {
        Unknown,
        Gem,
        Memory
    }

    internal sealed class LiveCandidateComparisonSnapshot
    {
        internal LiveCandidateComparisonSnapshot(
            long sequenceId,
            LiveCandidateComparisonStatus status,
            LiveCandidateComparisonCandidateKind candidateKind,
            string reason,
            BuildStateSnapshot buildState,
            GemState candidateGem,
            MemoryState candidateMemory,
            IEnumerable<BuildOptionComparison> comparisons,
            IEnumerable<ContextualEffectEvaluation> contextualEvaluations,
            float capturedAt)
        {
            SequenceId = sequenceId;
            Status = status;
            CandidateKind = candidateKind;
            Reason = reason ?? "";
            BuildState = buildState;
            CandidateGem = candidateGem;
            CandidateMemory = candidateMemory;
            Comparisons = ComparisonContractLists.Copy(comparisons);
            ContextualEvaluations = ComparisonContractLists.Copy(contextualEvaluations);
            CapturedAt = capturedAt;
        }

        internal long SequenceId { get; }
        internal LiveCandidateComparisonStatus Status { get; }
        internal LiveCandidateComparisonCandidateKind CandidateKind { get; }
        internal string Reason { get; }
        internal BuildStateSnapshot BuildState { get; }
        internal GemState CandidateGem { get; }
        internal MemoryState CandidateMemory { get; }
        internal IReadOnlyList<BuildOptionComparison> Comparisons { get; }
        internal IReadOnlyList<ContextualEffectEvaluation> ContextualEvaluations { get; }
        internal float CapturedAt { get; }
    }

    internal sealed class LiveGemReplacementTarget
    {
        internal LiveGemReplacementTarget(GemState currentGem, MemoryState memoryContext, int gemIndex)
            : this(CandidateEquipActionKind.ReplaceExisting, currentGem, memoryContext, gemIndex)
        {
        }

        internal LiveGemReplacementTarget(CandidateEquipActionKind actionKind, GemState currentGem, MemoryState memoryContext, int gemIndex)
        {
            ActionKind = actionKind;
            CurrentGem = currentGem;
            MemoryContext = memoryContext;
            GemIndex = gemIndex;
        }

        internal CandidateEquipActionKind ActionKind { get; }
        internal GemState CurrentGem { get; }
        internal MemoryState MemoryContext { get; }
        internal int GemIndex { get; }
    }

    internal sealed class LiveMemoryReplacementTarget
    {
        internal LiveMemoryReplacementTarget(MemoryState currentMemory)
            : this(CandidateEquipActionKind.ReplaceExisting, currentMemory, currentMemory != null ? currentMemory.Slot : -1)
        {
        }

        internal LiveMemoryReplacementTarget(CandidateEquipActionKind actionKind, MemoryState currentMemory, int slot)
        {
            ActionKind = actionKind;
            CurrentMemory = currentMemory;
            Slot = slot;
        }

        internal CandidateEquipActionKind ActionKind { get; }
        internal MemoryState CurrentMemory { get; }
        internal int Slot { get; }
    }

    internal sealed class LiveCandidateComparisonService
    {
        private LiveCandidateComparisonSnapshot _current = EmptySnapshot(0, "not initialized", 0f);
        private long _sequence;

        internal LiveCandidateComparisonSnapshot CurrentSnapshot
        {
            get { return _current; }
        }

        internal LiveCandidateComparisonSnapshot RefreshGemCandidate(
            BuildStateSnapshot buildState,
            GemState candidate,
            IEnumerable<LiveGemReplacementTarget> legalTargets,
            bool duplicateMergeDetected,
            float capturedAt,
            RunDamageSnapshot runSnapshot = null)
        {
            if (duplicateMergeDetected)
            {
                var comparisons = buildState != null && candidate != null
                    ? new[] { BuildUnsupportedDuplicateMergeComparison(candidate) }
                    : null;
                return SetSnapshot(new LiveCandidateComparisonSnapshot(
                    NextSequence(),
                    LiveCandidateComparisonStatus.Unsupported,
                    LiveCandidateComparisonCandidateKind.Gem,
                    "duplicate Gem merge comparison is unavailable until the correlated merge path is validated",
                    buildState,
                    candidate,
                    null,
                    comparisons,
                    null,
                    capturedAt));
            }

            if (buildState == null || candidate == null)
            {
                return SetSnapshot(new LiveCandidateComparisonSnapshot(
                    NextSequence(),
                    LiveCandidateComparisonStatus.Empty,
                    LiveCandidateComparisonCandidateKind.Gem,
                    "Gem candidate or build state is unavailable",
                    buildState,
                    candidate,
                    null,
                    null,
                    null,
                    capturedAt));
            }

            var targets = CopyTargets(legalTargets);
            if (targets.Count == 0)
            {
                return SetSnapshot(new LiveCandidateComparisonSnapshot(
                    NextSequence(),
                    LiveCandidateComparisonStatus.Unsupported,
                    LiveCandidateComparisonCandidateKind.Gem,
                    "no legal Gem replacement targets were available",
                    buildState,
                    candidate,
                    null,
                    null,
                    null,
                    capturedAt));
            }

            var comparisons = new List<BuildOptionComparison>();
            var evaluations = new List<ContextualEffectEvaluation>();
            for (var i = 0; i < targets.Count; i++)
            {
                comparisons.Add(EvaluateGemReplacement(buildState, candidate, targets[i], runSnapshot, out var evaluation));
                evaluations.Add(evaluation);
            }

            return SetSnapshot(new LiveCandidateComparisonSnapshot(
                NextSequence(),
                LiveCandidateComparisonStatus.Ready,
                LiveCandidateComparisonCandidateKind.Gem,
                "Gem candidate comparison ready",
                buildState,
                candidate,
                null,
                comparisons,
                evaluations,
                capturedAt));
        }

        internal LiveCandidateComparisonSnapshot RefreshMemoryCandidate(
            BuildStateSnapshot buildState,
            MemoryState candidate,
            IEnumerable<LiveMemoryReplacementTarget> legalTargets,
            float capturedAt,
            RunDamageSnapshot runSnapshot = null)
        {
            if (buildState == null || candidate == null)
            {
                return SetSnapshot(new LiveCandidateComparisonSnapshot(
                    NextSequence(),
                    LiveCandidateComparisonStatus.Empty,
                    LiveCandidateComparisonCandidateKind.Memory,
                    "Memory candidate or build state is unavailable",
                    buildState,
                    null,
                    candidate,
                    null,
                    null,
                    capturedAt));
            }

            var targets = CopyTargets(legalTargets);
            if (targets.Count == 0)
            {
                return SetSnapshot(new LiveCandidateComparisonSnapshot(
                    NextSequence(),
                    LiveCandidateComparisonStatus.Unsupported,
                    LiveCandidateComparisonCandidateKind.Memory,
                    "no legal Memory replacement targets were available",
                    buildState,
                    null,
                    candidate,
                    null,
                    null,
                    capturedAt));
            }

            var comparisons = new List<BuildOptionComparison>();
            var evaluations = new List<ContextualEffectEvaluation>();
            for (var i = 0; i < targets.Count; i++)
            {
                comparisons.Add(EvaluateMemoryReplacement(buildState, candidate, targets[i], runSnapshot, out var evaluation));
                evaluations.Add(evaluation);
            }

            return SetSnapshot(new LiveCandidateComparisonSnapshot(
                NextSequence(),
                LiveCandidateComparisonStatus.Ready,
                LiveCandidateComparisonCandidateKind.Memory,
                "Memory candidate comparison ready",
                buildState,
                null,
                candidate,
                comparisons,
                evaluations,
                capturedAt));
        }

        internal LiveCandidateComparisonSnapshot Clear(string reason, float capturedAt)
        {
            return SetSnapshot(EmptySnapshot(NextSequence(), reason, capturedAt));
        }

        private BuildOptionComparison EvaluateGemReplacement(
            BuildStateSnapshot buildState,
            GemState candidate,
            LiveGemReplacementTarget target,
            RunDamageSnapshot runSnapshot,
            out ContextualEffectEvaluation evaluation)
        {
            var replacement = target.CurrentGem != null ? (ComparisonSubject?)ToSubject(target.CurrentGem) : null;
            var candidateSubject = ToSubject(candidate);
            var isEmptySlot = target.ActionKind == CandidateEquipActionKind.EquipIntoEmptySlot || target.CurrentGem == null;
            var candidateChange = new CandidateChange(isEmptySlot ? BuildChangeKind.Equip : BuildChangeKind.Replace, candidateSubject, replacement);
            var currentAttachedGems = ResolveAttachedGems(buildState, target.MemoryContext);
            var attachedGems = ApplyGemCandidate(buildState, target.MemoryContext, target.CurrentGem, candidate);
            evaluation = ContextualEffectEvaluator.Evaluate(buildState, target.MemoryContext, attachedGems, candidateChange);
            var beforeConfiguration = BuildContextualGemConfigurationValues(target.MemoryContext, currentAttachedGems, HasDamageModifier(currentAttachedGems) || HasDamageModifier(attachedGems), "current Gem configuration");
            var afterConfiguration = BuildContextualGemConfigurationValues(target.MemoryContext, attachedGems, HasDamageModifier(currentAttachedGems) || HasDamageModifier(attachedGems), "candidate Gem configuration");
            var metrics = MergeMetricRows(
                EvaluateStructuredMetrics(beforeConfiguration, afterConfiguration),
                isEmptySlot ? EvaluateDirectGemMetrics(BuildEmptySlotBaselineValues(candidate.StructuredValues), candidate.StructuredValues) : EvaluateDirectGemMetrics(target.CurrentGem.StructuredValues, candidate.StructuredValues));
            var utility = EvaluateStructuredUtility(beforeConfiguration, afterConfiguration);
            var observedContext = !isEmptySlot
                ? BuildGemObservedContext(runSnapshot, buildState.PlayerKey, target.CurrentGem.GemKey, target.MemoryContext != null ? (MemoryKey?)target.MemoryContext.MemoryKey : null)
                : Array.Empty<ObservedContextMetric>();
            var limitations = ComparisonSemantics.CopyLimitations(evaluation.Limitations);
            limitations.Add(isEmptySlot
                ? "Gem empty-slot action compares the affected Memory with current attached Gems versus the same Memory with the candidate Gem added; no item is removed"
                : "Gem replacement compares the affected Memory with current attached Gems versus the same Memory with the candidate Gem");
            var primary = ResolvePrimaryDamageMetric(metrics, isEmptySlot ? "Gem empty-slot damage impact is unknown" : "Gem replacement damage impact is unknown");

            return new BuildOptionComparison(
                isEmptySlot ? CandidateEquipActionKind.EquipIntoEmptySlot : CandidateEquipActionKind.ReplaceExisting,
                candidateSubject,
                replacement,
                isEmptySlot ? "Equip into empty Gem slot" : "Replace existing Gem",
                target.GemIndex,
                metrics,
                utility,
                observedContext,
                primary,
                ComparisonSemantics.MostConservative(evaluation.Confidence, primary != null ? primary.Confidence : ComparisonConfidence.Unknown),
                limitations);
        }

        private static BuildOptionComparison BuildUnsupportedDuplicateMergeComparison(GemState candidate)
        {
            var candidateSubject = ToSubject(candidate);
            var primary = ComparisonMetric.Unsupported(
                "duplicate-gem-merge",
                "Duplicate Gem merge",
                new[] { "duplicate Gem merge projection is unsupported until the correlated merge path is validated" });
            return new BuildOptionComparison(
                CandidateEquipActionKind.UnsupportedDuplicateMerge,
                candidateSubject,
                null,
                "Unsupported duplicate Gem merge",
                -1,
                new[] { primary },
                new[]
                {
                    new ComparisonUtilityChange(
                        CommonComparisonValueIds.MaterialUtilityPrefix + "duplicate-merge",
                        "Duplicate Gem merge result is unsupported",
                        null,
                        null,
                        null,
                        "",
                        ComparisonResultClass.Unsupported,
                        ComparisonConfidence.Unsupported,
                        primary.Limitations)
                },
                null,
                primary,
                ComparisonConfidence.Unsupported,
                primary.Limitations);
        }

        private BuildOptionComparison EvaluateMemoryReplacement(
            BuildStateSnapshot buildState,
            MemoryState candidate,
            LiveMemoryReplacementTarget target,
            RunDamageSnapshot runSnapshot,
            out ContextualEffectEvaluation evaluation)
        {
            var replacement = target.CurrentMemory != null ? (ComparisonSubject?)ToSubject(target.CurrentMemory) : null;
            var candidateSubject = ToSubject(candidate);
            var isEmptySlot = target.ActionKind == CandidateEquipActionKind.EquipIntoEmptySlot || target.CurrentMemory == null;
            var candidateChange = new CandidateChange(isEmptySlot ? BuildChangeKind.Equip : BuildChangeKind.Replace, candidateSubject, replacement);
            var attachedGems = isEmptySlot ? Array.Empty<GemState>() : ResolveAttachedGems(buildState, target.CurrentMemory);
            var projectedCandidate = new MemoryState(
                candidate.MemoryKey,
                candidate.ContentId,
                isEmptySlot ? target.Slot : target.CurrentMemory.Slot,
                candidate.Rank,
                candidate.Level,
                candidate.Quality,
                isEmptySlot ? Array.Empty<GemKey>() : target.CurrentMemory.AttachedGemKeys,
                candidate.StructuredValues);
            evaluation = ContextualEffectEvaluator.Evaluate(buildState, projectedCandidate, attachedGems, candidateChange);
            var beforeValues = isEmptySlot ? BuildEmptySlotBaselineValues(candidate.StructuredValues) : target.CurrentMemory.StructuredValues;
            var metrics = EvaluateStructuredMetrics(beforeValues, candidate.StructuredValues);
            var utility = isEmptySlot
                ? EvaluateStructuredUtility(Array.Empty<ComparisonStructuredValue>(), candidate.StructuredValues)
                : EvaluateStructuredUtility(beforeValues, candidate.StructuredValues);
            var observedContext = !isEmptySlot ? BuildMemoryObservedContext(runSnapshot, buildState.PlayerKey, target.CurrentMemory.MemoryKey) : Array.Empty<ObservedContextMetric>();
            var limitations = ComparisonSemantics.CopyLimitations(evaluation.Limitations);
            limitations.Add(isEmptySlot
                ? "Memory empty-slot action equips the candidate into an empty legal slot; no item or attached utility is removed"
                : "Memory replacement keeps the target slot's attached Gem context");
            var primary = ResolvePrimaryDamageMetric(metrics, isEmptySlot ? "Memory empty-slot damage impact is unknown" : "Memory replacement damage impact is unknown");

            return new BuildOptionComparison(
                isEmptySlot ? CandidateEquipActionKind.EquipIntoEmptySlot : CandidateEquipActionKind.ReplaceExisting,
                candidateSubject,
                replacement,
                isEmptySlot ? "Equip into empty Memory slot" : "Replace existing Memory",
                isEmptySlot ? target.Slot : target.CurrentMemory.Slot,
                metrics,
                utility,
                observedContext,
                primary,
                ComparisonSemantics.MostConservative(evaluation.Confidence, primary != null ? primary.Confidence : ComparisonConfidence.Unknown),
                limitations);
        }

        private static IReadOnlyList<ObservedContextMetric> BuildGemObservedContext(
            RunDamageSnapshot runSnapshot,
            PlayerKey playerKey,
            GemKey gemKey,
            MemoryKey? memoryKey)
        {
            var observed = new List<ObservedContextMetric>();
            var player = FindRunPlayer(runSnapshot, playerKey);
            if (player == null)
            {
                return observed.AsReadOnly();
            }

            var gem = FindGemDamage(player, gemKey);
            if (gem != null)
            {
                observed.Add(new ObservedContextMetric(
                    "observed.current-gem-direct",
                    "Current Gem direct this run",
                    gem.DirectDamage,
                    "damage",
                    FormatObservedDamage(gem.DirectDamage, gem.PlayerShare),
                    "immutable run snapshot",
                    gem.GemKey.StableId));
            }

            if (memoryKey.HasValue)
            {
                var package = FindMemoryPackage(player, memoryKey.Value);
                if (package != null)
                {
                    observed.Add(new ObservedContextMetric(
                        "observed.current-memory-package",
                        "Current Memory package this run",
                        package.TotalPackageDamage,
                        "damage",
                        FormatObservedDamage(package.TotalPackageDamage, package.PlayerShare),
                        "immutable run snapshot",
                        package.MemoryKey.StableId));
                }
            }

            return observed.AsReadOnly();
        }

        private static IReadOnlyList<ObservedContextMetric> BuildMemoryObservedContext(
            RunDamageSnapshot runSnapshot,
            PlayerKey playerKey,
            MemoryKey memoryKey)
        {
            var observed = new List<ObservedContextMetric>();
            var player = FindRunPlayer(runSnapshot, playerKey);
            if (player == null)
            {
                return observed.AsReadOnly();
            }

            var memory = FindMemoryDamage(player, memoryKey);
            if (memory != null)
            {
                observed.Add(new ObservedContextMetric(
                    "observed.current-memory-direct",
                    "Current Memory direct this run",
                    memory.DirectDamage,
                    "damage",
                    FormatObservedDamage(memory.DirectDamage, memory.PlayerShare),
                    "immutable run snapshot",
                    memory.MemoryKey.StableId));
            }

            var package = FindMemoryPackage(player, memoryKey);
            if (package != null)
            {
                observed.Add(new ObservedContextMetric(
                    "observed.current-memory-package",
                    "Current Memory package this run",
                    package.TotalPackageDamage,
                    "damage",
                    FormatObservedDamage(package.TotalPackageDamage, package.PlayerShare),
                    "immutable run snapshot",
                    package.MemoryKey.StableId));
            }

            return observed.AsReadOnly();
        }

        private static PlayerDamageSnapshot FindRunPlayer(RunDamageSnapshot runSnapshot, PlayerKey playerKey)
        {
            if (runSnapshot == null)
            {
                return null;
            }

            for (var i = 0; i < runSnapshot.Players.Count; i++)
            {
                if (runSnapshot.Players[i].PlayerKey.Equals(playerKey))
                {
                    return runSnapshot.Players[i];
                }
            }

            return null;
        }

        private static GemDamageSnapshot FindGemDamage(PlayerDamageSnapshot player, GemKey gemKey)
        {
            for (var i = 0; i < player.Gems.Count; i++)
            {
                if (player.Gems[i].GemKey.Equals(gemKey))
                {
                    return player.Gems[i];
                }
            }

            return null;
        }

        private static MemoryDamageSnapshot FindMemoryDamage(PlayerDamageSnapshot player, MemoryKey memoryKey)
        {
            for (var i = 0; i < player.Memories.Count; i++)
            {
                if (player.Memories[i].MemoryKey.Equals(memoryKey))
                {
                    return player.Memories[i];
                }
            }

            return null;
        }

        private static MemoryPackageDamageSnapshot FindMemoryPackage(PlayerDamageSnapshot player, MemoryKey memoryKey)
        {
            for (var i = 0; i < player.MemoryPackages.Count; i++)
            {
                if (player.MemoryPackages[i].MemoryKey.Equals(memoryKey))
                {
                    return player.MemoryPackages[i];
                }
            }

            return null;
        }

        private static string FormatObservedDamage(float damage, float? share)
        {
            var text = damage.ToString("0.##", CultureInfo.InvariantCulture) + " damage";
            if (share.HasValue)
            {
                text += " (" + (share.Value * 100f).ToString("0.#", CultureInfo.InvariantCulture) + "% run)";
            }

            return text;
        }

        private static IReadOnlyList<ComparisonMetric> EvaluateStructuredMetrics(
            IReadOnlyList<ComparisonStructuredValue> beforeValues,
            IReadOnlyList<ComparisonStructuredValue> afterValues)
        {
            var metrics = new List<ComparisonMetric>();
            var beforeContextualDamage = FindValue(beforeValues, CommonComparisonValueIds.ContextualMemoryDamage);
            var afterContextualDamage = FindValue(afterValues, CommonComparisonValueIds.ContextualMemoryDamage);
            AddMetricIfPresent(metrics, beforeContextualDamage, afterContextualDamage, CommonComparisonEvaluators.EvaluateContextualMemoryDamage(beforeContextualDamage, afterContextualDamage));

            var beforeDamageModifier = FindValue(beforeValues, CommonComparisonValueIds.DamageModifier);
            var afterDamageModifier = FindValue(afterValues, CommonComparisonValueIds.DamageModifier);
            AddMetricIfPresent(metrics, beforeDamageModifier, afterDamageModifier, CommonComparisonEvaluators.EvaluateDamageModifier(beforeDamageModifier, afterDamageModifier));

            var beforeDamage = FindValue(beforeValues, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.DamagePerHit);
            var afterDamage = FindValue(afterValues, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.DamagePerHit);
            AddMetricIfPresent(metrics, beforeDamage, afterDamage, CommonComparisonEvaluators.EvaluateDirectDamage(beforeDamage, afterDamage));

            var beforeBaseDamage = FindValue(beforeValues, CommonComparisonValueIds.BaseDamage);
            var afterBaseDamage = FindValue(afterValues, CommonComparisonValueIds.BaseDamage);
            AddMetricIfPresent(metrics, beforeBaseDamage, afterBaseDamage, CommonComparisonEvaluators.EvaluateBaseDamage(beforeBaseDamage, afterBaseDamage));

            var beforeHits = FindValue(beforeValues, CommonComparisonValueIds.HitCount);
            var afterHits = FindValue(afterValues, CommonComparisonValueIds.HitCount);
            AddMetricIfPresent(metrics, beforeHits, afterHits, CommonComparisonEvaluators.EvaluateHitCount(beforeHits, afterHits));
            if (beforeHits != null || afterHits != null)
            {
                AddMetricIfPresent(metrics, beforeDamage ?? beforeHits, afterDamage ?? afterHits, CommonComparisonEvaluators.EvaluateActivationDamage(
                    beforeDamage,
                    afterDamage,
                    beforeHits,
                    afterHits));
            }

            var beforeCooldown = FindValue(beforeValues, CommonComparisonValueIds.Cooldown);
            var afterCooldown = FindValue(afterValues, CommonComparisonValueIds.Cooldown);
            AddMetricIfPresent(metrics, beforeCooldown, afterCooldown, CommonComparisonEvaluators.EvaluateCooldown(beforeCooldown, afterCooldown));

            AddStatMetricIfPresent(metrics, beforeValues, afterValues, CommonComparisonValueIds.AttackDamage);
            AddStatMetricIfPresent(metrics, beforeValues, afterValues, CommonComparisonValueIds.AbilityPower);

            if (metrics.Count == 0)
            {
                metrics.Add(ComparisonMetric.Unknown("structured-comparison", "Structured comparison", new[] { "no supported structured metric values were available" }));
            }

            return metrics.AsReadOnly();
        }

        private static IReadOnlyList<ComparisonMetric> EvaluateDirectGemMetrics(
            IReadOnlyList<ComparisonStructuredValue> beforeValues,
            IReadOnlyList<ComparisonStructuredValue> afterValues)
        {
            var metrics = new List<ComparisonMetric>();
            var beforeDamage = FindValue(beforeValues, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.DamagePerHit);
            var afterDamage = FindValue(afterValues, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.DamagePerHit);
            AddMetricIfPresent(metrics, beforeDamage, afterDamage, CommonComparisonEvaluators.EvaluateDirectDamage(beforeDamage, afterDamage));

            var beforeCooldown = FindValue(beforeValues, CommonComparisonValueIds.Cooldown);
            var afterCooldown = FindValue(afterValues, CommonComparisonValueIds.Cooldown);
            AddMetricIfPresent(metrics, beforeCooldown, afterCooldown, CommonComparisonEvaluators.EvaluateCooldown(beforeCooldown, afterCooldown));
            return metrics.AsReadOnly();
        }

        private static IReadOnlyList<ComparisonMetric> MergeMetricRows(
            IReadOnlyList<ComparisonMetric> primaryRows,
            IReadOnlyList<ComparisonMetric> secondaryRows)
        {
            var merged = new List<ComparisonMetric>();
            AddMetricRows(merged, primaryRows);
            AddMetricRows(merged, secondaryRows);
            return merged.AsReadOnly();
        }

        private static void AddMetricRows(List<ComparisonMetric> target, IReadOnlyList<ComparisonMetric> rows)
        {
            if (target == null || rows == null)
            {
                return;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                AddMetric(target, rows[i]);
            }
        }

        private static IReadOnlyList<ComparisonUtilityChange> EvaluateStructuredUtility(
            IReadOnlyList<ComparisonStructuredValue> beforeValues,
            IReadOnlyList<ComparisonStructuredValue> afterValues)
        {
            var utility = new List<ComparisonUtilityChange>();
            var beforeCharges = FindValue(beforeValues, CommonComparisonValueIds.Charges);
            var afterCharges = FindValue(afterValues, CommonComparisonValueIds.Charges);
            AddUtilityIfPresent(utility, beforeCharges, afterCharges, CommonComparisonEvaluators.EvaluateCharges(beforeCharges, afterCharges));

            var beforeRangeArea = FindValue(beforeValues, CommonComparisonValueIds.Radius, CommonComparisonValueIds.Area, CommonComparisonValueIds.Range);
            var afterRangeArea = FindValue(afterValues, CommonComparisonValueIds.Radius, CommonComparisonValueIds.Area, CommonComparisonValueIds.Range);
            AddUtilityIfPresent(utility, beforeRangeArea, afterRangeArea, CommonComparisonEvaluators.EvaluateRadiusAreaOrRange(beforeRangeArea, afterRangeArea));
            AddMaterialUtilityRows(utility, beforeValues, afterValues);
            return utility.AsReadOnly();
        }

        private static IReadOnlyList<ComparisonStructuredValue> BuildEmptySlotBaselineValues(
            IReadOnlyList<ComparisonStructuredValue> candidateValues)
        {
            var values = new List<ComparisonStructuredValue>();
            if (candidateValues == null)
            {
                return values.AsReadOnly();
            }

            for (var i = 0; i < candidateValues.Count; i++)
            {
                var value = candidateValues[i];
                if (value == null || !value.NumericValue.HasValue || !IsEmptySlotBaselineValue(value.ValueId))
                {
                    continue;
                }

                values.Add(new ComparisonStructuredValue(
                    value.ValueId,
                    value.Label,
                    0f,
                    value.Unit,
                    "0",
                    value.ResultClass,
                    value.Confidence,
                    new[] { "empty slot baseline; no removed item exists" }));
            }

            return values.AsReadOnly();
        }

        private static bool IsEmptySlotBaselineValue(string valueId)
        {
            return string.Equals(valueId, CommonComparisonValueIds.DirectDamage, StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, CommonComparisonValueIds.DamagePerHit, StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, CommonComparisonValueIds.BaseDamage, StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, CommonComparisonValueIds.ContextualMemoryDamage, StringComparison.OrdinalIgnoreCase);
        }

        private static void AddMaterialUtilityRows(
            List<ComparisonUtilityChange> utility,
            IReadOnlyList<ComparisonStructuredValue> beforeValues,
            IReadOnlyList<ComparisonStructuredValue> afterValues)
        {
            var ids = new List<string>();
            CollectMaterialUtilityIds(ids, beforeValues);
            CollectMaterialUtilityIds(ids, afterValues);
            for (var i = 0; i < ids.Count; i++)
            {
                var before = FindValue(beforeValues, ids[i]);
                var after = FindValue(afterValues, ids[i]);
                AddUtility(utility, BuildMaterialUtilityChange(before, after));
            }
        }

        private static void CollectMaterialUtilityIds(List<string> ids, IReadOnlyList<ComparisonStructuredValue> values)
        {
            if (ids == null || values == null)
            {
                return;
            }

            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (IsMaterialUtilityValue(value) && !ContainsOrdinal(ids, value.ValueId))
                {
                    ids.Add(value.ValueId);
                }
            }
        }

        private static ComparisonUtilityChange BuildMaterialUtilityChange(
            ComparisonStructuredValue before,
            ComparisonStructuredValue after)
        {
            var value = after ?? before;
            var description = ResolveMaterialUtilityDescription(before, after);
            var limitations = new List<string>();
            ComparisonSemantics.AppendLimitations(limitations, before != null ? before.Limitations : null);
            ComparisonSemantics.AppendLimitations(limitations, after != null ? after.Limitations : null);

            if (before != null && after == null)
            {
                limitations.Add("material utility is removed by this action and is not quantitatively evaluated");
                return new ComparisonUtilityChange(
                    before.ValueId,
                    "Lose " + description,
                    null,
                    null,
                    null,
                    "",
                    before.ResultClass == ComparisonResultClass.Unsupported ? ComparisonResultClass.Unsupported : ComparisonResultClass.Unknown,
                    before.ResultClass == ComparisonResultClass.Unsupported ? ComparisonConfidence.Unsupported : ComparisonConfidence.Unknown,
                    limitations);
            }

            if (before == null && after != null)
            {
                limitations.Add("material utility is gained by this action and is not quantitatively evaluated");
                return new ComparisonUtilityChange(
                    after.ValueId,
                    "Gain " + description,
                    null,
                    null,
                    null,
                    "",
                    after.ResultClass,
                    after.Confidence,
                    limitations);
            }

            return new ComparisonUtilityChange(
                value != null ? value.ValueId : "utility:material",
                description,
                before != null ? before.NumericValue : null,
                after != null ? after.NumericValue : null,
                before != null && after != null && before.NumericValue.HasValue && after.NumericValue.HasValue ? after.NumericValue.Value - before.NumericValue.Value : (float?)null,
                value != null ? value.Unit : "",
                value != null ? value.ResultClass : ComparisonResultClass.Unknown,
                value != null ? value.Confidence : ComparisonConfidence.Unknown,
                limitations);
        }

        private static string ResolveMaterialUtilityDescription(ComparisonStructuredValue before, ComparisonStructuredValue after)
        {
            var value = after ?? before;
            if (value == null)
            {
                return "material utility";
            }

            if (!string.IsNullOrEmpty(value.TextValue))
            {
                return value.TextValue;
            }

            return string.IsNullOrEmpty(value.Label) ? value.ValueId : value.Label;
        }

        private static bool IsMaterialUtilityValue(ComparisonStructuredValue value)
        {
            if (value == null || string.IsNullOrEmpty(value.ValueId))
            {
                return false;
            }

            if (value.ValueId.StartsWith(CommonComparisonValueIds.MaterialUtilityPrefix, StringComparison.Ordinal))
            {
                return true;
            }

            return value.ResultClass == ComparisonResultClass.Utility
                && !IsSupportedUtilityValueId(value.ValueId);
        }

        private static bool IsSupportedUtilityValueId(string valueId)
        {
            return string.Equals(valueId, CommonComparisonValueIds.Charges, StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, CommonComparisonValueIds.Radius, StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, CommonComparisonValueIds.Area, StringComparison.OrdinalIgnoreCase)
                || string.Equals(valueId, CommonComparisonValueIds.Range, StringComparison.OrdinalIgnoreCase);
        }

        private static void AddStatMetricIfPresent(
            List<ComparisonMetric> metrics,
            IReadOnlyList<ComparisonStructuredValue> beforeValues,
            IReadOnlyList<ComparisonStructuredValue> afterValues,
            string valueId)
        {
            var before = FindValue(beforeValues, valueId, CommonComparisonValueIds.StatPrefix + valueId);
            var after = FindValue(afterValues, valueId, CommonComparisonValueIds.StatPrefix + valueId);
            if (before != null || after != null)
            {
                AddMetric(metrics, CommonComparisonEvaluators.EvaluateStatBonus(before, after));
            }
        }

        private static void AddMetric(List<ComparisonMetric> metrics, ComparisonMetric metric)
        {
            if (metric != null)
            {
                metrics.Add(metric);
            }
        }

        private static void AddMetricIfPresent(
            List<ComparisonMetric> metrics,
            ComparisonStructuredValue before,
            ComparisonStructuredValue after,
            ComparisonMetric metric)
        {
            if (before != null || after != null)
            {
                AddMetric(metrics, metric);
            }
        }

        private static void AddUtility(List<ComparisonUtilityChange> utility, ComparisonUtilityChange change)
        {
            if (change != null)
            {
                utility.Add(change);
            }
        }

        private static void AddUtilityIfPresent(
            List<ComparisonUtilityChange> utility,
            ComparisonStructuredValue before,
            ComparisonStructuredValue after,
            ComparisonUtilityChange change)
        {
            if (before != null || after != null)
            {
                AddUtility(utility, change);
            }
        }

        private static bool ContainsOrdinal(List<string> values, string value)
        {
            if (values == null)
            {
                return false;
            }

            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static ComparisonMetric ResolvePrimaryDamageMetric(IReadOnlyList<ComparisonMetric> metrics, string fallbackReason)
        {
            if (metrics != null)
            {
                for (var i = 0; i < metrics.Count; i++)
                {
                    if (metrics[i] != null && metrics[i].HasKnownNumericDelta && string.Equals(metrics[i].MetricId, "damage-per-activation", StringComparison.Ordinal))
                    {
                        return metrics[i];
                    }
                }

                for (var i = 0; i < metrics.Count; i++)
                {
                    if (metrics[i] != null && metrics[i].HasKnownNumericDelta && IsDamageMetric(metrics[i]))
                    {
                        return metrics[i];
                    }
                }
            }

            return CommonComparisonEvaluators.WholeBuildDpsUnknown(new[] { fallbackReason });
        }

        private static IReadOnlyList<ComparisonStructuredValue> BuildContextualGemConfigurationValues(
            MemoryState memoryContext,
            IReadOnlyList<GemState> attachedGems,
            bool includeDamageModifier,
            string phaseLabel)
        {
            var values = new List<ComparisonStructuredValue>();
            CopySupportedContextValues(values, memoryContext != null ? memoryContext.StructuredValues : null);
            var modifier = SumAttachedGemValue(attachedGems, CommonComparisonValueIds.DamageModifier);
            if (includeDamageModifier)
            {
                if (modifier.Known)
                {
                    AddContextualValue(
                        values,
                        CommonComparisonValueIds.DamageModifier,
                        "Damage modifier",
                        modifier.Value,
                        "multiplier",
                        ComparisonResultClass.Derived,
                        modifier.Confidence,
                        ContextualLimitations(phaseLabel, modifier.Limitations));
                }
                else
                {
                    AddContextualUnknown(values, CommonComparisonValueIds.DamageModifier, "Damage modifier", phaseLabel + " damage modifier is unavailable");
                }
            }

            AddContextualMemoryDamage(values, memoryContext, attachedGems, modifier, phaseLabel);
            return values.AsReadOnly();
        }

        private static void AddContextualValue(
            List<ComparisonStructuredValue> values,
            string valueId,
            string label,
            float value,
            string unit,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            if (values == null || string.IsNullOrEmpty(valueId) || float.IsNaN(value) || float.IsInfinity(value) || ContainsStructuredValue(values, valueId))
            {
                return;
            }

            values.Add(new ComparisonStructuredValue(
                valueId,
                label,
                value,
                unit,
                value.ToString("0.###", CultureInfo.InvariantCulture),
                resultClass,
                confidence,
                limitations));
        }

        private static void AddContextualUnknown(List<ComparisonStructuredValue> values, string valueId, string label, string limitation)
        {
            if (values == null || ContainsStructuredValue(values, valueId))
            {
                return;
            }

            values.Add(new ComparisonStructuredValue(
                valueId,
                label,
                null,
                "",
                "Unknown",
                ComparisonResultClass.Unknown,
                ComparisonConfidence.Unknown,
                new[] { limitation }));
        }

        private static bool ContainsStructuredValue(IReadOnlyList<ComparisonStructuredValue> values, string valueId)
        {
            if (values == null)
            {
                return false;
            }

            for (var i = 0; i < values.Count; i++)
            {
                if (values[i] != null && string.Equals(values[i].ValueId, valueId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void CopySupportedContextValues(List<ComparisonStructuredValue> target, IReadOnlyList<ComparisonStructuredValue> values)
        {
            if (target == null || values == null)
            {
                return;
            }

            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value == null || string.Equals(value.ValueId, CommonComparisonValueIds.DirectDamage, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(value.ValueId, CommonComparisonValueIds.DamagePerHit, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(value.ValueId, CommonComparisonValueIds.BaseDamage, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!ContainsStructuredValue(target, value.ValueId))
                {
                    target.Add(value);
                }
            }
        }

        private static void AddContextualMemoryDamage(
            List<ComparisonStructuredValue> values,
            MemoryState memoryContext,
            IReadOnlyList<GemState> attachedGems,
            NumericAggregate modifier,
            string phaseLabel)
        {
            var memoryDamage = FindValue(
                memoryContext != null ? memoryContext.StructuredValues : null,
                CommonComparisonValueIds.DirectDamage,
                CommonComparisonValueIds.DamagePerHit,
                CommonComparisonValueIds.BaseDamage);
            var directGemDamage = SumAttachedGemValue(attachedGems, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.DamagePerHit);

            if (!IsKnownNumeric(memoryDamage) && !directGemDamage.Seen)
            {
                return;
            }

            if (!IsKnownNumeric(memoryDamage) && !directGemDamage.Known)
            {
                if (memoryDamage != null || directGemDamage.Seen)
                {
                    AddContextualUnknown(values, CommonComparisonValueIds.ContextualMemoryDamage, "Contextual Memory damage", phaseLabel + " damage inputs are unavailable");
                }

                return;
            }

            var limitations = ContextualLimitations(phaseLabel, null);
            var confidence = ComparisonConfidence.Verified;
            var memoryDamageValue = 0f;
            if (IsKnownNumeric(memoryDamage))
            {
                memoryDamageValue = memoryDamage.NumericValue.Value;
                confidence = ComparisonSemantics.MostConservative(confidence, memoryDamage.Confidence);
                ComparisonSemantics.AppendLimitations(limitations, memoryDamage.Limitations);
            }

            if (modifier.Seen)
            {
                if (!modifier.Known)
                {
                    AddContextualUnknown(values, CommonComparisonValueIds.ContextualMemoryDamage, "Contextual Memory damage", phaseLabel + " damage modifier is unavailable");
                    return;
                }

                memoryDamageValue *= 1f + modifier.Value;
                confidence = ComparisonSemantics.MostConservative(confidence, modifier.Confidence);
                ComparisonSemantics.AppendLimitations(limitations, modifier.Limitations);
                limitations.Add("explicit damage-modifier values are applied additively to the parent Memory damage");
            }

            if (directGemDamage.Seen)
            {
                if (!directGemDamage.Known)
                {
                    AddContextualUnknown(values, CommonComparisonValueIds.ContextualMemoryDamage, "Contextual Memory damage", phaseLabel + " direct Gem damage is unavailable");
                    return;
                }

                memoryDamageValue += directGemDamage.Value;
                confidence = ComparisonSemantics.MostConservative(confidence, directGemDamage.Confidence);
                ComparisonSemantics.AppendLimitations(limitations, directGemDamage.Limitations);
                limitations.Add("direct Gem damage is included only from structured direct-damage values, not observed package history");
            }

            AddContextualValue(
                values,
                CommonComparisonValueIds.ContextualMemoryDamage,
                "Contextual Memory damage",
                memoryDamageValue,
                "damage",
                ComparisonResultClass.Derived,
                confidence,
                limitations);
        }

        private static bool HasDamageModifier(IReadOnlyList<GemState> gems)
        {
            return SumAttachedGemValue(gems, CommonComparisonValueIds.DamageModifier).Seen;
        }

        private static NumericAggregate SumAttachedGemValue(IReadOnlyList<GemState> gems, params string[] valueIds)
        {
            var aggregate = NumericAggregate.Empty;
            if (gems == null)
            {
                return aggregate;
            }

            for (var i = 0; i < gems.Count; i++)
            {
                var value = FindValue(gems[i].StructuredValues, valueIds);
                if (value == null || value.ResultClass == ComparisonResultClass.NotApplicable)
                {
                    continue;
                }

                aggregate = aggregate.Add(value);
            }

            return aggregate;
        }

        private static bool IsKnownNumeric(ComparisonStructuredValue value)
        {
            return value != null
                && value.NumericValue.HasValue
                && !value.IsUnknownOrUnsupported
                && value.ResultClass != ComparisonResultClass.NotApplicable;
        }

        private static List<string> ContextualLimitations(string phaseLabel, IEnumerable<string> sourceLimitations)
        {
            var limitations = new List<string>
            {
                "contextual Gem comparison for " + phaseLabel
            };
            ComparisonSemantics.AppendLimitations(limitations, sourceLimitations);
            return limitations;
        }

        private struct NumericAggregate
        {
            internal static readonly NumericAggregate Empty = new NumericAggregate(false, true, 0f, ComparisonConfidence.Verified, null);

            private NumericAggregate(bool seen, bool known, float value, ComparisonConfidence confidence, IEnumerable<string> limitations)
            {
                Seen = seen;
                Known = known;
                Value = value;
                Confidence = confidence;
                Limitations = ComparisonContractLists.Copy(limitations);
            }

            internal bool Seen { get; }
            internal bool Known { get; }
            internal float Value { get; }
            internal ComparisonConfidence Confidence { get; }
            internal IReadOnlyList<string> Limitations { get; }

            internal NumericAggregate Add(ComparisonStructuredValue value)
            {
                var limitations = ComparisonSemantics.CopyLimitations(Limitations);
                if (value == null)
                {
                    return this;
                }

                ComparisonSemantics.AppendLimitations(limitations, value.Limitations);
                var confidence = Seen
                    ? ComparisonSemantics.MostConservative(Confidence, value.Confidence)
                    : value.Confidence;

                if (!value.NumericValue.HasValue || value.IsUnknownOrUnsupported || value.ResultClass == ComparisonResultClass.NotApplicable)
                {
                    return new NumericAggregate(true, false, Value, confidence, limitations);
                }

                return new NumericAggregate(true, Known, Value + value.NumericValue.Value, confidence, limitations);
            }
        }

        private static bool IsDamageMetric(ComparisonMetric metric)
        {
            return ContainsIgnoreCase(metric.MetricId, "damage")
                || ContainsIgnoreCase(metric.MetricId, "dps")
                || ContainsIgnoreCase(metric.Label, "damage")
                || ContainsIgnoreCase(metric.Label, "dps");
        }

        private static IReadOnlyList<GemState> ApplyGemCandidate(
            BuildStateSnapshot buildState,
            MemoryState memoryContext,
            GemState currentGem,
            GemState candidate)
        {
            var attached = new List<GemState>();
            var foundTarget = false;
            var current = ResolveAttachedGems(buildState, memoryContext);
            for (var i = 0; i < current.Count; i++)
            {
                if (currentGem != null && current[i].GemKey.Equals(currentGem.GemKey))
                {
                    attached.Add(new GemState(candidate.GemKey, candidate.ContentId, candidate.Quality, memoryContext.MemoryKey, candidate.StructuredValues));
                    foundTarget = true;
                    continue;
                }

                attached.Add(current[i]);
            }

            if (!foundTarget)
            {
                attached.Add(new GemState(candidate.GemKey, candidate.ContentId, candidate.Quality, memoryContext.MemoryKey, candidate.StructuredValues));
            }

            return attached.AsReadOnly();
        }

        private static IReadOnlyList<GemState> ResolveAttachedGems(BuildStateSnapshot buildState, MemoryState memoryContext)
        {
            var attached = new List<GemState>();
            if (buildState == null || memoryContext == null)
            {
                return attached.AsReadOnly();
            }

            for (var i = 0; i < buildState.Gems.Count; i++)
            {
                var gem = buildState.Gems[i];
                if (gem.AttachedMemoryKey.HasValue && gem.AttachedMemoryKey.Value.Equals(memoryContext.MemoryKey))
                {
                    attached.Add(gem);
                    continue;
                }

                for (var j = 0; j < memoryContext.AttachedGemKeys.Count; j++)
                {
                    if (gem.GemKey.Equals(memoryContext.AttachedGemKeys[j]))
                    {
                        attached.Add(gem);
                        break;
                    }
                }
            }

            return attached.AsReadOnly();
        }

        private static ComparisonStructuredValue FindValue(IReadOnlyList<ComparisonStructuredValue> values, params string[] valueIds)
        {
            if (values == null || valueIds == null)
            {
                return null;
            }

            for (var i = 0; i < values.Count; i++)
            {
                for (var j = 0; j < valueIds.Length; j++)
                {
                    if (string.Equals(values[i].ValueId, valueIds[j], StringComparison.OrdinalIgnoreCase))
                    {
                        return values[i];
                    }
                }
            }

            return null;
        }

        private LiveCandidateComparisonSnapshot SetSnapshot(LiveCandidateComparisonSnapshot snapshot)
        {
            _current = snapshot;
            return _current;
        }

        private long NextSequence()
        {
            _sequence++;
            return _sequence;
        }

        private static LiveCandidateComparisonSnapshot EmptySnapshot(long sequenceId, string reason, float capturedAt)
        {
            return new LiveCandidateComparisonSnapshot(
                sequenceId,
                LiveCandidateComparisonStatus.Empty,
                LiveCandidateComparisonCandidateKind.Unknown,
                reason,
                null,
                null,
                null,
                null,
                null,
                capturedAt);
        }

        private static List<T> CopyTargets<T>(IEnumerable<T> targets)
        {
            return targets == null ? new List<T>() : new List<T>(targets);
        }

        private static ComparisonSubject ToSubject(GemState gem)
        {
            return new ComparisonSubject(ComparisonSubjectKind.Gem, gem.GemKey.StableId, gem.ContentId, gem.GemKey.DisplayName);
        }

        private static ComparisonSubject ToSubject(MemoryState memory)
        {
            return new ComparisonSubject(ComparisonSubjectKind.Memory, memory.MemoryKey.StableId, memory.ContentId, memory.MemoryKey.DisplayName);
        }

        private static bool ContainsIgnoreCase(string value, string pattern)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    internal static class LiveCandidateComparisonCoordinator
    {
        private static readonly LiveCandidateComparisonService Service = new LiveCandidateComparisonService();

        internal static LiveCandidateComparisonSnapshot CurrentSnapshot
        {
            get { return Service.CurrentSnapshot; }
        }

        internal static LiveCandidateComparisonSnapshot RefreshForHeldCandidate(IItem item, HeroSkill heroSkill, string reason)
        {
            try
            {
                if (heroSkill == null)
                {
                    heroSkill = ResolveLocalHeroSkill(item as Actor);
                }

                var hero = heroSkill != null ? heroSkill.hero : DewPlayer.local != null ? DewPlayer.local.hero : null;
                if (heroSkill == null || hero == null || item == null)
                {
                    return Service.Clear(reason + ": no held candidate context", Time.time);
                }

                var build = CaptureBuildState(hero, heroSkill, Time.time);
                if (item is Gem gem)
                {
                    var candidate = CaptureGem(gem, build.PlayerKey, null, hero);
                    var duplicate = heroSkill.TryGetEquippedGemOfSameType(gem.GetType(), out var _, out var _);
                    return Service.RefreshGemCandidate(build, candidate, EnumerateGemTargets(heroSkill, build), duplicate, Time.time, DamageAnalyzerDiagnostics.GetRunSnapshot());
                }

                if (item is SkillTrigger skill)
                {
                    var candidate = CaptureMemory(skill, build.PlayerKey, -1, Array.Empty<GemKey>(), hero);
                    return Service.RefreshMemoryCandidate(build, candidate, EnumerateMemoryTargets(heroSkill, build), Time.time, DamageAnalyzerDiagnostics.GetRunSnapshot());
                }

                return Service.Clear(reason + ": held item is not a supported Gem or Memory candidate", Time.time);
            }
            catch (Exception ex)
            {
                return Service.Clear(reason + ": live candidate comparison error " + ex.GetType().Name, Time.time);
            }
        }

        internal static LiveCandidateComparisonSnapshot Clear(string reason)
        {
            return Service.Clear(reason, Time.time);
        }

        private static BuildStateSnapshot CaptureBuildState(Hero hero, HeroSkill heroSkill, float capturedAt)
        {
            var playerKey = ResolvePlayerKey(hero != null ? hero.owner : null);
            var memories = new List<MemoryState>();
            var gems = new List<GemState>();
            foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
            {
                var skill = heroSkill.GetSkill(location);
                if (skill == null)
                {
                    continue;
                }

                var attachedGemKeys = new List<GemKey>();
                foreach (var pair in heroSkill.GetGemsPairInSkill(location))
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }

                    var gemKey = BuildGemKey(playerKey, pair.Value);
                    attachedGemKeys.Add(gemKey);
                    gems.Add(CaptureGem(pair.Value, playerKey, BuildMemoryKey(playerKey, skill), hero));
                }

                memories.Add(CaptureMemory(skill, playerKey, (int)location, attachedGemKeys, hero));
            }

            return new BuildStateSnapshot(
                playerKey,
                hero != null ? hero.GetType().Name : "",
                CaptureFinalStats(hero),
                memories,
                gems,
                null,
                capturedAt);
        }

        private static IEnumerable<LiveGemReplacementTarget> EnumerateGemTargets(HeroSkill heroSkill, BuildStateSnapshot build)
        {
            var targets = new List<LiveGemReplacementTarget>();
            foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
            {
                if (heroSkill.GetMaxGemCount(location) <= 0 || heroSkill.GetSkill(location) == null)
                {
                    continue;
                }

                var memory = FindMemory(build, (int)location);
                if (memory == null)
                {
                    continue;
                }

                foreach (var pair in heroSkill.GetGemsPairInSkill(location))
                {
                    var gem = FindGem(build, BuildGemKey(build.PlayerKey, pair.Value));
                    if (gem != null)
                    {
                        targets.Add(new LiveGemReplacementTarget(gem, memory, pair.Key.index));
                    }
                }

                var emptyIndex = heroSkill.GetEmptyGemSlot(location);
                if (emptyIndex >= 0)
                {
                    targets.Add(new LiveGemReplacementTarget(CandidateEquipActionKind.EquipIntoEmptySlot, null, memory, emptyIndex));
                }
            }

            return targets.AsReadOnly();
        }

        private static IEnumerable<LiveMemoryReplacementTarget> EnumerateMemoryTargets(HeroSkill heroSkill, BuildStateSnapshot build)
        {
            var targets = new List<LiveMemoryReplacementTarget>();
            foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
            {
                if (!heroSkill.CanReplaceSkill(location))
                {
                    continue;
                }

                var memory = FindMemory(build, (int)location);
                if (memory != null)
                {
                    targets.Add(new LiveMemoryReplacementTarget(memory));
                }
                else
                {
                    targets.Add(new LiveMemoryReplacementTarget(CandidateEquipActionKind.EquipIntoEmptySlot, null, (int)location));
                }
            }

            return targets.AsReadOnly();
        }

        private static MemoryState FindMemory(BuildStateSnapshot build, int slot)
        {
            for (var i = 0; i < build.Memories.Count; i++)
            {
                if (build.Memories[i].Slot == slot)
                {
                    return build.Memories[i];
                }
            }

            return null;
        }

        private static GemState FindGem(BuildStateSnapshot build, GemKey key)
        {
            for (var i = 0; i < build.Gems.Count; i++)
            {
                if (build.Gems[i].GemKey.Equals(key))
                {
                    return build.Gems[i];
                }
            }

            return null;
        }

        private static HeroSkill ResolveLocalHeroSkill(Actor actor)
        {
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                return DewPlayer.local.hero.Skill;
            }

            if (actor is Gem gem && gem.owner != null)
            {
                return gem.owner.Skill;
            }

            if (actor is SkillTrigger skill && skill.owner != null)
            {
                return skill.owner.Skill;
            }

            return null;
        }

        private static PlayerKey ResolvePlayerKey(DewPlayer player)
        {
            if (player == null)
            {
                return new PlayerKey("unknown-player", true);
            }

            var stableId = !string.IsNullOrEmpty(player.guid) ? player.guid : player.playerNameRaw;
            if (string.IsNullOrEmpty(stableId))
            {
                stableId = player.GetInstanceID().ToString(CultureInfo.InvariantCulture);
            }

            return new PlayerKey(stableId, player == DewPlayer.local);
        }

        private static MemoryKey BuildMemoryKey(PlayerKey playerKey, Actor actor)
        {
            var contentId = actor != null ? actor.GetType().Name : "UNKNOWN_MEMORY";
            return new MemoryKey(playerKey, contentId, AnalyticsDisplayNameResolver.ToDisplayName(contentId, contentId));
        }

        private static GemKey BuildGemKey(PlayerKey playerKey, Actor actor)
        {
            var contentId = actor != null ? actor.GetType().Name : "UNKNOWN_GEM";
            return new GemKey(playerKey, contentId, AnalyticsDisplayNameResolver.ToDisplayName(contentId, contentId));
        }

        private static MemoryState CaptureMemory(SkillTrigger skill, PlayerKey playerKey, int slot, IEnumerable<GemKey> attachedGemKeys, Hero hero)
        {
            var key = BuildMemoryKey(playerKey, skill);
            return new MemoryState(
                key,
                key.ContentId,
                slot,
                null,
                skill != null ? (int?)skill.level : null,
                null,
                attachedGemKeys,
                CaptureStructuredValues(skill, hero));
        }

        private static GemState CaptureGem(Gem gem, PlayerKey playerKey, MemoryKey? attachedMemoryKey, Hero hero)
        {
            var key = BuildGemKey(playerKey, gem);
            return new GemState(
                key,
                key.ContentId,
                gem != null ? (float?)gem.quality : null,
                attachedMemoryKey,
                CaptureStructuredValues(gem, hero));
        }

        private static IReadOnlyList<BuildStatValue> CaptureFinalStats(Hero hero)
        {
            if (hero == null)
            {
                return Array.Empty<BuildStatValue>();
            }

            return new[]
            {
                new BuildStatValue(CommonComparisonValueIds.AttackDamage, hero.Status.attackDamage, "", ComparisonConfidence.Verified),
                new BuildStatValue(CommonComparisonValueIds.AbilityPower, hero.Status.abilityPower, "", ComparisonConfidence.Verified),
                new BuildStatValue("ability-haste", hero.Status.abilityHaste, "", ComparisonConfidence.Verified),
                new BuildStatValue("attack-speed-multiplier", hero.Status.attackSpeedMultiplier, "", ComparisonConfidence.Verified),
                new BuildStatValue("armor", hero.Status.armor, "", ComparisonConfidence.Verified),
                new BuildStatValue("max-health", hero.Status.maxHealth, "", ComparisonConfidence.Verified)
            };
        }

        private static IReadOnlyList<ComparisonStructuredValue> CaptureStructuredValues(Actor actor, Hero hero)
        {
            var values = new List<ComparisonStructuredValue>();
            if (actor == null)
            {
                return values.AsReadOnly();
            }

            var level = ResolveEffectiveLevel(actor);
            CaptureNumericFields(values, actor, actor.GetType(), level, hero);
            CaptureTriggerConfigValues(values, actor as AbilityTrigger);
            CaptureKnownMaterialUtility(values, actor);
            return values.AsReadOnly();
        }

        private static void CaptureKnownMaterialUtility(List<ComparisonStructuredValue> values, Actor actor)
        {
            if (values == null || actor == null)
            {
                return;
            }

            var contentId = actor.GetType().Name;
            if (ContainsIgnoreCase(contentId, "Adventure"))
            {
                AddTextValue(
                    values,
                    CommonComparisonValueIds.MaterialUtilityPrefix + "adventure-essence",
                    "Adventure Essence utility",
                    "Whenever you move to a new place, increase another equipped Essence on this Memory by +10% quality and gain Gold",
                    ComparisonResultClass.Unsupported,
                    ComparisonConfidence.Unsupported,
                    new[] { "Adventure Essence quality propagation is material utility but is not quantitatively evaluated by Stage 4C" });
            }
        }

        private static void CaptureNumericFields(List<ComparisonStructuredValue> values, Actor actor, Type type, int level, Hero hero)
        {
            while (type != null && type != typeof(Actor))
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (!IsInterestingField(field))
                    {
                        continue;
                    }

                    try
                    {
                        AddFieldValue(values, field.Name, field.GetValue(actor), level, hero);
                    }
                    catch
                    {
                        AddUnknown(values, NormalizeValueId(field.Name), field.Name, "field extraction failed");
                    }
                }

                type = type.BaseType;
            }
        }

        private static void AddFieldValue(List<ComparisonStructuredValue> values, string fieldName, object value, int level, Hero hero)
        {
            if (value is ScalingValue scaling)
            {
                AddScalingValue(values, fieldName, scaling, level, hero);
                return;
            }

            if (value is CastMethodData cast)
            {
                AddValue(values, CommonComparisonValueIds.Range, "Range", SafeFloat(() => cast.pointData.range), "m", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, null);
                AddValue(values, CommonComparisonValueIds.Radius, "Radius", SafeFloat(() => cast.pointData.radius), "m", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, null);
                return;
            }

            if (value is StatBonus stat)
            {
                AddStatBonusValues(values, stat);
                return;
            }

            if (IsMutableRuntimeStateField(fieldName))
            {
                return;
            }

            if (value is float f)
            {
                AddValue(values, ResolveNumericFieldValueId(fieldName), fieldName, f, "", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, null);
                return;
            }

            if (value is int i)
            {
                AddValue(values, ResolveNumericFieldValueId(fieldName), fieldName, i, "", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, null);
            }
        }

        private static void AddScalingValue(List<ComparisonStructuredValue> values, string fieldName, ScalingValue scaling, int level, Hero hero)
        {
            var valueId = ResolveNumericFieldValueId(fieldName);
            if (IsInactiveScalingValue(scaling))
            {
                if (IsSupportedScalingValueId(valueId))
                {
                    AddNotApplicable(values, valueId, FieldLabel(fieldName), fieldName + " native ScalingValue has no base, stat, level, armor, health, or crit contribution");
                }

                return;
            }

            if (valueId == CommonComparisonValueIds.DamagePerHit || valueId == CommonComparisonValueIds.DirectDamage)
            {
                AddValue(values, CommonComparisonValueIds.DamagePerHit, FieldLabel(fieldName), SafeFloat(() => scaling.GetValue(level, hero)), "damage", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence, new[] { "contextual ScalingValue.GetValue at observed level/build context: " + fieldName });
                AddValue(values, CommonComparisonValueIds.BaseDamage, "Base damage", scaling.baseValue, "damage", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, new[] { "native ScalingValue.baseValue: " + fieldName });
                AddValue(values, CommonComparisonValueIds.AttackDamageCoefficient, "AD coefficient", scaling.adFactor, "", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, new[] { "native ScalingValue.adFactor: " + fieldName });
                AddValue(values, CommonComparisonValueIds.AbilityPowerCoefficient, "AP coefficient", scaling.apFactor, "", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, new[] { "native ScalingValue.apFactor: " + fieldName });
                return;
            }

            if (valueId == CommonComparisonValueIds.Cooldown)
            {
                AddValue(values, CommonComparisonValueIds.Cooldown, FieldLabel(fieldName), SafeFloat(() => scaling.GetValue(level, hero)), "s", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence, new[] { "contextual ScalingValue.GetValue at observed level/build context: " + fieldName });
                return;
            }

            if (valueId == CommonComparisonValueIds.Charges || valueId == CommonComparisonValueIds.HitCount)
            {
                AddValue(values, valueId, FieldLabel(fieldName), SafeFloat(() => scaling.GetValue(level, hero)), "", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence, new[] { "contextual ScalingValue.GetValue at observed level/build context: " + fieldName });
                return;
            }

            if (valueId == CommonComparisonValueIds.Radius || valueId == CommonComparisonValueIds.Range || valueId == CommonComparisonValueIds.Area)
            {
                AddValue(values, valueId, FieldLabel(fieldName), SafeFloat(() => scaling.GetValue(level, hero)), "m", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence, new[] { "contextual ScalingValue.GetValue at observed level/build context: " + fieldName });
            }
        }

        private static bool IsInactiveScalingValue(ScalingValue scaling)
        {
            return Math.Abs(scaling.baseValue) <= 0.0001f
                && Math.Abs(scaling.adFactor) <= 0.0001f
                && Math.Abs(scaling.apFactor) <= 0.0001f
                && Math.Abs(scaling.lvlFactor) <= 0.0001f
                && Math.Abs(scaling.armorFactor) <= 0.0001f
                && Math.Abs(scaling.addedHpFactor) <= 0.0001f
                && Math.Abs(scaling.critPercentageFactor) <= 0.0001f;
        }

        private static bool IsSupportedScalingValueId(string valueId)
        {
            return valueId == CommonComparisonValueIds.DirectDamage
                || valueId == CommonComparisonValueIds.DamagePerHit
                || valueId == CommonComparisonValueIds.Cooldown
                || valueId == CommonComparisonValueIds.Charges
                || valueId == CommonComparisonValueIds.HitCount
                || valueId == CommonComparisonValueIds.Radius
                || valueId == CommonComparisonValueIds.Range
                || valueId == CommonComparisonValueIds.Area;
        }

        private static bool IsMutableRuntimeStateField(string fieldName)
        {
            var normalized = NormalizeValueId(fieldName);
            return normalized.StartsWith("current", StringComparison.Ordinal)
                || normalized.StartsWith("currentconfig", StringComparison.Ordinal);
        }

        private static string FieldLabel(string fieldName)
        {
            return string.IsNullOrEmpty(fieldName) ? "Value" : fieldName;
        }

        private static bool IsDamageField(string normalized)
        {
            return normalized.Contains("damage") || normalized.Contains("dmg") || normalized == "amount";
        }

        private static bool IsHitCountField(string normalized)
        {
            return normalized.Contains("hitcount") || normalized.Contains("hits") || normalized == "hit";
        }

        private static bool IsAreaField(string normalized)
        {
            return normalized.Contains("area") || normalized.Contains("width") || normalized.Contains("length");
        }

        private static void CaptureTriggerConfigValues(List<ComparisonStructuredValue> values, AbilityTrigger trigger)
        {
            if (trigger == null || trigger.configs == null)
            {
                return;
            }

            for (var i = 0; i < trigger.configs.Length; i++)
            {
                var config = trigger.configs[i];
                if (config == null)
                {
                    continue;
                }

                AddValue(values, CommonComparisonValueIds.Cooldown, "Cooldown", config.cooldownTime, "s", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, new[] { "trigger config " + i.ToString(CultureInfo.InvariantCulture) });
                AddValue(values, CommonComparisonValueIds.Charges, "Charges", ResolveDisplayCharges(config), "charges", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, ChargeLimitations(i, config));
                AddValue(values, CommonComparisonValueIds.Range, "Range", config.effectiveRange, "m", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, new[] { "trigger config " + i.ToString(CultureInfo.InvariantCulture) });
            }
        }

        private static void AddStatBonusValues(List<ComparisonStructuredValue> values, StatBonus stat)
        {
            AddValue(values, CommonComparisonValueIds.StatPrefix + CommonComparisonValueIds.AttackDamage, "Attack Damage", stat.attackDamageFlat, "", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, null);
            AddValue(values, CommonComparisonValueIds.StatPrefix + CommonComparisonValueIds.AbilityPower, "Ability Power", stat.abilityPowerFlat, "", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, null);
            AddValue(values, "stat:ability-haste", "Ability Haste", stat.abilityHasteFlat, "", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, null);
            AddValue(values, "stat:armor", "Armor", stat.armorFlat, "", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, null);
        }

        private static void AddValue(
            List<ComparisonStructuredValue> values,
            string valueId,
            string label,
            float numericValue,
            string unit,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            if (float.IsNaN(numericValue) || float.IsInfinity(numericValue))
            {
                return;
            }

            if (ContainsValue(values, valueId))
            {
                return;
            }

            values.Add(new ComparisonStructuredValue(valueId, label, numericValue, unit, "", resultClass, confidence, limitations));
        }

        private static void AddTextValue(
            List<ComparisonStructuredValue> values,
            string valueId,
            string label,
            string textValue,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            IEnumerable<string> limitations)
        {
            if (values == null || string.IsNullOrEmpty(valueId) || ContainsValue(values, valueId))
            {
                return;
            }

            values.Add(new ComparisonStructuredValue(valueId, label, null, "", textValue, resultClass, confidence, limitations));
        }

        private static void AddUnknown(List<ComparisonStructuredValue> values, string valueId, string label, string limitation)
        {
            if (!ContainsValue(values, valueId))
            {
                values.Add(new ComparisonStructuredValue(valueId, label, null, "", "", ComparisonResultClass.Unknown, ComparisonConfidence.Unknown, new[] { limitation }));
            }
        }

        private static void AddNotApplicable(List<ComparisonStructuredValue> values, string valueId, string label, string limitation)
        {
            if (!ContainsValue(values, valueId))
            {
                values.Add(new ComparisonStructuredValue(valueId, label, null, "", "N/A", ComparisonResultClass.NotApplicable, ComparisonConfidence.Verified, new[] { limitation }));
            }
        }

        private static bool ContainsValue(List<ComparisonStructuredValue> values, string valueId)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i].ValueId, valueId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsInterestingField(FieldInfo field)
        {
            var type = field.FieldType;
            if (type == typeof(ScalingValue) || type == typeof(StatBonus) || type == typeof(CastMethodData))
            {
                return true;
            }

            if (type != typeof(float) && type != typeof(int))
            {
                return false;
            }

            var name = field.Name.ToLowerInvariant();
            return name.Contains("damage") || name.Contains("dmg") || name.Contains("cooldown") ||
                name.Contains("radius") || name.Contains("range") || name.Contains("count") ||
                name.Contains("charge") || name.Contains("hit");
        }

        private static string ResolveNumericFieldValueId(string fieldName)
        {
            var normalized = NormalizeValueId(fieldName);
            if (IsDamageField(normalized))
            {
                return CommonComparisonValueIds.DamagePerHit;
            }

            if (normalized.Contains("cooldown"))
            {
                return CommonComparisonValueIds.Cooldown;
            }

            if (normalized.Contains("charge"))
            {
                return CommonComparisonValueIds.Charges;
            }

            if (normalized.Contains("radius"))
            {
                return CommonComparisonValueIds.Radius;
            }

            if (normalized.Contains("range"))
            {
                return CommonComparisonValueIds.Range;
            }

            if (IsAreaField(normalized))
            {
                return CommonComparisonValueIds.Area;
            }

            if (IsHitCountField(normalized))
            {
                return CommonComparisonValueIds.HitCount;
            }

            return normalized;
        }

        private static string NormalizeValueId(string value)
        {
            return string.IsNullOrEmpty(value) ? "unknown-value" : value.Replace("_", "").ToLowerInvariant();
        }

        private static int ResolveEffectiveLevel(Actor actor)
        {
            if (actor is SkillTrigger skill)
            {
                return skill.level;
            }

            if (actor is Gem gem)
            {
                return gem.effectiveLevel;
            }

            return 0;
        }

        private static float SafeFloat(Func<float> read)
        {
            try
            {
                return read();
            }
            catch
            {
                return float.NaN;
            }
        }

        private static int ResolveDisplayCharges(TriggerConfig config)
        {
            return config.maxCharges;
        }

        private static IReadOnlyList<string> ChargeLimitations(int configIndex, TriggerConfig config)
        {
            return new[]
            {
                "trigger config " + configIndex.ToString(CultureInfo.InvariantCulture),
                "config.maxCharges is the displayed/usable charge cap; config.addedCharges is recharge increment"
            };
        }
    }
}

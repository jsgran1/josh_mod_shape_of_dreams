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
        {
            CurrentGem = currentGem;
            MemoryContext = memoryContext;
            GemIndex = gemIndex;
        }

        internal GemState CurrentGem { get; }
        internal MemoryState MemoryContext { get; }
        internal int GemIndex { get; }
    }

    internal sealed class LiveMemoryReplacementTarget
    {
        internal LiveMemoryReplacementTarget(MemoryState currentMemory)
        {
            CurrentMemory = currentMemory;
        }

        internal MemoryState CurrentMemory { get; }
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
                return SetSnapshot(new LiveCandidateComparisonSnapshot(
                    NextSequence(),
                    LiveCandidateComparisonStatus.Unsupported,
                    LiveCandidateComparisonCandidateKind.Gem,
                    "duplicate Gem merge comparison is unavailable until the correlated merge path is validated",
                    buildState,
                    candidate,
                    null,
                    null,
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
            var replacement = ToSubject(target.CurrentGem);
            var candidateSubject = ToSubject(candidate);
            var candidateChange = new CandidateChange(BuildChangeKind.Replace, candidateSubject, replacement);
            var attachedGems = ReplaceAttachedGem(buildState, target.MemoryContext, target.CurrentGem, candidate);
            evaluation = ContextualEffectEvaluator.Evaluate(buildState, target.MemoryContext, attachedGems, candidateChange);
            var metrics = EvaluateStructuredMetrics(target.CurrentGem.StructuredValues, candidate.StructuredValues);
            var utility = EvaluateStructuredUtility(target.CurrentGem.StructuredValues, candidate.StructuredValues);
            var observedContext = BuildGemObservedContext(runSnapshot, buildState.PlayerKey, target.CurrentGem.GemKey, target.MemoryContext != null ? (MemoryKey?)target.MemoryContext.MemoryKey : null);
            var limitations = ComparisonSemantics.CopyLimitations(evaluation.Limitations);
            var primary = ResolvePrimaryDamageMetric(metrics, "Gem replacement damage impact is unknown");

            return new BuildOptionComparison(
                candidateSubject,
                replacement,
                metrics,
                utility,
                observedContext,
                primary,
                ComparisonSemantics.MostConservative(evaluation.Confidence, primary != null ? primary.Confidence : ComparisonConfidence.Unknown),
                limitations);
        }

        private BuildOptionComparison EvaluateMemoryReplacement(
            BuildStateSnapshot buildState,
            MemoryState candidate,
            LiveMemoryReplacementTarget target,
            RunDamageSnapshot runSnapshot,
            out ContextualEffectEvaluation evaluation)
        {
            var replacement = ToSubject(target.CurrentMemory);
            var candidateSubject = ToSubject(candidate);
            var candidateChange = new CandidateChange(BuildChangeKind.Replace, candidateSubject, replacement);
            var attachedGems = ResolveAttachedGems(buildState, target.CurrentMemory);
            var projectedCandidate = new MemoryState(
                candidate.MemoryKey,
                candidate.ContentId,
                target.CurrentMemory.Slot,
                candidate.Rank,
                candidate.Level,
                candidate.Quality,
                target.CurrentMemory.AttachedGemKeys,
                candidate.StructuredValues);
            evaluation = ContextualEffectEvaluator.Evaluate(buildState, projectedCandidate, attachedGems, candidateChange);
            var metrics = EvaluateStructuredMetrics(target.CurrentMemory.StructuredValues, candidate.StructuredValues);
            var utility = EvaluateStructuredUtility(target.CurrentMemory.StructuredValues, candidate.StructuredValues);
            var observedContext = BuildMemoryObservedContext(runSnapshot, buildState.PlayerKey, target.CurrentMemory.MemoryKey);
            var limitations = ComparisonSemantics.CopyLimitations(evaluation.Limitations);
            limitations.Add("Memory replacement keeps the target slot's attached Gem context");
            var primary = ResolvePrimaryDamageMetric(metrics, "Memory replacement damage impact is unknown");

            return new BuildOptionComparison(
                candidateSubject,
                replacement,
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
            AddMetric(metrics, CommonComparisonEvaluators.EvaluateDirectDamage(
                FindValue(beforeValues, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.DamagePerHit, CommonComparisonValueIds.BaseDamage),
                FindValue(afterValues, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.DamagePerHit, CommonComparisonValueIds.BaseDamage)));
            AddMetric(metrics, CommonComparisonEvaluators.EvaluateActivationDamage(
                FindValue(beforeValues, CommonComparisonValueIds.DamagePerHit, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.BaseDamage),
                FindValue(afterValues, CommonComparisonValueIds.DamagePerHit, CommonComparisonValueIds.DirectDamage, CommonComparisonValueIds.BaseDamage),
                FindValue(beforeValues, CommonComparisonValueIds.HitCount),
                FindValue(afterValues, CommonComparisonValueIds.HitCount)));
            AddMetric(metrics, CommonComparisonEvaluators.EvaluateCooldown(
                FindValue(beforeValues, CommonComparisonValueIds.Cooldown),
                FindValue(afterValues, CommonComparisonValueIds.Cooldown)));

            AddStatMetricIfPresent(metrics, beforeValues, afterValues, CommonComparisonValueIds.AttackDamage);
            AddStatMetricIfPresent(metrics, beforeValues, afterValues, CommonComparisonValueIds.AbilityPower);

            if (metrics.Count == 0)
            {
                metrics.Add(ComparisonMetric.Unknown("structured-comparison", "Structured comparison", new[] { "no supported structured metric values were available" }));
            }

            return metrics.AsReadOnly();
        }

        private static IReadOnlyList<ComparisonUtilityChange> EvaluateStructuredUtility(
            IReadOnlyList<ComparisonStructuredValue> beforeValues,
            IReadOnlyList<ComparisonStructuredValue> afterValues)
        {
            var utility = new List<ComparisonUtilityChange>();
            AddUtility(utility, CommonComparisonEvaluators.EvaluateCharges(
                FindValue(beforeValues, CommonComparisonValueIds.Charges),
                FindValue(afterValues, CommonComparisonValueIds.Charges)));
            AddUtility(utility, CommonComparisonEvaluators.EvaluateRadiusAreaOrRange(
                FindValue(beforeValues, CommonComparisonValueIds.Radius, CommonComparisonValueIds.Area, CommonComparisonValueIds.Range),
                FindValue(afterValues, CommonComparisonValueIds.Radius, CommonComparisonValueIds.Area, CommonComparisonValueIds.Range)));
            return utility.AsReadOnly();
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
            if (metric != null && !metric.IsUnknownOrUnsupported)
            {
                metrics.Add(metric);
            }
        }

        private static void AddUtility(List<ComparisonUtilityChange> utility, ComparisonUtilityChange change)
        {
            if (change != null && change.Confidence != ComparisonConfidence.Unknown && change.Confidence != ComparisonConfidence.Unsupported)
            {
                utility.Add(change);
            }
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

        private static bool IsDamageMetric(ComparisonMetric metric)
        {
            return ContainsIgnoreCase(metric.MetricId, "damage")
                || ContainsIgnoreCase(metric.MetricId, "dps")
                || ContainsIgnoreCase(metric.Label, "damage")
                || ContainsIgnoreCase(metric.Label, "dps");
        }

        private static IReadOnlyList<GemState> ReplaceAttachedGem(
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
            return values.AsReadOnly();
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
                AddValue(values, ResolveNumericFieldValueId(fieldName), fieldName, scaling.GetValue(level, hero), "damage", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence, null);
                if (Math.Abs(scaling.adFactor) > 0.0001f)
                {
                    AddValue(values, CommonComparisonValueIds.AttackDamageCoefficient, "AD coefficient", scaling.adFactor, "", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence, new[] { fieldName });
                }

                if (Math.Abs(scaling.apFactor) > 0.0001f)
                {
                    AddValue(values, CommonComparisonValueIds.AbilityPowerCoefficient, "AP coefficient", scaling.apFactor, "", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence, new[] { fieldName });
                }

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
                AddValue(values, CommonComparisonValueIds.Charges, "Charges", config.maxCharges + config.addedCharges, "charges", ComparisonResultClass.Exact, ComparisonConfidence.HighConfidence, new[] { "trigger config " + i.ToString(CultureInfo.InvariantCulture) });
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

        private static void AddUnknown(List<ComparisonStructuredValue> values, string valueId, string label, string limitation)
        {
            if (!ContainsValue(values, valueId))
            {
                values.Add(new ComparisonStructuredValue(valueId, label, null, "", "", ComparisonResultClass.Unknown, ComparisonConfidence.Unknown, new[] { limitation }));
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
            if (normalized.Contains("damage") || normalized.Contains("dmg"))
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

            if (normalized.Contains("hit") || normalized.Contains("count"))
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
                return 0f;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum DamageSourceCategory
    {
        MemoryDirect,
        GemDirect,
        BasicAttack,
        StatusDot,
        Summon,
        CharacterPassive,
        OtherIdentified,
        Unattributed
    }

    internal enum TargetRelationship
    {
        Unknown,
        Hostile,
        SelfOrOwned,
        Friendly,
        Environment
    }

    [Flags]
    internal enum DamageAttributionFlags
    {
        None = 0,
        OwnerResolved = 1 << 0,
        SourceResolved = 1 << 1,
        TargetRelationshipResolved = 1 << 2,
        MemoryResolved = 1 << 3,
        GemResolved = 1 << 4,
        OriginatingMemoryResolved = 1 << 5
    }

    internal readonly struct PlayerKey : IEquatable<PlayerKey>
    {
        internal PlayerKey(string stableId, bool isLocalPlayer)
        {
            StableId = stableId ?? "";
            IsLocalPlayer = isLocalPlayer;
        }

        internal string StableId { get; }
        internal bool IsLocalPlayer { get; }

        public bool Equals(PlayerKey other)
        {
            return string.Equals(StableId, other.StableId, StringComparison.Ordinal) && IsLocalPlayer == other.IsLocalPlayer;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((StableId != null ? StableId.GetHashCode() : 0) * 397) ^ IsLocalPlayer.GetHashCode();
            }
        }

        public override string ToString()
        {
            return IsLocalPlayer ? "local:" + StableId : "partner:" + StableId;
        }
    }

    internal readonly struct SourceKey : IEquatable<SourceKey>
    {
        internal SourceKey(DamageSourceCategory category, string stableId, string displayName)
        {
            Category = category;
            StableId = string.IsNullOrEmpty(stableId) ? category.ToString() : stableId;
            DisplayName = string.IsNullOrEmpty(displayName) ? StableId : displayName;
        }

        internal DamageSourceCategory Category { get; }
        internal string StableId { get; }
        internal string DisplayName { get; }
        internal bool IsUnattributed
        {
            get { return Category == DamageSourceCategory.Unattributed; }
        }

        internal static SourceKey ForCategory(DamageSourceCategory category)
        {
            var stableId = category == DamageSourceCategory.Unattributed ? "UNATTRIBUTED" : category.ToString();
            return new SourceKey(category, stableId, stableId);
        }

        public bool Equals(SourceKey other)
        {
            return Category == other.Category && string.Equals(StableId, other.StableId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is SourceKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Category * 397) ^ (StableId != null ? StableId.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return Category + ":" + StableId;
        }
    }

    internal readonly struct MemoryKey : IEquatable<MemoryKey>
    {
        internal MemoryKey(PlayerKey ownerPlayerKey, string contentId, string displayName)
        {
            OwnerPlayerKey = ownerPlayerKey;
            ContentId = string.IsNullOrEmpty(contentId) ? "UNKNOWN_MEMORY" : contentId;
            DisplayName = string.IsNullOrEmpty(displayName) ? ContentId : displayName;
        }

        internal PlayerKey OwnerPlayerKey { get; }
        internal string ContentId { get; }
        internal string DisplayName { get; }
        internal string StableId
        {
            get { return OwnerPlayerKey + ":" + ContentId; }
        }

        public bool Equals(MemoryKey other)
        {
            return OwnerPlayerKey.Equals(other.OwnerPlayerKey) && string.Equals(ContentId, other.ContentId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (OwnerPlayerKey.GetHashCode() * 397) ^ (ContentId != null ? ContentId.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return StableId;
        }
    }

    internal readonly struct GemKey : IEquatable<GemKey>
    {
        internal GemKey(PlayerKey ownerPlayerKey, string contentId, string displayName)
        {
            OwnerPlayerKey = ownerPlayerKey;
            ContentId = string.IsNullOrEmpty(contentId) ? "UNKNOWN_GEM" : contentId;
            DisplayName = string.IsNullOrEmpty(displayName) ? ContentId : displayName;
        }

        internal PlayerKey OwnerPlayerKey { get; }
        internal string ContentId { get; }
        internal string DisplayName { get; }
        internal string StableId
        {
            get { return OwnerPlayerKey + ":" + ContentId; }
        }

        public bool Equals(GemKey other)
        {
            return OwnerPlayerKey.Equals(other.OwnerPlayerKey) && string.Equals(ContentId, other.ContentId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is GemKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (OwnerPlayerKey.GetHashCode() * 397) ^ (ContentId != null ? ContentId.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return StableId;
        }
    }

    internal sealed class DamageEventRecord
    {
        internal DamageEventRecord(
            long sequenceId,
            float timestamp,
            float amount,
            PlayerKey? ownerPlayerKey,
            TargetRelationship targetRelationship,
            DamageSourceCategory sourceCategory,
            SourceKey sourceKey,
            MemoryKey? memoryKey,
            GemKey? gemKey,
            MemoryKey? originatingMemoryKey,
            DamageAttributionFlags attributionFlags)
        {
            SequenceId = sequenceId;
            Timestamp = timestamp;
            Amount = amount;
            OwnerPlayerKey = ownerPlayerKey;
            TargetRelationship = targetRelationship;
            SourceCategory = sourceCategory;
            SourceKey = sourceKey;
            MemoryKey = memoryKey;
            GemKey = gemKey;
            OriginatingMemoryKey = originatingMemoryKey;
            AttributionFlags = attributionFlags;
        }

        internal long SequenceId { get; }
        internal float Timestamp { get; }
        internal float Amount { get; }
        internal PlayerKey? OwnerPlayerKey { get; }
        internal TargetRelationship TargetRelationship { get; }
        internal DamageSourceCategory SourceCategory { get; }
        internal SourceKey SourceKey { get; }
        internal MemoryKey? MemoryKey { get; }
        internal GemKey? GemKey { get; }
        internal MemoryKey? OriginatingMemoryKey { get; }
        internal DamageAttributionFlags AttributionFlags { get; }

        internal bool IsEligiblePlayerDamage
        {
            get { return Amount > 0f && OwnerPlayerKey.HasValue && TargetRelationship == TargetRelationship.Hostile; }
        }
    }

    internal sealed class DamageAggregateSnapshot
    {
        internal DamageAggregateSnapshot(float damage, int hitCount, float? firstDamageAt, float? lastDamageAt)
        {
            Damage = damage;
            HitCount = hitCount;
            FirstDamageAt = firstDamageAt;
            LastDamageAt = lastDamageAt;
        }

        internal float Damage { get; }
        internal int HitCount { get; }
        internal float? FirstDamageAt { get; }
        internal float? LastDamageAt { get; }
    }

    internal sealed class PlayerDamageSnapshot
    {
        internal PlayerDamageSnapshot(
            PlayerKey playerKey,
            DamageAggregateSnapshot aggregate,
            float? dps,
            float? partyShare,
            IReadOnlyList<SourceDamageSnapshot> sources,
            IReadOnlyList<MemoryDamageSnapshot> memories,
            IReadOnlyList<GemDamageSnapshot> gems,
            IReadOnlyList<MemoryPackageDamageSnapshot> memoryPackages,
            SourceCoverageSnapshot sourceCoverage,
            MemoryGemCoverageSnapshot memoryGemCoverage)
        {
            PlayerKey = playerKey;
            Aggregate = aggregate;
            Dps = dps;
            PartyShare = partyShare;
            Sources = sources;
            Memories = memories;
            Gems = gems;
            MemoryPackages = memoryPackages;
            SourceCoverage = sourceCoverage;
            MemoryGemCoverage = memoryGemCoverage;
        }

        internal PlayerKey PlayerKey { get; }
        internal DamageAggregateSnapshot Aggregate { get; }
        internal float? Dps { get; }
        internal float? PartyShare { get; }
        internal IReadOnlyList<SourceDamageSnapshot> Sources { get; }
        internal IReadOnlyList<MemoryDamageSnapshot> Memories { get; }
        internal IReadOnlyList<GemDamageSnapshot> Gems { get; }
        internal IReadOnlyList<MemoryPackageDamageSnapshot> MemoryPackages { get; }
        internal SourceCoverageSnapshot SourceCoverage { get; }
        internal MemoryGemCoverageSnapshot MemoryGemCoverage { get; }
    }

    internal sealed class SourceDamageSnapshot
    {
        internal SourceDamageSnapshot(SourceKey sourceKey, DamageAggregateSnapshot aggregate, float? playerShare, float? dps)
        {
            SourceKey = sourceKey;
            Aggregate = aggregate;
            PlayerShare = playerShare;
            Dps = dps;
        }

        internal SourceKey SourceKey { get; }
        internal DamageSourceCategory Category
        {
            get { return SourceKey.Category; }
        }

        internal string DisplayName
        {
            get { return SourceKey.DisplayName; }
        }

        internal DamageAggregateSnapshot Aggregate { get; }
        internal float? PlayerShare { get; }
        internal float? Dps { get; }
        internal bool IsUnattributed
        {
            get { return SourceKey.IsUnattributed; }
        }
    }

    internal sealed class SourceCoverageSnapshot
    {
        internal SourceCoverageSnapshot(float eligibleDamage, float attributedDamage, float unattributedDamage)
        {
            EligibleDamage = eligibleDamage;
            AttributedDamage = attributedDamage;
            UnattributedDamage = unattributedDamage;
        }

        internal float EligibleDamage { get; }
        internal float AttributedDamage { get; }
        internal float UnattributedDamage { get; }
        internal float? AttributionCoverageRatio
        {
            get { return EligibleDamage > 0f ? AttributedDamage / EligibleDamage : (float?)null; }
        }
    }

    internal sealed class MemoryGemCoverageSnapshot
    {
        internal MemoryGemCoverageSnapshot(
            float eligibleDamage,
            float directMemoryDamage,
            float directGemDamage,
            float memoryIdentifiedDamage,
            float gemIdentifiedDamage,
            float packageAssignableGemDamage,
            float packageRelationshipUnknownGemDamage,
            float packageAssignableChildDamage,
            float packageRelationshipUnknownChildDamage)
        {
            EligibleDamage = eligibleDamage;
            DirectMemoryDamage = directMemoryDamage;
            DirectGemDamage = directGemDamage;
            MemoryIdentifiedDamage = memoryIdentifiedDamage;
            GemIdentifiedDamage = gemIdentifiedDamage;
            PackageAssignableGemDamage = packageAssignableGemDamage;
            PackageRelationshipUnknownGemDamage = packageRelationshipUnknownGemDamage;
            PackageAssignableChildDamage = packageAssignableChildDamage;
            PackageRelationshipUnknownChildDamage = packageRelationshipUnknownChildDamage;
        }

        internal float EligibleDamage { get; }
        internal float DirectMemoryDamage { get; }
        internal float DirectGemDamage { get; }
        internal float MemoryIdentifiedDamage { get; }
        internal float GemIdentifiedDamage { get; }
        internal float PackageAssignableGemDamage { get; }
        internal float PackageRelationshipUnknownGemDamage { get; }
        internal float PackageAssignableChildDamage { get; }
        internal float PackageRelationshipUnknownChildDamage { get; }
        internal float? MemoryIdentityCoverageRatio
        {
            get { return DirectMemoryDamage > 0f ? MemoryIdentifiedDamage / DirectMemoryDamage : (float?)null; }
        }
        internal float? GemIdentityCoverageRatio
        {
            get { return DirectGemDamage > 0f ? GemIdentifiedDamage / DirectGemDamage : (float?)null; }
        }
        internal float? PackageAssignmentCoverageRatio
        {
            get { return PackageAssignableChildDamage + PackageRelationshipUnknownChildDamage > 0f ? PackageAssignableChildDamage / (PackageAssignableChildDamage + PackageRelationshipUnknownChildDamage) : (float?)null; }
        }
    }

    internal sealed class MemoryDamageSnapshot
    {
        internal MemoryDamageSnapshot(MemoryKey memoryKey, DamageAggregateSnapshot directAggregate, float? playerShare, float? directDps)
        {
            MemoryKey = memoryKey;
            DirectAggregate = directAggregate;
            PlayerShare = playerShare;
            DirectDps = directDps;
        }

        internal MemoryKey MemoryKey { get; }
        internal DamageAggregateSnapshot DirectAggregate { get; }
        internal float DirectDamage
        {
            get { return DirectAggregate.Damage; }
        }
        internal int DirectHitCount
        {
            get { return DirectAggregate.HitCount; }
        }
        internal float? PlayerShare { get; }
        internal float? DirectDps { get; }
    }

    internal sealed class GemDamageSnapshot
    {
        internal GemDamageSnapshot(GemKey gemKey, DamageAggregateSnapshot directAggregate, float? playerShare, float? directDps)
        {
            GemKey = gemKey;
            DirectAggregate = directAggregate;
            PlayerShare = playerShare;
            DirectDps = directDps;
        }

        internal GemKey GemKey { get; }
        internal DamageAggregateSnapshot DirectAggregate { get; }
        internal float DirectDamage
        {
            get { return DirectAggregate.Damage; }
        }
        internal int DamageEventCount
        {
            get { return DirectAggregate.HitCount; }
        }
        internal float? PlayerShare { get; }
        internal float? DirectDps { get; }
    }

    internal sealed class MemoryPackageChildDamageSnapshot
    {
        internal MemoryPackageChildDamageSnapshot(SourceKey sourceKey, GemKey? gemKey, DamageAggregateSnapshot directAggregate, float? playerShare, float? dps)
        {
            SourceKey = sourceKey;
            GemKey = gemKey;
            DirectAggregate = directAggregate;
            PlayerShare = playerShare;
            Dps = dps;
        }

        internal SourceKey SourceKey { get; }
        internal GemKey? GemKey { get; }
        internal DamageAggregateSnapshot DirectAggregate { get; }
        internal float? PlayerShare { get; }
        internal float? Dps { get; }
    }

    internal sealed class MemoryPackageDamageSnapshot
    {
        internal MemoryPackageDamageSnapshot(
            MemoryKey memoryKey,
            DamageAggregateSnapshot directMemoryAggregate,
            DamageAggregateSnapshot attachedDirectGemAggregate,
            DamageAggregateSnapshot otherDirectlyAttributedChildAggregate,
            DamageAggregateSnapshot totalAggregate,
            float? playerShare,
            float? dps,
            IReadOnlyList<MemoryPackageChildDamageSnapshot> childBreakdown)
        {
            MemoryKey = memoryKey;
            DirectMemoryAggregate = directMemoryAggregate;
            AttachedDirectGemAggregate = attachedDirectGemAggregate;
            OtherDirectlyAttributedChildAggregate = otherDirectlyAttributedChildAggregate;
            TotalAggregate = totalAggregate;
            PlayerShare = playerShare;
            Dps = dps;
            ChildBreakdown = childBreakdown;
        }

        internal MemoryKey MemoryKey { get; }
        internal DamageAggregateSnapshot DirectMemoryAggregate { get; }
        internal DamageAggregateSnapshot AttachedDirectGemAggregate { get; }
        internal DamageAggregateSnapshot OtherDirectlyAttributedChildAggregate { get; }
        internal DamageAggregateSnapshot TotalAggregate { get; }
        internal float DirectMemoryDamage
        {
            get { return DirectMemoryAggregate.Damage; }
        }
        internal float AttachedDirectGemDamage
        {
            get { return AttachedDirectGemAggregate.Damage; }
        }
        internal float OtherDirectlyAttributedChildDamage
        {
            get { return OtherDirectlyAttributedChildAggregate.Damage; }
        }
        internal float AttributableChildDamage
        {
            get { return AttachedDirectGemDamage + OtherDirectlyAttributedChildDamage; }
        }
        internal float TotalPackageDamage
        {
            get { return TotalAggregate.Damage; }
        }
        internal float? PlayerShare { get; }
        internal float? Dps { get; }
        internal IReadOnlyList<MemoryPackageChildDamageSnapshot> ChildBreakdown { get; }
    }

    internal sealed class DamageCoverageSnapshot
    {
        internal DamageCoverageSnapshot(
            float totalObservedDamage,
            float eligibleHostileDamage,
            float playerOwnedHostileDamage,
            float unknownOwnerHostileDamage,
            float sourceAttributedHostileDamage,
            float unattributedSourceHostileDamage,
            int totalObservedEvents,
            int eligibleHostileEvents)
        {
            TotalObservedDamage = totalObservedDamage;
            EligibleHostileDamage = eligibleHostileDamage;
            PlayerOwnedHostileDamage = playerOwnedHostileDamage;
            UnknownOwnerHostileDamage = unknownOwnerHostileDamage;
            SourceAttributedHostileDamage = sourceAttributedHostileDamage;
            UnattributedSourceHostileDamage = unattributedSourceHostileDamage;
            TotalObservedEvents = totalObservedEvents;
            EligibleHostileEvents = eligibleHostileEvents;
        }

        internal float TotalObservedDamage { get; }
        internal float EligibleHostileDamage { get; }
        internal float PlayerOwnedHostileDamage { get; }
        internal float UnknownOwnerHostileDamage { get; }
        internal float SourceAttributedHostileDamage { get; }
        internal float UnattributedSourceHostileDamage { get; }
        internal int TotalObservedEvents { get; }
        internal int EligibleHostileEvents { get; }
        internal float? SourceAttributionCoverageRatio
        {
            get { return PlayerOwnedHostileDamage > 0f ? SourceAttributedHostileDamage / PlayerOwnedHostileDamage : (float?)null; }
        }
    }

    internal sealed class EncounterDamageSnapshot
    {
        internal EncounterDamageSnapshot(
            long encounterId,
            bool isActive,
            bool isCompleted,
            float? duration,
            bool durationIsValidated,
            IReadOnlyList<PlayerDamageSnapshot> players,
            DamageCoverageSnapshot coverage,
            long revision)
        {
            EncounterId = encounterId;
            IsActive = isActive;
            IsCompleted = isCompleted;
            Duration = duration;
            DurationIsValidated = durationIsValidated;
            Players = players;
            Coverage = coverage;
            Revision = revision;
        }

        internal long EncounterId { get; }
        internal bool IsActive { get; }
        internal bool IsCompleted { get; }
        internal float? Duration { get; }
        internal bool DurationIsValidated { get; }
        internal IReadOnlyList<PlayerDamageSnapshot> Players { get; }
        internal DamageCoverageSnapshot Coverage { get; }
        internal long Revision { get; }
    }

    internal sealed class RunDamageSnapshot
    {
        internal RunDamageSnapshot(
            long runId,
            float? combatDuration,
            bool durationIsValidated,
            int encounterCount,
            IReadOnlyList<PlayerDamageSnapshot> players,
            DamageCoverageSnapshot coverage,
            long revision)
        {
            RunId = runId;
            CombatDuration = combatDuration;
            DurationIsValidated = durationIsValidated;
            EncounterCount = encounterCount;
            Players = players;
            Coverage = coverage;
            Revision = revision;
        }

        internal long RunId { get; }
        internal float? CombatDuration { get; }
        internal bool DurationIsValidated { get; }
        internal int EncounterCount { get; }
        internal IReadOnlyList<PlayerDamageSnapshot> Players { get; }
        internal DamageCoverageSnapshot Coverage { get; }
        internal long Revision { get; }
    }

    internal sealed class DamageAnalyticsService
    {
        private long _nextSequenceId;
        private long _nextEncounterId;
        private long _nextRunId;
        private RunAccumulator _run;
        private EncounterAccumulator _currentEncounter;
        private EncounterDamageSnapshot _lastCompletedEncounter;
        private RunDamageSnapshot _lastCompletedRun;
        private bool _runFinalized;
        private PlayerKey? _confirmedSoloPlayerKey;

        internal DamageAnalyticsService()
        {
            StartNewRun();
        }

        internal void SetConfirmedSoloPlayer(PlayerKey? localPlayerKey)
        {
            _confirmedSoloPlayerKey = localPlayerKey;
        }

        internal DamageEventRecord CaptureDamage(
            float timestamp,
            float amount,
            PlayerKey? ownerPlayerKey,
            TargetRelationship targetRelationship,
            DamageSourceCategory sourceCategory)
        {
            ownerPlayerKey = ApplyConfirmedSoloInvariant(ownerPlayerKey);
            var flags = DamageAttributionFlags.None;
            if (ownerPlayerKey.HasValue)
            {
                flags |= DamageAttributionFlags.OwnerResolved;
            }

            if (sourceCategory != DamageSourceCategory.Unattributed)
            {
                flags |= DamageAttributionFlags.SourceResolved;
            }

            if (targetRelationship != TargetRelationship.Unknown)
            {
                flags |= DamageAttributionFlags.TargetRelationshipResolved;
            }

            var record = new DamageEventRecord(
                ++_nextSequenceId,
                timestamp,
                amount,
                ownerPlayerKey,
                targetRelationship,
                sourceCategory,
                SourceKey.ForCategory(sourceCategory),
                null,
                null,
                null,
                flags);

            Ingest(record);
            return record;
        }

        internal DamageEventRecord CaptureDamage(
            float timestamp,
            float amount,
            PlayerKey? ownerPlayerKey,
            TargetRelationship targetRelationship,
            SourceKey sourceKey)
        {
            return CaptureDamage(timestamp, amount, ownerPlayerKey, targetRelationship, sourceKey, null, null, null);
        }

        internal DamageEventRecord CaptureDamage(
            float timestamp,
            float amount,
            PlayerKey? ownerPlayerKey,
            TargetRelationship targetRelationship,
            SourceKey sourceKey,
            MemoryKey? memoryKey,
            GemKey? gemKey,
            MemoryKey? originatingMemoryKey)
        {
            ownerPlayerKey = ApplyConfirmedSoloInvariant(ownerPlayerKey);
            var sourceCategory = sourceKey.Category;
            var flags = DamageAttributionFlags.None;
            if (ownerPlayerKey.HasValue)
            {
                flags |= DamageAttributionFlags.OwnerResolved;
            }

            if (!sourceKey.IsUnattributed)
            {
                flags |= DamageAttributionFlags.SourceResolved;
            }

            if (targetRelationship != TargetRelationship.Unknown)
            {
                flags |= DamageAttributionFlags.TargetRelationshipResolved;
            }

            if (memoryKey.HasValue)
            {
                flags |= DamageAttributionFlags.MemoryResolved;
            }

            if (gemKey.HasValue)
            {
                flags |= DamageAttributionFlags.GemResolved;
            }

            if (originatingMemoryKey.HasValue)
            {
                flags |= DamageAttributionFlags.OriginatingMemoryResolved;
            }

            var record = new DamageEventRecord(
                ++_nextSequenceId,
                timestamp,
                amount,
                ownerPlayerKey,
                targetRelationship,
                sourceCategory,
                sourceKey,
                memoryKey,
                gemKey,
                originatingMemoryKey,
                flags);

            Ingest(record);
            return record;
        }

        private PlayerKey? ApplyConfirmedSoloInvariant(PlayerKey? ownerPlayerKey)
        {
            if (!ownerPlayerKey.HasValue || !_confirmedSoloPlayerKey.HasValue)
            {
                return ownerPlayerKey;
            }

            return ownerPlayerKey.Value.Equals(_confirmedSoloPlayerKey.Value) ? ownerPlayerKey : (PlayerKey?)null;
        }

        internal void OnRoomStarted(float timestamp)
        {
            if (_currentEncounter != null && _currentEncounter.IsActive)
            {
                CompleteCurrentEncounter(timestamp, false);
            }
        }

        internal void OnCombatChanged(float timestamp, bool isInCombat)
        {
            if (isInCombat)
            {
                EnsureEncounter(timestamp);
            }
        }

        internal void OnRoomCompleted(float timestamp)
        {
            CompleteCurrentEncounter(timestamp, false);
        }

        internal void FinalizeActiveEncounterForRunEnd(float timestamp)
        {
            CompleteCurrentEncounter(timestamp, false);
        }

        internal void FinalizeRunForGameResult(float timestamp)
        {
            FinalizeActiveEncounterForRunEnd(timestamp);
            if (!_runFinalized)
            {
                _lastCompletedRun = _run.ToSnapshot();
                _runFinalized = true;
            }
        }

        internal void ResetRunForNewGame(float timestamp)
        {
            FinalizeActiveEncounterForRunEnd(timestamp);
            _currentEncounter = null;
            _lastCompletedEncounter = null;
            _lastCompletedRun = null;
            _runFinalized = false;
            _confirmedSoloPlayerKey = null;
            StartNewRun();
        }

        internal bool HasRunState
        {
            get
            {
                return _currentEncounter != null
                    || _lastCompletedEncounter != null
                    || _runFinalized
                    || _run.HasData;
            }
        }

        internal bool ResetRunForNewGameIfNeeded(float timestamp)
        {
            if (!HasRunState)
            {
                return false;
            }

            ResetRunForNewGame(timestamp);
            return true;
        }

        internal EncounterDamageSnapshot GetCurrentEncounterSnapshot(float timestamp)
        {
            return _currentEncounter != null ? _currentEncounter.ToSnapshot(timestamp, false) : null;
        }

        internal EncounterDamageSnapshot GetLastEncounterSnapshot()
        {
            return _lastCompletedEncounter;
        }

        internal RunDamageSnapshot GetRunSnapshot()
        {
            return _lastCompletedRun ?? _run.ToSnapshot();
        }

        private void Ingest(DamageEventRecord record)
        {
            if (_runFinalized)
            {
                return;
            }

            if (record.TargetRelationship == TargetRelationship.Hostile)
            {
                EnsureEncounter(record.Timestamp);
            }

            _currentEncounter?.Ingest(record);
            _run.Ingest(record);
        }

        private void EnsureEncounter(float timestamp)
        {
            if (_runFinalized)
            {
                return;
            }

            if (_currentEncounter != null && _currentEncounter.IsActive)
            {
                return;
            }

            _currentEncounter = new EncounterAccumulator(++_nextEncounterId, timestamp);
        }

        private void CompleteCurrentEncounter(float timestamp, bool validatedBoundary)
        {
            if (_currentEncounter == null || !_currentEncounter.IsActive)
            {
                return;
            }

            _currentEncounter.Complete(timestamp, validatedBoundary);
            if (_currentEncounter.HasHostileDamage)
            {
                _lastCompletedEncounter = _currentEncounter.ToSnapshot(timestamp, validatedBoundary);
                _run.AddEncounterDuration(_currentEncounter.Duration, validatedBoundary);
            }

            _currentEncounter = null;
        }

        private void StartNewRun()
        {
            _run = new RunAccumulator(++_nextRunId);
        }
    }

    internal sealed class DamageAggregate
    {
        internal float Damage { get; private set; }
        internal int HitCount { get; private set; }
        internal float? FirstDamageAt { get; private set; }
        internal float? LastDamageAt { get; private set; }

        internal void Add(DamageEventRecord record)
        {
            Damage += record.Amount;
            HitCount++;

            if (!FirstDamageAt.HasValue || record.Timestamp < FirstDamageAt.Value)
            {
                FirstDamageAt = record.Timestamp;
            }

            if (!LastDamageAt.HasValue || record.Timestamp > LastDamageAt.Value)
            {
                LastDamageAt = record.Timestamp;
            }
        }

        internal DamageAggregateSnapshot ToSnapshot()
        {
            return new DamageAggregateSnapshot(Damage, HitCount, FirstDamageAt, LastDamageAt);
        }
    }

    internal sealed class CoverageAccumulator
    {
        internal float TotalObservedDamage { get; private set; }
        internal float EligibleHostileDamage { get; private set; }
        internal float PlayerOwnedHostileDamage { get; private set; }
        internal float UnknownOwnerHostileDamage { get; private set; }
        internal float SourceAttributedHostileDamage { get; private set; }
        internal float UnattributedSourceHostileDamage { get; private set; }
        internal int TotalObservedEvents { get; private set; }
        internal int EligibleHostileEvents { get; private set; }

        internal void Add(DamageEventRecord record)
        {
            TotalObservedDamage += record.Amount;
            TotalObservedEvents++;

            if (record.TargetRelationship != TargetRelationship.Hostile)
            {
                return;
            }

            EligibleHostileDamage += record.Amount;
            EligibleHostileEvents++;

            if (record.OwnerPlayerKey.HasValue)
            {
                PlayerOwnedHostileDamage += record.Amount;
                if (record.SourceKey.IsUnattributed)
                {
                    UnattributedSourceHostileDamage += record.Amount;
                }
                else
                {
                    SourceAttributedHostileDamage += record.Amount;
                }
            }
            else
            {
                UnknownOwnerHostileDamage += record.Amount;
            }
        }

        internal DamageCoverageSnapshot ToSnapshot()
        {
            return new DamageCoverageSnapshot(
                TotalObservedDamage,
                EligibleHostileDamage,
                PlayerOwnedHostileDamage,
                UnknownOwnerHostileDamage,
                SourceAttributedHostileDamage,
                UnattributedSourceHostileDamage,
                TotalObservedEvents,
                EligibleHostileEvents);
        }
    }

    internal sealed class PlayerDamageAccumulator
    {
        private readonly Dictionary<SourceKey, DamageAggregate> _sources = new Dictionary<SourceKey, DamageAggregate>();
        private readonly Dictionary<MemoryKey, DamageAggregate> _memories = new Dictionary<MemoryKey, DamageAggregate>();
        private readonly Dictionary<GemKey, DamageAggregate> _gems = new Dictionary<GemKey, DamageAggregate>();
        private readonly Dictionary<MemoryKey, MemoryPackageAggregate> _memoryPackages = new Dictionary<MemoryKey, MemoryPackageAggregate>();

        internal DamageAggregate Aggregate { get; } = new DamageAggregate();
        internal float SourceAttributedDamage { get; private set; }
        internal float UnattributedDamage { get; private set; }
        internal float DirectMemoryDamage { get; private set; }
        internal float DirectGemDamage { get; private set; }
        internal float MemoryIdentifiedDamage { get; private set; }
        internal float GemIdentifiedDamage { get; private set; }
        internal float PackageAssignableGemDamage { get; private set; }
        internal float PackageRelationshipUnknownGemDamage { get; private set; }
        internal float PackageAssignableChildDamage { get; private set; }
        internal float PackageRelationshipUnknownChildDamage { get; private set; }

        internal void Add(DamageEventRecord record)
        {
            Aggregate.Add(record);

            if (record.SourceKey.IsUnattributed)
            {
                UnattributedDamage += record.Amount;
            }
            else
            {
                SourceAttributedDamage += record.Amount;
            }

            if (!_sources.TryGetValue(record.SourceKey, out var sourceAggregate))
            {
                sourceAggregate = new DamageAggregate();
                _sources[record.SourceKey] = sourceAggregate;
            }

            sourceAggregate.Add(record);

            if (record.SourceCategory == DamageSourceCategory.MemoryDirect)
            {
                DirectMemoryDamage += record.Amount;
                if (record.MemoryKey.HasValue)
                {
                    MemoryIdentifiedDamage += record.Amount;
                    var memoryKey = record.MemoryKey.Value;
                    AddAggregate(_memories, memoryKey, record);
                    GetMemoryPackage(memoryKey).AddDirectMemory(record);
                }
            }

            if (record.SourceCategory == DamageSourceCategory.GemDirect)
            {
                DirectGemDamage += record.Amount;
                if (record.GemKey.HasValue)
                {
                    GemIdentifiedDamage += record.Amount;
                    var gemKey = record.GemKey.Value;
                    AddAggregate(_gems, gemKey, record);
                }

                if (record.OriginatingMemoryKey.HasValue)
                {
                    PackageAssignableGemDamage += record.Amount;
                    PackageAssignableChildDamage += record.Amount;
                    GetMemoryPackage(record.OriginatingMemoryKey.Value).AddDirectGem(record, record.GemKey);
                }
                else
                {
                    PackageRelationshipUnknownGemDamage += record.Amount;
                    PackageRelationshipUnknownChildDamage += record.Amount;
                }
            }

            if (record.SourceCategory == DamageSourceCategory.StatusDot)
            {
                if (record.OriginatingMemoryKey.HasValue)
                {
                    PackageAssignableChildDamage += record.Amount;
                    GetMemoryPackage(record.OriginatingMemoryKey.Value).AddDirectChild(record, record.SourceKey);
                }
                else
                {
                    PackageRelationshipUnknownChildDamage += record.Amount;
                }
            }
        }

        internal IReadOnlyList<SourceDamageSnapshot> BuildSourceSnapshots(float playerDamage, float? duration)
        {
            var entries = new List<KeyValuePair<SourceKey, DamageAggregate>>(_sources);
            entries.Sort(CompareSources);

            var snapshots = new List<SourceDamageSnapshot>(entries.Count);
            foreach (var entry in entries)
            {
                var aggregate = entry.Value.ToSnapshot();
                var playerShare = playerDamage > 0f ? aggregate.Damage / playerDamage : (float?)null;
                var dps = duration.HasValue && duration.Value > 0f ? aggregate.Damage / duration.Value : (float?)null;
                snapshots.Add(new SourceDamageSnapshot(entry.Key, aggregate, playerShare, dps));
            }

            return snapshots.AsReadOnly();
        }

        internal SourceCoverageSnapshot BuildSourceCoverageSnapshot()
        {
            return new SourceCoverageSnapshot(Aggregate.Damage, SourceAttributedDamage, UnattributedDamage);
        }

        internal MemoryGemCoverageSnapshot BuildMemoryGemCoverageSnapshot()
        {
            return new MemoryGemCoverageSnapshot(
                Aggregate.Damage,
                DirectMemoryDamage,
                DirectGemDamage,
                MemoryIdentifiedDamage,
                GemIdentifiedDamage,
                PackageAssignableGemDamage,
                PackageRelationshipUnknownGemDamage,
                PackageAssignableChildDamage,
                PackageRelationshipUnknownChildDamage);
        }

        internal IReadOnlyList<MemoryDamageSnapshot> BuildMemorySnapshots(float playerDamage, float? duration)
        {
            var entries = new List<KeyValuePair<MemoryKey, DamageAggregate>>(_memories);
            entries.Sort(CompareMemories);

            var snapshots = new List<MemoryDamageSnapshot>(entries.Count);
            foreach (var entry in entries)
            {
                var aggregate = entry.Value.ToSnapshot();
                var playerShare = playerDamage > 0f ? aggregate.Damage / playerDamage : (float?)null;
                var dps = duration.HasValue && duration.Value > 0f ? aggregate.Damage / duration.Value : (float?)null;
                snapshots.Add(new MemoryDamageSnapshot(entry.Key, aggregate, playerShare, dps));
            }

            return snapshots.AsReadOnly();
        }

        internal IReadOnlyList<GemDamageSnapshot> BuildGemSnapshots(float playerDamage, float? duration)
        {
            var entries = new List<KeyValuePair<GemKey, DamageAggregate>>(_gems);
            entries.Sort(CompareGems);

            var snapshots = new List<GemDamageSnapshot>(entries.Count);
            foreach (var entry in entries)
            {
                var aggregate = entry.Value.ToSnapshot();
                var playerShare = playerDamage > 0f ? aggregate.Damage / playerDamage : (float?)null;
                var dps = duration.HasValue && duration.Value > 0f ? aggregate.Damage / duration.Value : (float?)null;
                snapshots.Add(new GemDamageSnapshot(entry.Key, aggregate, playerShare, dps));
            }

            return snapshots.AsReadOnly();
        }

        internal IReadOnlyList<MemoryPackageDamageSnapshot> BuildMemoryPackageSnapshots(float playerDamage, float? duration)
        {
            var entries = new List<KeyValuePair<MemoryKey, MemoryPackageAggregate>>(_memoryPackages);
            entries.Sort(CompareMemoryPackages);

            var snapshots = new List<MemoryPackageDamageSnapshot>(entries.Count);
            foreach (var entry in entries)
            {
                snapshots.Add(entry.Value.ToSnapshot(entry.Key, playerDamage, duration));
            }

            return snapshots.AsReadOnly();
        }

        internal static void AddAggregate<TKey>(Dictionary<TKey, DamageAggregate> aggregates, TKey key, DamageEventRecord record)
        {
            if (!aggregates.TryGetValue(key, out var aggregate))
            {
                aggregate = new DamageAggregate();
                aggregates[key] = aggregate;
            }

            aggregate.Add(record);
        }

        private MemoryPackageAggregate GetMemoryPackage(MemoryKey memoryKey)
        {
            if (!_memoryPackages.TryGetValue(memoryKey, out var package))
            {
                package = new MemoryPackageAggregate();
                _memoryPackages[memoryKey] = package;
            }

            return package;
        }

        private static int CompareSources(KeyValuePair<SourceKey, DamageAggregate> left, KeyValuePair<SourceKey, DamageAggregate> right)
        {
            var damageComparison = right.Value.Damage.CompareTo(left.Value.Damage);
            if (damageComparison != 0)
            {
                return damageComparison;
            }

            return string.Compare(left.Key.StableId, right.Key.StableId, StringComparison.Ordinal);
        }

        private static int CompareMemories(KeyValuePair<MemoryKey, DamageAggregate> left, KeyValuePair<MemoryKey, DamageAggregate> right)
        {
            var damageComparison = right.Value.Damage.CompareTo(left.Value.Damage);
            if (damageComparison != 0)
            {
                return damageComparison;
            }

            return string.Compare(left.Key.StableId, right.Key.StableId, StringComparison.Ordinal);
        }

        private static int CompareGems(KeyValuePair<GemKey, DamageAggregate> left, KeyValuePair<GemKey, DamageAggregate> right)
        {
            var damageComparison = right.Value.Damage.CompareTo(left.Value.Damage);
            if (damageComparison != 0)
            {
                return damageComparison;
            }

            return string.Compare(left.Key.StableId, right.Key.StableId, StringComparison.Ordinal);
        }

        private static int CompareMemoryPackages(KeyValuePair<MemoryKey, MemoryPackageAggregate> left, KeyValuePair<MemoryKey, MemoryPackageAggregate> right)
        {
            var damageComparison = right.Value.Total.Damage.CompareTo(left.Value.Total.Damage);
            if (damageComparison != 0)
            {
                return damageComparison;
            }

            return string.Compare(left.Key.StableId, right.Key.StableId, StringComparison.Ordinal);
        }
    }

    internal sealed class MemoryPackageAggregate
    {
        private readonly Dictionary<GemKey, DamageAggregate> _attachedGems = new Dictionary<GemKey, DamageAggregate>();
        private readonly Dictionary<SourceKey, DamageAggregate> _directChildren = new Dictionary<SourceKey, DamageAggregate>();

        internal DamageAggregate DirectMemory { get; } = new DamageAggregate();
        internal DamageAggregate AttachedDirectGem { get; } = new DamageAggregate();
        internal DamageAggregate OtherDirectlyAttributedChild { get; } = new DamageAggregate();
        internal DamageAggregate Total { get; } = new DamageAggregate();

        internal void AddDirectMemory(DamageEventRecord record)
        {
            DirectMemory.Add(record);
            Total.Add(record);
        }

        internal void AddDirectGem(DamageEventRecord record, GemKey? gemKey)
        {
            AttachedDirectGem.Add(record);
            Total.Add(record);
            if (gemKey.HasValue)
            {
                PlayerDamageAccumulator.AddAggregate(_attachedGems, gemKey.Value, record);
            }
        }

        internal void AddDirectChild(DamageEventRecord record, SourceKey sourceKey)
        {
            OtherDirectlyAttributedChild.Add(record);
            Total.Add(record);
            PlayerDamageAccumulator.AddAggregate(_directChildren, sourceKey, record);
        }

        internal MemoryPackageDamageSnapshot ToSnapshot(MemoryKey memoryKey, float playerDamage, float? duration)
        {
            var total = Total.ToSnapshot();
            var playerShare = playerDamage > 0f ? total.Damage / playerDamage : (float?)null;
            var dps = duration.HasValue && duration.Value > 0f ? total.Damage / duration.Value : (float?)null;
            return new MemoryPackageDamageSnapshot(
                memoryKey,
                DirectMemory.ToSnapshot(),
                AttachedDirectGem.ToSnapshot(),
                OtherDirectlyAttributedChild.ToSnapshot(),
                total,
                playerShare,
                dps,
                BuildChildSnapshots(playerDamage, duration));
        }

        private IReadOnlyList<MemoryPackageChildDamageSnapshot> BuildChildSnapshots(float playerDamage, float? duration)
        {
            var entries = new List<MemoryPackageChildEntry>(_attachedGems.Count + _directChildren.Count);
            foreach (var entry in _attachedGems)
            {
                entries.Add(new MemoryPackageChildEntry(
                    new SourceKey(DamageSourceCategory.GemDirect, entry.Key.ContentId, entry.Key.DisplayName),
                    entry.Key,
                    entry.Value));
            }

            foreach (var entry in _directChildren)
            {
                entries.Add(new MemoryPackageChildEntry(entry.Key, null, entry.Value));
            }

            entries.Sort(CompareChildren);

            var snapshots = new List<MemoryPackageChildDamageSnapshot>(entries.Count);
            foreach (var entry in entries)
            {
                var aggregate = entry.Aggregate.ToSnapshot();
                var playerShare = playerDamage > 0f ? aggregate.Damage / playerDamage : (float?)null;
                var dps = duration.HasValue && duration.Value > 0f ? aggregate.Damage / duration.Value : (float?)null;
                snapshots.Add(new MemoryPackageChildDamageSnapshot(entry.SourceKey, entry.GemKey, aggregate, playerShare, dps));
            }

            return snapshots.AsReadOnly();
        }

        private static int CompareChildren(MemoryPackageChildEntry left, MemoryPackageChildEntry right)
        {
            var damageComparison = right.Aggregate.Damage.CompareTo(left.Aggregate.Damage);
            if (damageComparison != 0)
            {
                return damageComparison;
            }

            return string.Compare(left.SourceKey.StableId, right.SourceKey.StableId, StringComparison.Ordinal);
        }

        private readonly struct MemoryPackageChildEntry
        {
            internal MemoryPackageChildEntry(SourceKey sourceKey, GemKey? gemKey, DamageAggregate aggregate)
            {
                SourceKey = sourceKey;
                GemKey = gemKey;
                Aggregate = aggregate;
            }

            internal SourceKey SourceKey { get; }
            internal GemKey? GemKey { get; }
            internal DamageAggregate Aggregate { get; }
        }
    }

    internal abstract class DamageScopeAccumulator
    {
        private readonly Dictionary<PlayerKey, PlayerDamageAccumulator> _players = new Dictionary<PlayerKey, PlayerDamageAccumulator>();
        private readonly CoverageAccumulator _coverage = new CoverageAccumulator();

        internal long Revision { get; private set; }
        internal bool HasHostileDamage
        {
            get { return _coverage.EligibleHostileEvents > 0; }
        }

        internal void Ingest(DamageEventRecord record)
        {
            _coverage.Add(record);

            if (record.IsEligiblePlayerDamage)
            {
                var key = record.OwnerPlayerKey.Value;
                if (!_players.TryGetValue(key, out var aggregate))
                {
                    aggregate = new PlayerDamageAccumulator();
                    _players[key] = aggregate;
                }

                aggregate.Add(record);
            }

            Revision++;
        }

        protected IReadOnlyList<PlayerDamageSnapshot> BuildPlayerSnapshots(float? duration)
        {
            var entries = new List<KeyValuePair<PlayerKey, PlayerDamageAccumulator>>(_players);
            entries.Sort(ComparePlayers);

            var totalPlayerDamage = 0f;
            foreach (var entry in entries)
            {
                totalPlayerDamage += entry.Value.Aggregate.Damage;
            }

            var snapshots = new List<PlayerDamageSnapshot>(entries.Count);
            foreach (var entry in entries)
            {
                var aggregate = entry.Value.Aggregate.ToSnapshot();
                var dps = duration.HasValue && duration.Value > 0f ? aggregate.Damage / duration.Value : (float?)null;
                var partyShare = totalPlayerDamage > 0f ? aggregate.Damage / totalPlayerDamage : (float?)null;
                snapshots.Add(new PlayerDamageSnapshot(
                    entry.Key,
                    aggregate,
                    dps,
                    partyShare,
                    entry.Value.BuildSourceSnapshots(aggregate.Damage, duration),
                    entry.Value.BuildMemorySnapshots(aggregate.Damage, duration),
                    entry.Value.BuildGemSnapshots(aggregate.Damage, duration),
                    entry.Value.BuildMemoryPackageSnapshots(aggregate.Damage, duration),
                    entry.Value.BuildSourceCoverageSnapshot(),
                    entry.Value.BuildMemoryGemCoverageSnapshot()));
            }

            return snapshots.AsReadOnly();
        }

        protected DamageCoverageSnapshot BuildCoverageSnapshot()
        {
            return _coverage.ToSnapshot();
        }

        private static int ComparePlayers(KeyValuePair<PlayerKey, PlayerDamageAccumulator> left, KeyValuePair<PlayerKey, PlayerDamageAccumulator> right)
        {
            if (left.Key.IsLocalPlayer != right.Key.IsLocalPlayer)
            {
                return left.Key.IsLocalPlayer ? -1 : 1;
            }

            var damageComparison = right.Value.Aggregate.Damage.CompareTo(left.Value.Aggregate.Damage);
            if (damageComparison != 0)
            {
                return damageComparison;
            }

            return string.Compare(left.Key.StableId, right.Key.StableId, StringComparison.Ordinal);
        }
    }

    internal sealed class EncounterAccumulator : DamageScopeAccumulator
    {
        private readonly float _startedAt;
        private float? _endedAt;
        private bool _boundaryValidated;

        internal EncounterAccumulator(long encounterId, float startedAt)
        {
            EncounterId = encounterId;
            _startedAt = startedAt;
            IsActive = true;
        }

        internal long EncounterId { get; }
        internal bool IsActive { get; private set; }
        internal float? Duration
        {
            get
            {
                if (!_endedAt.HasValue)
                {
                    return null;
                }

                return Math.Max(0f, _endedAt.Value - _startedAt);
            }
        }

        internal void Complete(float timestamp, bool boundaryValidated)
        {
            IsActive = false;
            _endedAt = timestamp;
            _boundaryValidated = boundaryValidated;
        }

        internal EncounterDamageSnapshot ToSnapshot(float timestamp, bool durationIsValidated)
        {
            var duration = _endedAt.HasValue ? Duration : Math.Max(0f, timestamp - _startedAt);
            return new EncounterDamageSnapshot(
                EncounterId,
                IsActive,
                !IsActive,
                duration,
                durationIsValidated && _boundaryValidated,
                BuildPlayerSnapshots(duration),
                BuildCoverageSnapshot(),
                Revision);
        }
    }

    internal sealed class RunAccumulator : DamageScopeAccumulator
    {
        private float _combatDuration;
        private bool _durationValidated = true;

        internal RunAccumulator(long runId)
        {
            RunId = runId;
        }

        internal long RunId { get; }
        internal int EncounterCount { get; private set; }
        internal bool HasData => EncounterCount > 0 || Revision > 0;

        internal void AddEncounterDuration(float? duration, bool durationValidated)
        {
            if (duration.HasValue)
            {
                _combatDuration += duration.Value;
            }

            _durationValidated = _durationValidated && durationValidated;
            EncounterCount++;
        }

        internal RunDamageSnapshot ToSnapshot()
        {
            var duration = _combatDuration > 0f ? _combatDuration : (float?)null;
            return new RunDamageSnapshot(
                RunId,
                duration,
                _durationValidated && duration.HasValue,
                EncounterCount,
                BuildPlayerSnapshots(duration),
                BuildCoverageSnapshot(),
                Revision);
        }
    }
}

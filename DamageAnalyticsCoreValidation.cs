using System;
using System.Collections.Generic;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal static class DamageAnalyticsCoreValidation
    {
        private const float Tolerance = 0.001f;

        internal static void RunAll()
        {
            ValidatesSingleLocalPlayerEvent();
            ValidatesSingleLocalSourceRow();
            ValidatesMultipleLocalSources();
            ValidatesRepeatedEventsFromSameSourceAggregate();
            ValidatesMultipleEventsAreNotDuplicated();
            ValidatesLocalAndPartnerShares();
            ValidatesLocalAndPartnerSourceBreakdownIsolation();
            ValidatesUnknownOwnerCoverage();
            ValidatesConfirmedSoloRejectsSecondPlayerBucket();
            ValidatesConfirmedSoloKeepsSummonOwnedByLocalPlayer();
            ValidatesConfirmedSoloNonPlayerHostileDamageDoesNotCreatePartner();
            ValidatesConfirmedSoloResetAllowsCoopIsolation();
            ValidatesUnattributedSourceDamageIsRetained();
            ValidatesNonHostileDamageFiltering();
            ValidatesNonHostileDamageDoesNotContaminateSources();
            ValidatesEncounterSnapshotsFreezeAndRunPersists();
            ValidatesFrozenEncounterSourceSnapshotsAreImmutable();
            ValidatesRunAccumulatesSourcesAcrossEncounters();
            ValidatesSequenceIdsAreMonotonic();
            ValidatesEmptyEncounterDoesNotIncrementRun();
            ValidatesFinalGameResultFreezesCurrentRun();
            ValidatesCompletedRunRemainsReadableAfterFinalization();
            ValidatesCompletedRunSnapshotIsImmutable();
            ValidatesPostFinalizationDamageDoesNotMutateCompletedTotals();
            ValidatesNewGameBoundaryResetClearsPreviousCompletedRunState();
            ValidatesNextRunStartsAtZeroAndDoesNotLeak();
            ValidatesOrdinaryRoomTransitionsDoNotClearRun();
            ValidatesNewGameBoundaryResetsRunState();
            ValidatesRunResetClearsSourceTotals();
            ValidatesNewGameBoundaryResetsOnlyWhenRunStateExists();
            ValidatesSourceOrderingIsDeterministic();
            ValidatesDirectMemoryAnalyticsAggregateWithoutDoubleCounting();
            ValidatesDirectGemAnalyticsAggregate();
            ValidatesMemoryPackageDoesNotInflatePlayerTotal();
            ValidatesMultipleMemoryPackagesAssignCorrectChildren();
            ValidatesUnknownOriginatingMemoryStaysOutOfPackages();
            ValidatesKnownOriginatingMemoryAssignsPackageWhenGemIdentityUnknown();
            ValidatesKnownOriginatingMemoryAssignsStatusDotToPackage();
            ValidatesAmbiguousStatusDotStaysOutOfPackages();
            ValidatesGemAndStatusDotChildrenRollIntoSamePackage();
            ValidatesLocalAndPartnerStatusDotPackageIsolation();
            ValidatesRunAccumulatesMemoryGemPackagesAcrossEncounters();
            ValidatesRunResetClearsMemoryGemPackages();
            ValidatesLocalAndPartnerMemoryGemPackageIsolation();
            ValidatesRunSnapshotMemoryGemPackageImmutability();
            ValidatesMemoryGemPackageOrderingIsDeterministic();
            ValidatesAnalyticsPanelsStackMidLeftWithoutOverlap();
            ValidatesAnalyticsPanelHitTestMatchesRenderedRegion();
            ValidatesGameInputSuppressionInsidePanelsOnly();
            ValidatesRunPanelTabSelectionChangesMode();
            ValidatesAnalyticsPanelPresentationLabels();
            ValidatesAnalyticsPanelsHideForNativeMemoryGemTooltips();
            ValidatesAnalyticsUiLifecycleVisibilityPolicy();
            ValidatesEncounterPanelPrefersCurrentThenLast();
            ValidatesEncounterPanelSourceRowsAndCoverage();
            ValidatesEncounterPanelPlayerSelectionSwitchesRowsAndCoverage();
            ValidatesEncounterPanelAbsentPartnerFallsBackToLocal();
            ValidatesEncounterPanelFriendlyDisplayNamesAndCoverageSummary();
            ValidatesEncounterPanelKeepsUnattributedSourceVisible();
            ValidatesRunPanelMemoryGemPackageRows();
            ValidatesRunPanelPlayerSelectionSwitchesRowsAndCoverage();
            ValidatesRunPanelAbsentPartnerFallsBackToLocal();
            ValidatesRunPanelCoverageAndEmptyStates();
            ValidatesRunPanelDefaultAndProvisionalPresentation();
            ValidatesBuildStateSnapshotCopiesInputs();
            ValidatesComparisonResultAndConfidenceSemantics();
            ValidatesUnknownAndUnsupportedComparisonValuesAreNotZero();
            ValidatesNotApplicableComparisonValuesAreNotZero();
            ValidatesBuildOptionComparisonComposition();
            ValidatesContextualEffectEvaluatorPassesThroughStructuredValues();
            ValidatesContextualEffectEvaluatorResolvesMatchingCondition();
            ValidatesContextualEffectEvaluatorFallsBackToUnknownWhenContextIsInsufficient();
            ValidatesContextualEffectEvaluatorUsesMostConservativeConfidence();
            ValidatesContextualEffectEvaluatorEffectiveElementStates();
            ValidatesContextualTooltipPresentationBuildsSupportedGemContributionRows();
            ValidatesContextualTooltipPresentationPreservesEstimatedUnknownAndUnsupportedRows();
            ValidatesEffectiveMemoryElementIndicatorKnownState();
            ValidatesEffectiveMemoryElementIndicatorNativeFallbackState();
            ValidatesEffectiveMemoryElementIndicatorNeutralStates();
            ValidatesContextualTooltipPresentationHasNoNativeUiHookAssumptions();
            ValidatesCommonComparisonDirectDamageAndActivationDamage();
            ValidatesCommonComparisonAdAndApScalingRequireCoefficients();
            ValidatesCommonComparisonCooldownChargesAndRangeUtility();
            ValidatesCommonComparisonSimpleStatBonuses();
            ValidatesCommonComparisonUnsupportedRowsDoNotFabricateZeroesOrDps();
            ValidatesComparisonPresentationRanksMultipleKnownDamageTargets();
            ValidatesComparisonPresentationSuppressesUnsafeRecommendations();
            ValidatesComparisonPresentationFormatsExactDerivedAndEstimatedMetrics();
            ValidatesComparisonPresentationKeepsUtilityRowsSeparate();
            ValidatesComparisonPresentationEmptyState();
            ValidatesComparisonPresentationKeepsObservedContextSeparate();
            ValidatesLiveCandidateGemComparisonEvaluatesLegalTargetsIndependently();
            ValidatesLiveCandidateGemComparisonIncludesEmptySlotAction();
            ValidatesModifierGemUsesContextualMemoryComparison();
            ValidatesContextualGemComparisonUsesTargetParentMemory();
            ValidatesDirectDamageGemRetainsDirectMetricInContextualComparison();
            ValidatesLiveCandidateComparisonDistinguishesReplacementAndInsertion();
            ValidatesLiveCandidateComparisonSurfacesMaterialUtility();
            ValidatesMaterialUtilitySuppressesOverallRecommendation();
            ValidatesBestKnownDamageStaysDistinctFromBestReplacement();
            ValidatesObservedPackageContextDoesNotAffectGemRanking();
            ValidatesLiveCandidateComparisonAddsObservedRunContextFromSnapshotsOnly();
            ValidatesLiveCandidateComparisonOmitsMissingObservedHistory();
            ValidatesLiveCandidateComparisonPreservesNotApplicableAndUnknownRows();
            ValidatesChargeRegressionUsesNativeDisplayedCap();
            ValidatesLiveCandidateMemoryComparisonKeepsSlotGemContext();
            ValidatesLiveCandidateMemoryComparisonIncludesEmptySlotAction();
            ValidatesLiveCandidateComparisonLifecycleClearReplaceAndDuplicateFallback();
            ValidatesLiveComparisonPanelRendersSupportedGemAndMemoryTargets();
            ValidatesLiveComparisonPanelPreservesUnknownAndDuplicateUnsupportedStates();
            ValidatesLiveComparisonPanelClearsAndRefreshesBySequence();
            ValidatesLiveComparisonPanelLayoutBoundsAndHitTesting();
            ValidatesLiveComparisonPanelLongNamesAndOverflowPresentation();
            ValidatesLiveComparisonPanelStateLabelsSeparateUnsupportedRows();
        }

        private static void ValidatesSingleLocalPlayerEvent()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            var encounter = Require(service.GetCurrentEncounterSnapshot(2f), "current encounter");
            var run = service.GetRunSnapshot();

            AssertPlayerDamage(encounter, local, 100f, 1, "encounter local");
            AssertPlayerDamage(run, local, 100f, 1, "run local");
        }

        private static void ValidatesSingleLocalSourceRow()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var source = Source(DamageSourceCategory.BasicAttack, "BASIC_ATTACK");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, source);

            var player = FindPlayer(Require(service.GetCurrentEncounterSnapshot(2f), "current encounter"), local, "encounter local");
            AssertSourceCount(player, 1, "single source count");
            AssertSourceDamage(player, source, 100f, 1, "single source");
            AssertApproximately(100f, player.SourceCoverage.EligibleDamage, "single source eligible");
            AssertApproximately(100f, player.SourceCoverage.AttributedDamage, "single source attributed");
            AssertApproximately(1f, Require(player.SourceCoverage.AttributionCoverageRatio, "single source coverage"), "single source coverage");
        }

        private static void ValidatesMultipleLocalSources()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var attack = Source(DamageSourceCategory.BasicAttack, "BASIC_ATTACK");
            var memory = Source(DamageSourceCategory.MemoryDirect, "Mem_Fireball");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, attack);
            service.CaptureDamage(2f, 75f, local, TargetRelationship.Hostile, memory);

            var player = FindPlayer(Require(service.GetCurrentEncounterSnapshot(3f), "current encounter"), local, "encounter local");
            AssertSourceCount(player, 2, "multiple source count");
            AssertSourceDamage(player, attack, 100f, 1, "attack source");
            AssertSourceDamage(player, memory, 75f, 1, "memory source");
            AssertSourceSumEqualsPlayer(player, "multiple source sum");
        }

        private static void ValidatesRepeatedEventsFromSameSourceAggregate()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var source = Source(DamageSourceCategory.MemoryDirect, "Mem_Icicle");

            service.CaptureDamage(1f, 10f, local, TargetRelationship.Hostile, source);
            service.CaptureDamage(2f, 15f, local, TargetRelationship.Hostile, source);
            service.CaptureDamage(3f, 20f, local, TargetRelationship.Hostile, source);

            var player = FindPlayer(Require(service.GetCurrentEncounterSnapshot(4f), "current encounter"), local, "encounter local");
            AssertSourceCount(player, 1, "repeated source count");
            AssertSourceDamage(player, source, 45f, 3, "repeated source");
        }

        private static void ValidatesMultipleEventsAreNotDuplicated()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(2f, 50f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(3f, 25f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            var encounter = Require(service.GetCurrentEncounterSnapshot(4f), "current encounter");
            var run = service.GetRunSnapshot();

            AssertPlayerDamage(encounter, local, 175f, 3, "encounter local");
            AssertPlayerDamage(run, local, 175f, 3, "run local");
        }

        private static void ValidatesLocalAndPartnerShares()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(2f, 50f, partner, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            var encounter = Require(service.GetCurrentEncounterSnapshot(3f), "current encounter");
            var localSnapshot = FindPlayer(encounter, local, "encounter local");
            var partnerSnapshot = FindPlayer(encounter, partner, "encounter partner");

            AssertApproximately(100f, localSnapshot.Aggregate.Damage, "local damage");
            AssertApproximately(50f, partnerSnapshot.Aggregate.Damage, "partner damage");
            AssertApproximately(100f / 150f, Require(localSnapshot.PartyShare, "local share"), "local share");
            AssertApproximately(50f / 150f, Require(partnerSnapshot.PartyShare, "partner share"), "partner share");
        }

        private static void ValidatesLocalAndPartnerSourceBreakdownIsolation()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");
            var sharedSourceId = Source(DamageSourceCategory.BasicAttack, "BASIC_ATTACK");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, sharedSourceId);
            service.CaptureDamage(2f, 50f, partner, TargetRelationship.Hostile, sharedSourceId);

            var encounter = Require(service.GetCurrentEncounterSnapshot(3f), "current encounter");
            var localSnapshot = FindPlayer(encounter, local, "encounter local");
            var partnerSnapshot = FindPlayer(encounter, partner, "encounter partner");

            AssertSourceDamage(localSnapshot, sharedSourceId, 100f, 1, "local isolated source");
            AssertSourceDamage(partnerSnapshot, sharedSourceId, 50f, 1, "partner isolated source");
            AssertApproximately(100f / 150f, Require(localSnapshot.PartyShare, "local isolated share"), "local isolated share");
            AssertApproximately(50f / 150f, Require(partnerSnapshot.PartyShare, "partner isolated share"), "partner isolated share");
        }

        private static void ValidatesUnknownOwnerCoverage()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(2f, 40f, null, TargetRelationship.Hostile, DamageSourceCategory.Unattributed);

            var encounter = Require(service.GetCurrentEncounterSnapshot(3f), "current encounter");
            var run = service.GetRunSnapshot();

            AssertPlayerDamage(encounter, local, 100f, 1, "encounter local");
            AssertPlayerCount(encounter, 1, "encounter players");
            AssertApproximately(140f, encounter.Coverage.EligibleHostileDamage, "encounter hostile coverage");
            AssertApproximately(100f, encounter.Coverage.PlayerOwnedHostileDamage, "encounter player-owned hostile coverage");
            AssertApproximately(40f, encounter.Coverage.UnknownOwnerHostileDamage, "encounter unknown hostile coverage");
            AssertPlayerDamage(run, local, 100f, 1, "run local");
            AssertApproximately(40f, run.Coverage.UnknownOwnerHostileDamage, "run unknown hostile coverage");
        }

        private static void ValidatesConfirmedSoloRejectsSecondPlayerBucket()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var phantom = Partner("Dreamer");

            service.SetConfirmedSoloPlayer(local);
            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(2f, 40f, phantom, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            var run = service.GetRunSnapshot();
            AssertPlayerCount(run, 1, "confirmed solo player count");
            AssertPlayerDamage(run, local, 100f, 1, "confirmed solo local damage");
            AssertApproximately(140f, run.Coverage.EligibleHostileDamage, "confirmed solo hostile coverage");
            AssertApproximately(100f, run.Coverage.PlayerOwnedHostileDamage, "confirmed solo player-owned coverage");
            AssertApproximately(40f, run.Coverage.UnknownOwnerHostileDamage, "confirmed solo rejected owner coverage");
        }

        private static void ValidatesConfirmedSoloKeepsSummonOwnedByLocalPlayer()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var summon = Source(DamageSourceCategory.Summon, "Sum_Local");

            service.SetConfirmedSoloPlayer(local);
            service.CaptureDamage(1f, 75f, local, TargetRelationship.Hostile, summon);

            var player = FindPlayer(service.GetRunSnapshot(), local, "confirmed solo summon local");
            AssertSourceDamage(player, summon, 75f, 1, "confirmed solo summon source");
            AssertPlayerCount(service.GetRunSnapshot(), 1, "confirmed solo summon player count");
        }

        private static void ValidatesConfirmedSoloNonPlayerHostileDamageDoesNotCreatePartner()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.SetConfirmedSoloPlayer(local);
            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(2f, 25f, null, TargetRelationship.Hostile, DamageSourceCategory.Unattributed);

            var run = service.GetRunSnapshot();
            AssertPlayerCount(run, 1, "confirmed solo non-player player count");
            AssertPlayerDamage(run, local, 100f, 1, "confirmed solo non-player local damage");
            AssertApproximately(25f, run.Coverage.UnknownOwnerHostileDamage, "confirmed solo non-player unknown coverage");
        }

        private static void ValidatesConfirmedSoloResetAllowsCoopIsolation()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");

            service.SetConfirmedSoloPlayer(local);
            service.CaptureDamage(1f, 100f, partner, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            AssertPlayerCount(service.GetRunSnapshot(), 0, "confirmed solo rejects partner before reset");

            service.ResetRunForNewGame(5f);
            service.CaptureDamage(6f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(7f, 50f, partner, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            var run = service.GetRunSnapshot();
            AssertPlayerCount(run, 2, "coop isolation after solo reset");
            AssertPlayerDamage(run, local, 100f, 1, "coop local after solo reset");
            AssertPlayerDamage(run, partner, 50f, 1, "coop partner after solo reset");
        }

        private static void ValidatesUnattributedSourceDamageIsRetained()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var attributed = Source(DamageSourceCategory.BasicAttack, "BASIC_ATTACK");
            var unattributed = SourceKey.ForCategory(DamageSourceCategory.Unattributed);

            service.CaptureDamage(1f, 60f, local, TargetRelationship.Hostile, attributed);
            service.CaptureDamage(2f, 40f, local, TargetRelationship.Hostile, unattributed);

            var player = FindPlayer(Require(service.GetCurrentEncounterSnapshot(3f), "current encounter"), local, "encounter local");
            AssertPlayerDamage(Require(service.GetCurrentEncounterSnapshot(3f), "current encounter"), local, 100f, 2, "unattributed player total");
            AssertSourceDamage(player, unattributed, 40f, 1, "unattributed source");
            AssertApproximately(100f, player.SourceCoverage.EligibleDamage, "unattributed eligible");
            AssertApproximately(60f, player.SourceCoverage.AttributedDamage, "unattributed attributed");
            AssertApproximately(40f, player.SourceCoverage.UnattributedDamage, "unattributed damage");
            AssertApproximately(0.6f, Require(player.SourceCoverage.AttributionCoverageRatio, "unattributed coverage"), "unattributed coverage");
        }

        private static void ValidatesNonHostileDamageFiltering()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 20f, local, TargetRelationship.Friendly, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(2f, 30f, local, TargetRelationship.SelfOrOwned, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(3f, 40f, local, TargetRelationship.Environment, DamageSourceCategory.BasicAttack);
            service.CaptureDamage(4f, 50f, local, TargetRelationship.Unknown, DamageSourceCategory.BasicAttack);

            AssertNull(service.GetCurrentEncounterSnapshot(5f), "current encounter");
            AssertPlayerCount(service.GetRunSnapshot(), 0, "run players");
        }

        private static void ValidatesNonHostileDamageDoesNotContaminateSources()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var source = Source(DamageSourceCategory.BasicAttack, "BASIC_ATTACK");

            service.CaptureDamage(1f, 20f, local, TargetRelationship.Friendly, source);
            service.CaptureDamage(2f, 30f, local, TargetRelationship.SelfOrOwned, source);

            AssertPlayerCount(service.GetRunSnapshot(), 0, "non-hostile source run players");
            AssertNull(service.GetCurrentEncounterSnapshot(3f), "non-hostile source current encounter");
        }

        private static void ValidatesEncounterSnapshotsFreezeAndRunPersists()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.OnRoomCompleted(6f);

            var last = Require(service.GetLastEncounterSnapshot(), "last encounter");
            AssertPlayerDamage(last, local, 100f, 1, "last encounter local");
            AssertApproximately(20f, Require(FindPlayer(last, local, "last encounter local").Dps, "last encounter dps"), "last encounter dps");
            AssertFalse(last.DurationIsValidated, "last encounter duration validation");

            service.CaptureDamage(10f, 25f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            AssertPlayerDamage(last, local, 100f, 1, "frozen last encounter local");
            AssertPlayerDamage(service.GetRunSnapshot(), local, 125f, 2, "run local after second encounter");
        }

        private static void ValidatesFrozenEncounterSourceSnapshotsAreImmutable()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var firstSource = Source(DamageSourceCategory.BasicAttack, "BASIC_ATTACK");
            var secondSource = Source(DamageSourceCategory.MemoryDirect, "Mem_Later");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, firstSource);
            service.OnRoomCompleted(6f);

            var last = Require(service.GetLastEncounterSnapshot(), "last encounter");
            var frozenPlayer = FindPlayer(last, local, "frozen local");

            service.CaptureDamage(10f, 25f, local, TargetRelationship.Hostile, secondSource);

            AssertSourceCount(frozenPlayer, 1, "frozen source count");
            AssertSourceDamage(frozenPlayer, firstSource, 100f, 1, "frozen source");
        }

        private static void ValidatesRunAccumulatesSourcesAcrossEncounters()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var attack = Source(DamageSourceCategory.BasicAttack, "BASIC_ATTACK");
            var memory = Source(DamageSourceCategory.MemoryDirect, "Mem_Run");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, attack);
            service.OnRoomCompleted(5f);
            service.CaptureDamage(10f, 40f, local, TargetRelationship.Hostile, attack);
            service.CaptureDamage(11f, 60f, local, TargetRelationship.Hostile, memory);

            var player = FindPlayer(service.GetRunSnapshot(), local, "run local");
            AssertSourceDamage(player, attack, 140f, 2, "run attack source");
            AssertSourceDamage(player, memory, 60f, 1, "run memory source");
            AssertSourceSumEqualsPlayer(player, "run source sum");
        }

        private static void ValidatesSequenceIdsAreMonotonic()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            var first = service.CaptureDamage(1f, 1f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            var second = service.CaptureDamage(2f, 1f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            var third = service.CaptureDamage(3f, 1f, null, TargetRelationship.Hostile, DamageSourceCategory.Unattributed);

            AssertEqual(1L, first.SequenceId, "first sequence");
            AssertEqual(2L, second.SequenceId, "second sequence");
            AssertEqual(3L, third.SequenceId, "third sequence");
        }

        private static void ValidatesEmptyEncounterDoesNotIncrementRun()
        {
            var service = new DamageAnalyticsService();

            service.OnCombatChanged(1f, true);
            service.OnRoomCompleted(3f);

            var run = service.GetRunSnapshot();
            AssertEqual(0, run.EncounterCount, "empty encounter count");
            AssertNull(run.CombatDuration, "empty encounter run duration");
            AssertNull(service.GetLastEncounterSnapshot(), "empty encounter last snapshot");
        }

        private static void ValidatesFinalGameResultFreezesCurrentRun()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.FinalizeRunForGameResult(6f);

            AssertNull(service.GetCurrentEncounterSnapshot(7f), "finalized current encounter");
            var last = Require(service.GetLastEncounterSnapshot(), "finalized last encounter");
            AssertPlayerDamage(last, local, 100f, 1, "finalized last encounter local");
            var run = service.GetRunSnapshot();
            AssertEqual(1L, run.RunId, "finalized run id");
            AssertEqual(1, run.EncounterCount, "finalized run encounter count");
            AssertPlayerDamage(run, local, 100f, 1, "finalized run local");
        }

        private static void ValidatesCompletedRunRemainsReadableAfterFinalization()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.FinalizeRunForGameResult(6f);

            AssertPlayerDamage(service.GetRunSnapshot(), local, 100f, 1, "completed run readable");
            AssertPlayerDamage(service.GetRunSnapshot(), local, 100f, 1, "completed run still readable");
            AssertTrue(service.HasRunState, "completed run retained as run state");
        }

        private static void ValidatesCompletedRunSnapshotIsImmutable()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.FinalizeRunForGameResult(6f);
            var completedRun = service.GetRunSnapshot();
            var completedPlayer = FindPlayer(completedRun, local, "completed immutable local");

            service.ResetRunForNewGame(8f);
            service.CaptureDamage(9f, 25f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            AssertEqual(1L, completedRun.RunId, "completed immutable run id");
            AssertEqual(1, completedRun.EncounterCount, "completed immutable encounter count");
            AssertApproximately(100f, completedPlayer.Aggregate.Damage, "completed immutable damage");
            AssertEqual(1, completedPlayer.Aggregate.HitCount, "completed immutable hit count");
        }

        private static void ValidatesPostFinalizationDamageDoesNotMutateCompletedTotals()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.FinalizeRunForGameResult(6f);
            service.CaptureDamage(7f, 50f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            AssertPlayerDamage(service.GetRunSnapshot(), local, 100f, 1, "post-finalization run local");
            AssertPlayerDamage(Require(service.GetLastEncounterSnapshot(), "post-finalization last encounter"), local, 100f, 1, "post-finalization last encounter local");
            AssertNull(service.GetCurrentEncounterSnapshot(8f), "post-finalization current encounter");
        }

        private static void ValidatesNewGameBoundaryResetClearsPreviousCompletedRunState()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.FinalizeRunForGameResult(6f);
            AssertTrue(service.ResetRunForNewGameIfNeeded(8f), "completed run boundary reset applied");

            var resetRun = service.GetRunSnapshot();
            AssertEqual(2L, resetRun.RunId, "completed reset run id");
            AssertEqual(0, resetRun.EncounterCount, "completed reset encounter count");
            AssertPlayerCount(resetRun, 0, "completed reset run players");
            AssertNull(resetRun.CombatDuration, "completed reset run duration");
            AssertNull(service.GetCurrentEncounterSnapshot(9f), "completed reset current encounter");
            AssertNull(service.GetLastEncounterSnapshot(), "completed reset last encounter");
            AssertFalse(service.HasRunState, "completed reset has no run state");
        }

        private static void ValidatesNextRunStartsAtZeroAndDoesNotLeak()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.FinalizeRunForGameResult(6f);
            service.ResetRunForNewGame(8f);

            service.CaptureDamage(8f, 25f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            var nextRun = service.GetRunSnapshot();
            AssertEqual(2L, nextRun.RunId, "next run id");
            AssertEqual(0, nextRun.EncounterCount, "next run starts before encounter completion");
            AssertPlayerDamage(nextRun, local, 25f, 1, "next run local");
            AssertApproximately(25f, nextRun.Coverage.PlayerOwnedHostileDamage, "next run coverage");
        }

        private static void ValidatesOrdinaryRoomTransitionsDoNotClearRun()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.OnRoomCompleted(6f);
            service.OnRoomStarted(7f);

            var run = service.GetRunSnapshot();
            AssertEqual(1L, run.RunId, "ordinary transition run id");
            AssertEqual(1, run.EncounterCount, "ordinary transition encounter count");
            AssertPlayerDamage(run, local, 100f, 1, "ordinary transition run local");
        }

        private static void ValidatesNewGameBoundaryResetsRunState()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.ResetRunForNewGame(6f);

            var resetRun = service.GetRunSnapshot();
            AssertEqual(2L, resetRun.RunId, "reset run id");
            AssertEqual(0, resetRun.EncounterCount, "reset encounter count");
            AssertPlayerCount(resetRun, 0, "reset run players");
            AssertNull(resetRun.CombatDuration, "reset run duration");
            AssertNull(service.GetCurrentEncounterSnapshot(7f), "reset current encounter");
            AssertNull(service.GetLastEncounterSnapshot(), "reset last encounter");
        }

        private static void ValidatesRunResetClearsSourceTotals()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var source = Source(DamageSourceCategory.BasicAttack, "BASIC_ATTACK");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, source);
            service.ResetRunForNewGame(6f);

            AssertPlayerCount(service.GetRunSnapshot(), 0, "reset source run players");

            service.CaptureDamage(8f, 25f, local, TargetRelationship.Hostile, source);
            var player = FindPlayer(service.GetRunSnapshot(), local, "new run local");
            AssertSourceDamage(player, source, 25f, 1, "new run source");
        }

        private static void ValidatesNewGameBoundaryResetsOnlyWhenRunStateExists()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var source = Source(DamageSourceCategory.MemoryDirect, "St_R_OldMemory");
            var memory = Memory(local, "St_R_OldMemory");

            AssertFalse(service.ResetRunForNewGameIfNeeded(0f), "empty new-game reset skipped");
            AssertEqual(1L, service.GetRunSnapshot().RunId, "empty new-game run id");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, source, memory, null, null);
            AssertPlayerDamage(service.GetRunSnapshot(), local, 100f, 1, "pre-boundary run local");
            AssertEqual(1, FindPlayer(service.GetRunSnapshot(), local, "pre-boundary run local").Memories.Count, "pre-boundary memory rows");

            AssertTrue(service.ResetRunForNewGameIfNeeded(6f), "new-game reset applied");

            var resetRun = service.GetRunSnapshot();
            AssertEqual(2L, resetRun.RunId, "new-game reset run id");
            AssertEqual(0, resetRun.EncounterCount, "new-game reset encounter count");
            AssertPlayerCount(resetRun, 0, "new-game reset players");
            AssertNull(service.GetCurrentEncounterSnapshot(7f), "new-game reset current encounter");
            AssertNull(service.GetLastEncounterSnapshot(), "new-game reset last encounter");

            AssertFalse(service.ResetRunForNewGameIfNeeded(8f), "second empty new-game reset skipped");
            AssertEqual(2L, service.GetRunSnapshot().RunId, "second empty new-game run id");
        }

        private static void ValidatesSourceOrderingIsDeterministic()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var high = Source(DamageSourceCategory.MemoryDirect, "b-high");
            var tieA = Source(DamageSourceCategory.MemoryDirect, "a-tie");
            var tieB = Source(DamageSourceCategory.MemoryDirect, "b-tie");

            service.CaptureDamage(1f, 10f, local, TargetRelationship.Hostile, tieB);
            service.CaptureDamage(2f, 30f, local, TargetRelationship.Hostile, high);
            service.CaptureDamage(3f, 10f, local, TargetRelationship.Hostile, tieA);

            var player = FindPlayer(Require(service.GetCurrentEncounterSnapshot(4f), "current encounter"), local, "ordered local");
            AssertEqual("b-high", player.Sources[0].SourceKey.StableId, "first source order");
            AssertEqual("a-tie", player.Sources[1].SourceKey.StableId, "second source order");
            AssertEqual("b-tie", player.Sources[2].SourceKey.StableId, "third source order");
        }

        private static void ValidatesDirectMemoryAnalyticsAggregateWithoutDoubleCounting()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "Mem_Fireball");
            var source = new SourceKey(DamageSourceCategory.MemoryDirect, memory.ContentId, memory.DisplayName);

            service.CaptureDamage(1f, 40f, local, TargetRelationship.Hostile, source, memory, null, memory);
            service.CaptureDamage(2f, 60f, local, TargetRelationship.Hostile, source, memory, null, memory);

            var player = FindPlayer(service.GetRunSnapshot(), local, "memory run local");
            AssertPlayerDamage(service.GetRunSnapshot(), local, 100f, 2, "memory player total");
            AssertEqual(1, player.Memories.Count, "memory row count");
            AssertMemoryDamage(player, memory, 100f, 2, "direct memory aggregate");
            AssertApproximately(1f, Require(player.Memories[0].PlayerShare, "memory share"), "memory share");
        }

        private static void ValidatesDirectGemAnalyticsAggregate()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var gem = Gem(local, "Gem_Shock");
            var source = new SourceKey(DamageSourceCategory.GemDirect, gem.ContentId, gem.DisplayName);

            service.CaptureDamage(1f, 15f, local, TargetRelationship.Hostile, source, null, gem, null);
            service.CaptureDamage(2f, 25f, local, TargetRelationship.Hostile, source, null, gem, null);

            var player = FindPlayer(service.GetRunSnapshot(), local, "gem run local");
            AssertEqual(1, player.Gems.Count, "gem row count");
            AssertGemDamage(player, gem, 40f, 2, "direct gem aggregate");
            AssertApproximately(40f, player.MemoryGemCoverage.DirectGemDamage, "gem coverage direct");
            AssertApproximately(40f, player.MemoryGemCoverage.GemIdentifiedDamage, "gem coverage identified");
        }

        private static void ValidatesMemoryPackageDoesNotInflatePlayerTotal()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "Mem_Fireball");
            var gemA = Gem(local, "Gem_A");
            var gemB = Gem(local, "Gem_B");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);
            service.CaptureDamage(2f, 40f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gemA.ContentId), null, gemA, memory);
            service.CaptureDamage(3f, 20f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gemB.ContentId), null, gemB, memory);

            var player = FindPlayer(service.GetRunSnapshot(), local, "package run local");
            AssertApproximately(160f, player.Aggregate.Damage, "package player total not doubled");
            AssertMemoryDamage(player, memory, 100f, 1, "package direct memory row");
            AssertGemDamage(player, gemA, 40f, 1, "package gem a row");
            AssertGemDamage(player, gemB, 20f, 1, "package gem b row");
            AssertMemoryPackage(player, memory, 100f, 60f, 160f, 2, "memory package aggregate");
        }

        private static void ValidatesMultipleMemoryPackagesAssignCorrectChildren()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memoryA = Memory(local, "Mem_A");
            var memoryB = Memory(local, "Mem_B");
            var gemA = Gem(local, "Gem_A");
            var gemB = Gem(local, "Gem_B");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memoryA.ContentId), memoryA, null, memoryA);
            service.CaptureDamage(2f, 30f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gemA.ContentId), null, gemA, memoryA);
            service.CaptureDamage(3f, 200f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memoryB.ContentId), memoryB, null, memoryB);
            service.CaptureDamage(4f, 50f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gemB.ContentId), null, gemB, memoryB);

            var player = FindPlayer(service.GetRunSnapshot(), local, "multi package run local");
            AssertMemoryPackage(player, memoryA, 100f, 30f, 130f, 1, "package memory a");
            AssertMemoryPackage(player, memoryB, 200f, 50f, 250f, 1, "package memory b");
        }

        private static void ValidatesUnknownOriginatingMemoryStaysOutOfPackages()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var gem = Gem(local, "Gem_UnknownParent");

            service.CaptureDamage(1f, 70f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, null);

            var player = FindPlayer(service.GetRunSnapshot(), local, "unknown package local");
            AssertApproximately(70f, player.Aggregate.Damage, "unknown package player total");
            AssertGemDamage(player, gem, 70f, 1, "unknown package direct gem");
            AssertEqual(0, player.MemoryPackages.Count, "unknown package count");
            AssertApproximately(70f, player.MemoryGemCoverage.PackageRelationshipUnknownGemDamage, "unknown package coverage");
        }

        private static void ValidatesKnownOriginatingMemoryAssignsPackageWhenGemIdentityUnknown()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "Mem_KnownParent");

            service.CaptureDamage(1f, 35f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, "Gem_SourceOnly"), null, null, memory);

            var player = FindPlayer(service.GetRunSnapshot(), local, "known package unknown gem local");
            AssertApproximately(35f, player.Aggregate.Damage, "known package unknown gem player total");
            AssertEqual(0, player.Gems.Count, "known package unknown gem direct gem rows");
            AssertMemoryPackage(player, memory, 0f, 35f, 35f, 0, "known package unknown gem package");
            AssertApproximately(35f, player.MemoryGemCoverage.PackageAssignableGemDamage, "known package unknown gem assigned coverage");
            AssertApproximately(0f, player.MemoryGemCoverage.GemIdentifiedDamage, "known package unknown gem identity coverage");
        }

        private static void ValidatesKnownOriginatingMemoryAssignsStatusDotToPackage()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "St_L_MentalCorruption");
            var memorySource = Source(DamageSourceCategory.MemoryDirect, memory.ContentId);
            var dotSource = new SourceKey(DamageSourceCategory.StatusDot, "Se_L_MentalCorruption", "Mental Corruption DoT");

            service.CaptureDamage(1f, 80f, local, TargetRelationship.Hostile, memorySource, memory, null, memory);
            service.CaptureDamage(2f, 45f, local, TargetRelationship.Hostile, dotSource, null, null, memory);

            var player = FindPlayer(service.GetRunSnapshot(), local, "status dot package local");
            AssertPlayerDamage(service.GetRunSnapshot(), local, 125f, 2, "status dot package player total");
            AssertSourceDamage(player, memorySource, 80f, 1, "status dot package memory source");
            AssertSourceDamage(player, dotSource, 45f, 1, "status dot source remains separate");
            AssertSourceSumEqualsPlayer(player, "status dot source total not doubled");
            AssertMemoryPackage(player, memory, 80f, 0f, 125f, 1, "status dot package aggregate");
            AssertApproximately(45f, FindMemoryPackage(player, memory, "status dot package child").OtherDirectlyAttributedChildDamage, "status dot package other child");
            AssertEqual("Mental Corruption DoT", RunAnalyticsPanelPresenter.GetPackageChildLabel(FindMemoryPackage(player, memory, "status dot child label").ChildBreakdown[0]), "status dot child label");
        }

        private static void ValidatesAmbiguousStatusDotStaysOutOfPackages()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var dotSource = new SourceKey(DamageSourceCategory.StatusDot, "Se_L_MentalCorruption", "Mental Corruption DoT");

            service.CaptureDamage(1f, 45f, local, TargetRelationship.Hostile, dotSource, null, null, null);

            var player = FindPlayer(service.GetRunSnapshot(), local, "ambiguous status dot local");
            AssertSourceDamage(player, dotSource, 45f, 1, "ambiguous status dot source remains");
            AssertEqual(0, player.MemoryPackages.Count, "ambiguous status dot package count");
            AssertApproximately(45f, player.MemoryGemCoverage.PackageRelationshipUnknownChildDamage, "ambiguous status dot unknown child coverage");
        }

        private static void ValidatesGemAndStatusDotChildrenRollIntoSamePackage()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "Mem_ChildMix");
            var gem = Gem(local, "Gem_Child");
            var dotSource = new SourceKey(DamageSourceCategory.StatusDot, "Se_ChildDot", "Child DoT");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);
            service.CaptureDamage(2f, 25f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, memory);
            service.CaptureDamage(3f, 15f, local, TargetRelationship.Hostile, dotSource, null, null, memory);

            var player = FindPlayer(service.GetRunSnapshot(), local, "mixed child package local");
            var package = FindMemoryPackage(player, memory, "mixed child package");
            AssertMemoryPackage(player, memory, 100f, 25f, 140f, 2, "mixed child package aggregate");
            AssertApproximately(40f, package.AttributableChildDamage, "mixed child package child total");
            AssertGemDamage(player, gem, 25f, 1, "mixed child direct gem row");
            AssertSourceDamage(player, dotSource, 15f, 1, "mixed child status source row");
            AssertSourceSumEqualsPlayer(player, "mixed child source total not doubled");
        }

        private static void ValidatesLocalAndPartnerStatusDotPackageIsolation()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");
            var localMemory = Memory(local, "Mem_SharedDot");
            var partnerMemory = Memory(partner, "Mem_SharedDot");
            var dotSource = new SourceKey(DamageSourceCategory.StatusDot, "Se_SharedDot", "Shared Dot");

            service.CaptureDamage(1f, 10f, local, TargetRelationship.Hostile, dotSource, null, null, localMemory);
            service.CaptureDamage(2f, 20f, partner, TargetRelationship.Hostile, dotSource, null, null, partnerMemory);

            var run = service.GetRunSnapshot();
            var localPlayer = FindPlayer(run, local, "local status dot isolation");
            var partnerPlayer = FindPlayer(run, partner, "partner status dot isolation");
            AssertMemoryPackage(localPlayer, localMemory, 0f, 0f, 10f, 1, "local status dot package isolation");
            AssertMemoryPackage(partnerPlayer, partnerMemory, 0f, 0f, 20f, 1, "partner status dot package isolation");
        }

        private static void ValidatesRunAccumulatesMemoryGemPackagesAcrossEncounters()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "Mem_RunPackage");
            var gem = Gem(local, "Gem_RunPackage");

            service.CaptureDamage(1f, 20f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);
            service.CaptureDamage(2f, 10f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, memory);
            service.OnRoomCompleted(5f);
            service.CaptureDamage(10f, 30f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);
            service.CaptureDamage(11f, 40f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, memory);

            var player = FindPlayer(service.GetRunSnapshot(), local, "run aggregate memory package");
            AssertMemoryDamage(player, memory, 50f, 2, "run aggregate memory");
            AssertGemDamage(player, gem, 50f, 2, "run aggregate gem");
            AssertMemoryPackage(player, memory, 50f, 50f, 100f, 1, "run aggregate package");
        }

        private static void ValidatesRunResetClearsMemoryGemPackages()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "Mem_Reset");
            var gem = Gem(local, "Gem_Reset");

            service.CaptureDamage(1f, 20f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);
            service.CaptureDamage(2f, 10f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, memory);
            service.ResetRunForNewGame(5f);

            AssertPlayerCount(service.GetRunSnapshot(), 0, "reset memory package players");

            service.CaptureDamage(7f, 5f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, null);
            var player = FindPlayer(service.GetRunSnapshot(), local, "post-reset memory package local");
            AssertEqual(0, player.MemoryPackages.Count, "post-reset package count");
            AssertGemDamage(player, gem, 5f, 1, "post-reset gem");
        }

        private static void ValidatesLocalAndPartnerMemoryGemPackageIsolation()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");
            var localMemory = Memory(local, "Mem_Shared");
            var partnerMemory = Memory(partner, "Mem_Shared");
            var localGem = Gem(local, "Gem_Shared");
            var partnerGem = Gem(partner, "Gem_Shared");

            service.CaptureDamage(1f, 40f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, "Mem_Shared"), localMemory, null, localMemory);
            service.CaptureDamage(2f, 20f, partner, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, "Mem_Shared"), partnerMemory, null, partnerMemory);
            service.CaptureDamage(3f, 10f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, "Gem_Shared"), null, localGem, localMemory);
            service.CaptureDamage(4f, 5f, partner, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, "Gem_Shared"), null, partnerGem, partnerMemory);

            var run = service.GetRunSnapshot();
            var localPlayer = FindPlayer(run, local, "local memory package isolation");
            var partnerPlayer = FindPlayer(run, partner, "partner memory package isolation");
            AssertMemoryPackage(localPlayer, localMemory, 40f, 10f, 50f, 1, "local package isolation");
            AssertMemoryPackage(partnerPlayer, partnerMemory, 20f, 5f, 25f, 1, "partner package isolation");
        }

        private static void ValidatesRunSnapshotMemoryGemPackageImmutability()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "Mem_Frozen");
            var gem = Gem(local, "Gem_Frozen");

            service.CaptureDamage(1f, 30f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);
            var frozenRun = service.GetRunSnapshot();
            var frozenPlayer = FindPlayer(frozenRun, local, "frozen run local");

            service.CaptureDamage(2f, 20f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, memory);

            AssertMemoryDamage(frozenPlayer, memory, 30f, 1, "frozen memory row");
            AssertEqual(0, frozenPlayer.Gems.Count, "frozen gem rows");
            AssertMemoryPackage(frozenPlayer, memory, 30f, 0f, 30f, 0, "frozen package row");
        }

        private static void ValidatesMemoryGemPackageOrderingIsDeterministic()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var high = Memory(local, "b-high");
            var tieA = Memory(local, "a-tie");
            var tieB = Memory(local, "b-tie");
            var gemHigh = Gem(local, "gem-high");
            var gemTieA = Gem(local, "gem-a");
            var gemTieB = Gem(local, "gem-b");

            service.CaptureDamage(1f, 10f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, tieB.ContentId), tieB, null, tieB);
            service.CaptureDamage(2f, 30f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, high.ContentId), high, null, high);
            service.CaptureDamage(3f, 10f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, tieA.ContentId), tieA, null, tieA);
            service.CaptureDamage(4f, 10f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gemTieB.ContentId), null, gemTieB, tieB);
            service.CaptureDamage(5f, 30f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gemHigh.ContentId), null, gemHigh, high);
            service.CaptureDamage(6f, 10f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gemTieA.ContentId), null, gemTieA, tieA);

            var player = FindPlayer(service.GetRunSnapshot(), local, "ordered memory gem package");
            AssertEqual("local:local-a:b-high", player.Memories[0].MemoryKey.StableId, "first memory order");
            AssertEqual("local:local-a:a-tie", player.Memories[1].MemoryKey.StableId, "second memory order");
            AssertEqual("local:local-a:b-tie", player.Memories[2].MemoryKey.StableId, "third memory order");
            AssertEqual("local:local-a:gem-high", player.Gems[0].GemKey.StableId, "first gem order");
            AssertEqual("local:local-a:gem-a", player.Gems[1].GemKey.StableId, "second gem order");
            AssertEqual("local:local-a:gem-b", player.Gems[2].GemKey.StableId, "third gem order");
            AssertEqual("local:local-a:b-high", player.MemoryPackages[0].MemoryKey.StableId, "first package order");
            AssertEqual("local:local-a:a-tie", player.MemoryPackages[1].MemoryKey.StableId, "second package order");
            AssertEqual("local:local-a:b-tie", player.MemoryPackages[2].MemoryKey.StableId, "third package order");
        }

        private static void ValidatesAnalyticsPanelsStackMidLeftWithoutOverlap()
        {
            var layout = DamageAnalyticsUiInput.CalculateMidLeftLayout(
                1920f,
                1080f,
                DamageAnalyticsEncounterPanel.PanelWidthForLayout,
                386f,
                DamageAnalyticsRunPanel.PanelWidthForLayout,
                456f);
            AssertApproximately(DamageAnalyticsUiInput.PanelMargin, layout.EncounterRect.x, "desktop encounter x");
            AssertApproximately(DamageAnalyticsUiInput.PanelMargin, layout.RunRect.x, "desktop run x");
            AssertApproximately((1080f - 386f - DamageAnalyticsUiInput.PanelGap - 456f) * 0.5f, layout.EncounterRect.y, "desktop group centered");
            AssertApproximately(layout.EncounterRect.y + layout.EncounterRect.height + DamageAnalyticsUiInput.PanelGap, layout.RunRect.y, "desktop run below encounter");
            AssertTrue(!layout.EncounterRect.Overlaps(layout.RunRect), "desktop panels do not overlap");

            var ultrawide = DamageAnalyticsUiInput.CalculateMidLeftLayout(
                3440f,
                1440f,
                DamageAnalyticsEncounterPanel.PanelWidthForLayout,
                386f,
                DamageAnalyticsRunPanel.PanelWidthForLayout,
                456f);
            AssertApproximately(DamageAnalyticsUiInput.PanelMargin, ultrawide.EncounterRect.x, "ultrawide encounter x");
            AssertApproximately((1440f - 386f - DamageAnalyticsUiInput.PanelGap - 456f) * 0.5f, ultrawide.EncounterRect.y, "ultrawide group centered");
            AssertTrue(!ultrawide.EncounterRect.Overlaps(ultrawide.RunRect), "ultrawide panels do not overlap");

            var compact = DamageAnalyticsUiInput.CalculateMidLeftLayout(
                1366f,
                768f,
                DamageAnalyticsEncounterPanel.PanelWidthForLayout,
                320f,
                DamageAnalyticsRunPanel.PanelWidthForLayout,
                360f);
            AssertApproximately(DamageAnalyticsUiInput.PanelMargin, compact.EncounterRect.x, "compact encounter x");
            AssertTrue(!compact.EncounterRect.Overlaps(compact.RunRect), "compact panels do not overlap");
            AssertTrue(compact.EncounterRect.y >= DamageAnalyticsUiInput.PanelMargin, "compact encounter top margin");
            AssertTrue(compact.RunRect.y + compact.RunRect.height <= 768f - DamageAnalyticsUiInput.PanelMargin + Tolerance, "compact run bottom margin");
        }

        private static void ValidatesAnalyticsPanelHitTestMatchesRenderedRegion()
        {
            var rect = new UnityEngine.Rect(22f, 120f, 314f, 220f);
            DamageAnalyticsUiInput.RegisterPanelRect(DamageAnalyticsPanelKind.Encounter, rect);
            AssertTrue(DamageAnalyticsUiInput.IsGuiPointOverModPanel(new UnityEngine.Vector2(23f, 121f)), "hit test inside rendered rect");
            AssertTrue(!DamageAnalyticsUiInput.IsGuiPointOverModPanel(new UnityEngine.Vector2(21f, 121f)), "hit test outside rendered rect");
            DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Encounter);
            AssertTrue(!DamageAnalyticsUiInput.IsGuiPointOverModPanel(new UnityEngine.Vector2(23f, 121f)), "hit test cleared rect");
        }

        private static void ValidatesGameInputSuppressionInsidePanelsOnly()
        {
            var rect = new UnityEngine.Rect(22f, 120f, 314f, 220f);
            DamageAnalyticsUiInput.RegisterPanelRect(DamageAnalyticsPanelKind.Run, rect);

            AssertTrue(
                DamageAnalyticsUiInput.ShouldSuppressGameMouseInput(MouseButton.Left, new UnityEngine.Vector3(23f, 1080f - 121f, 0f), 1080f, true),
                "left click inside mod panel suppresses game input");
            AssertTrue(
                DamageAnalyticsUiInput.ShouldSuppressGameMouseInput(MouseButton.Right, new UnityEngine.Vector3(23f, 1080f - 121f, 0f), 1080f, true),
                "right click inside mod panel suppresses game input");
            AssertFalse(
                DamageAnalyticsUiInput.ShouldSuppressGameMouseInput(MouseButton.Left, new UnityEngine.Vector3(21f, 1080f - 121f, 0f), 1080f, true),
                "left click outside mod panel remains game input");
            AssertFalse(
                DamageAnalyticsUiInput.ShouldSuppressGameMouseInput(MouseButton.Left, new UnityEngine.Vector3(23f, 1080f - 121f, 0f), 1080f, false),
                "hidden panel does not suppress game input");

            DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Run);
        }

        private static void ValidatesRunPanelTabSelectionChangesMode()
        {
            DamageAnalyticsRunPanel.SelectMode(RunAnalyticsPanelMode.Package);
            AssertEqual((int)RunAnalyticsPanelMode.Package, (int)DamageAnalyticsRunPanel.CurrentModeForValidation, "run panel starts package mode");

            DamageAnalyticsRunPanel.SelectMode(RunAnalyticsPanelMode.Memory);
            AssertEqual((int)RunAnalyticsPanelMode.Memory, (int)DamageAnalyticsRunPanel.CurrentModeForValidation, "run panel selects memory mode");

            DamageAnalyticsRunPanel.SelectMode(RunAnalyticsPanelMode.Gem);
            AssertEqual((int)RunAnalyticsPanelMode.Gem, (int)DamageAnalyticsRunPanel.CurrentModeForValidation, "run panel selects gem mode");

            DamageAnalyticsRunPanel.SelectMode(RunAnalyticsPanelMode.Package);
            AssertEqual((int)RunAnalyticsPanelMode.Package, (int)DamageAnalyticsRunPanel.CurrentModeForValidation, "run panel selects package mode");
        }

        private static void ValidatesAnalyticsPanelPresentationLabels()
        {
            AssertEqual("Source", EncounterAnalyticsPanelPresenter.SourceColumnHeader, "encounter source header");
            AssertEqual("Damage", EncounterAnalyticsPanelPresenter.DamageColumnHeader, "encounter damage header");
            AssertEqual("Share", EncounterAnalyticsPanelPresenter.ShareColumnHeader, "encounter share header");
            AssertEqual("Damage", RunAnalyticsPanelPresenter.DamageColumnHeader, "run damage header");
            AssertEqual("Share", RunAnalyticsPanelPresenter.ShareColumnHeader, "run share header");
            AssertEqual("DPS", RunAnalyticsPanelPresenter.DpsColumnHeader, "run dps header");
            AssertEqual("Memory Package", RunAnalyticsPanelPresenter.MemoryPackageColumnHeader, "run package header");
            AssertEqual("Memory", RunAnalyticsPanelPresenter.MemoryColumnHeader, "run package memory header");
            AssertEqual("Children", RunAnalyticsPanelPresenter.ChildrenColumnHeader, "run package children header");
            AssertEqual("Total", RunAnalyticsPanelPresenter.TotalColumnHeader, "run package total header");
            AssertEqual("Memory", RunAnalyticsPanelPresenter.GetTabLabel(RunAnalyticsPanelMode.Memory), "run memory tab");
            AssertEqual("Direct Gems", RunAnalyticsPanelPresenter.GetTabLabel(RunAnalyticsPanelMode.Gem), "run gem tab");
            AssertEqual("Package", RunAnalyticsPanelPresenter.GetTabLabel(RunAnalyticsPanelMode.Package), "run package tab");
        }

        private static void ValidatesAnalyticsPanelsHideForNativeMemoryGemTooltips()
        {
            AssertTrue(
                DamageAnalyticsNativeUiOcclusion.ShouldHideForNativeTooltipState(true, true, false, false, false, false, null),
                "native skill tooltip hides analytics");
            AssertTrue(
                DamageAnalyticsNativeUiOcclusion.ShouldHideForNativeTooltipState(true, false, true, false, false, false, null),
                "native gem tooltip hides analytics");
            AssertTrue(
                DamageAnalyticsNativeUiOcclusion.ShouldHideForNativeTooltipState(true, false, false, true, false, false, null),
                "native skill equip tooltip hides analytics");
            AssertTrue(
                DamageAnalyticsNativeUiOcclusion.ShouldHideForNativeTooltipState(false, false, false, false, false, true, null),
                "native hero detail hides analytics");
            AssertTrue(
                DamageAnalyticsNativeUiOcclusion.ShouldHideForNativeTooltipState(true, false, false, false, false, false, new object[] { new MemoryBuildTooltipProbe() }),
                "native build object hides analytics");
            AssertTrue(
                !DamageAnalyticsNativeUiOcclusion.ShouldHideForNativeTooltipState(true, false, false, false, false, false, new object[] { "quest tooltip" }),
                "ordinary small tooltip does not hide analytics");
            AssertTrue(
                !DamageAnalyticsNativeUiOcclusion.ShouldHideForNativeTooltipState(false, true, true, true, true, false, null),
                "inactive tooltip does not hide analytics");
        }

        private static void ValidatesAnalyticsUiLifecycleVisibilityPolicy()
        {
            AssertFalse(
                DamageAnalyticsUiVisibility.ShouldShowAnalyticsUi(DamageAnalyticsUiContext.NonGameplay),
                "ui hidden for non-gameplay context");
            AssertTrue(
                DamageAnalyticsUiVisibility.ShouldShowAnalyticsUi(DamageAnalyticsUiContext.ActiveGameplay),
                "ui visible for active gameplay context");
            AssertTrue(
                DamageAnalyticsUiVisibility.ShouldShowAnalyticsUi(DamageAnalyticsUiContext.FinalResults),
                "ui visible for final results context");

            AssertEqual(
                (int)DamageAnalyticsUiContext.NonGameplay,
                (int)DamageAnalyticsUiVisibility.ResolveGameplayContext(
                    "Title",
                    false,
                    false,
                    false,
                    false,
                    false,
                    DamageAnalyticsUiContext.ActiveGameplay),
                "title scene hides analytics");
            AssertEqual(
                (int)DamageAnalyticsUiContext.NonGameplay,
                (int)DamageAnalyticsUiVisibility.ResolveGameplayContext(
                    "PlayLobby",
                    false,
                    false,
                    false,
                    false,
                    false,
                    DamageAnalyticsUiContext.FinalResults),
                "lobby scene hides analytics");
            AssertEqual(
                (int)DamageAnalyticsUiContext.ActiveGameplay,
                (int)DamageAnalyticsUiVisibility.ResolveGameplayContext(
                    "PlayGame",
                    true,
                    true,
                    false,
                    false,
                    false,
                    DamageAnalyticsUiContext.NonGameplay),
                "active PlayGame shows analytics");
            AssertEqual(
                (int)DamageAnalyticsUiContext.ActiveGameplay,
                (int)DamageAnalyticsUiVisibility.ResolveGameplayContext(
                    "Room_Forest_A",
                    true,
                    true,
                    false,
                    false,
                    false,
                    DamageAnalyticsUiContext.ActiveGameplay),
                "room transition inside run keeps analytics visible");
            AssertEqual(
                (int)DamageAnalyticsUiContext.FinalResults,
                (int)DamageAnalyticsUiVisibility.ResolveGameplayContext(
                    "PlayGame",
                    true,
                    true,
                    true,
                    false,
                    false,
                    DamageAnalyticsUiContext.ActiveGameplay),
                "final results keep analytics visible");
            AssertEqual(
                (int)DamageAnalyticsUiContext.NonGameplay,
                (int)DamageAnalyticsUiVisibility.ResolveGameplayContext(
                    "OtherMenu",
                    false,
                    false,
                    false,
                    false,
                    false,
                    DamageAnalyticsUiContext.FinalResults),
                "unknown non-gameplay without run state hides analytics");
            AssertEqual(
                (int)DamageAnalyticsUiContext.NonGameplay,
                (int)DamageAnalyticsUiVisibility.ResolveGameplayContext(
                    "PlayLobby",
                    false,
                    false,
                    false,
                    false,
                    false,
                    DamageAnalyticsUiContext.FinalResults),
                "return to lobby hides analytics after final results");
            AssertEqual(
                (int)DamageAnalyticsUiContext.ActiveGameplay,
                (int)DamageAnalyticsUiVisibility.ResolveGameplayContext(
                    "PlayGame",
                    true,
                    true,
                    false,
                    false,
                    false,
                    DamageAnalyticsUiContext.NonGameplay),
                "re-entering gameplay restores analytics once");

            var rect = new UnityEngine.Rect(22f, 120f, 314f, 220f);
            DamageAnalyticsUiInput.RegisterPanelRect(DamageAnalyticsPanelKind.Encounter, rect);
            DamageAnalyticsUiInput.RegisterPanelRect(DamageAnalyticsPanelKind.Run, rect);
            DamageAnalyticsUiVisibility.MarkNonGameplay();
            AssertFalse(
                DamageAnalyticsUiInput.IsGuiPointOverModPanel(new UnityEngine.Vector2(23f, 121f)),
                "hidden analytics expose no hit-test region");

            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            var before = service.GetRunSnapshot();
            DamageAnalyticsUiVisibility.MarkActiveGameplay();
            DamageAnalyticsUiVisibility.MarkNonGameplay();
            DamageAnalyticsUiVisibility.MarkFinalResults();
            var after = service.GetRunSnapshot();
            AssertEqual(before.RunId, after.RunId, "visibility changes do not reset run id");
            AssertPlayerDamage(after, local, 100f, 1, "visibility changes preserve analytics state");

            DamageAnalyticsUiVisibility.MarkNonGameplay();
        }

        private static void ValidatesEncounterPanelPrefersCurrentThenLast()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            service.OnRoomCompleted(6f);
            var last = Require(service.GetLastEncounterSnapshot(), "panel last encounter");

            var lastView = Require(EncounterAnalyticsPanelPresenter.BuildView(null, last), "panel last view");
            AssertEqual("Last Encounter", lastView.StateLabel, "panel last state");
            AssertApproximately(100f, Require(lastView.LocalPlayer, "panel last local").Aggregate.Damage, "panel last local damage");

            service.CaptureDamage(10f, 25f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);
            var current = Require(service.GetCurrentEncounterSnapshot(11f), "panel current encounter");

            var currentView = Require(EncounterAnalyticsPanelPresenter.BuildView(current, last), "panel current view");
            AssertEqual("Current Encounter", currentView.StateLabel, "panel current state");
            AssertEqual(current.EncounterId, currentView.Encounter.EncounterId, "panel current id");
            AssertApproximately(25f, Require(currentView.LocalPlayer, "panel current local").Aggregate.Damage, "panel current local damage");
        }

        private static void ValidatesEncounterPanelSourceRowsAndCoverage()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");
            var memory = new SourceKey(DamageSourceCategory.MemoryDirect, "Skill_Super_Nova", "Super Nova");
            var attack = new SourceKey(DamageSourceCategory.BasicAttack, "BASIC_ATTACK", "Basic Attack");

            service.CaptureDamage(1f, 75f, local, TargetRelationship.Hostile, memory);
            service.CaptureDamage(2f, 25f, local, TargetRelationship.Hostile, attack);
            service.CaptureDamage(3f, 50f, partner, TargetRelationship.Hostile, attack);

            var view = Require(
                EncounterAnalyticsPanelPresenter.BuildView(service.GetCurrentEncounterSnapshot(4f), null),
                "panel source view");

            AssertApproximately(100f, Require(view.LocalPlayer, "panel source local").Aggregate.Damage, "panel source local damage");
            AssertApproximately(50f, Require(view.PartnerPlayer, "panel source partner").Aggregate.Damage, "panel source partner damage");
            AssertApproximately(100f / 150f, Require(view.LocalPlayer.PartyShare, "panel source local share"), "panel source local share");
            AssertEqual("Super Nova", EncounterAnalyticsPanelPresenter.GetSourceLabel(view.LocalSources[0]), "panel source first label");
            AssertEqual("Basic Attack", EncounterAnalyticsPanelPresenter.GetSourceLabel(view.LocalSources[1]), "panel source second label");
            AssertEqual("67%", EncounterAnalyticsPanelPresenter.FormatPercent(view.LocalPlayer.PartyShare), "panel source share display");
            AssertEqual("100%", EncounterAnalyticsPanelPresenter.FormatPercent(view.LocalPlayer.SourceCoverage.AttributionCoverageRatio), "panel source coverage display");
        }

        private static void ValidatesEncounterPanelPlayerSelectionSwitchesRowsAndCoverage()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");
            var localMemory = new SourceKey(DamageSourceCategory.MemoryDirect, "Skill_Local", "Local Memory");
            var partnerMemory = new SourceKey(DamageSourceCategory.MemoryDirect, "Skill_Partner", "Partner Memory");
            var unattributed = SourceKey.ForCategory(DamageSourceCategory.Unattributed);

            service.CaptureDamage(1f, 90f, local, TargetRelationship.Hostile, localMemory);
            service.CaptureDamage(2f, 10f, local, TargetRelationship.Hostile, unattributed);
            service.CaptureDamage(3f, 50f, partner, TargetRelationship.Hostile, partnerMemory);

            var encounter = service.GetCurrentEncounterSnapshot(4f);
            var localView = Require(
                EncounterAnalyticsPanelPresenter.BuildView(encounter, null, AnalyticsPlayerSelection.Local),
                "encounter panel selected local view");
            var partnerView = Require(
                EncounterAnalyticsPanelPresenter.BuildView(encounter, null, AnalyticsPlayerSelection.Partner),
                "encounter panel selected partner view");

            AssertEqual("You", localView.SelectedPlayerLabel, "encounter local selected label");
            AssertEqual("Partner", partnerView.SelectedPlayerLabel, "encounter partner selected label");
            AssertTrue(localView.SelectedPlayer.PlayerKey.Equals(local), "encounter selected local player");
            AssertTrue(partnerView.SelectedPlayer.PlayerKey.Equals(partner), "encounter selected partner player");
            AssertEqual("Local Memory", EncounterAnalyticsPanelPresenter.GetSourceLabel(localView.SelectedSources[0]), "encounter selected local source");
            AssertEqual("Partner Memory", EncounterAnalyticsPanelPresenter.GetSourceLabel(partnerView.SelectedSources[0]), "encounter selected partner source");
            AssertApproximately(0.9f, Require(localView.SelectedPlayer.SourceCoverage.AttributionCoverageRatio, "encounter selected local coverage"), "encounter selected local coverage");
            AssertApproximately(1f, Require(partnerView.SelectedPlayer.SourceCoverage.AttributionCoverageRatio, "encounter selected partner coverage"), "encounter selected partner coverage");
            AssertEqual(2, localView.SelectedSources.Count, "encounter selected local source count");
            AssertEqual(1, partnerView.SelectedSources.Count, "encounter selected partner source count");
        }

        private static void ValidatesEncounterPanelAbsentPartnerFallsBackToLocal()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, DamageSourceCategory.BasicAttack);

            var view = Require(
                EncounterAnalyticsPanelPresenter.BuildView(service.GetCurrentEncounterSnapshot(2f), null, AnalyticsPlayerSelection.Partner),
                "encounter panel absent partner view");

            AssertEqual((int)AnalyticsPlayerSelection.Local, (int)view.SelectedPlayerSelection, "encounter absent partner selected local");
            AssertTrue(Require(view.SelectedPlayer, "encounter absent partner selected player").PlayerKey.Equals(local), "encounter absent partner local player");
            AssertEqual(1, view.SelectedSources.Count, "encounter absent partner local rows");
            AssertTrue(!view.HasPartner, "encounter solo selector hidden state");
        }

        private static void ValidatesEncounterPanelFriendlyDisplayNamesAndCoverageSummary()
        {
            var friendly = SourceSnapshot(new SourceKey(DamageSourceCategory.MemoryDirect, "Skill_QuickTrigger", "St R QuickTrigger"), 75f, 0.75f);
            AssertEqual("Quick Trigger", EncounterAnalyticsPanelPresenter.GetSourceLabel(friendly), "encounter friendly source name");
            AssertEqual("Coverage 100%", EncounterAnalyticsPanelPresenter.GetCoverageSummary(new SourceCoverageSnapshot(100f, 100f, 0f)), "encounter full coverage summary");
            AssertEqual("Coverage 91% - 5.6K unattributed", EncounterAnalyticsPanelPresenter.GetCoverageSummary(new SourceCoverageSnapshot(62000f, 56400f, 5600f)), "encounter partial coverage summary");
        }

        private static void ValidatesEncounterPanelKeepsUnattributedSourceVisible()
        {
            var sources = new System.Collections.Generic.List<SourceDamageSnapshot>();
            for (var i = 0; i < 8; i++)
            {
                sources.Add(SourceSnapshot(new SourceKey(DamageSourceCategory.MemoryDirect, "source-" + i, "Source " + i), 100f - i, 0.1f));
            }

            var unattributed = SourceSnapshot(SourceKey.ForCategory(DamageSourceCategory.Unattributed), 1f, 0.01f);
            sources.Add(unattributed);

            var displaySources = EncounterAnalyticsPanelPresenter.GetDisplaySources(sources, 8);
            AssertEqual(8, displaySources.Count, "panel display source count");
            AssertEqual("UNATTRIBUTED", displaySources[7].SourceKey.StableId, "panel unattributed kept");
            AssertEqual("Unattributed", EncounterAnalyticsPanelPresenter.GetSourceLabel(displaySources[7]), "panel unattributed label");
        }

        private static void ValidatesRunPanelMemoryGemPackageRows()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");
            var memory = new MemoryKey(local, "Mem_SuperNova", "Super Nova");
            var gem = new GemKey(local, "Gem_Shock", "Shock Gem");
            var partnerMemory = Memory(partner, "Mem_Partner");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);
            service.CaptureDamage(2f, 40f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, memory);
            service.CaptureDamage(3f, 60f, partner, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, partnerMemory.ContentId), partnerMemory, null, partnerMemory);
            service.OnRoomCompleted(6f);

            var view = Require(RunAnalyticsPanelPresenter.BuildView(service.GetRunSnapshot(), RunAnalyticsPanelMode.Package), "run panel view");
            AssertApproximately(140f, Require(view.LocalPlayer, "run panel local").Aggregate.Damage, "run panel local total");
            AssertApproximately(60f, Require(view.PartnerPlayer, "run panel partner").Aggregate.Damage, "run panel partner total");
            AssertEqual(1, view.Memories.Count, "run panel memory count");
            AssertEqual(1, view.Gems.Count, "run panel gem count");
            AssertEqual(1, view.MemoryPackages.Count, "run panel package count");
            AssertEqual("Super Nova", RunAnalyticsPanelPresenter.GetMemoryLabel(view.Memories[0]), "run panel memory label");
            AssertEqual("Shock Gem", RunAnalyticsPanelPresenter.GetGemLabel(view.Gems[0]), "run panel gem label");
            AssertEqual("Super Nova", RunAnalyticsPanelPresenter.GetPackageLabel(view.MemoryPackages[0]), "run panel package label");
            AssertEqual("20/s", RunAnalyticsPanelPresenter.FormatDps(view.Memories[0].DirectDps), "run panel memory direct dps");
            AssertEqual("8/s", RunAnalyticsPanelPresenter.FormatDps(view.Gems[0].DirectDps), "run panel gem direct dps");
            AssertApproximately(100f, view.MemoryPackages[0].DirectMemoryDamage, "run panel package direct memory");
            AssertApproximately(40f, view.MemoryPackages[0].AttachedDirectGemDamage, "run panel package attached gem");
            AssertApproximately(140f, view.MemoryPackages[0].TotalPackageDamage, "run panel package total");
            AssertEqual(1, view.MemoryPackages[0].ChildBreakdown.Count, "run panel package child count");
            AssertEqual("Shock Gem", RunAnalyticsPanelPresenter.GetPackageChildLabel(view.MemoryPackages[0].ChildBreakdown[0]), "run panel child label");
            AssertEqual("70%", RunAnalyticsPanelPresenter.FormatPercent(view.LocalPlayer.PartyShare), "run panel party share");
            AssertEqual("0:05", RunAnalyticsPanelPresenter.FormatDuration(view.Run.CombatDuration), "run panel duration");
            AssertEqual("28/s", RunAnalyticsPanelPresenter.FormatDps(view.LocalPlayer.Dps), "run panel dps");
            AssertTrue(view.HasPartner, "run co-op selector visible state");
        }

        private static void ValidatesRunPanelPlayerSelectionSwitchesRowsAndCoverage()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var partner = Partner("partner-a");
            var localMemory = new MemoryKey(local, "Mem_Local", "Local Memory");
            var localGem = new GemKey(local, "Gem_Local", "Local Gem");
            var partnerMemory = new MemoryKey(partner, "Mem_Partner", "Partner Memory");
            var partnerGem = new GemKey(partner, "Gem_Partner", "Partner Gem");
            var partnerUnassignedGem = new GemKey(partner, "Gem_Partner_Unassigned", "Partner Loose Gem");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, localMemory.ContentId), localMemory, null, localMemory);
            service.CaptureDamage(2f, 40f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, localGem.ContentId), null, localGem, localMemory);
            service.CaptureDamage(3f, 60f, partner, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, partnerMemory.ContentId), partnerMemory, null, partnerMemory);
            service.CaptureDamage(4f, 30f, partner, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, partnerGem.ContentId), null, partnerGem, partnerMemory);
            service.CaptureDamage(5f, 10f, partner, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, partnerUnassignedGem.ContentId), null, partnerUnassignedGem, null);
            service.OnRoomCompleted(6f);

            var run = service.GetRunSnapshot();
            var localView = Require(
                RunAnalyticsPanelPresenter.BuildView(run, RunAnalyticsPanelMode.Package, AnalyticsPlayerSelection.Local),
                "run panel selected local view");
            var partnerView = Require(
                RunAnalyticsPanelPresenter.BuildView(run, RunAnalyticsPanelMode.Package, AnalyticsPlayerSelection.Partner),
                "run panel selected partner view");

            AssertEqual("You", localView.SelectedPlayerLabel, "run selected local label");
            AssertEqual("Partner", partnerView.SelectedPlayerLabel, "run selected partner label");
            AssertTrue(localView.SelectedPlayer.PlayerKey.Equals(local), "run selected local player");
            AssertTrue(partnerView.SelectedPlayer.PlayerKey.Equals(partner), "run selected partner player");
            AssertEqual("Local Memory", RunAnalyticsPanelPresenter.GetMemoryLabel(localView.Memories[0]), "run selected local memory");
            AssertEqual("Partner Memory", RunAnalyticsPanelPresenter.GetMemoryLabel(partnerView.Memories[0]), "run selected partner memory");
            AssertEqual("Local Gem", RunAnalyticsPanelPresenter.GetGemLabel(localView.Gems[0]), "run selected local gem");
            AssertEqual("Partner Gem", RunAnalyticsPanelPresenter.GetGemLabel(partnerView.Gems[0]), "run selected partner gem");
            AssertEqual("Local Memory", RunAnalyticsPanelPresenter.GetPackageLabel(localView.MemoryPackages[0]), "run selected local package");
            AssertEqual("Partner Memory", RunAnalyticsPanelPresenter.GetPackageLabel(partnerView.MemoryPackages[0]), "run selected partner package");
            AssertEqual("Partner Gem", RunAnalyticsPanelPresenter.GetPackageChildLabel(partnerView.MemoryPackages[0].ChildBreakdown[0]), "run selected partner package child");
            AssertApproximately(140f, localView.SelectedPlayer.Aggregate.Damage, "run selected local total");
            AssertApproximately(100f, partnerView.SelectedPlayer.Aggregate.Damage, "run selected partner total");
            AssertApproximately(140f, localView.MemoryPackages[0].TotalPackageDamage, "run selected local package total");
            AssertApproximately(90f, partnerView.MemoryPackages[0].TotalPackageDamage, "run selected partner package total");
            AssertApproximately(10f, partnerView.SelectedPlayer.MemoryGemCoverage.PackageRelationshipUnknownGemDamage, "run selected partner unknown package damage");
            AssertEqual(2, partnerView.Gems.Count, "run selected partner gem count");
            AssertEqual(1, partnerView.MemoryPackages.Count, "run selected partner package count");
        }

        private static void ValidatesRunPanelAbsentPartnerFallsBackToLocal()
        {
            var service = new DamageAnalyticsService();
            var local = Local("local-a");
            var memory = Memory(local, "Mem_Local");

            service.CaptureDamage(1f, 100f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);

            var view = Require(
                RunAnalyticsPanelPresenter.BuildView(service.GetRunSnapshot(), RunAnalyticsPanelMode.Memory, AnalyticsPlayerSelection.Partner),
                "run panel absent partner view");

            AssertEqual((int)AnalyticsPlayerSelection.Local, (int)view.SelectedPlayerSelection, "run absent partner selected local");
            AssertTrue(Require(view.SelectedPlayer, "run absent partner selected player").PlayerKey.Equals(local), "run absent partner local player");
            AssertEqual(1, view.Memories.Count, "run absent partner local rows");
            AssertTrue(!view.HasPartner, "run solo selector hidden state");
        }

        private static void ValidatesRunPanelCoverageAndEmptyStates()
        {
            var service = new DamageAnalyticsService();
            var emptyView = Require(RunAnalyticsPanelPresenter.BuildView(service.GetRunSnapshot(), RunAnalyticsPanelMode.Memory), "run panel empty view");
            AssertNull(emptyView.LocalPlayer, "run panel empty local");
            AssertEqual(0, emptyView.Memories.Count, "run panel empty memories");
            AssertEqual("Unknown", RunAnalyticsPanelPresenter.GetCoverageLabel(null), "run panel unknown coverage");

            var local = Local("local-a");
            var memory = Memory(local, "Mem_Known");
            var gem = Gem(local, "Gem_UnknownParent");

            service.CaptureDamage(1f, 60f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.MemoryDirect, memory.ContentId), memory, null, memory);
            service.CaptureDamage(2f, 40f, local, TargetRelationship.Hostile, Source(DamageSourceCategory.GemDirect, gem.ContentId), null, gem, null);

            var gemView = Require(RunAnalyticsPanelPresenter.BuildView(service.GetRunSnapshot(), RunAnalyticsPanelMode.Gem), "run panel gem view");
            AssertEqual(1, gemView.Gems.Count, "run panel unknown package gem count");
            AssertEqual(1, gemView.MemoryPackages.Count, "run panel unknown package package count");
            AssertApproximately(40f, gemView.LocalPlayer.MemoryGemCoverage.PackageRelationshipUnknownGemDamage, "run panel unknown package damage");
            AssertEqual("Verified", RunAnalyticsPanelPresenter.GetCoverageLabel(gemView.LocalPlayer.MemoryGemCoverage.MemoryIdentityCoverageRatio), "run panel memory coverage label");
            AssertEqual("Partial", RunAnalyticsPanelPresenter.GetCoverageLabel(gemView.LocalPlayer.MemoryGemCoverage.PackageAssignmentCoverageRatio), "run panel package coverage label");
            AssertEqual("Coverage 100%", RunAnalyticsPanelPresenter.FormatCoverageSummary(RunAnalyticsPanelMode.Memory, gemView.LocalPlayer), "run panel memory coverage summary");
            AssertEqual("Coverage 0% - 40 unattributed", RunAnalyticsPanelPresenter.FormatCoverageSummary(RunAnalyticsPanelMode.Package, gemView.LocalPlayer), "run panel package coverage summary");

            service.ResetRunForNewGame(5f);
            var resetView = Require(RunAnalyticsPanelPresenter.BuildView(service.GetRunSnapshot(), RunAnalyticsPanelMode.Package), "run panel reset view");
            AssertNull(resetView.LocalPlayer, "run panel reset local");
            AssertEqual(0, resetView.MemoryPackages.Count, "run panel reset packages");
        }

        private static void ValidatesRunPanelDefaultAndProvisionalPresentation()
        {
            AssertEqual((int)RunAnalyticsPanelMode.Package, (int)DamageAnalyticsRunPanel.DefaultMode, "run panel default package mode");
            AssertEqual("734", RunAnalyticsPanelPresenter.FormatDps(734f, true), "run panel validated dps format");
            AssertEqual("~734", RunAnalyticsPanelPresenter.FormatDps(734f, false), "run panel provisional dps format");
            AssertEqual("Quick Trigger", RunAnalyticsPanelPresenter.GetMemoryLabel(new MemoryDamageSnapshot(new MemoryKey(Local("local-a"), "St_R_QuickTrigger", "St R QuickTrigger"), new DamageAggregateSnapshot(1f, 1, 1f, 1f), 1f, 1f)), "run panel friendly memory name");
            AssertEqual("Summon Little Beam", RunAnalyticsPanelPresenter.GetGemLabel(new GemDamageSnapshot(new GemKey(Local("local-a"), "St_R_SummonLittleBeam", "St R SummonLittleBeam"), new DamageAggregateSnapshot(1f, 1, 1f, 1f), 1f, 1f)), "run panel friendly gem name");
        }

        private static void ValidatesBuildStateSnapshotCopiesInputs()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_Fireball");
            var gem = Gem(player, "Gem_Flame");
            var attachedGemKeys = new List<GemKey> { gem };
            var memoryValues = new List<ComparisonStructuredValue>
            {
                new ComparisonStructuredValue(
                    "base-damage",
                    "Base damage",
                    120f,
                    "damage",
                    "",
                    ComparisonResultClass.Exact,
                    ComparisonConfidence.Verified,
                    null)
            };
            var memoryState = new MemoryState(memory, memory.ContentId, 2, null, 4, null, attachedGemKeys, memoryValues);
            var gemState = new GemState(gem, gem.ContentId, 250f, memory, memoryValues);
            var stats = new List<BuildStatValue>
            {
                new BuildStatValue("attack-damage", 31f, "", ComparisonConfidence.Verified)
            };
            var memories = new List<MemoryState> { memoryState };
            var gems = new List<GemState> { gemState };
            var modifiers = new List<BuildModifierState>
            {
                new BuildModifierState("chaos-ad", "chaos", memoryValues)
            };

            var snapshot = new BuildStateSnapshot(player, "Traveler_Primus", stats, memories, gems, modifiers, 12f);

            attachedGemKeys.Clear();
            memoryValues.Clear();
            stats.Clear();
            memories.Clear();
            gems.Clear();
            modifiers.Clear();

            AssertEqual(1, snapshot.FinalStats.Count, "build snapshot stats copied");
            AssertEqual(1, snapshot.Memories.Count, "build snapshot memories copied");
            AssertEqual(1, snapshot.Gems.Count, "build snapshot gems copied");
            AssertEqual(1, snapshot.PersistentModifiers.Count, "build snapshot modifiers copied");
            AssertEqual(1, snapshot.Memories[0].AttachedGemKeys.Count, "memory attached gems copied");
            AssertEqual(1, snapshot.Memories[0].StructuredValues.Count, "memory structured values copied");
            AssertEqual(1, snapshot.Gems[0].StructuredValues.Count, "gem structured values copied");
            AssertEqual(1, snapshot.PersistentModifiers[0].Values.Count, "modifier values copied");
        }

        private static void ValidatesComparisonResultAndConfidenceSemantics()
        {
            var exactMetric = new ComparisonMetric(
                "damage-per-use",
                "Damage per use",
                100f,
                125f,
                25f,
                0.25f,
                "damage",
                ComparisonResultClass.Exact,
                ComparisonConfidence.Verified,
                null);
            var estimatedMetric = new ComparisonMetric(
                "estimated-dps",
                "Estimated DPS",
                10f,
                14f,
                4f,
                0.4f,
                "damage/s",
                ComparisonResultClass.Estimated,
                ComparisonConfidence.Estimated,
                new[] { "uses observed hit frequency" });
            var utility = new ComparisonUtilityChange(
                "radius",
                "larger area",
                3f,
                4f,
                1f,
                "m",
                ComparisonConfidence.HighConfidence,
                null);

            AssertFalse(exactMetric.IsUnknownOrUnsupported, "exact metric known");
            AssertTrue(exactMetric.HasKnownNumericDelta, "exact metric numeric delta");
            AssertFalse(estimatedMetric.IsUnknownOrUnsupported, "estimated metric known but estimated");
            AssertEqual((int)ComparisonResultClass.Estimated, (int)estimatedMetric.ResultClass, "estimated result class");
            AssertEqual((int)ComparisonConfidence.Estimated, (int)estimatedMetric.Confidence, "estimated confidence");
            AssertEqual((int)ComparisonResultClass.Utility, (int)utility.ResultClass, "utility result class");
        }

        private static void ValidatesUnknownAndUnsupportedComparisonValuesAreNotZero()
        {
            var unknownMetric = ComparisonMetric.Unknown("whole-build-dps", "Whole-build DPS", new[] { "unsupported frequency model" });
            var unsupportedMetric = ComparisonMetric.Unsupported("passive-interaction", "Passive interaction", new[] { "no evaluator for this mechanic" });
            var unknownEffect = EffectiveEffect.Unknown("conditional-effect", "conditional damage branch", null);
            var unsupportedEffect = EffectiveEffect.Unsupported("summon-ai", "summon behavior", null);

            AssertTrue(unknownMetric.IsUnknownOrUnsupported, "unknown metric semantic");
            AssertTrue(unsupportedMetric.IsUnknownOrUnsupported, "unsupported metric semantic");
            AssertFalse(unknownMetric.HasNumericProjection, "unknown metric has no numeric projection");
            AssertFalse(unsupportedMetric.HasNumericProjection, "unsupported metric has no numeric projection");
            AssertFalse(unknownMetric.HasKnownNumericDelta, "unknown metric has no known delta");
            AssertFalse(unsupportedMetric.HasKnownNumericDelta, "unsupported metric has no known delta");
            AssertNull(unknownMetric.DeltaValue, "unknown metric delta null");
            AssertNull(unsupportedMetric.DeltaPercent, "unsupported metric delta percent null");
            AssertTrue(unknownEffect.IsUnknownOrUnsupported, "unknown effect semantic");
            AssertTrue(unsupportedEffect.IsUnknownOrUnsupported, "unsupported effect semantic");
            AssertFalse(unknownEffect.HasNumericProjection, "unknown effect has no numeric projection");
            AssertFalse(unsupportedEffect.HasNumericProjection, "unsupported effect has no numeric projection");
        }

        private static void ValidatesNotApplicableComparisonValuesAreNotZero()
        {
            var notApplicableMetric = ComparisonMetric.NotApplicable("direct-gem-damage", "Direct Gem damage", new[] { "Gem has no direct damage event" });
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var replacement = ComparisonGem("Gem_Current", "Current Gem");
            var view = ComparisonPresentationShell.BuildView(new[]
            {
                ComparisonOption(candidate, replacement, notApplicableMetric)
            });

            AssertTrue(notApplicableMetric.IsUnknownOrUnsupported, "not-applicable metric is non-comparable");
            AssertFalse(notApplicableMetric.HasNumericProjection, "not-applicable metric has no numeric projection");
            AssertFalse(notApplicableMetric.HasKnownNumericDelta, "not-applicable metric has no known delta");
            AssertEqual((int)ComparisonResultClass.NotApplicable, (int)notApplicableMetric.ResultClass, "not-applicable result class");
            AssertEqual(0, view.RankedDamageOptions.Count, "not-applicable not ranked as damage");
            AssertEqual(1, view.UnrankedOptions.Count, "not-applicable remains visible");
            AssertEqual((int)ComparisonPresentationMetricState.NotApplicable, (int)view.UnrankedOptions[0].PrimaryDamageRow.State, "not-applicable presentation state");
            AssertEqual("N/A", view.UnrankedOptions[0].PrimaryDamageRow.ValueText, "not-applicable presentation text");
        }

        private static void ValidatesBuildOptionComparisonComposition()
        {
            var player = Local("local-a");
            var candidateGem = Gem(player, "Gem_Light");
            var currentGem = Gem(player, "Gem_Fire");
            var memory = Memory(player, "Mem_Beam");
            var candidate = new ComparisonSubject(ComparisonSubjectKind.Gem, candidateGem.StableId, candidateGem.ContentId, candidateGem.DisplayName);
            var replacement = new ComparisonSubject(ComparisonSubjectKind.Gem, currentGem.StableId, currentGem.ContentId, currentGem.DisplayName);
            var memoryState = new MemoryState(memory, memory.ContentId, 1, null, 3, null, new[] { currentGem }, null);
            var candidateChange = new CandidateChange(BuildChangeKind.Replace, candidate, replacement);
            var attachedGemContext = new List<GemState>
            {
                new GemState(currentGem, currentGem.ContentId, 100f, memory, null)
            };
            var effectRows = new List<EffectiveEffect>
            {
                new EffectiveEffect(
                    "light-damage",
                    "generic damage bonus",
                    "light-context damage bonus",
                    100f,
                    130f,
                    30f,
                    0.3f,
                    "damage",
                    "Light",
                    ComparisonResultClass.Derived,
                    ComparisonConfidence.HighConfidence,
                    null)
            };
            var evaluation = new ContextualEffectEvaluation(
                candidate,
                memoryState,
                attachedGemContext,
                candidateChange,
                effectRows,
                ComparisonConfidence.HighConfidence,
                null);
            var metrics = new List<ComparisonMetric>
            {
                new ComparisonMetric(
                    "damage-per-use",
                    "Damage per use",
                    100f,
                    130f,
                    30f,
                    0.3f,
                    "damage",
                    ComparisonResultClass.Derived,
                    ComparisonConfidence.HighConfidence,
                    null)
            };
            var utilityChanges = new List<ComparisonUtilityChange>
            {
                new ComparisonUtilityChange("range", "more range", 6f, 7f, 1f, "m", ComparisonConfidence.HighConfidence, null)
            };
            var observedContext = new List<ObservedContextMetric>
            {
                new ObservedContextMetric("run-direct-damage", "Run direct damage", 8400f, "damage", "", "run", currentGem.StableId)
            };
            var comparison = new BuildOptionComparison(
                candidate,
                replacement,
                metrics,
                utilityChanges,
                observedContext,
                metrics[0],
                ComparisonConfidence.Partial,
                new[] { "whole-build DPS not evaluated" });

            metrics.Clear();
            utilityChanges.Clear();
            observedContext.Clear();
            attachedGemContext.Clear();
            effectRows.Clear();

            AssertTrue(comparison.Candidate.Equals(candidate), "comparison candidate");
            AssertTrue(comparison.ReplacementTarget.HasValue, "comparison replacement target");
            AssertTrue(comparison.ReplacementTarget.Value.Equals(replacement), "comparison replacement");
            AssertEqual(1, comparison.Metrics.Count, "comparison metrics copied");
            AssertEqual(1, comparison.UtilityChanges.Count, "comparison utility copied");
            AssertEqual(1, comparison.ObservedContext.Count, "comparison observed copied");
            AssertEqual("damage-per-use", comparison.PrimaryDamageDelta.MetricId, "comparison primary damage metric");
            AssertEqual((int)ComparisonConfidence.Partial, (int)comparison.Confidence, "comparison confidence");
            AssertEqual(1, comparison.Limitations.Count, "comparison limitations");
            AssertEqual(1, evaluation.AttachedGemContext.Count, "evaluation attached gems copied");
            AssertEqual(1, evaluation.EffectiveEffects.Count, "evaluation effects copied");
            AssertEqual("Light", evaluation.EffectiveEffects[0].EffectiveElement, "evaluation effective element");
        }

        private static void ValidatesContextualEffectEvaluatorPassesThroughStructuredValues()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_Bolt");
            var gem = Gem(player, "Gem_Focus");
            var structuredValue = new ComparisonStructuredValue(
                "base-damage",
                "Base damage",
                120f,
                "damage",
                "120 damage",
                ComparisonResultClass.Exact,
                ComparisonConfidence.Verified,
                null);
            var memoryState = new MemoryState(memory, memory.ContentId, 0, null, 3, null, new[] { gem }, new[] { structuredValue });
            var gemState = new GemState(gem, gem.ContentId, 100f, memory, null);
            var buildState = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { gemState }, null, 10f);

            var evaluation = ContextualEffectEvaluator.Evaluate(buildState, memoryState, null, null);

            AssertEqual((int)ComparisonSubjectKind.Memory, (int)evaluation.Subject.Kind, "passthrough subject kind");
            AssertEqual(1, evaluation.AttachedGemContext.Count, "passthrough attached gems resolved");
            AssertEqual(1, evaluation.EffectiveEffects.Count, "passthrough effect count");
            AssertEqual("base-damage", evaluation.EffectiveEffects[0].EffectId, "passthrough effect id");
            AssertEqual("120 damage", evaluation.EffectiveEffects[0].EffectiveDescription, "passthrough description");
            AssertApproximately(120f, Require(evaluation.EffectiveEffects[0].AfterValue, "passthrough after value"), "passthrough after value");
            AssertEqual((int)ComparisonResultClass.Exact, (int)evaluation.EffectiveEffects[0].ResultClass, "passthrough result class");
            AssertEqual((int)ComparisonConfidence.Verified, (int)evaluation.Confidence, "passthrough confidence");
            AssertFalse(evaluation.EffectiveEffects[0].IsUnknownOrUnsupported, "passthrough known effect");
        }

        private static void ValidatesContextualEffectEvaluatorResolvesMatchingCondition()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_Ray");
            var gem = Gem(player, "Gem_LightAmplifier");
            var candidateGem = Gem(player, "Gem_CandidateLight");
            var memoryElement = new ComparisonStructuredValue(
                "memory-element",
                "Element",
                null,
                "",
                "Light",
                ComparisonResultClass.Exact,
                ComparisonConfidence.Verified,
                null);
            var effectValue = new ComparisonStructuredValue(
                "conditional-light-damage",
                "Conditional damage",
                0.35f,
                "multiplier",
                "+35% Light damage",
                ComparisonResultClass.Derived,
                ComparisonConfidence.HighConfidence,
                null);
            var memoryState = new MemoryState(memory, memory.ContentId, 1, null, 4, null, new[] { gem }, new[] { memoryElement });
            var gemState = new GemState(gem, gem.ContentId, 500f, memory, null);
            var buildState = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { gemState }, null, 11f);
            var candidate = new ComparisonSubject(ComparisonSubjectKind.Gem, candidateGem.StableId, candidateGem.ContentId, candidateGem.DisplayName);
            var replacement = new ComparisonSubject(ComparisonSubjectKind.Gem, gem.StableId, gem.ContentId, gem.DisplayName);
            var candidateChange = new CandidateChange(BuildChangeKind.Replace, candidate, replacement);
            var rule = new ContextualEffectRule(
                effectValue,
                "conditional elemental damage bonus",
                "Light-context damage bonus",
                "Light",
                new[] { ContextualEffectCondition.MemoryElement("Light") },
                null);

            var evaluation = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(
                buildState,
                memoryState,
                new[] { gemState },
                candidateChange,
                new[] { rule }));

            AssertTrue(evaluation.Subject.Equals(candidate), "conditional subject is candidate");
            AssertEqual((int)BuildChangeKind.Replace, (int)evaluation.CandidateChange.ChangeKind, "conditional candidate change carried");
            AssertEqual(1, evaluation.EffectiveEffects.Count, "conditional effect count");
            AssertEqual("Light-context damage bonus", evaluation.EffectiveEffects[0].EffectiveDescription, "conditional effective description");
            AssertApproximately(0.35f, Require(evaluation.EffectiveEffects[0].AfterValue, "conditional after value"), "conditional after value");
            AssertEqual("Light", evaluation.EffectiveEffects[0].EffectiveElement, "conditional effective element");
            AssertEqual((int)EffectiveElementResultKind.Known, (int)evaluation.EffectiveElement.Kind, "conditional aggregate element known");
            AssertEqual("Light", evaluation.EffectiveElement.Element, "conditional aggregate element");
            AssertEqual((int)ComparisonConfidence.HighConfidence, (int)evaluation.Confidence, "conditional confidence");
        }

        private static void ValidatesContextualEffectEvaluatorFallsBackToUnknownWhenContextIsInsufficient()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_UnknownElement");
            var gem = Gem(player, "Gem_ElementalBranch");
            var effectValue = new ComparisonStructuredValue(
                "conditional-light-damage",
                "Conditional damage",
                0.35f,
                "multiplier",
                "+35% Light damage",
                ComparisonResultClass.Derived,
                ComparisonConfidence.HighConfidence,
                null);
            var memoryState = new MemoryState(memory, memory.ContentId, 2, null, null, null, new[] { gem }, null);
            var gemState = new GemState(gem, gem.ContentId, 200f, memory, null);
            var buildState = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { gemState }, null, 12f);
            var rule = new ContextualEffectRule(
                effectValue,
                "conditional elemental damage bonus",
                "Light-context damage bonus",
                "Light",
                new[] { ContextualEffectCondition.MemoryElement("Light") },
                null);

            var evaluation = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(
                buildState,
                memoryState,
                new[] { gemState },
                null,
                new[] { rule }));

            AssertEqual(1, evaluation.EffectiveEffects.Count, "insufficient context effect count");
            AssertTrue(evaluation.EffectiveEffects[0].IsUnknownOrUnsupported, "insufficient context unknown effect");
            AssertEqual("conditional elemental damage bonus", evaluation.EffectiveEffects[0].GenericDescription, "insufficient context preserves generic description");
            AssertFalse(evaluation.EffectiveEffects[0].HasNumericProjection, "insufficient context numeric projection");
            AssertNull(evaluation.EffectiveEffects[0].AfterValue, "insufficient context after value null");
            AssertEqual((int)ComparisonConfidence.Unknown, (int)evaluation.Confidence, "insufficient context evaluation confidence");
            AssertEqual((int)EffectiveElementResultKind.Unknown, (int)evaluation.EffectiveElement.Kind, "insufficient context aggregate element unknown");
            AssertTrue(evaluation.Limitations.Count > 0, "insufficient context limitation preserved");
        }

        private static void ValidatesContextualEffectEvaluatorUsesMostConservativeConfidence()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_PartialElement");
            var gem = Gem(player, "Gem_LightBranch");
            var partialElement = new ComparisonStructuredValue(
                "memory-element",
                "Element",
                null,
                "",
                "Light",
                ComparisonResultClass.StronglyInferred,
                ComparisonConfidence.Partial,
                null);
            var effectValue = new ComparisonStructuredValue(
                "conditional-light-damage",
                "Conditional damage",
                0.2f,
                "multiplier",
                "+20% Light damage",
                ComparisonResultClass.Derived,
                ComparisonConfidence.HighConfidence,
                null);
            var memoryState = new MemoryState(memory, memory.ContentId, 3, null, null, null, new[] { gem }, new[] { partialElement });
            var gemState = new GemState(gem, gem.ContentId, 300f, memory, null);
            var buildState = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { gemState }, null, 13f);
            var rule = new ContextualEffectRule(
                effectValue,
                "conditional elemental damage bonus",
                "Light-context damage bonus",
                "Light",
                new[] { ContextualEffectCondition.MemoryElement("Light") },
                null);

            var evaluation = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(
                buildState,
                memoryState,
                new[] { gemState },
                null,
                new[] { rule }));

            AssertEqual((int)ComparisonConfidence.Partial, (int)evaluation.EffectiveEffects[0].Confidence, "conservative effect confidence");
            AssertEqual((int)ComparisonConfidence.Partial, (int)evaluation.Confidence, "conservative evaluation confidence");
            AssertEqual((int)ComparisonConfidence.Partial, (int)evaluation.EffectiveElement.Confidence, "conservative element confidence");
        }

        private static void ValidatesContextualEffectEvaluatorEffectiveElementStates()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_ElementStates");
            var memoryState = new MemoryState(memory, memory.ContentId, 4, null, null, null, null, null);
            var buildState = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, null, null, 14f);
            var fireRule = new ContextualEffectRule(
                new ComparisonStructuredValue("fire-damage", "Fire damage", 10f, "damage", "10 Fire damage", ComparisonResultClass.Derived, ComparisonConfidence.Verified, null),
                "fire damage",
                "Fire damage",
                "Fire",
                null,
                null);
            var lightRule = new ContextualEffectRule(
                new ComparisonStructuredValue("light-damage", "Light damage", 12f, "damage", "12 Light damage", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence, null),
                "light damage",
                "Light damage",
                "Light",
                null,
                null);
            var unknownRule = new ContextualEffectRule(
                new ComparisonStructuredValue("unknown-damage", "Unknown damage", null, "damage", "unknown damage", ComparisonResultClass.Unknown, ComparisonConfidence.Unknown, null),
                "unknown damage",
                "unknown damage",
                "",
                null,
                null);

            var known = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(buildState, memoryState, null, null, new[] { fireRule }));
            var mixed = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(buildState, memoryState, null, null, new[] { fireRule, lightRule }));
            var unknown = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(buildState, memoryState, null, null, new[] { unknownRule }));

            AssertEqual((int)EffectiveElementResultKind.Known, (int)known.EffectiveElement.Kind, "known element kind");
            AssertEqual("Fire", known.EffectiveElement.Element, "known element value");
            AssertEqual((int)EffectiveElementResultKind.Mixed, (int)mixed.EffectiveElement.Kind, "mixed element kind");
            AssertEqual("Mixed", mixed.EffectiveElement.Element, "mixed element value");
            AssertEqual(2, mixed.EffectiveElement.Elements.Count, "mixed element count");
            AssertEqual((int)ComparisonConfidence.HighConfidence, (int)mixed.EffectiveElement.Confidence, "mixed element confidence");
            AssertEqual((int)EffectiveElementResultKind.Unknown, (int)unknown.EffectiveElement.Kind, "unknown element kind");
            AssertFalse(unknown.EffectiveEffects[0].HasNumericProjection, "unknown element effect has no numeric projection");
        }

        private static void ValidatesContextualTooltipPresentationBuildsSupportedGemContributionRows()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_Ray");
            var gem = Gem(player, "Gem_LightAmplifier");
            var memoryElement = Structured("memory-element", "Element", null, "", "Light", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var effectValue = Structured("conditional-light-damage", "Conditional damage", 0.35f, "multiplier", "+35% Light damage", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence);
            var memoryState = new MemoryState(memory, memory.ContentId, 1, null, 4, null, new[] { gem }, new[] { memoryElement });
            var gemState = new GemState(gem, gem.ContentId, 500f, memory, null);
            var buildState = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { gemState }, null, 15f);
            var rule = new ContextualEffectRule(
                effectValue,
                "conditional elemental damage bonus",
                "Light-context damage bonus",
                "Light",
                new[] { ContextualEffectCondition.MemoryElement("Light") },
                null);
            var evaluation = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(
                buildState,
                memoryState,
                new[] { gemState },
                null,
                new[] { rule }));

            var view = ContextualTooltipPresentationModels.BuildGemContributionView(evaluation, "Fire");

            AssertTrue(view.HasContributionRows, "tooltip supported rows present");
            AssertEqual((int)ContextualTooltipIntegrationScope.ModelOnly, (int)view.IntegrationScope, "tooltip supported model-only scope");
            AssertEqual(memory.DisplayName, view.MemoryLabel, "tooltip supported memory label");
            AssertEqual(1, view.AttachedGemLabels.Count, "tooltip supported attached gem count");
            AssertEqual(gem.DisplayName, view.AttachedGemLabels[0], "tooltip supported attached gem label");
            AssertEqual(1, view.ContributionRows.Count, "tooltip supported row count");
            AssertEqual((int)ContextualTooltipPresentationState.Known, (int)view.ContributionRows[0].State, "tooltip supported row state");
            AssertEqual("conditional-light-damage", view.ContributionRows[0].EffectId, "tooltip supported row effect id");
            AssertEqual("Light-context damage bonus", view.ContributionRows[0].Label, "tooltip supported row label");
            AssertEqual("0.35 multiplier", view.ContributionRows[0].AfterText, "tooltip supported after text");
            AssertEqual("0.35 multiplier", view.ContributionRows[0].ValueText, "tooltip supported value text");
            AssertEqual("Light", view.ContributionRows[0].EffectiveElement, "tooltip supported row effective element");
            AssertEqual((int)ContextualTooltipPresentationState.Known, (int)view.EffectiveElementIndicator.State, "tooltip supported element indicator state");
            AssertEqual("Light", view.EffectiveElementIndicator.Element, "tooltip supported element indicator");
            AssertTrue(view.EffectiveElementIndicator.HasIndicator, "tooltip supported indicator visible");
            AssertFalse(view.EffectiveElementIndicator.UsesNativeElement, "tooltip supported indicator is contextual");
        }

        private static void ValidatesContextualTooltipPresentationPreservesEstimatedUnknownAndUnsupportedRows()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_MixedSupport");
            var memoryState = new MemoryState(memory, memory.ContentId, 2, null, null, null, null, null);
            var buildState = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, null, null, 16f);
            var estimatedRule = new ContextualEffectRule(
                new ComparisonStructuredValue("estimated-dps", "Estimated DPS", 4f, "damage/s", "+4 DPS", ComparisonResultClass.Estimated, ComparisonConfidence.Estimated, new[] { "uses observed activation frequency" }),
                "estimated damage contribution",
                "Estimated contextual DPS",
                "",
                null,
                null);
            var unknownRule = new ContextualEffectRule(
                new ComparisonStructuredValue("passive-interaction", "Passive interaction", null, "", "unknown passive value", ComparisonResultClass.Unknown, ComparisonConfidence.Unknown, new[] { "passive trigger is unmapped" }),
                "passive interaction",
                "Passive interaction",
                "",
                null,
                null);
            var unsupportedRule = new ContextualEffectRule(
                new ComparisonStructuredValue("summon-scaling", "Summon scaling", null, "", "unsupported summon scaling", ComparisonResultClass.Unsupported, ComparisonConfidence.Unsupported, new[] { "summon formula is unsupported" }),
                "summon scaling",
                "Summon scaling",
                "",
                null,
                null);
            var evaluation = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(
                buildState,
                memoryState,
                null,
                null,
                new[] { estimatedRule, unknownRule, unsupportedRule }));

            var view = ContextualTooltipPresentationModels.BuildGemContributionView(evaluation, "Fire");

            AssertEqual(3, view.ContributionRows.Count, "tooltip mixed support row count");
            AssertEqual((int)ContextualTooltipPresentationState.Estimated, (int)view.ContributionRows[0].State, "tooltip estimated row state");
            AssertEqual("4 damage/s", view.ContributionRows[0].ValueText, "tooltip estimated value");
            AssertTrue(view.ContributionRows[0].Limitations.Count > 0, "tooltip estimated limitations preserved");
            AssertEqual((int)ContextualTooltipPresentationState.Unknown, (int)view.ContributionRows[1].State, "tooltip unknown row state");
            AssertEqual("Unknown", view.ContributionRows[1].ValueText, "tooltip unknown value");
            AssertTrue(view.ContributionRows[1].Limitations.Count > 0, "tooltip unknown limitations preserved");
            AssertEqual((int)ContextualTooltipPresentationState.Unsupported, (int)view.ContributionRows[2].State, "tooltip unsupported row state");
            AssertEqual("Unsupported", view.ContributionRows[2].ValueText, "tooltip unsupported value");
            AssertTrue(view.ContributionRows[2].Limitations.Count > 0, "tooltip unsupported limitations preserved");
            AssertFalse(view.EffectiveElementIndicator.HasIndicator, "tooltip unknown element does not render indicator");
        }

        private static void ValidatesEffectiveMemoryElementIndicatorKnownState()
        {
            var indicator = ContextualTooltipPresentationModels.BuildEffectiveMemoryElementIndicator(
                EffectiveElementResult.Known("Light", ComparisonConfidence.HighConfidence, null),
                "Fire");

            AssertEqual((int)ContextualTooltipPresentationState.Known, (int)indicator.State, "effective indicator known state");
            AssertEqual("Light", indicator.Element, "effective indicator known element");
            AssertEqual("Fire", indicator.NativeElement, "effective indicator native element retained");
            AssertTrue(indicator.HasIndicator, "effective indicator known visible");
            AssertFalse(indicator.UsesNativeElement, "effective indicator known not native fallback");
            AssertFalse(indicator.IsNeutral, "effective indicator known not neutral");
        }

        private static void ValidatesEffectiveMemoryElementIndicatorNativeFallbackState()
        {
            var indicator = ContextualTooltipPresentationModels.BuildEffectiveMemoryElementIndicator(
                EffectiveElementResult.Known("Fire", ComparisonConfidence.Verified, null),
                "Fire");

            AssertEqual((int)ContextualTooltipPresentationState.NativeFallback, (int)indicator.State, "effective indicator native fallback state");
            AssertEqual("Fire", indicator.Element, "effective indicator native fallback element");
            AssertTrue(indicator.HasIndicator, "effective indicator native fallback visible");
            AssertTrue(indicator.UsesNativeElement, "effective indicator native fallback uses native");
            AssertFalse(indicator.IsNeutral, "effective indicator native fallback not neutral");
        }

        private static void ValidatesEffectiveMemoryElementIndicatorNeutralStates()
        {
            var mixed = ContextualTooltipPresentationModels.BuildEffectiveMemoryElementIndicator(
                EffectiveElementResult.Mixed(new[] { "Fire", "Light" }, ComparisonConfidence.HighConfidence, null),
                "Fire");
            var unknown = ContextualTooltipPresentationModels.BuildEffectiveMemoryElementIndicator(
                EffectiveElementResult.Unknown(new[] { "effective element unavailable" }),
                "Fire");

            AssertEqual((int)ContextualTooltipPresentationState.Mixed, (int)mixed.State, "effective indicator mixed state");
            AssertFalse(mixed.HasIndicator, "effective indicator mixed absent");
            AssertTrue(mixed.IsNeutral, "effective indicator mixed neutral");
            AssertEqual("Mixed", mixed.DisplayText, "effective indicator mixed text");
            AssertEqual((int)ContextualTooltipPresentationState.Unknown, (int)unknown.State, "effective indicator unknown state");
            AssertFalse(unknown.HasIndicator, "effective indicator unknown absent");
            AssertTrue(unknown.IsNeutral, "effective indicator unknown neutral");
            AssertEqual("Unknown", unknown.DisplayText, "effective indicator unknown text");
            AssertTrue(unknown.Limitations.Count > 0, "effective indicator unknown limitations");
        }

        private static void ValidatesContextualTooltipPresentationHasNoNativeUiHookAssumptions()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_ModelOnly");
            var memoryState = new MemoryState(memory, memory.ContentId, 0, null, null, null, null, null);
            var buildState = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, null, null, 17f);
            var rule = new ContextualEffectRule(
                Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 80f, "damage", "80 damage", ComparisonResultClass.Exact, ComparisonConfidence.Verified),
                "direct damage",
                "Direct damage",
                "Fire",
                null,
                null);
            var evaluation = ContextualEffectEvaluator.Evaluate(new ContextualEffectEvaluationRequest(
                buildState,
                memoryState,
                null,
                null,
                new[] { rule }));

            var view = ContextualTooltipPresentationModels.BuildGemContributionView(evaluation, "Fire");

            AssertEqual((int)ContextualTooltipIntegrationScope.ModelOnly, (int)view.IntegrationScope, "tooltip no native ui hook scope");
            AssertEqual(1, view.ContributionRows.Count, "tooltip no hook row count");
            AssertEqual((int)ContextualTooltipPresentationState.NativeFallback, (int)view.EffectiveElementIndicator.State, "tooltip no hook native fallback");
            AssertTrue(view.EffectiveElementIndicator.UsesNativeElement, "tooltip no hook native element fallback");
            AssertEqual("Fire", view.EffectiveElementIndicator.NativeElement, "tooltip no hook native element retained as data");
        }

        private static void ValidatesCommonComparisonDirectDamageAndActivationDamage()
        {
            var beforeDamage = Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 50f, "damage", "50 damage", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var afterDamage = Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 60f, "damage", "60 damage", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var beforeHits = Structured(CommonComparisonValueIds.HitCount, "Hit count", 2f, "hits", "2 hits", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var afterHits = Structured(CommonComparisonValueIds.HitCount, "Hit count", 3f, "hits", "3 hits", ComparisonResultClass.Exact, ComparisonConfidence.Verified);

            var directDamage = CommonComparisonEvaluators.EvaluateDirectDamage(beforeDamage, afterDamage);
            var hitCount = CommonComparisonEvaluators.EvaluateHitCount(beforeHits, afterHits);
            var activationDamage = CommonComparisonEvaluators.EvaluateActivationDamage(beforeDamage, afterDamage, beforeHits, afterHits);

            AssertEqual("direct-damage", directDamage.MetricId, "common direct damage metric id");
            AssertApproximately(50f, Require(directDamage.BeforeValue, "common direct before"), "common direct before");
            AssertApproximately(60f, Require(directDamage.AfterValue, "common direct after"), "common direct after");
            AssertApproximately(10f, Require(directDamage.DeltaValue, "common direct delta"), "common direct delta");
            AssertApproximately(0.2f, Require(directDamage.DeltaPercent, "common direct delta percent"), "common direct delta percent");
            AssertEqual((int)ComparisonResultClass.Exact, (int)directDamage.ResultClass, "common direct result class");
            AssertFalse(directDamage.IsUnknownOrUnsupported, "common direct known");

            AssertEqual("hit-count", hitCount.MetricId, "common hit count metric id");
            AssertApproximately(1f, Require(hitCount.DeltaValue, "common hit count delta"), "common hit count delta");
            AssertEqual("hits", hitCount.Unit, "common hit count unit");

            AssertEqual("damage-per-activation", activationDamage.MetricId, "common activation metric id");
            AssertApproximately(100f, Require(activationDamage.BeforeValue, "common activation before"), "common activation before");
            AssertApproximately(180f, Require(activationDamage.AfterValue, "common activation after"), "common activation after");
            AssertApproximately(80f, Require(activationDamage.DeltaValue, "common activation delta"), "common activation delta");
            AssertEqual((int)ComparisonResultClass.Derived, (int)activationDamage.ResultClass, "common activation result class");
            AssertFalse(activationDamage.IsUnknownOrUnsupported, "common activation known");
        }

        private static void ValidatesCommonComparisonAdAndApScalingRequireCoefficients()
        {
            var beforeAd = new BuildStatValue(CommonComparisonValueIds.AttackDamage, 120f, "", ComparisonConfidence.Verified);
            var afterAd = new BuildStatValue(CommonComparisonValueIds.AttackDamage, 150f, "", ComparisonConfidence.Verified);
            var beforeAp = new BuildStatValue(CommonComparisonValueIds.AbilityPower, 200f, "", ComparisonConfidence.Verified);
            var afterAp = new BuildStatValue(CommonComparisonValueIds.AbilityPower, 250f, "", ComparisonConfidence.Verified);
            var adCoefficient = Structured(CommonComparisonValueIds.AttackDamageCoefficient, "AD coefficient", 0.5f, "coefficient", "50% AD", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var apCoefficient = Structured(CommonComparisonValueIds.AbilityPowerCoefficient, "AP coefficient", 0.25f, "coefficient", "25% AP", ComparisonResultClass.Exact, ComparisonConfidence.Verified);

            var adDamage = CommonComparisonEvaluators.EvaluateAdScalingDamage(beforeAd, afterAd, adCoefficient);
            var apDamage = CommonComparisonEvaluators.EvaluateApScalingDamage(beforeAp, afterAp, apCoefficient);
            var missingCoefficient = CommonComparisonEvaluators.EvaluateAdScalingDamage(beforeAd, afterAd, null);
            var wrongCoefficient = CommonComparisonEvaluators.EvaluateAdScalingDamage(beforeAd, afterAd, apCoefficient);

            AssertEqual("ad-scaling-damage", adDamage.MetricId, "common ad metric id");
            AssertApproximately(60f, Require(adDamage.BeforeValue, "common ad before"), "common ad before");
            AssertApproximately(75f, Require(adDamage.AfterValue, "common ad after"), "common ad after");
            AssertApproximately(15f, Require(adDamage.DeltaValue, "common ad delta"), "common ad delta");
            AssertEqual((int)ComparisonResultClass.Derived, (int)adDamage.ResultClass, "common ad result class");
            AssertTrue(adDamage.Limitations.Count > 0, "common ad limitation keeps dps boundary");

            AssertEqual("ap-scaling-damage", apDamage.MetricId, "common ap metric id");
            AssertApproximately(50f, Require(apDamage.BeforeValue, "common ap before"), "common ap before");
            AssertApproximately(62.5f, Require(apDamage.AfterValue, "common ap after"), "common ap after");
            AssertApproximately(12.5f, Require(apDamage.DeltaValue, "common ap delta"), "common ap delta");

            AssertTrue(missingCoefficient.IsUnknownOrUnsupported, "common missing coefficient unknown");
            AssertFalse(missingCoefficient.HasNumericProjection, "common missing coefficient no numeric projection");
            AssertEqual((int)ComparisonResultClass.Unknown, (int)missingCoefficient.ResultClass, "common missing coefficient result class");
            AssertTrue(wrongCoefficient.IsUnknownOrUnsupported, "common wrong coefficient unsupported");
            AssertEqual((int)ComparisonResultClass.Unsupported, (int)wrongCoefficient.ResultClass, "common wrong coefficient result class");
        }

        private static void ValidatesCommonComparisonCooldownChargesAndRangeUtility()
        {
            var beforeCooldown = Structured(CommonComparisonValueIds.Cooldown, "Cooldown", 8f, "s", "8s", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var afterCooldown = Structured(CommonComparisonValueIds.Cooldown, "Cooldown", 6f, "s", "6s", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var beforeCharges = Structured(CommonComparisonValueIds.Charges, "Charges", 1f, "charges", "1 charge", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var afterCharges = Structured(CommonComparisonValueIds.Charges, "Charges", 2f, "charges", "2 charges", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var beforeRadius = Structured(CommonComparisonValueIds.Radius, "Radius", 3f, "m", "3m", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var afterRadius = Structured(CommonComparisonValueIds.Radius, "Radius", 4.5f, "m", "4.5m", ComparisonResultClass.Exact, ComparisonConfidence.Verified);

            var cooldown = CommonComparisonEvaluators.EvaluateCooldown(beforeCooldown, afterCooldown);
            var charges = CommonComparisonEvaluators.EvaluateCharges(beforeCharges, afterCharges);
            var radius = CommonComparisonEvaluators.EvaluateRadiusAreaOrRange(beforeRadius, afterRadius);

            AssertEqual("cooldown", cooldown.MetricId, "common cooldown metric id");
            AssertApproximately(-2f, Require(cooldown.DeltaValue, "common cooldown delta"), "common cooldown delta");
            AssertApproximately(-0.25f, Require(cooldown.DeltaPercent, "common cooldown delta percent"), "common cooldown delta percent");
            AssertEqual("s", cooldown.Unit, "common cooldown unit");

            AssertEqual((int)ComparisonResultClass.Utility, (int)charges.ResultClass, "common charges utility result class");
            AssertApproximately(1f, Require(charges.DeltaValue, "common charges delta"), "common charges delta");
            AssertEqual("charges", charges.Unit, "common charges unit");

            AssertEqual((int)ComparisonResultClass.Utility, (int)radius.ResultClass, "common radius utility result class");
            AssertEqual("radius", radius.UtilityId, "common radius utility id");
            AssertApproximately(1.5f, Require(radius.DeltaValue, "common radius delta"), "common radius delta");
            AssertEqual("m", radius.Unit, "common radius unit");
        }

        private static void ValidatesCommonComparisonSimpleStatBonuses()
        {
            var beforeArmor = Structured(CommonComparisonValueIds.StatPrefix + "armor", "Armor", 40f, "", "40 Armor", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var afterArmor = Structured(CommonComparisonValueIds.StatPrefix + "armor", "Armor", 55f, "", "55 Armor", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var beforeCritChance = Structured(CommonComparisonValueIds.StatPrefix + "crit-chance", "Crit chance", 10f, "%", "10%", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var afterCritChance = Structured(CommonComparisonValueIds.StatPrefix + "crit-chance", "Crit chance", 15f, "%", "15%", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var finalBeforeAp = new BuildStatValue(CommonComparisonValueIds.AbilityPower, 160f, "", ComparisonConfidence.Verified);
            var finalAfterAp = new BuildStatValue(CommonComparisonValueIds.AbilityPower, 184f, "", ComparisonConfidence.Verified);

            var armor = CommonComparisonEvaluators.EvaluateStatBonus(beforeArmor, afterArmor);
            var critChance = CommonComparisonEvaluators.EvaluateStatBonus(beforeCritChance, afterCritChance);
            var finalAp = CommonComparisonEvaluators.EvaluateFinalStat(finalBeforeAp, finalAfterAp, "Ability Power");

            AssertEqual("stat.armor", armor.MetricId, "common flat stat metric id");
            AssertApproximately(15f, Require(armor.DeltaValue, "common flat stat delta"), "common flat stat delta");
            AssertEqual((int)ComparisonResultClass.Exact, (int)armor.ResultClass, "common flat stat result class");

            AssertEqual("stat.crit-chance", critChance.MetricId, "common percentage stat metric id");
            AssertApproximately(5f, Require(critChance.DeltaValue, "common percentage stat delta"), "common percentage stat delta");
            AssertEqual("%", critChance.Unit, "common percentage stat unit");

            AssertEqual("stat.ability-power", finalAp.MetricId, "common final stat metric id");
            AssertApproximately(24f, Require(finalAp.DeltaValue, "common final stat delta"), "common final stat delta");
            AssertApproximately(0.15f, Require(finalAp.DeltaPercent, "common final stat delta percent"), "common final stat delta percent");
        }

        private static void ValidatesCommonComparisonUnsupportedRowsDoNotFabricateZeroesOrDps()
        {
            var textOnlyDamage = Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", null, "damage", "damage varies by target", ComparisonResultClass.Unknown, ComparisonConfidence.Unknown);
            var supportedDamage = Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 40f, "damage", "40 damage", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var unsupportedArea = Structured("pull-distance", "Pull distance", 5f, "m", "5m", ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var wholeBuildDps = CommonComparisonEvaluators.WholeBuildDpsUnknown(null);

            var directDamage = CommonComparisonEvaluators.EvaluateDirectDamage(textOnlyDamage, supportedDamage);
            var range = CommonComparisonEvaluators.EvaluateRadiusAreaOrRange(unsupportedArea, unsupportedArea);

            AssertTrue(directDamage.IsUnknownOrUnsupported, "common text damage unknown");
            AssertFalse(directDamage.HasNumericProjection, "common text damage no numeric projection");
            AssertNull(directDamage.BeforeValue, "common text damage before null");
            AssertNull(directDamage.DeltaValue, "common text damage delta null");

            AssertEqual((int)ComparisonResultClass.Utility, (int)range.ResultClass, "common unsupported range utility row");
            AssertEqual((int)ComparisonConfidence.Unsupported, (int)range.Confidence, "common unsupported range confidence");
            AssertNull(range.BeforeValue, "common unsupported range before null");
            AssertNull(range.DeltaValue, "common unsupported range delta null");

            AssertEqual(CommonComparisonValueIds.WholeBuildDps, wholeBuildDps.MetricId, "common whole dps metric id");
            AssertTrue(wholeBuildDps.IsUnknownOrUnsupported, "common whole dps unknown");
            AssertFalse(wholeBuildDps.HasNumericProjection, "common whole dps no numeric projection");
            AssertNull(wholeBuildDps.AfterValue, "common whole dps after null");
            AssertNull(wholeBuildDps.DeltaPercent, "common whole dps delta percent null");
        }

        private static void ValidatesComparisonPresentationRanksMultipleKnownDamageTargets()
        {
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var replaceA = ComparisonGem("Gem_A", "Gem A");
            var replaceB = ComparisonGem("Gem_B", "Gem B");
            var replaceC = ComparisonGem("Gem_C", "Gem C");
            var options = new[]
            {
                ComparisonOption(candidate, replaceA, DamageMetric("damage-per-use", "Damage per use", 100f, 112f, ComparisonResultClass.Exact, ComparisonConfidence.Verified)),
                ComparisonOption(candidate, replaceB, DamageMetric("damage-per-use", "Damage per use", 100f, 125f, ComparisonResultClass.Exact, ComparisonConfidence.Verified)),
                ComparisonOption(candidate, replaceC, DamageMetric("damage-per-use", "Damage per use", 100f, 95f, ComparisonResultClass.Exact, ComparisonConfidence.Verified))
            };

            var view = ComparisonPresentationShell.BuildView(options);

            AssertEqual("Candidate Gem", view.CandidateLabel, "presentation candidate label");
            AssertEqual(3, view.RankedDamageOptions.Count, "presentation ranked count");
            AssertEqual("Gem B", view.RankedDamageOptions[0].ReplacementLabel, "presentation first ranked target");
            AssertEqual("Gem A", view.RankedDamageOptions[1].ReplacementLabel, "presentation second ranked target");
            AssertEqual("Gem C", view.RankedDamageOptions[2].ReplacementLabel, "presentation third ranked target");
            AssertEqual((int)ComparisonRecommendationState.Recommended, (int)view.RecommendationState, "presentation recommendation state");
            AssertEqual("Gem B", Require(view.RecommendedOption, "presentation recommended option").ReplacementLabel, "presentation recommended target");
            AssertEqual("Damage per use", view.DamageDimensionLabel, "presentation damage dimension");
            AssertEqual("+25 damage (+25%)", view.RankedDamageOptions[0].PrimaryDamageRow.DeltaText, "presentation ranked delta text");
        }

        private static void ValidatesComparisonPresentationSuppressesUnsafeRecommendations()
        {
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var known = ComparisonOption(
                candidate,
                ComparisonGem("Gem_Known", "Known Gem"),
                DamageMetric("damage-per-use", "Damage per use", 100f, 125f, ComparisonResultClass.Exact, ComparisonConfidence.Verified));
            var unknown = ComparisonOption(
                candidate,
                ComparisonGem("Gem_Unknown", "Unknown Gem"),
                ComparisonMetric.Unknown("damage-per-use", "Damage per use", new[] { "mechanic unsupported" }));
            var unsupported = ComparisonOption(
                candidate,
                ComparisonGem("Gem_Unsupported", "Unsupported Gem"),
                ComparisonMetric.Unsupported("damage-per-use", "Damage per use", new[] { "no evaluator" }));

            var mixedView = ComparisonPresentationShell.BuildView(new[] { known, unknown, unsupported });

            AssertEqual(1, mixedView.RankedDamageOptions.Count, "presentation mixed ranked count");
            AssertEqual(2, mixedView.UnrankedOptions.Count, "presentation mixed unranked count");
            AssertEqual((int)ComparisonRecommendationState.Suppressed, (int)mixedView.RecommendationState, "presentation mixed recommendation suppressed");
            AssertNull(mixedView.RecommendedOption, "presentation mixed no recommended option");
            AssertEqual((int)ComparisonPresentationMetricState.Unknown, (int)mixedView.UnrankedOptions[0].PrimaryDamageRow.State, "presentation unknown row state");
            AssertEqual((int)ComparisonPresentationMetricState.Unsupported, (int)mixedView.UnrankedOptions[1].PrimaryDamageRow.State, "presentation unsupported row state");

            var weakA = ComparisonOption(
                candidate,
                ComparisonGem("Gem_WeakA", "Weak A"),
                DamageMetric("damage-per-use", "Damage per use", 100f, 130f, ComparisonResultClass.Estimated, ComparisonConfidence.Estimated));
            var weakB = ComparisonOption(
                candidate,
                ComparisonGem("Gem_WeakB", "Weak B"),
                DamageMetric("damage-per-use", "Damage per use", 100f, 120f, ComparisonResultClass.Estimated, ComparisonConfidence.Estimated));

            var weakView = ComparisonPresentationShell.BuildView(new[] { weakA, weakB });

            AssertEqual(2, weakView.RankedDamageOptions.Count, "presentation weak ranked count");
            AssertEqual((int)ComparisonPresentationMetricState.Estimated, (int)weakView.RankedDamageOptions[0].PrimaryDamageRow.State, "presentation weak row estimated");
            AssertEqual((int)ComparisonRecommendationState.Suppressed, (int)weakView.RecommendationState, "presentation weak recommendation suppressed");
            AssertEqual("Damage confidence is too weak for a recommendation", weakView.RecommendationText, "presentation weak suppression reason");
        }

        private static void ValidatesComparisonPresentationFormatsExactDerivedAndEstimatedMetrics()
        {
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var replacement = ComparisonGem("Gem_Current", "Current Gem");
            var exact = DamageMetric("damage-per-use", "Damage per use", 100f, 125f, ComparisonResultClass.Exact, ComparisonConfidence.Verified);
            var derived = DamageMetric("damage-per-activation", "Damage per activation", 200f, 260f, ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence);
            var estimated = DamageMetric("estimated-dps", "Estimated DPS", 10f, 14f, ComparisonResultClass.Estimated, ComparisonConfidence.Estimated, "damage/s");

            var view = ComparisonPresentationShell.BuildView(new[]
            {
                ComparisonOption(candidate, replacement, exact, new[] { derived, estimated }, null, null)
            });
            var option = view.RankedDamageOptions[0];

            AssertEqual(3, option.ProjectedDamageRows.Count, "presentation projected metric count");
            AssertEqual((int)ComparisonPresentationMetricState.Known, (int)option.ProjectedDamageRows[0].State, "presentation exact row state");
            AssertEqual("100 damage", option.ProjectedDamageRows[0].BeforeText, "presentation exact before");
            AssertEqual("125 damage", option.ProjectedDamageRows[0].AfterText, "presentation exact after");
            AssertEqual("+25 damage (+25%)", option.ProjectedDamageRows[0].DeltaText, "presentation exact delta");
            AssertEqual((int)ComparisonPresentationMetricState.Known, (int)option.ProjectedDamageRows[1].State, "presentation derived row state");
            AssertEqual("+60 damage (+30%)", option.ProjectedDamageRows[1].DeltaText, "presentation derived delta");
            AssertEqual((int)ComparisonPresentationMetricState.Estimated, (int)option.ProjectedDamageRows[2].State, "presentation estimated row state");
            AssertEqual("Estimated / Estimated", option.ProjectedDamageRows[2].DetailText, "presentation estimated label");
            AssertEqual("+4 damage/s (+40%)", option.ProjectedDamageRows[2].DeltaText, "presentation estimated delta");
        }

        private static void ValidatesComparisonPresentationKeepsUtilityRowsSeparate()
        {
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var replacement = ComparisonGem("Gem_Current", "Current Gem");
            var damage = DamageMetric("damage-per-use", "Damage per use", 100f, 110f, ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence);
            var utility = new ComparisonUtilityChange("radius", "Radius", 3f, 4.5f, 1.5f, "m", ComparisonConfidence.HighConfidence, null);

            var view = ComparisonPresentationShell.BuildView(new[]
            {
                ComparisonOption(candidate, replacement, damage, null, new[] { utility }, null)
            });
            var option = view.RankedDamageOptions[0];

            AssertEqual(1, option.ProjectedDamageRows.Count, "presentation utility damage row count");
            AssertEqual("damage-per-use", option.ProjectedDamageRows[0].MetricId, "presentation utility damage metric");
            AssertEqual(1, option.UtilityRows.Count, "presentation utility row count");
            AssertEqual((int)ComparisonPresentationMetricState.Utility, (int)option.UtilityRows[0].State, "presentation utility row state");
            AssertEqual("3 m", option.UtilityRows[0].BeforeText, "presentation utility before");
            AssertEqual("4.5 m", option.UtilityRows[0].AfterText, "presentation utility after");
            AssertEqual("+1.5 m", option.UtilityRows[0].DeltaText, "presentation utility delta");
        }

        private static void ValidatesComparisonPresentationEmptyState()
        {
            var view = ComparisonPresentationShell.BuildView(null);

            AssertFalse(view.HasOptions, "presentation empty has no options");
            AssertEqual((int)ComparisonRecommendationState.Empty, (int)view.RecommendationState, "presentation empty recommendation state");
            AssertEqual("No comparison options available", view.RecommendationText, "presentation empty text");
            AssertEqual((int)ComparisonPresentationMetricState.Empty, (int)Require(view.EmptyRow, "presentation empty row").State, "presentation empty row state");
        }

        private static void ValidatesComparisonPresentationKeepsObservedContextSeparate()
        {
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var replacement = ComparisonGem("Gem_Current", "Current Gem");
            var damage = DamageMetric("damage-per-use", "Damage per use", 100f, 120f, ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence);
            var observed = new ObservedContextMetric("run-direct-damage", "Current Gem this run", 8420f, "damage", "", "run", replacement.StableId);

            var view = ComparisonPresentationShell.BuildView(new[]
            {
                ComparisonOption(candidate, replacement, damage, null, null, new[] { observed })
            });
            var option = view.RankedDamageOptions[0];

            AssertEqual(1, option.ProjectedDamageRows.Count, "presentation observed projected row count");
            AssertEqual(0, option.ObservedContextRows[0].Label.IndexOf("Current Gem", StringComparison.Ordinal), "presentation observed label");
            AssertEqual(1, option.ObservedContextRows.Count, "presentation observed row count");
            AssertEqual("8420 damage", option.ObservedContextRows[0].ValueText, "presentation observed value");
            AssertEqual("run", option.ObservedContextRows[0].Scope, "presentation observed scope");
            AssertEqual(1, option.UtilityRows.Count, "presentation observed utility empty row only");
            AssertEqual((int)ComparisonPresentationMetricState.Empty, (int)option.UtilityRows[0].State, "presentation observed empty utility");
        }

        private static void ValidatesLiveCandidateGemComparisonEvaluatesLegalTargetsIndependently()
        {
            var player = Local("local-a");
            var memoryA = Memory(player, "Mem_A");
            var memoryB = Memory(player, "Mem_B");
            var gemA = Gem(player, "Gem_A");
            var gemB = Gem(player, "Gem_B");
            var candidateKey = Gem(player, "Gem_Candidate");
            var memoryStateA = new MemoryState(memoryA, memoryA.ContentId, 0, null, 3, null, new[] { gemA }, new[] { Structured("memory-element", "Element", null, "", "Fire", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var memoryStateB = new MemoryState(memoryB, memoryB.ContentId, 1, null, 3, null, new[] { gemB }, null);
            var currentGemA = new GemState(gemA, gemA.ContentId, 100f, memoryA, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 80f, "damage", "80", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var currentGemB = new GemState(gemB, gemB.ContentId, 100f, memoryB, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 120f, "damage", "120", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var candidate = new GemState(candidateKey, candidateKey.ContentId, 250f, null, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 150f, "damage", "150", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryStateA, memoryStateB }, new[] { currentGemA, currentGemB }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(build, candidate, new[]
            {
                new LiveGemReplacementTarget(currentGemA, memoryStateA, 0),
                new LiveGemReplacementTarget(currentGemB, memoryStateB, 0)
            }, false, 2f);

            AssertEqual((int)LiveCandidateComparisonStatus.Ready, (int)snapshot.Status, "live gem ready");
            AssertEqual((int)LiveCandidateComparisonCandidateKind.Gem, (int)snapshot.CandidateKind, "live gem candidate kind");
            AssertEqual(2, snapshot.Comparisons.Count, "live gem comparison count");
            AssertEqual(2, snapshot.ContextualEvaluations.Count, "live gem contextual evaluation count");
            AssertApproximately(70f, Require(snapshot.Comparisons[0].PrimaryDamageDelta.DeltaValue, "live gem first delta"), "live gem first delta");
            AssertApproximately(30f, Require(snapshot.Comparisons[1].PrimaryDamageDelta.DeltaValue, "live gem second delta"), "live gem second delta");
            AssertTrue(snapshot.Comparisons[0].ReplacementTarget.Value.StableId != snapshot.Comparisons[1].ReplacementTarget.Value.StableId, "live gem independent replacements");
            AssertEqual(2, build.Gems.Count, "live gem original build gem count unchanged");
            AssertTrue(build.Gems[0].GemKey.Equals(gemA), "live gem first original unchanged");
            AssertTrue(build.Gems[1].GemKey.Equals(gemB), "live gem second original unchanged");
        }

        private static void ValidatesLiveCandidateGemComparisonIncludesEmptySlotAction()
        {
            var player = Local("local-empty-gem");
            var memory = Memory(player, "Mem_OpenGemSlot");
            var candidateKey = Gem(player, "Gem_Candidate");
            var memoryState = new MemoryState(
                memory,
                memory.ContentId,
                0,
                null,
                3,
                null,
                Array.Empty<GemKey>(),
                new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var candidate = new GemState(
                candidateKey,
                candidateKey.ContentId,
                100f,
                null,
                new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.5f, "multiplier", "+50%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, Array.Empty<GemState>(), null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(
                build,
                candidate,
                new[] { new LiveGemReplacementTarget(CandidateEquipActionKind.EquipIntoEmptySlot, null, memoryState, 0) },
                false,
                2f);
            var option = snapshot.Comparisons[0];

            AssertEqual((int)LiveCandidateComparisonStatus.Ready, (int)snapshot.Status, "live empty gem ready");
            AssertEqual((int)CandidateEquipActionKind.EquipIntoEmptySlot, (int)option.ActionKind, "live empty gem action kind");
            AssertNull(option.ReplacementTarget, "live empty gem removed item");
            AssertEqual(0, option.ObservedContext.Count, "live empty gem no removed observed context");
            AssertEqual("contextual-memory-damage", option.PrimaryDamageDelta.MetricId, "live empty gem contextual primary");
            AssertApproximately(50f, Require(option.PrimaryDamageDelta.DeltaValue, "live empty gem damage delta"), "live empty gem delta");
        }

        private static void ValidatesModifierGemUsesContextualMemoryComparison()
        {
            var player = Local("local-contextual-modifier");
            var memory = Memory(player, "Mem_ModifiedDamage");
            var currentGemKey = Gem(player, "Gem_CurrentModifier");
            var candidateGemKey = Gem(player, "Gem_CandidateModifier");
            var memoryState = new MemoryState(
                memory,
                memory.ContentId,
                0,
                null,
                12,
                null,
                new[] { currentGemKey },
                new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var currentGem = new GemState(
                currentGemKey,
                currentGemKey.ContentId,
                100f,
                memory,
                new[]
                {
                    Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.2f, "multiplier", "+20%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence),
                    Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", null, "", "N/A", ComparisonResultClass.NotApplicable, ComparisonConfidence.Verified)
                });
            var candidateGem = new GemState(
                candidateGemKey,
                candidateGemKey.ContentId,
                200f,
                null,
                new[]
                {
                    Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.5f, "multiplier", "+50%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence),
                    Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", null, "", "N/A", ComparisonResultClass.NotApplicable, ComparisonConfidence.Verified)
                });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { currentGem }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(
                build,
                candidateGem,
                new[] { new LiveGemReplacementTarget(currentGem, memoryState, 0) },
                false,
                2f);
            var option = snapshot.Comparisons[0];

            AssertEqual((int)LiveCandidateComparisonStatus.Ready, (int)snapshot.Status, "modifier gem contextual snapshot ready");
            AssertEqual("contextual-memory-damage", option.PrimaryDamageDelta.MetricId, "modifier gem contextual primary metric");
            AssertApproximately(120f, Require(option.PrimaryDamageDelta.BeforeValue, "modifier gem contextual before"), "modifier gem contextual before");
            AssertApproximately(150f, Require(option.PrimaryDamageDelta.AfterValue, "modifier gem contextual after"), "modifier gem contextual after");
            AssertApproximately(30f, Require(option.PrimaryDamageDelta.DeltaValue, "modifier gem contextual delta"), "modifier gem contextual delta");
            AssertHasMetric(option.Metrics, "damage-modifier", "modifier gem damage modifier row");
            AssertHasMetric(option.Metrics, "direct-damage", "modifier gem direct N/A row");
        }

        private static void ValidatesContextualGemComparisonUsesTargetParentMemory()
        {
            var player = Local("local-parent-memory");
            var memoryA = Memory(player, "Mem_LowDamage");
            var memoryB = Memory(player, "Mem_HighDamage");
            var gemA = Gem(player, "Gem_A");
            var gemB = Gem(player, "Gem_B");
            var candidateKey = Gem(player, "Gem_ContextCandidate");
            var memoryStateA = new MemoryState(memoryA, memoryA.ContentId, 0, null, 5, null, new[] { gemA }, new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 50f, "damage", "50", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var memoryStateB = new MemoryState(memoryB, memoryB.ContentId, 1, null, 5, null, new[] { gemB }, new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 200f, "damage", "200", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var currentGemA = new GemState(gemA, gemA.ContentId, 100f, memoryA, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0f, "multiplier", "0", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var currentGemB = new GemState(gemB, gemB.ContentId, 100f, memoryB, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0f, "multiplier", "0", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var candidate = new GemState(candidateKey, candidateKey.ContentId, 100f, null, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.25f, "multiplier", "+25%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryStateA, memoryStateB }, new[] { currentGemA, currentGemB }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(
                build,
                candidate,
                new[]
                {
                    new LiveGemReplacementTarget(currentGemA, memoryStateA, 0),
                    new LiveGemReplacementTarget(currentGemB, memoryStateB, 0)
                },
                false,
                2f);

            AssertApproximately(12.5f, Require(snapshot.Comparisons[0].PrimaryDamageDelta.DeltaValue, "parent memory low delta"), "parent memory low contextual delta");
            AssertApproximately(50f, Require(snapshot.Comparisons[1].PrimaryDamageDelta.DeltaValue, "parent memory high delta"), "parent memory high contextual delta");
            AssertTrue(snapshot.ContextualEvaluations[0].MemoryContext.MemoryKey.Equals(memoryA), "parent memory first evaluation context");
            AssertTrue(snapshot.ContextualEvaluations[1].MemoryContext.MemoryKey.Equals(memoryB), "parent memory second evaluation context");
        }

        private static void ValidatesDirectDamageGemRetainsDirectMetricInContextualComparison()
        {
            var player = Local("local-direct-gem");
            var memory = Memory(player, "Mem_DirectGem");
            var currentGemKey = Gem(player, "Gem_CurrentDirect");
            var candidateGemKey = Gem(player, "Gem_CandidateDirect");
            var memoryState = new MemoryState(memory, memory.ContentId, 0, null, 3, null, new[] { currentGemKey }, new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var currentGem = new GemState(currentGemKey, currentGemKey.ContentId, 100f, memory, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 20f, "damage", "20", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var candidateGem = new GemState(candidateGemKey, candidateGemKey.ContentId, 100f, null, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 35f, "damage", "35", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { currentGem }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(build, candidateGem, new[] { new LiveGemReplacementTarget(currentGem, memoryState, 0) }, false, 2f);
            var option = snapshot.Comparisons[0];

            AssertEqual("contextual-memory-damage", option.PrimaryDamageDelta.MetricId, "direct gem contextual primary retained");
            AssertApproximately(15f, Require(option.PrimaryDamageDelta.DeltaValue, "direct gem contextual delta"), "direct gem contextual delta");
            AssertHasMetric(option.Metrics, "direct-damage", "direct gem direct projected row retained");
        }

        private static void ValidatesLiveCandidateComparisonDistinguishesReplacementAndInsertion()
        {
            var player = Local("local-insert-replace");
            var memory = Memory(player, "Mem_InsertReplace");
            var currentKey = Gem(player, "Gem_Current");
            var candidateKey = Gem(player, "Gem_Candidate");
            var memoryState = new MemoryState(
                memory,
                memory.ContentId,
                0,
                null,
                3,
                null,
                new[] { currentKey },
                new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var current = new GemState(currentKey, currentKey.ContentId, 100f, memory, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.2f, "multiplier", "+20%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var candidate = new GemState(candidateKey, candidateKey.ContentId, 100f, null, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.5f, "multiplier", "+50%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { current }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(
                build,
                candidate,
                new[]
                {
                    new LiveGemReplacementTarget(current, memoryState, 0),
                    new LiveGemReplacementTarget(CandidateEquipActionKind.EquipIntoEmptySlot, null, memoryState, 1)
                },
                false,
                2f);

            AssertEqual((int)CandidateEquipActionKind.ReplaceExisting, (int)snapshot.Comparisons[0].ActionKind, "live replace action kind");
            AssertEqual(currentKey.StableId, snapshot.Comparisons[0].ReplacementTarget.Value.StableId, "live replace removed item");
            AssertEqual((int)CandidateEquipActionKind.EquipIntoEmptySlot, (int)snapshot.Comparisons[1].ActionKind, "live insert action kind");
            AssertNull(snapshot.Comparisons[1].ReplacementTarget, "live insert no removed item");
            AssertApproximately(30f, Require(snapshot.Comparisons[0].PrimaryDamageDelta.DeltaValue, "live replace delta"), "live replacement semantics delta");
            AssertApproximately(50f, Require(snapshot.Comparisons[1].PrimaryDamageDelta.DeltaValue, "live insert delta"), "live insertion semantics delta");
        }

        private static void ValidatesLiveCandidateComparisonSurfacesMaterialUtility()
        {
            var player = Local("local-utility");
            var memory = Memory(player, "Mem_Utility");
            var adventureKey = Gem(player, "Gem_Adventure");
            var candidateKey = Gem(player, "Gem_Damage");
            var memoryState = new MemoryState(memory, memory.ContentId, 0, null, 3, null, new[] { adventureKey }, new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var adventure = new GemState(
                adventureKey,
                adventureKey.ContentId,
                100f,
                memory,
                new[]
                {
                    Structured(CommonComparisonValueIds.MaterialUtilityPrefix + "adventure-essence", "Adventure Essence utility", null, "", "increase another Essence by +10% quality and gain Gold", ComparisonResultClass.Unsupported, ComparisonConfidence.Unsupported)
                });
            var candidate = new GemState(candidateKey, candidateKey.ContentId, 100f, null, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.5f, "multiplier", "+50%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { adventure }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(build, candidate, new[] { new LiveGemReplacementTarget(adventure, memoryState, 0) }, false, 2f);
            var view = ComparisonPresentationShell.BuildView(snapshot.Comparisons);

            AssertHasPresentationState(view.RankedDamageOptions[0].UtilityRows, ComparisonPresentationMetricState.Unsupported, "material utility unsupported visible");
            AssertTrue(view.RankedDamageOptions[0].UtilityRows[0].Label.IndexOf("Adventure", StringComparison.OrdinalIgnoreCase) >= 0, "material utility label");
        }

        private static void ValidatesMaterialUtilitySuppressesOverallRecommendation()
        {
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var replaceA = ComparisonGem("Gem_A", "Gem A");
            var replaceB = ComparisonGem("Gem_B", "Gem B");
            var unsafeUtility = new ComparisonUtilityChange(
                CommonComparisonValueIds.MaterialUtilityPrefix + "adventure-essence",
                "Lose Adventure Essence quality propagation",
                null,
                null,
                null,
                "",
                ComparisonResultClass.Unsupported,
                ComparisonConfidence.Unsupported,
                new[] { "material utility is removed and unsupported" });
            var safe = ComparisonOption(candidate, replaceA, DamageMetric("damage-per-use", "Damage per use", 100f, 110f, ComparisonResultClass.Exact, ComparisonConfidence.Verified));
            var unsafeOption = ComparisonOption(
                candidate,
                replaceB,
                DamageMetric("damage-per-use", "Damage per use", 100f, 150f, ComparisonResultClass.Exact, ComparisonConfidence.Verified),
                null,
                new[] { unsafeUtility },
                null);

            var view = ComparisonPresentationShell.BuildView(new[] { safe, unsafeOption });

            AssertEqual(2, view.RankedDamageOptions.Count, "material utility ranked damage still visible");
            AssertEqual((int)ComparisonRecommendationState.Suppressed, (int)view.RecommendationState, "material utility suppresses recommendation");
            AssertNull(view.RecommendedOption, "material utility no overall recommendation");
        }

        private static void ValidatesBestKnownDamageStaysDistinctFromBestReplacement()
        {
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var replace = ComparisonGem("Gem_A", "Gem A");
            var option = ComparisonOption(candidate, replace, DamageMetric("damage-per-use", "Damage per use", 100f, 130f, ComparisonResultClass.Exact, ComparisonConfidence.Verified));

            var view = ComparisonPresentationShell.BuildView(new[] { option });

            AssertEqual((int)ComparisonRecommendationState.Recommended, (int)view.RecommendationState, "best known damage recommendation state");
            AssertEqual("Best known damage improvement", view.RecommendationText, "best known damage recommendation label");
            AssertFalse(string.Equals("Best replacement", view.RecommendationText, StringComparison.Ordinal), "best known damage not best replacement");
        }

        private static void ValidatesObservedPackageContextDoesNotAffectGemRanking()
        {
            var player = Local("local-observed-ranking");
            var memoryLow = Memory(player, "Mem_LowObserved");
            var memoryHigh = Memory(player, "Mem_HighObserved");
            var gemLow = Gem(player, "Gem_LowObserved");
            var gemHigh = Gem(player, "Gem_HighObserved");
            var candidateKey = Gem(player, "Gem_ContextualObserved");
            var lowMemory = new MemoryState(memoryLow, memoryLow.ContentId, 0, null, 5, null, new[] { gemLow }, new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var highMemory = new MemoryState(memoryHigh, memoryHigh.ContentId, 1, null, 5, null, new[] { gemHigh }, new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Memory damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var lowGem = new GemState(gemLow, gemLow.ContentId, 100f, memoryLow, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0f, "multiplier", "0", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var highGem = new GemState(gemHigh, gemHigh.ContentId, 100f, memoryHigh, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.1f, "multiplier", "+10%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var candidate = new GemState(candidateKey, candidateKey.ContentId, 100f, null, new[] { Structured(CommonComparisonValueIds.DamageModifier, "Damage modifier", 0.5f, "multiplier", "+50%", ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { lowMemory, highMemory }, new[] { lowGem, highGem }, null, 1f);
            var run = RunSnapshotWithObservedRows(player, memoryHigh, gemHigh, 100f, 0f, 10000f, 10000f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(
                build,
                candidate,
                new[]
                {
                    new LiveGemReplacementTarget(lowGem, lowMemory, 0),
                    new LiveGemReplacementTarget(highGem, highMemory, 0)
                },
                false,
                2f,
                run);
            var view = ComparisonPresentationShell.BuildView(snapshot.Comparisons);

            AssertEqual(gemLow.StableId, view.RankedDamageOptions[0].Comparison.ReplacementTarget.Value.StableId, "observed package not used for ranking");
            AssertApproximately(50f, Require(snapshot.Comparisons[0].PrimaryDamageDelta.DeltaValue, "observed ranking low delta"), "observed ranking low projected delta");
            AssertApproximately(40f, Require(snapshot.Comparisons[1].PrimaryDamageDelta.DeltaValue, "observed ranking high delta"), "observed ranking high projected delta");
            AssertEqual(0, snapshot.Comparisons[0].ObservedContext.Count, "observed ranking low no context");
            AssertEqual(2, snapshot.Comparisons[1].ObservedContext.Count, "observed ranking high context separate");
            AssertEqual("observed.current-memory-package", snapshot.Comparisons[1].ObservedContext[1].MetricId, "observed ranking package context separate");
        }

        private static void ValidatesLiveCandidateComparisonAddsObservedRunContextFromSnapshotsOnly()
        {
            var player = Local("local-observed");
            var memoryKey = Memory(player, "Mem_Observed");
            var currentGemKey = Gem(player, "Gem_Observed");
            var candidateKey = Gem(player, "Gem_Candidate");
            var memoryState = new MemoryState(memoryKey, memoryKey.ContentId, 0, null, 2, null, new[] { currentGemKey }, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var currentGem = new GemState(currentGemKey, currentGemKey.ContentId, 100f, memoryKey, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 80f, "damage", "80", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var candidate = new GemState(candidateKey, candidateKey.ContentId, 150f, null, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 120f, "damage", "120", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { currentGem }, null, 1f);
            var run = RunSnapshotWithObservedRows(
                player,
                memoryKey,
                currentGemKey,
                memoryDirectDamage: 300f,
                gemDirectDamage: 120f,
                packageTotalDamage: 420f,
                playerDamage: 600f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(
                build,
                candidate,
                new[] { new LiveGemReplacementTarget(currentGem, memoryState, 0) },
                false,
                2f,
                run);
            var option = snapshot.Comparisons[0];
            var view = ComparisonPresentationShell.BuildView(snapshot.Comparisons);

            AssertEqual(2, option.ObservedContext.Count, "live observed gem context count");
            AssertEqual("observed.current-gem-direct", option.ObservedContext[0].MetricId, "live observed gem direct id");
            AssertEqual("120 damage (20% run)", option.ObservedContext[0].TextValue, "live observed gem direct text");
            AssertEqual("observed.current-memory-package", option.ObservedContext[1].MetricId, "live observed memory package id");
            AssertEqual("420 damage (70% run)", option.ObservedContext[1].TextValue, "live observed memory package text");
            AssertEqual(1, view.RankedDamageOptions.Count, "live observed ranking unchanged");
            AssertEqual(2, view.RankedDamageOptions[0].ObservedContextRows.Count, "live observed presentation count");
            AssertHasPresentationMetric(view.RankedDamageOptions[0].ProjectedDamageRows, "contextual-memory-damage", "live observed contextual projected row");
            AssertHasPresentationMetric(view.RankedDamageOptions[0].ProjectedDamageRows, "direct-damage", "live observed direct projected row");
        }

        private static void ValidatesLiveCandidateComparisonOmitsMissingObservedHistory()
        {
            var player = Local("local-missing-observed");
            var memoryKey = Memory(player, "Mem_MissingObserved");
            var candidateMemoryKey = Memory(player, "Mem_CandidateObserved");
            var currentMemory = new MemoryState(memoryKey, memoryKey.ContentId, 0, null, 2, null, null, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var candidateMemory = new MemoryState(candidateMemoryKey, candidateMemoryKey.ContentId, -1, null, 2, null, null, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 130f, "damage", "130", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { currentMemory }, null, null, 1f);
            var run = RunSnapshotWithObservedRows(
                player,
                Memory(player, "Mem_OtherObserved"),
                Gem(player, "Gem_OtherObserved"),
                memoryDirectDamage: 10f,
                gemDirectDamage: 20f,
                packageTotalDamage: 30f,
                playerDamage: 100f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshMemoryCandidate(
                build,
                candidateMemory,
                new[] { new LiveMemoryReplacementTarget(currentMemory) },
                2f,
                run);

            AssertEqual(0, snapshot.Comparisons[0].ObservedContext.Count, "live missing observed context omitted");
            AssertApproximately(30f, Require(snapshot.Comparisons[0].PrimaryDamageDelta.DeltaValue, "live missing observed projected delta"), "live missing observed projected delta");
        }

        private static void ValidatesLiveCandidateComparisonPreservesNotApplicableAndUnknownRows()
        {
            var player = Local("local-fidelity");
            var memoryKey = Memory(player, "Mem_Fidelity");
            var currentGemKey = Gem(player, "Gem_CurrentFidelity");
            var candidateGemKey = Gem(player, "Gem_CandidateFidelity");
            var memoryState = new MemoryState(memoryKey, memoryKey.ContentId, 0, null, 2, null, new[] { currentGemKey }, null);
            var currentGem = new GemState(
                currentGemKey,
                currentGemKey.ContentId,
                100f,
                memoryKey,
                new[]
                {
                    Structured(CommonComparisonValueIds.Cooldown, "Cooldown", 0f, "s", "0", ComparisonResultClass.Exact, ComparisonConfidence.Verified),
                    Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", null, "", "N/A", ComparisonResultClass.NotApplicable, ComparisonConfidence.Verified)
                });
            var candidateGem = new GemState(
                candidateGemKey,
                candidateGemKey.ContentId,
                110f,
                null,
                new[]
                {
                    Structured(CommonComparisonValueIds.Cooldown, "Cooldown", null, "s", "unknown", ComparisonResultClass.Unknown, ComparisonConfidence.Unknown),
                    Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", null, "", "N/A", ComparisonResultClass.NotApplicable, ComparisonConfidence.Verified)
                });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { currentGem }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshGemCandidate(
                build,
                candidateGem,
                new[] { new LiveGemReplacementTarget(currentGem, memoryState, 0) },
                false,
                2f);
            var option = snapshot.Comparisons[0];
            var view = ComparisonPresentationShell.BuildView(snapshot.Comparisons);

            AssertEqual((int)LiveCandidateComparisonStatus.Ready, (int)snapshot.Status, "live fidelity snapshot ready");
            AssertTrue(option.Metrics.Count >= 2, "live fidelity keeps non-numeric metric rows");
            AssertHasMetricResultClass(option.Metrics, "direct-damage", ComparisonResultClass.NotApplicable, "live fidelity direct damage not applicable");
            AssertHasMetricResultClass(option.Metrics, "cooldown", ComparisonResultClass.Unknown, "live fidelity cooldown unknown");
            AssertEqual(0, view.RankedDamageOptions.Count, "live fidelity no false damage ranking");
            AssertEqual(1, view.UnrankedOptions.Count, "live fidelity visible unranked option");
            AssertHasPresentationState(view.UnrankedOptions[0].ProjectedDamageRows, ComparisonPresentationMetricState.NotApplicable, "live fidelity not-applicable row visible");
            AssertHasPresentationState(view.UnrankedOptions[0].ProjectedDamageRows, ComparisonPresentationMetricState.Unknown, "live fidelity unknown row visible");
            AssertNoKnownZeroDamageRow(view.UnrankedOptions[0].ProjectedDamageRows, "live fidelity no false known zero damage row");
        }

        private static void ValidatesChargeRegressionUsesNativeDisplayedCap()
        {
            var sylvanCall = CommonComparisonEvaluators.EvaluateCharges(
                Structured(CommonComparisonValueIds.Charges, "Charges", 3f, "charges", "3", ComparisonResultClass.Exact, ComparisonConfidence.Verified),
                Structured(CommonComparisonValueIds.Charges, "Charges", 3f, "charges", "3", ComparisonResultClass.Exact, ComparisonConfidence.Verified));
            var pressurePoint = CommonComparisonEvaluators.EvaluateCharges(
                Structured(CommonComparisonValueIds.Charges, "Charges", 2f, "charges", "2", ComparisonResultClass.Exact, ComparisonConfidence.Verified),
                Structured(CommonComparisonValueIds.Charges, "Charges", 3f, "charges", "3", ComparisonResultClass.Exact, ComparisonConfidence.Verified));

            AssertApproximately(3f, Require(sylvanCall.BeforeValue, "charge regression SylvanCall before"), "charge regression SylvanCall maxCharges");
            AssertApproximately(0f, Require(sylvanCall.DeltaValue, "charge regression SylvanCall delta"), "charge regression unchanged charge delta");
            AssertApproximately(2f, Require(pressurePoint.BeforeValue, "charge regression PressurePoint before"), "charge regression PressurePoint observed maxCharges");
            AssertApproximately(1f, Require(pressurePoint.DeltaValue, "charge regression displayed cap delta"), "charge regression displayed cap delta");
        }

        private static void ValidatesLiveCandidateMemoryComparisonKeepsSlotGemContext()
        {
            var player = Local("local-a");
            var currentMemoryKey = Memory(player, "Mem_Current");
            var candidateMemoryKey = Memory(player, "Mem_Candidate");
            var gemKey = Gem(player, "Gem_Attached");
            var currentMemory = new MemoryState(currentMemoryKey, currentMemoryKey.ContentId, 0, null, 2, null, new[] { gemKey }, new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Damage per hit", 100f, "damage", "100", ComparisonResultClass.Exact, ComparisonConfidence.Verified), Structured(CommonComparisonValueIds.HitCount, "Hit count", 2f, "hits", "2", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var candidateMemory = new MemoryState(candidateMemoryKey, candidateMemoryKey.ContentId, -1, null, 4, null, null, new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Damage per hit", 130f, "damage", "130", ComparisonResultClass.Exact, ComparisonConfidence.Verified), Structured(CommonComparisonValueIds.HitCount, "Hit count", 2f, "hits", "2", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var attachedGem = new GemState(gemKey, gemKey.ContentId, 100f, currentMemoryKey, new[] { Structured(CommonComparisonValueIds.Radius, "Radius", 3f, "m", "3m", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { currentMemory }, new[] { attachedGem }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshMemoryCandidate(build, candidateMemory, new[] { new LiveMemoryReplacementTarget(currentMemory) }, 2f);

            AssertEqual((int)LiveCandidateComparisonStatus.Ready, (int)snapshot.Status, "live memory ready");
            AssertEqual((int)LiveCandidateComparisonCandidateKind.Memory, (int)snapshot.CandidateKind, "live memory candidate kind");
            AssertEqual(1, snapshot.Comparisons.Count, "live memory comparison count");
            AssertEqual(1, snapshot.ContextualEvaluations.Count, "live memory contextual evaluation count");
            AssertEqual(1, snapshot.ContextualEvaluations[0].AttachedGemContext.Count, "live memory contextual attached gem count");
            AssertEqual(currentMemoryKey.StableId, snapshot.Comparisons[0].ReplacementTarget.Value.StableId, "live memory replacement target");
            AssertEqual("damage-per-activation", snapshot.Comparisons[0].PrimaryDamageDelta.MetricId, "live memory activation metric");
            AssertApproximately(60f, Require(snapshot.Comparisons[0].PrimaryDamageDelta.DeltaValue, "live memory activation delta"), "live memory activation delta");
            AssertEqual(1, currentMemory.AttachedGemKeys.Count, "live memory original attached gem context unchanged");
        }

        private static void ValidatesLiveCandidateMemoryComparisonIncludesEmptySlotAction()
        {
            var player = Local("local-empty-memory");
            var candidateMemoryKey = Memory(player, "Mem_Candidate");
            var candidateMemory = new MemoryState(
                candidateMemoryKey,
                candidateMemoryKey.ContentId,
                -1,
                null,
                4,
                null,
                null,
                new[] { Structured(CommonComparisonValueIds.DamagePerHit, "Damage per hit", 130f, "damage", "130", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, Array.Empty<MemoryState>(), Array.Empty<GemState>(), null, 1f);
            var service = new LiveCandidateComparisonService();

            var snapshot = service.RefreshMemoryCandidate(
                build,
                candidateMemory,
                new[] { new LiveMemoryReplacementTarget(CandidateEquipActionKind.EquipIntoEmptySlot, null, 2) },
                2f);
            var option = snapshot.Comparisons[0];

            AssertEqual((int)LiveCandidateComparisonStatus.Ready, (int)snapshot.Status, "live empty memory ready");
            AssertEqual((int)CandidateEquipActionKind.EquipIntoEmptySlot, (int)option.ActionKind, "live empty memory action kind");
            AssertEqual(2, option.TargetSlot, "live empty memory target slot");
            AssertNull(option.ReplacementTarget, "live empty memory removed item");
            AssertEqual(0, option.ObservedContext.Count, "live empty memory no removed observed context");
            AssertEqual("direct-damage", option.PrimaryDamageDelta.MetricId, "live empty memory primary damage");
            AssertApproximately(130f, Require(option.PrimaryDamageDelta.DeltaValue, "live empty memory delta"), "live empty memory damage delta");
            AssertEqual(0, snapshot.ContextualEvaluations[0].AttachedGemContext.Count, "live empty memory no attached gem context");
        }

        private static void ValidatesLiveCandidateComparisonLifecycleClearReplaceAndDuplicateFallback()
        {
            var player = Local("local-a");
            var memory = Memory(player, "Mem_A");
            var gemA = Gem(player, "Gem_A");
            var gemB = Gem(player, "Gem_B");
            var memoryState = new MemoryState(memory, memory.ContentId, 0, null, 3, null, new[] { gemA }, null);
            var currentGem = new GemState(gemA, gemA.ContentId, 100f, memory, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 80f, "damage", "80", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var candidate = new GemState(gemB, gemB.ContentId, 120f, null, new[] { Structured(CommonComparisonValueIds.DirectDamage, "Direct damage", 90f, "damage", "90", ComparisonResultClass.Exact, ComparisonConfidence.Verified) });
            var build = new BuildStateSnapshot(player, "Traveler_Primus", null, new[] { memoryState }, new[] { currentGem }, null, 1f);
            var service = new LiveCandidateComparisonService();

            var ready = service.RefreshGemCandidate(build, candidate, new[] { new LiveGemReplacementTarget(currentGem, memoryState, 0) }, false, 2f);
            var cleared = service.Clear("cancel", 3f);
            var duplicate = service.RefreshGemCandidate(build, candidate, new[] { new LiveGemReplacementTarget(currentGem, memoryState, 0) }, true, 4f);
            var replaced = service.RefreshMemoryCandidate(build, new MemoryState(Memory(player, "Mem_New"), "Mem_New", -1, null, 1, null, null, null), new[] { new LiveMemoryReplacementTarget(memoryState) }, 5f);

            AssertEqual((int)LiveCandidateComparisonStatus.Ready, (int)ready.Status, "live lifecycle ready");
            AssertEqual((int)LiveCandidateComparisonStatus.Empty, (int)cleared.Status, "live lifecycle cleared");
            AssertEqual(0, cleared.Comparisons.Count, "live lifecycle clear comparisons");
            AssertEqual(0, cleared.ContextualEvaluations.Count, "live lifecycle clear evaluations");
            AssertEqual((int)LiveCandidateComparisonStatus.Unsupported, (int)duplicate.Status, "live duplicate unsupported");
            AssertEqual(1, duplicate.Comparisons.Count, "live duplicate unsupported action option");
            AssertEqual((int)CandidateEquipActionKind.UnsupportedDuplicateMerge, (int)duplicate.Comparisons[0].ActionKind, "live duplicate unsupported action kind");
            AssertEqual(0, duplicate.ContextualEvaluations.Count, "live duplicate no contextual evaluations");
            AssertEqual((int)LiveCandidateComparisonStatus.Ready, (int)replaced.Status, "live replacement refreshed");
            AssertEqual((int)LiveCandidateComparisonCandidateKind.Memory, (int)replaced.CandidateKind, "live replacement candidate kind");
            AssertTrue(ready.SequenceId < cleared.SequenceId && cleared.SequenceId < duplicate.SequenceId && duplicate.SequenceId < replaced.SequenceId, "live lifecycle sequence increments");
        }

        private static void ValidatesLiveComparisonPanelRendersSupportedGemAndMemoryTargets()
        {
            var player = Local("local-panel");
            var gemKey = Gem(player, "Gem_Candidate");
            var gemCandidate = new GemState(gemKey, gemKey.ContentId, 200f, null, null);
            var gemOption = ComparisonOption(
                ComparisonGem("Gem_Candidate", "Candidate Gem"),
                ComparisonGem("Gem_Target", "Target Gem"),
                DamageMetric("damage-per-use", "Damage per use", 100f, 125f, ComparisonResultClass.Exact, ComparisonConfidence.Verified),
                null,
                new[] { new ComparisonUtilityChange("range", "Range", 3f, 4f, 1f, "m", ComparisonConfidence.Verified, null) },
                null);
            var gemSnapshot = new LiveCandidateComparisonSnapshot(
                10,
                LiveCandidateComparisonStatus.Ready,
                LiveCandidateComparisonCandidateKind.Gem,
                "Gem candidate comparison ready",
                null,
                gemCandidate,
                null,
                new[] { gemOption },
                null,
                1f);

            var gemView = Require(LiveCandidateComparisonPanelPresenter.BuildView(gemSnapshot), "live panel Gem view");
            AssertEqual("Gem", gemView.CandidateKindLabel, "live panel Gem kind");
            AssertEqual("Candidate Gem", gemView.CandidateLabel, "live panel Gem candidate");
            AssertEqual(1, gemView.Options.Count, "live panel Gem targets");
            AssertEqual("Target Gem", gemView.Options[0].ReplacementLabel, "live panel Gem target label");
            AssertTrue(gemView.HasRecommendation, "live panel safe Gem recommendation");
            AssertEqual(1, gemView.Options[0].UtilityRows.Count, "live panel Gem utility rows");

            var memoryKey = Memory(player, "Mem_Candidate");
            var memoryCandidate = new MemoryState(memoryKey, memoryKey.ContentId, -1, null, 2, null, null, null);
            var memorySubject = new ComparisonSubject(ComparisonSubjectKind.Memory, "Mem_Candidate", "Mem_Candidate", "Candidate Memory");
            var memoryTarget = new ComparisonSubject(ComparisonSubjectKind.Memory, "Mem_Target", "Mem_Target", "Target Memory");
            var memorySnapshot = new LiveCandidateComparisonSnapshot(
                11,
                LiveCandidateComparisonStatus.Ready,
                LiveCandidateComparisonCandidateKind.Memory,
                "Memory candidate comparison ready",
                null,
                null,
                memoryCandidate,
                new[] { ComparisonOption(memorySubject, memoryTarget, DamageMetric("damage-per-activation", "Damage per activation", 200f, 260f, ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence)) },
                null,
                2f);

            var memoryView = Require(LiveCandidateComparisonPanelPresenter.BuildView(memorySnapshot), "live panel Memory view");
            AssertEqual("Memory", memoryView.CandidateKindLabel, "live panel Memory kind");
            AssertEqual("Candidate Memory", memoryView.CandidateLabel, "live panel Memory candidate");
            AssertEqual("Target Memory", memoryView.Options[0].ReplacementLabel, "live panel Memory target label");
        }

        private static void ValidatesLiveComparisonPanelPreservesUnknownAndDuplicateUnsupportedStates()
        {
            var player = Local("local-panel");
            var gemKey = Gem(player, "Gem_Candidate");
            var candidate = new GemState(gemKey, gemKey.ContentId, 100f, null, null);
            var known = ComparisonOption(
                ComparisonGem("Gem_Candidate", "Candidate Gem"),
                ComparisonGem("Gem_Known", "Known Gem"),
                DamageMetric("damage-per-use", "Damage per use", 100f, 110f, ComparisonResultClass.Exact, ComparisonConfidence.Verified));
            var unknown = ComparisonOption(
                ComparisonGem("Gem_Candidate", "Candidate Gem"),
                ComparisonGem("Gem_Unknown", "Unknown Gem"),
                ComparisonMetric.Unknown("damage-per-use", "Damage per use", new[] { "unsupported mechanic" }));
            var mixedSnapshot = new LiveCandidateComparisonSnapshot(
                20,
                LiveCandidateComparisonStatus.Ready,
                LiveCandidateComparisonCandidateKind.Gem,
                "Gem candidate comparison ready",
                null,
                candidate,
                null,
                new[] { known, unknown },
                null,
                1f);

            var mixedView = Require(LiveCandidateComparisonPanelPresenter.BuildView(mixedSnapshot), "live panel mixed view");
            AssertFalse(mixedView.HasRecommendation, "live panel mixed recommendation suppressed");
            AssertEqual(2, mixedView.Options.Count, "live panel mixed target count");
            AssertEqual((int)ComparisonPresentationMetricState.Unknown, (int)mixedView.Options[1].PrimaryDamageRow.State, "live panel unknown visible");
            AssertTrue(LiveCandidateComparisonPanelPresenter.FormatMetricRow(mixedView.Options[1].PrimaryDamageRow).IndexOf("Unknown", StringComparison.Ordinal) >= 0, "live panel unknown text");

            var duplicateSnapshot = new LiveCandidateComparisonSnapshot(
                21,
                LiveCandidateComparisonStatus.Unsupported,
                LiveCandidateComparisonCandidateKind.Gem,
                "duplicate Gem merge comparison is unavailable",
                null,
                candidate,
                null,
                null,
                null,
                2f);
            var duplicateView = Require(LiveCandidateComparisonPanelPresenter.BuildView(duplicateSnapshot), "live panel duplicate view");
            AssertEqual((int)LiveCandidateComparisonStatus.Unsupported, (int)duplicateView.Status, "live panel duplicate unsupported");
            AssertFalse(duplicateView.HasRecommendation, "live panel duplicate no recommendation");
            AssertEqual(0, duplicateView.Options.Count, "live panel duplicate no targets");
        }

        private static void ValidatesLiveComparisonPanelClearsAndRefreshesBySequence()
        {
            var empty = new LiveCandidateComparisonSnapshot(
                30,
                LiveCandidateComparisonStatus.Empty,
                LiveCandidateComparisonCandidateKind.Unknown,
                "cancelled",
                null,
                null,
                null,
                null,
                null,
                1f);
            AssertNull(LiveCandidateComparisonPanelPresenter.BuildView(empty), "live panel clears empty snapshot");

            var player = Local("local-panel");
            var firstKey = Gem(player, "Gem_First");
            var secondKey = Gem(player, "Gem_Second");
            var first = new LiveCandidateComparisonSnapshot(
                31,
                LiveCandidateComparisonStatus.Unsupported,
                LiveCandidateComparisonCandidateKind.Gem,
                "first",
                null,
                new GemState(firstKey, firstKey.ContentId, 100f, null, null),
                null,
                null,
                null,
                2f);
            var second = new LiveCandidateComparisonSnapshot(
                32,
                LiveCandidateComparisonStatus.Unsupported,
                LiveCandidateComparisonCandidateKind.Gem,
                "second",
                null,
                new GemState(secondKey, secondKey.ContentId, 100f, null, null),
                null,
                null,
                null,
                3f);

            var firstView = Require(LiveCandidateComparisonPanelPresenter.BuildView(first), "live panel first candidate");
            var secondView = Require(LiveCandidateComparisonPanelPresenter.BuildView(second), "live panel second candidate");
            AssertTrue(firstView.SequenceId < secondView.SequenceId, "live panel sequence refresh");
            AssertTrue(!string.Equals(firstView.CandidateLabel, secondView.CandidateLabel, StringComparison.Ordinal), "live panel candidate replacement refresh");
        }

        private static void ValidatesLiveComparisonPanelLayoutBoundsAndHitTesting()
        {
            var player = Local("local-panel");
            var candidate = ComparisonGem("Gem_Candidate", "Candidate Gem");
            var options = new List<BuildOptionComparison>();
            for (var i = 0; i < 6; i++)
            {
                options.Add(ComparisonOption(
                    candidate,
                    ComparisonGem("Gem_Target_" + i.ToString(), "Target Gem " + i.ToString()),
                    DamageMetric("damage-per-use", "Damage per use", 100f, 101f + i, ComparisonResultClass.Exact, ComparisonConfidence.Verified)));
            }

            var gemKey = Gem(player, "Gem_Candidate");
            var snapshot = new LiveCandidateComparisonSnapshot(
                40,
                LiveCandidateComparisonStatus.Ready,
                LiveCandidateComparisonCandidateKind.Gem,
                "Gem candidate comparison ready",
                null,
                new GemState(gemKey, gemKey.ContentId, 100f, null, null),
                null,
                options,
                null,
                1f);
            var view = Require(LiveCandidateComparisonPanelPresenter.BuildView(snapshot), "live panel layout view");
            var comparisonRect = LiveCandidateComparisonPanel.CalculatePanelRect(1366f, 768f, view);
            var leftLayout = DamageAnalyticsUiInput.CalculateMidLeftLayout(
                1366f,
                768f,
                DamageAnalyticsEncounterPanel.PanelWidthForLayout,
                320f,
                DamageAnalyticsRunPanel.PanelWidthForLayout,
                360f);

            AssertTrue(comparisonRect.x >= 1366f - LiveCandidateComparisonPanel.PanelWidth - DamageAnalyticsUiInput.PanelMargin - Tolerance, "live panel top-right x");
            AssertTrue(comparisonRect.y >= DamageAnalyticsUiInput.PanelMargin, "live panel top margin");
            AssertTrue(comparisonRect.xMax <= 1366f - DamageAnalyticsUiInput.PanelMargin + Tolerance, "live panel right bound");
            AssertTrue(comparisonRect.yMax <= 768f - DamageAnalyticsUiInput.PanelMargin + Tolerance, "live panel bottom bound");
            AssertTrue(!comparisonRect.Overlaps(leftLayout.EncounterRect), "live panel avoids encounter panel");
            AssertTrue(!comparisonRect.Overlaps(leftLayout.RunRect), "live panel avoids run panel");

            DamageAnalyticsUiInput.RegisterPanelRect(DamageAnalyticsPanelKind.Comparison, comparisonRect);
            AssertTrue(
                DamageAnalyticsUiInput.IsGuiPointOverModPanel(new UnityEngine.Vector2(comparisonRect.x + 1f, comparisonRect.y + 1f)),
                "live panel hit test inside comparison rect");
            DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Comparison);
            AssertFalse(
                DamageAnalyticsUiInput.IsGuiPointOverModPanel(new UnityEngine.Vector2(comparisonRect.x + 1f, comparisonRect.y + 1f)),
                "live panel hit test clears comparison rect");
        }

        private static void ValidatesLiveComparisonPanelLongNamesAndOverflowPresentation()
        {
            var longName = "Candidate With An Extremely Long Runtime Generated Name That Would Otherwise Clip The Panel";
            var trimmed = LiveCandidateComparisonPanelPresenter.TrimForPanel(longName, 32);
            AssertEqual(32, trimmed.Length, "live panel trimmed length");
            AssertTrue(trimmed.EndsWith("...", StringComparison.Ordinal), "live panel trimmed ellipsis");

            var candidate = ComparisonGem("Gem_Candidate_Long", longName);
            var replacement = ComparisonGem("Gem_Target_Long", "Replacement Target With A Long Display Name");
            var metrics = new[]
            {
                DamageMetric("damage-per-use", "Damage per use", 100f, 140f, ComparisonResultClass.Exact, ComparisonConfidence.Verified),
                DamageMetric("damage-per-hit", "Damage per hit", 10f, 14f, ComparisonResultClass.Exact, ComparisonConfidence.Verified),
                DamageMetric("activation-damage", "Activation damage", 50f, 70f, ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence),
                DamageMetric("bonus-damage", "Bonus damage", 5f, 6f, ComparisonResultClass.Derived, ComparisonConfidence.HighConfidence)
            };
            var utility = new[]
            {
                new ComparisonUtilityChange("range", "Range", 3f, 4f, 1f, "m", ComparisonConfidence.Verified, null),
                new ComparisonUtilityChange("charges", "Charges", 1f, 2f, 1f, "charges", ComparisonConfidence.Verified, null),
                new ComparisonUtilityChange("radius", "Radius", 2f, 3f, 1f, "m", ComparisonConfidence.Verified, null)
            };
            var option = ComparisonOption(candidate, replacement, metrics[0], metrics, utility, null);
            var snapshot = new LiveCandidateComparisonSnapshot(
                41,
                LiveCandidateComparisonStatus.Ready,
                LiveCandidateComparisonCandidateKind.Gem,
                "Gem candidate comparison ready",
                null,
                new GemState(Gem(Local("local-panel"), "Gem_Candidate_Long"), "Gem_Candidate_Long", 100f, null, null),
                null,
                new[] { option },
                null,
                1f);
            var view = Require(LiveCandidateComparisonPanelPresenter.BuildView(snapshot), "live panel overflow view");
            var height = LiveCandidateComparisonPanel.CalculateHeight(view);

            AssertTrue(height > 0f, "live panel overflow height");
            AssertTrue(view.Options[0].ProjectedDamageRows.Count > 3, "live panel projected overflow available");
            AssertTrue(view.Options[0].UtilityRows.Count > 2, "live panel utility overflow available");
        }

        private static void ValidatesLiveComparisonPanelStateLabelsSeparateUnsupportedRows()
        {
            AssertEqual("Damage", LiveCandidateComparisonPanelPresenter.FormatStateLabel(ComparisonPresentationMetricState.Known), "live panel known state label");
            AssertEqual("Estimated", LiveCandidateComparisonPanelPresenter.FormatStateLabel(ComparisonPresentationMetricState.Estimated), "live panel estimated state label");
            AssertEqual("Utility", LiveCandidateComparisonPanelPresenter.FormatStateLabel(ComparisonPresentationMetricState.Utility), "live panel utility state label");
            AssertEqual("N/A", LiveCandidateComparisonPanelPresenter.FormatStateLabel(ComparisonPresentationMetricState.NotApplicable), "live panel not-applicable state label");
            AssertEqual("Unknown", LiveCandidateComparisonPanelPresenter.FormatStateLabel(ComparisonPresentationMetricState.Unknown), "live panel unknown state label");
            AssertEqual("Unsupported", LiveCandidateComparisonPanelPresenter.FormatStateLabel(ComparisonPresentationMetricState.Unsupported), "live panel unsupported state label");

            var unsupported = ComparisonMetric.Unsupported("damage-per-use", "Damage per use", new[] { "unsupported mechanic" });
            var row = LiveCandidateComparisonPanelPresenter.FormatMetricRow(
                new ComparisonPresentationMetricRow(
                    ComparisonPresentationMetricState.Unsupported,
                    unsupported.MetricId,
                    unsupported.Label,
                    "",
                    "",
                    "",
                    "Unsupported",
                    "Unsupported",
                    unsupported.ResultClass,
                    unsupported.Confidence,
                    unsupported.Limitations));
            AssertTrue(row.IndexOf("Unsupported", StringComparison.Ordinal) >= 0, "live panel unsupported row text");
        }

        private static PlayerKey Local(string stableId)
        {
            return new PlayerKey(stableId, true);
        }

        private sealed class MemoryBuildTooltipProbe
        {
        }

        private static PlayerKey Partner(string stableId)
        {
            return new PlayerKey(stableId, false);
        }

        private static SourceKey Source(DamageSourceCategory category, string stableId)
        {
            return new SourceKey(category, stableId, stableId);
        }

        private static MemoryKey Memory(PlayerKey owner, string contentId)
        {
            return new MemoryKey(owner, contentId, contentId);
        }

        private static GemKey Gem(PlayerKey owner, string contentId)
        {
            return new GemKey(owner, contentId, contentId);
        }

        private static ComparisonStructuredValue Structured(
            string valueId,
            string label,
            float? numericValue,
            string unit,
            string textValue,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence)
        {
            return new ComparisonStructuredValue(valueId, label, numericValue, unit, textValue, resultClass, confidence, null);
        }

        private static ComparisonSubject ComparisonGem(string stableId, string displayName)
        {
            return new ComparisonSubject(ComparisonSubjectKind.Gem, stableId, stableId, displayName);
        }

        private static ComparisonMetric DamageMetric(
            string metricId,
            string label,
            float beforeValue,
            float afterValue,
            ComparisonResultClass resultClass,
            ComparisonConfidence confidence,
            string unit = "damage")
        {
            float? deltaPercent = null;
            if (Math.Abs(beforeValue) > Tolerance)
            {
                deltaPercent = (afterValue - beforeValue) / beforeValue;
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
                null);
        }

        private static BuildOptionComparison ComparisonOption(
            ComparisonSubject candidate,
            ComparisonSubject replacement,
            ComparisonMetric primaryDamageDelta)
        {
            return ComparisonOption(candidate, replacement, primaryDamageDelta, null, null, null);
        }

        private static BuildOptionComparison ComparisonOption(
            ComparisonSubject candidate,
            ComparisonSubject replacement,
            ComparisonMetric primaryDamageDelta,
            IEnumerable<ComparisonMetric> metrics,
            IEnumerable<ComparisonUtilityChange> utilityChanges,
            IEnumerable<ObservedContextMetric> observedContext)
        {
            return new BuildOptionComparison(
                candidate,
                replacement,
                metrics,
                utilityChanges,
                observedContext,
                primaryDamageDelta,
                primaryDamageDelta != null ? primaryDamageDelta.Confidence : ComparisonConfidence.Unknown,
                null);
        }

        private static RunDamageSnapshot RunSnapshotWithObservedRows(
            PlayerKey playerKey,
            MemoryKey memoryKey,
            GemKey gemKey,
            float memoryDirectDamage,
            float gemDirectDamage,
            float packageTotalDamage,
            float playerDamage)
        {
            var player = new PlayerDamageSnapshot(
                playerKey,
                new DamageAggregateSnapshot(playerDamage, 3, 1f, 3f),
                null,
                1f,
                Array.Empty<SourceDamageSnapshot>(),
                new[]
                {
                    new MemoryDamageSnapshot(
                        memoryKey,
                        new DamageAggregateSnapshot(memoryDirectDamage, 1, 1f, 1f),
                        playerDamage > 0f ? memoryDirectDamage / playerDamage : (float?)null,
                        null)
                },
                new[]
                {
                    new GemDamageSnapshot(
                        gemKey,
                        new DamageAggregateSnapshot(gemDirectDamage, 1, 2f, 2f),
                        playerDamage > 0f ? gemDirectDamage / playerDamage : (float?)null,
                        null)
                },
                new[]
                {
                    new MemoryPackageDamageSnapshot(
                        memoryKey,
                        new DamageAggregateSnapshot(memoryDirectDamage, 1, 1f, 1f),
                        new DamageAggregateSnapshot(gemDirectDamage, 1, 2f, 2f),
                        new DamageAggregateSnapshot(packageTotalDamage - memoryDirectDamage - gemDirectDamage, 1, 3f, 3f),
                        new DamageAggregateSnapshot(packageTotalDamage, 3, 1f, 3f),
                        playerDamage > 0f ? packageTotalDamage / playerDamage : (float?)null,
                        null,
                        Array.Empty<MemoryPackageChildDamageSnapshot>())
                },
                new SourceCoverageSnapshot(playerDamage, playerDamage, 0f),
                new MemoryGemCoverageSnapshot(playerDamage, memoryDirectDamage, gemDirectDamage, memoryDirectDamage, gemDirectDamage, gemDirectDamage, 0f, packageTotalDamage - memoryDirectDamage, 0f));

            return new RunDamageSnapshot(
                10,
                3f,
                false,
                1,
                new[] { player },
                new DamageCoverageSnapshot(playerDamage, playerDamage, playerDamage, 0f, playerDamage, 0f, 3, 3),
                20);
        }

        private static SourceDamageSnapshot SourceSnapshot(SourceKey sourceKey, float damage, float share)
        {
            return new SourceDamageSnapshot(sourceKey, new DamageAggregateSnapshot(damage, 1, 1f, 1f), share, null);
        }

        private static void AssertPlayerDamage(EncounterDamageSnapshot snapshot, PlayerKey playerKey, float damage, int hitCount, string label)
        {
            var player = FindPlayer(snapshot, playerKey, label);
            AssertApproximately(damage, player.Aggregate.Damage, label + " damage");
            AssertEqual(hitCount, player.Aggregate.HitCount, label + " hit count");
        }

        private static void AssertPlayerDamage(RunDamageSnapshot snapshot, PlayerKey playerKey, float damage, int hitCount, string label)
        {
            var player = FindPlayer(snapshot, playerKey, label);
            AssertApproximately(damage, player.Aggregate.Damage, label + " damage");
            AssertEqual(hitCount, player.Aggregate.HitCount, label + " hit count");
        }

        private static PlayerDamageSnapshot FindPlayer(EncounterDamageSnapshot snapshot, PlayerKey playerKey, string label)
        {
            return FindPlayer(snapshot.Players, playerKey, label);
        }

        private static PlayerDamageSnapshot FindPlayer(RunDamageSnapshot snapshot, PlayerKey playerKey, string label)
        {
            return FindPlayer(snapshot.Players, playerKey, label);
        }

        private static PlayerDamageSnapshot FindPlayer(System.Collections.Generic.IReadOnlyList<PlayerDamageSnapshot> players, PlayerKey playerKey, string label)
        {
            for (var i = 0; i < players.Count; i++)
            {
                if (players[i].PlayerKey.Equals(playerKey))
                {
                    return players[i];
                }
            }

            throw new InvalidOperationException("Missing player snapshot: " + label);
        }

        private static void AssertSourceDamage(PlayerDamageSnapshot player, SourceKey sourceKey, float damage, int hitCount, string label)
        {
            var source = FindSource(player, sourceKey, label);
            AssertApproximately(damage, source.Aggregate.Damage, label + " damage");
            AssertEqual(hitCount, source.Aggregate.HitCount, label + " hit count");
        }

        private static SourceDamageSnapshot FindSource(PlayerDamageSnapshot player, SourceKey sourceKey, string label)
        {
            for (var i = 0; i < player.Sources.Count; i++)
            {
                if (player.Sources[i].SourceKey.Equals(sourceKey))
                {
                    return player.Sources[i];
                }
            }

            throw new InvalidOperationException("Missing source snapshot: " + label);
        }

        private static void AssertSourceCount(PlayerDamageSnapshot player, int expected, string label)
        {
            AssertEqual(expected, player.Sources.Count, label);
        }

        private static void AssertSourceSumEqualsPlayer(PlayerDamageSnapshot player, string label)
        {
            var total = 0f;
            for (var i = 0; i < player.Sources.Count; i++)
            {
                total += player.Sources[i].Aggregate.Damage;
            }

            AssertApproximately(player.Aggregate.Damage, total, label);
        }

        private static void AssertMemoryDamage(PlayerDamageSnapshot player, MemoryKey memoryKey, float damage, int hitCount, string label)
        {
            var memory = FindMemory(player, memoryKey, label);
            AssertApproximately(damage, memory.DirectDamage, label + " damage");
            AssertEqual(hitCount, memory.DirectHitCount, label + " hit count");
        }

        private static MemoryDamageSnapshot FindMemory(PlayerDamageSnapshot player, MemoryKey memoryKey, string label)
        {
            for (var i = 0; i < player.Memories.Count; i++)
            {
                if (player.Memories[i].MemoryKey.Equals(memoryKey))
                {
                    return player.Memories[i];
                }
            }

            throw new InvalidOperationException("Missing memory snapshot: " + label);
        }

        private static void AssertGemDamage(PlayerDamageSnapshot player, GemKey gemKey, float damage, int hitCount, string label)
        {
            var gem = FindGem(player, gemKey, label);
            AssertApproximately(damage, gem.DirectDamage, label + " damage");
            AssertEqual(hitCount, gem.DamageEventCount, label + " hit count");
        }

        private static GemDamageSnapshot FindGem(PlayerDamageSnapshot player, GemKey gemKey, string label)
        {
            for (var i = 0; i < player.Gems.Count; i++)
            {
                if (player.Gems[i].GemKey.Equals(gemKey))
                {
                    return player.Gems[i];
                }
            }

            throw new InvalidOperationException("Missing gem snapshot: " + label);
        }

        private static void AssertMemoryPackage(PlayerDamageSnapshot player, MemoryKey memoryKey, float directMemoryDamage, float attachedGemDamage, float totalPackageDamage, int childCount, string label)
        {
            var package = FindMemoryPackage(player, memoryKey, label);
            AssertApproximately(directMemoryDamage, package.DirectMemoryDamage, label + " direct memory damage");
            AssertApproximately(attachedGemDamage, package.AttachedDirectGemDamage, label + " attached gem damage");
            AssertApproximately(totalPackageDamage, package.TotalPackageDamage, label + " total damage");
            AssertEqual(childCount, package.ChildBreakdown.Count, label + " child count");
        }

        private static MemoryPackageDamageSnapshot FindMemoryPackage(PlayerDamageSnapshot player, MemoryKey memoryKey, string label)
        {
            for (var i = 0; i < player.MemoryPackages.Count; i++)
            {
                if (player.MemoryPackages[i].MemoryKey.Equals(memoryKey))
                {
                    return player.MemoryPackages[i];
                }
            }

            throw new InvalidOperationException("Missing memory package snapshot: " + label);
        }

        private static void AssertPlayerCount(EncounterDamageSnapshot snapshot, int expected, string label)
        {
            AssertEqual(expected, snapshot.Players.Count, label);
        }

        private static void AssertPlayerCount(RunDamageSnapshot snapshot, int expected, string label)
        {
            AssertEqual(expected, snapshot.Players.Count, label);
        }

        private static void AssertApproximately(float expected, float actual, string label)
        {
            if (Math.Abs(expected - actual) > Tolerance)
            {
                throw new InvalidOperationException(label + " expected " + expected + " but was " + actual);
            }
        }

        private static void AssertEqual(long expected, long actual, string label)
        {
            if (expected != actual)
            {
                throw new InvalidOperationException(label + " expected " + expected + " but was " + actual);
            }
        }

        private static void AssertEqual(int expected, int actual, string label)
        {
            if (expected != actual)
            {
                throw new InvalidOperationException(label + " expected " + expected + " but was " + actual);
            }
        }

        private static void AssertEqual(string expected, string actual, string label)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(label + " expected " + expected + " but was " + actual);
            }
        }

        private static void AssertFalse(bool actual, string label)
        {
            if (actual)
            {
                throw new InvalidOperationException(label + " expected false");
            }
        }

        private static void AssertTrue(bool actual, string label)
        {
            if (!actual)
            {
                throw new InvalidOperationException(label + " expected true");
            }
        }

        private static void AssertHasPresentationState(IReadOnlyList<ComparisonPresentationMetricRow> rows, ComparisonPresentationMetricState expected, string label)
        {
            if (rows != null)
            {
                for (var i = 0; i < rows.Count; i++)
                {
                    if (rows[i] != null && rows[i].State == expected)
                    {
                        return;
                    }
                }
            }

            throw new InvalidOperationException(label + " expected state " + expected);
        }

        private static void AssertHasPresentationMetric(IReadOnlyList<ComparisonPresentationMetricRow> rows, string metricId, string label)
        {
            if (rows != null)
            {
                for (var i = 0; i < rows.Count; i++)
                {
                    if (rows[i] != null && string.Equals(rows[i].MetricId, metricId, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
            }

            throw new InvalidOperationException(label + " expected metric " + metricId);
        }

        private static void AssertNoKnownZeroDamageRow(IReadOnlyList<ComparisonPresentationMetricRow> rows, string label)
        {
            if (rows == null)
            {
                return;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row != null
                    && row.State == ComparisonPresentationMetricState.Known
                    && row.MetricId.IndexOf("damage", StringComparison.OrdinalIgnoreCase) >= 0
                    && row.BeforeText.StartsWith("0", StringComparison.Ordinal)
                    && row.AfterText.StartsWith("0", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(label + " found " + row.MetricId);
                }
            }
        }

        private static void AssertHasMetric(IReadOnlyList<ComparisonMetric> metrics, string metricId, string label)
        {
            if (metrics != null)
            {
                for (var i = 0; i < metrics.Count; i++)
                {
                    if (metrics[i] != null && string.Equals(metrics[i].MetricId, metricId, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
            }

            throw new InvalidOperationException(label + " expected metric " + metricId);
        }

        private static void AssertHasMetricResultClass(IReadOnlyList<ComparisonMetric> metrics, string metricId, ComparisonResultClass resultClass, string label)
        {
            if (metrics != null)
            {
                for (var i = 0; i < metrics.Count; i++)
                {
                    if (metrics[i] != null
                        && string.Equals(metrics[i].MetricId, metricId, StringComparison.OrdinalIgnoreCase)
                        && metrics[i].ResultClass == resultClass)
                    {
                        return;
                    }
                }
            }

            throw new InvalidOperationException(label + " expected metric " + metricId + " with result class " + resultClass);
        }

        private static void AssertNull(object value, string label)
        {
            if (value != null)
            {
                throw new InvalidOperationException(label + " expected null");
            }
        }

        private static T Require<T>(T value, string label) where T : class
        {
            if (value == null)
            {
                throw new InvalidOperationException(label + " expected a value");
            }

            return value;
        }

        private static float Require(float? value, string label)
        {
            if (!value.HasValue)
            {
                throw new InvalidOperationException(label + " expected a value");
            }

            return value.Value;
        }
    }
}

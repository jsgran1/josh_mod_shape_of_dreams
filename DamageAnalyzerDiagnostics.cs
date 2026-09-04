using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mirror;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal static class DamageAnalyzerDiagnostics
    {
        internal static bool Enabled = true;

        private const int DamageLogLimit = 2000;
        private const int DetailedDamageLogLimit = 240;
        private const int AggregateReportInterval = 500;
        private const int HudHierarchyLogLimit = 3;
        private const int ParentChainLimit = 8;
        private const int NumericFieldLimit = 24;
        private const int BucketDisplayLimit = 16;

        private static ClientEventManager _damageManager;
        private static ClientEventManager _heroLifecycleManager;
        private static int _damageLogCount;
        private static int _detailedDamageLogCount;
        private static int _hudHierarchyLogCount;
        private static int _roomDamageLogCount;
        private static int _roomHeroCombatTransitions;
        private static bool _reportedDamageLimit;
        private static string _currentRoom = "none";
        private static readonly Dictionary<string, DamageBucket> DamageByOwner = new Dictionary<string, DamageBucket>();
        private static readonly Dictionary<string, DamageBucket> DamageByClass = new Dictionary<string, DamageBucket>();
        private static readonly Dictionary<string, DamageBucket> DamageByTargetKind = new Dictionary<string, DamageBucket>();
        private static readonly Dictionary<string, DamageBucket> DamageByEligibility = new Dictionary<string, DamageBucket>();
        private static readonly Dictionary<string, DamageBucket> DamageByProcCandidate = new Dictionary<string, DamageBucket>();
        private static readonly Dictionary<string, DamageBucket> RoomDamageByClass = new Dictionary<string, DamageBucket>();
        private static readonly Dictionary<string, DamageBucket> RoomDamageByEligibility = new Dictionary<string, DamageBucket>();
        private static readonly Dictionary<int, bool> KnownHeroCombatStates = new Dictionary<int, bool>();
        private static readonly Dictionary<int, int> CandidateFlowIds = new Dictionary<int, int>();
        private static readonly HashSet<string> SeenSourcePatterns = new HashSet<string>();
        private static readonly HashSet<string> SeenTargetPatterns = new HashSet<string>();
        private static DamageAnalyticsService _analytics = new DamageAnalyticsService();
        private static int _candidateFlowSequence;

        internal static void Initialize()
        {
            if (!Enabled)
            {
                return;
            }

            DamageAnalyticsUiVisibility.MarkNonGameplay();
            Log("diagnostics enabled");
#if DEBUG
            DamageAnalyticsCoreValidation.RunAll();
            MemoryGemStateDiagnosticsValidation.RunAll();
            Log("analytics-core-validation passed");
#endif
            MemoryGemStateDiagnostics.Reset();
            _analytics = new DamageAnalyticsService();
            DamageAnalyzerOverlay.EnsureCreated();
            DamageAnalyticsEncounterPanel.EnsureCreated();
            DamageAnalyticsRunPanel.EnsureCreated();
            LiveCandidateComparisonPanel.EnsureCreated();
            TrySubscribeExistingDamageManager();
            LogHasteConfigSnapshot("initialize");
        }

        internal static void Shutdown()
        {
            if (_damageLogCount > 0)
            {
                LogAggregateSummary();
                LogRoomSummary("SHUTDOWN");
                LogAnalyticsSummary("SHUTDOWN");
            }

            if (_damageManager != null && _damageManager.OnTakeDamage != null)
            {
                _damageManager.OnTakeDamage.Remove(HandleDamage);
                Log("unsubscribed from ClientEventManager.OnTakeDamage");
            }

            if (_heroLifecycleManager != null)
            {
                if (_heroLifecycleManager.OnHeroKnockedOut != null)
                {
                    _heroLifecycleManager.OnHeroKnockedOut.Remove(LogHeroKnockedOut);
                }

                if (_heroLifecycleManager.OnHeroRevive != null)
                {
                    _heroLifecycleManager.OnHeroRevive.Remove(LogHeroRevived);
                }

                Log("unsubscribed from ClientEventManager hero lifecycle events");
            }

            DamageAnalyzerOverlay.DestroyOverlay();
            DamageAnalyticsEncounterPanel.DestroyPanel();
            DamageAnalyticsRunPanel.DestroyPanel();
            LiveCandidateComparisonPanel.DestroyPanel();
            _damageManager = null;
            _heroLifecycleManager = null;
            _damageLogCount = 0;
            _detailedDamageLogCount = 0;
            _hudHierarchyLogCount = 0;
            _roomDamageLogCount = 0;
            _roomHeroCombatTransitions = 0;
            _reportedDamageLimit = false;
            _currentRoom = "none";
            DamageByOwner.Clear();
            DamageByClass.Clear();
            DamageByTargetKind.Clear();
            DamageByEligibility.Clear();
            DamageByProcCandidate.Clear();
            RoomDamageByClass.Clear();
            RoomDamageByEligibility.Clear();
            KnownHeroCombatStates.Clear();
            CandidateFlowIds.Clear();
            MemoryGemStateDiagnostics.Reset();
            SeenSourcePatterns.Clear();
            SeenTargetPatterns.Clear();
            _analytics = new DamageAnalyticsService();
            _candidateFlowSequence = 0;
        }

        internal static EncounterDamageSnapshot GetCurrentEncounterSnapshot(float timestamp)
        {
            return Enabled ? _analytics.GetCurrentEncounterSnapshot(timestamp) : null;
        }

        internal static EncounterDamageSnapshot GetLastEncounterSnapshot()
        {
            return Enabled ? _analytics.GetLastEncounterSnapshot() : null;
        }

        internal static RunDamageSnapshot GetRunSnapshot()
        {
            return Enabled ? _analytics.GetRunSnapshot() : null;
        }

        internal static LiveCandidateComparisonSnapshot GetLiveCandidateComparisonSnapshot()
        {
            return LiveCandidateComparisonCoordinator.CurrentSnapshot;
        }

        internal static void TrySubscribeDamage(ClientEventManager manager, string reason)
        {
            if (!Enabled || manager == null)
            {
                return;
            }

            if (_damageManager != null && _damageManager != manager && _damageManager.OnTakeDamage != null)
            {
                _damageManager.OnTakeDamage.Remove(HandleDamage);
            }

            _damageManager = manager;
            TrySubscribeHeroLifecycle(manager, reason);

            if (manager.OnTakeDamage == null)
            {
                Log($"damage subscription skipped; OnTakeDamage is null; reason={reason}");
                return;
            }

            manager.OnTakeDamage.Remove(HandleDamage);
            manager.OnTakeDamage.Add(HandleDamage);
            Log($"subscribed to ClientEventManager.OnTakeDamage; reason={reason}");
        }

        internal static void LogGemEquipTooltip(SkillTrigger skill, Gem prevGem, Gem newGem)
        {
            if (!Enabled)
            {
                return;
            }

            var local = DewPlayer.local;
            var hero = local != null ? local.hero : null;
            var heroSkill = hero != null ? hero.Skill : null;

            var locationText = "unknown";
            var equippedGems = "unavailable";

            if (skill != null && heroSkill != null && heroSkill.TryGetSkillLocation(skill, out var skillLocation))
            {
                locationText = skillLocation.ToString();
                equippedGems = FormatGems(heroSkill.GetGemsInSkill(skillLocation));
            }

            Log(
                "gem-tooltip " +
                "interaction=EXISTING_GEM_REARRANGE " +
                $"player={FormatPlayer(local)} " +
                $"skill={FormatSkill(skill, heroSkill)} " +
                $"skillLocation={locationText} " +
                $"prevGem={FormatGem(prevGem, heroSkill)} " +
                $"newGem={FormatGem(newGem, heroSkill)} " +
                $"equippedGems=[{equippedGems}]");

            LogComparisonSnapshot("gem-tooltip", skill, prevGem, newGem, hero);

            LogHudHierarchy("gem-tooltip");
        }

        internal static void LogSkillEquipTooltip(SkillTrigger prevSkill, SkillTrigger newSkill)
        {
            if (!Enabled)
            {
                return;
            }

            var local = DewPlayer.local;
            var hero = local != null ? local.hero : null;
            var heroSkill = hero != null ? hero.Skill : null;

            Log(
                "skill-tooltip " +
                "interaction=NEW_CANDIDATE " +
                $"player={FormatPlayer(local)} " +
                $"prevSkill={FormatSkill(prevSkill, heroSkill)} " +
                $"newSkill={FormatSkill(newSkill, heroSkill)} " +
                $"currentSkills=[{FormatSkills(heroSkill)}]");

            LogNumericSnapshot("candidate-skill", newSkill, hero);
            LogNumericSnapshot("previous-skill", prevSkill, hero);

            if (heroSkill != null)
            {
                foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
                {
                    var current = heroSkill.GetSkill(location);
                    var gems = FormatGems(heroSkill.GetGemsInSkill(location));
                    Log($"skill-replacement-target slot={location} current={FormatSkill(current, heroSkill)} attachedGems=[{gems}]");
                }
            }

            LogHudHierarchy("skill-tooltip");
        }

        internal static void LogStartEquipGem(Gem gem)
        {
            Log($"edit-flow StartEquipGem interaction=NEW_CANDIDATE candidate={FormatGem(gem, null)} provider={FormatObject(EditSkillManager.instance != null ? EditSkillManager.instance.currentProvider : null)}");
            LogCandidateFlow("START_EQUIP_GEM", gem, null, null);
            LogMemoryGemState("START_EQUIP_GEM", MemoryGemStatePhase.Observation, "candidate", gem, null, true);
            LogLiveCandidateComparison("START_EQUIP_GEM", gem);
            LogNumericSnapshot("start-equip-gem", gem, DewPlayer.local != null ? DewPlayer.local.hero : null);
            LogHasteConfigSnapshot("start-equip-gem");
        }

        internal static void LogStartEquipSkill(SkillTrigger skill)
        {
            Log($"edit-flow StartEquipSkill interaction=NEW_CANDIDATE candidate={FormatSkill(skill, null)} provider={FormatObject(EditSkillManager.instance != null ? EditSkillManager.instance.currentProvider : null)}");
            LogCandidateFlow("START_EQUIP_MEMORY", skill, null, null);
            LogMemoryGemState("START_EQUIP_MEMORY", MemoryGemStatePhase.Observation, "candidate", skill, null, true);
            LogLiveCandidateComparison("START_EQUIP_MEMORY", skill);
            LogNumericSnapshot("start-equip-skill", skill, DewPlayer.local != null ? DewPlayer.local.hero : null);
            LogHasteConfigSnapshot("start-equip-skill", skill);
        }

        internal static void LogHoldingObjectChanged(HeroSkill heroSkill, IItem oldObject, IItem newObject)
        {
            var candidate = newObject ?? oldObject;
            var extra = $"old={FormatItem(oldObject)} new={FormatItem(newObject)}";
            LogCandidateFlow(newObject != null ? "HELD_AVAILABLE" : "HELD_CLEARED", candidate, heroSkill, extra);
            if (newObject is Actor heldActor)
            {
                LogMemoryGemState("HELD_AVAILABLE", MemoryGemStatePhase.Observation, "candidate", heldActor, heroSkill, true);
            }
            LogLiveCandidateComparison(newObject != null ? "HELD_AVAILABLE" : "HELD_CLEARED", newObject);
        }

        internal static void LogSkillPickup(HeroSkill heroSkill, SkillTrigger skill)
        {
            LogCandidateFlow("PICKUP_MEMORY", skill, heroSkill, null);
            LogMemoryGemState("PICKUP_MEMORY", MemoryGemStatePhase.Observation, "candidate", skill, heroSkill, true);
            LogHasteConfigSnapshot("memory-pickup", skill);
        }

        internal static void LogGemPickup(HeroSkill heroSkill, Gem gem)
        {
            LogCandidateFlow("PICKUP_GEM", gem, heroSkill, null);
            LogMemoryGemState("PICKUP_GEM", MemoryGemStatePhase.Observation, "candidate", gem, heroSkill, true);
        }

        internal static void LogSkillDrop(HeroSkill heroSkill, SkillTrigger skill)
        {
            LogCandidateFlow("DROP_MEMORY", skill, heroSkill, null);
            LogMemoryGemState("DROP_MEMORY", MemoryGemStatePhase.Before, "removed-memory", skill, heroSkill, false);
            LogLiveCandidateComparison("DROP_MEMORY");
            ForgetCandidateFlow(skill);
        }

        internal static void LogGemDrop(HeroSkill heroSkill, Gem gem)
        {
            LogCandidateFlow("DROP_GEM", gem, heroSkill, null);
            LogMemoryGemState("DROP_GEM", MemoryGemStatePhase.Before, "removed-gem", gem, heroSkill, false);
            LogLiveCandidateComparison("DROP_GEM");
            ForgetCandidateFlow(gem);
        }

        internal static void LogSkillClientSlotSelected(HeroSkillLocation type, SkillTrigger skill)
        {
            LogCandidateFlow("COMMIT_INTENT_MEMORY", skill, null, $"selectedSkillSlot={type}");
            var heroSkill = DewPlayer.local != null && DewPlayer.local.hero != null ? DewPlayer.local.hero.Skill : null;
            var flowId = GetCandidateFlowId(skill);
            LogMemoryGemState(flowId, "COMMIT_INTENT_MEMORY", MemoryGemStatePhase.Before, "candidate", skill, heroSkill, true);
            LogMemoryGemState(flowId, "COMMIT_INTENT_MEMORY", MemoryGemStatePhase.Before, "replacement-target", heroSkill != null ? heroSkill.GetSkill(type) : null, heroSkill, false);
            LogLiveCandidateComparison("COMMIT_INTENT_MEMORY", skill);
        }

        internal static void LogGemClientSlotSelected(GemLocation loc, Gem gem)
        {
            LogCandidateFlow("COMMIT_INTENT_GEM", gem, null, $"selectedGemSlot={FormatGemLocation(loc)}");
            var heroSkill = DewPlayer.local != null && DewPlayer.local.hero != null ? DewPlayer.local.hero.Skill : null;
            var current = heroSkill != null ? heroSkill.GetGem(loc) : null;
            var flowItem = (IItem)gem ?? current;
            var flowId = GetCandidateFlowId(flowItem);
            LogMemoryGemState(flowId, "COMMIT_INTENT_GEM", MemoryGemStatePhase.Before, "candidate", gem, heroSkill, gem != null);
            LogMemoryGemState(flowId, "COMMIT_INTENT_GEM", MemoryGemStatePhase.Before, gem == null ? "removed-gem" : "replacement-target", current, heroSkill, false);
            LogLiveCandidateComparison("COMMIT_INTENT_GEM", gem);
        }

        internal static void LogSkillEquipApplied(HeroSkill heroSkill, SkillTrigger skill)
        {
            LogCandidateFlow("COMMIT_APPLIED_MEMORY", skill, heroSkill, null);
            LogMemoryGemState("COMMIT_APPLIED_MEMORY", MemoryGemStatePhase.After, "equipped-memory", skill, heroSkill, false);
            LogLiveCandidateComparison("COMMIT_APPLIED_MEMORY");
            LogHasteConfigSnapshot("memory-equip-applied", skill);
            ForgetCandidateFlow(skill);
        }

        internal static void LogGemEquipApplied(HeroSkill heroSkill, Gem gem)
        {
            LogCandidateFlow("COMMIT_APPLIED_GEM", gem, heroSkill, null);
            LogMemoryGemState("COMMIT_APPLIED_GEM", MemoryGemStatePhase.After, "equipped-gem", gem, heroSkill, false);
            LogLiveCandidateComparison("COMMIT_APPLIED_GEM");
            ForgetCandidateFlow(gem);
        }

        internal static void LogEditEnded(EditSkillManager manager)
        {
            if (manager == null)
            {
                return;
            }

            var mode = manager.mode;
            if (mode != EditSkillManager.ModeType.EquipGem && mode != EditSkillManager.ModeType.EquipSkill)
            {
                return;
            }

            var heroSkill = DewPlayer.local != null && DewPlayer.local.hero != null ? DewPlayer.local.hero.Skill : null;
            var held = heroSkill != null ? heroSkill.holdingObject : null;
            if (held == null)
            {
                return;
            }

            LogCandidateFlow(
                mode == EditSkillManager.ModeType.EquipGem ? "CANCEL_REQUEST_GEM" : "CANCEL_REQUEST_MEMORY",
                held,
                heroSkill,
                $"selectedSkillSlot={FormatNullableSkillLocation(manager.selectedSkillSlot)} selectedGemSlot={FormatNullableGemLocation(manager.selectedGemSlot)}");
            LogLiveCandidateComparison(mode == EditSkillManager.ModeType.EquipGem ? "CANCEL_REQUEST_GEM" : "CANCEL_REQUEST_MEMORY");
        }

        internal sealed class MergeGemState
        {
            internal int FlowId;
            internal int VictimQuality;
            internal int ReceiverQuality;
            internal int ExpectedQuality;
        }

        internal static MergeGemState LogGemMergeBegin(HeroSkill heroSkill, Gem victim, Gem receivingGem)
        {
            var state = new MergeGemState
            {
                FlowId = GetCandidateFlowId(victim),
                VictimQuality = victim != null ? victim.quality : -1,
                ReceiverQuality = receivingGem != null ? receivingGem.quality : -1,
                ExpectedQuality = victim != null && receivingGem != null ? Gem.GetMergedQuality(victim.quality, receivingGem.quality) : -1
            };

            LogCandidateFlow(
                "MERGE_BEGIN_GEM",
                victim,
                heroSkill,
                $"mergeReceiver={FormatGem(receivingGem, heroSkill)} victimQualityBefore={state.VictimQuality} receiverQualityBefore={state.ReceiverQuality} expectedQuality={state.ExpectedQuality}");
            if (receivingGem != null)
            {
                CandidateFlowIds[receivingGem.GetInstanceID()] = state.FlowId;
            }
            LogMemoryGemState(state.FlowId, "MERGE_BEGIN_GEM", MemoryGemStatePhase.Before, "merge-victim", victim, heroSkill, true);
            LogMemoryGemState(state.FlowId, "MERGE_BEGIN_GEM", MemoryGemStatePhase.Before, "merge-receiver", receivingGem, heroSkill, false);
            LogLiveCandidateComparison("MERGE_BEGIN_GEM", victim);
            return state;
        }

        internal static void LogGemMergeEnd(HeroSkill heroSkill, Gem victim, Gem receivingGem, MergeGemState state)
        {
            LogCandidateFlow(
                "MERGE_APPLIED_GEM",
                victim,
                heroSkill,
                $"flow={FormatFlowId(state != null ? state.FlowId : GetCandidateFlowId(victim))} mergeReceiver={FormatGem(receivingGem, heroSkill)} victimQualityBefore={(state != null ? state.VictimQuality : -1)} receiverQualityBefore={(state != null ? state.ReceiverQuality : -1)} receiverQualityAfter={(receivingGem != null ? receivingGem.quality : -1)} expectedQuality={(state != null ? state.ExpectedQuality : -1)}");
            LogMemoryGemState(state != null ? state.FlowId : GetCandidateFlowId(victim), "MERGE_APPLIED_GEM", MemoryGemStatePhase.After, "merge-receiver", receivingGem, heroSkill, false);
            LogLiveCandidateComparison("MERGE_APPLIED_GEM");
            ForgetCandidateFlow(victim);
        }

        internal static void LogGemMergeClientConfirmed(Hero hero, Gem gem)
        {
            var heroSkill = hero != null ? hero.Skill : null;
            LogCandidateFlow("MERGE_CONFIRMED_GEM", gem, heroSkill, $"receiver={FormatGem(gem, heroSkill)}");
            LogMemoryGemState("MERGE_CONFIRMED_GEM", MemoryGemStatePhase.After, "merge-receiver-confirmed", gem, heroSkill, false);
            LogLiveCandidateComparison("MERGE_CONFIRMED_GEM");
            ForgetCandidateFlow(gem);
        }

        internal sealed class UpgradeState
        {
            internal int FlowId;
            internal Actor Target;
            internal HeroSkill HeroSkill;
        }

        internal static UpgradeState LogUpgradeBegin(Actor target)
        {
            var state = new UpgradeState
            {
                FlowId = GetCandidateFlowId(target as IItem),
                Target = target,
                HeroSkill = null
            };
            LogMemoryGemState(state.FlowId, "ITEM_UPGRADE", MemoryGemStatePhase.Before, "upgrade-target", target, state.HeroSkill, false);
            return state;
        }

        internal static void LogUpgradeEnd(UpgradeState state)
        {
            if (state == null)
            {
                return;
            }

            LogMemoryGemState(state.FlowId, "ITEM_UPGRADE", MemoryGemStatePhase.After, "upgrade-target", state.Target, state.HeroSkill, false);
            ForgetCandidateFlow(state.Target as IItem);
        }

        internal static void LogItemUpgradeConfirmed(Hero hero, NetworkBehaviour target)
        {
            var actor = target as Actor;
            if (!(actor is SkillTrigger) && !(actor is Gem))
            {
                return;
            }

            var flowId = GetCandidateFlowId(actor as IItem);
            LogMemoryGemState(flowId, "ITEM_UPGRADE", MemoryGemStatePhase.After, "upgrade-target", actor, hero != null ? hero.Skill : null, false);
            ForgetCandidateFlow(actor as IItem);
        }

        internal static void LogGemUnequipApplied(HeroSkill heroSkill, Gem gem)
        {
            LogMemoryGemState("UNEQUIP_APPLIED_GEM", MemoryGemStatePhase.After, "removed-gem", gem, heroSkill, false);
            ForgetCandidateFlow(gem);
        }

        internal static void LogSkillUnequipApplied(HeroSkill heroSkill, SkillTrigger skill)
        {
            LogMemoryGemState("UNEQUIP_APPLIED_MEMORY", MemoryGemStatePhase.After, "removed-memory", skill, heroSkill, false);
            ForgetCandidateFlow(skill);
        }

        internal static void LogRoomTimeline(string evt, Room room)
        {
            if (evt == "ROOM_START")
            {
                DamageAnalyticsUiVisibility.MarkActiveGameplay();
                StartRoomScope(room);
                _analytics.OnRoomStarted(Time.time);
            }

            Log(
                "timeline " +
                $"event={evt} " +
                $"t={FormatFloat(Time.time)} " +
                $"room={FormatObject(room)} " +
                $"didClear={SafeBool(() => room != null && room.didClearRoom)} " +
                $"activeCombatAreas={SafeInt(() => room != null ? room.numOfActivatedCombatAreas : -1)} " +
                $"roomDamageEvents={_roomDamageLogCount} " +
                $"roomHeroCombatTransitions={_roomHeroCombatTransitions}");

            if (evt == "ROOM_CLEAR_BEGIN" || evt == "ROOM_STOP")
            {
                _analytics.OnRoomCompleted(Time.time);
                LogRoomSummary(evt);
                LogAnalyticsSummary(evt);
            }

            DamageAnalyzerOverlay.Show($"{evt} collected\n{FormatObject(room)}");
        }

        internal static void LogHeroCombatChanged(Hero hero, bool newValue)
        {
            var oldValueText = "unknown";
            if (hero != null)
            {
                var heroId = hero.GetInstanceID();
                if (KnownHeroCombatStates.TryGetValue(heroId, out var previousValue))
                {
                    oldValueText = previousValue.ToString();
                }

                KnownHeroCombatStates[heroId] = newValue;
            }

            _roomHeroCombatTransitions++;
            if (newValue)
            {
                DamageAnalyticsUiVisibility.MarkActiveGameplay();
            }

            _analytics.OnCombatChanged(Time.time, newValue);
            Log($"timeline event=HERO_COMBAT_CHANGED t={FormatFloat(Time.time)} room={_currentRoom} hero={FormatObject(hero)} owner={FormatPlayer(hero != null ? hero.owner : null)} old={oldValueText} new={newValue} roomHeroCombatTransitions={_roomHeroCombatTransitions}");
        }

        internal static void LogHeroKnockedOut(Hero hero)
        {
            LogHeroLifecycle("HERO_KNOCKED_OUT", hero);
            LogAnalyticsSummary("HERO_KNOCKED_OUT");
        }

        internal static void LogHeroRevived(Hero hero)
        {
            LogHeroLifecycle("HERO_REVIVED", hero);
            LogAnalyticsSummary("HERO_REVIVED");
        }

        internal static void LogGameResultRegistered(bool didGameEnd)
        {
            Log($"timeline event=GAME_RESULT_REGISTERED t={FormatFloat(Time.time)} didGameEnd={didGameEnd}");
            if (!didGameEnd)
            {
                return;
            }

            _analytics.FinalizeRunForGameResult(Time.time);
            LiveCandidateComparisonCoordinator.Clear("game result finalized");
            DamageAnalyticsUiVisibility.MarkFinalResults();
            LogAnalyticsSummary("GAME_RESULT_END_FINALIZED");
        }

        internal static void LogGameSessionStarting()
        {
            if (!Enabled)
            {
                return;
            }

            var hasRunState = _analytics.HasRunState;
            DamageAnalyticsUiVisibility.MarkNonGameplay();
            Log($"timeline event=GAME_SESSION_STARTING t={FormatFloat(Time.time)} hasRunState={hasRunState}");
            if (!hasRunState)
            {
                return;
            }

            LogAnalyticsSummary("GAME_SESSION_START_BEFORE_RESET");
            _analytics.ResetRunForNewGameIfNeeded(Time.time);
            LiveCandidateComparisonCoordinator.Clear("game session starting");
            LogAnalyticsSummary("GAME_SESSION_START_AFTER_RESET");
        }

        internal static void LogHudHierarchy(string reason)
        {
            if (!Enabled || _hudHierarchyLogCount >= HudHierarchyLogLimit)
            {
                return;
            }

            _hudHierarchyLogCount++;
            Log($"hud-hierarchy begin reason={reason} sample={_hudHierarchyLogCount}/{HudHierarchyLogLimit}");
            LogObjectsOfType<Canvas>("Canvas");
            LogObjectsOfType<UI_InGame_SkillButtons>("UI_InGame_SkillButtons");
            LogObjectsOfType<UI_InGame_RoomModDisplay>("UI_InGame_RoomModDisplay");
            LogObjectsOfType<UI_InGame_HeroInfoBar>("UI_InGame_HeroInfoBar");
            LogObjectsOfType<UI_DamageNumberGroup>("UI_DamageNumberGroup");
            LogObjectsOfType<UI_EntityHealthBarGroup>("UI_EntityHealthBarGroup");
            Log($"hud-hierarchy end reason={reason}");
        }

        private static void TrySubscribeExistingDamageManager()
        {
            var managers = Resources.FindObjectsOfTypeAll(typeof(ClientEventManager));
            if (managers == null || managers.Length == 0)
            {
                Log("no existing ClientEventManager found during initialize");
                return;
            }

            foreach (var obj in managers)
            {
                if (obj is ClientEventManager manager)
                {
                    TrySubscribeDamage(manager, "initialize-existing-manager");
                    return;
                }
            }
        }

        private static void HandleDamage(EventInfoDamage info)
        {
            if (!Enabled)
            {
                return;
            }

            _damageLogCount++;

            var actor = info.actor;
            var victim = info.victim;
            var ability = actor as AbilityInstance;
            var actorGem = actor as Gem;
            var abilityGem = ability != null ? ability.gem : null;
            var caster = ability != null ? ability.info.caster : null;
            var owner = ResolveOwner(actor);
            var memory = FindParent<SkillTrigger>(actor);
            var damageClass = ClassifyDamage(actor, abilityGem, memory);
            var targetKind = ClassifyTarget(victim);
            var eligibility = ClassifyEligibility(owner, victim, targetKind);
            var procCandidateKey = FormatProcCandidateKey(damageClass, actor, abilityGem, memory, caster);
            var playerKey = ResolvePlayerKey(owner);
            var targetRelationship = MapTargetRelationship(eligibility, targetKind);
            var sourceCategory = MapSourceCategory(damageClass);
            var sourceKey = ResolveSourceKey(damageClass, sourceCategory, actor, abilityGem, memory);
            var memoryKey = ResolveMemoryKey(sourceCategory, playerKey, memory != null ? memory : actor);
            var gemKey = ResolveGemKey(sourceCategory, playerKey, abilityGem != null ? (Actor)abilityGem : actorGem != null ? (Actor)actorGem : actor);
            var originatingMemoryKey = ResolveOriginatingMemoryKey(sourceCategory, playerKey, memory);
            var record = _analytics.CaptureDamage(Time.time, info.damage.amount, playerKey, targetRelationship, sourceKey, memoryKey, gemKey, originatingMemoryKey);

            _roomDamageLogCount++;

            AddDamage(DamageByOwner, FormatPlayer(owner), info.damage.amount, damageClass);
            AddDamage(DamageByClass, damageClass, info.damage.amount, damageClass);
            AddDamage(DamageByTargetKind, targetKind, info.damage.amount, damageClass);
            AddDamage(DamageByEligibility, eligibility, info.damage.amount, damageClass);
            AddDamage(RoomDamageByClass, damageClass, info.damage.amount, damageClass);
            AddDamage(RoomDamageByEligibility, eligibility, info.damage.amount, damageClass);

            if (damageClass == "GEM_DIRECT")
            {
                AddDamage(DamageByProcCandidate, procCandidateKey, info.damage.amount, damageClass);
            }

            var pattern = $"{damageClass}|{FormatType(actor)}|{FormatType(memory)}|{FormatType(abilityGem)}|{FormatType(victim)}";
            var isNewPattern = SeenSourcePatterns.Add(pattern);
            var targetPattern = $"{eligibility}|{targetKind}|{FormatType(actor)}|{FormatType(victim)}|{FormatPlayer(owner)}";
            var isNewTargetPattern = SeenTargetPatterns.Add(targetPattern);
            var unresolvedPlayerDamage = owner != null && damageClass == "UNATTRIBUTED";
            var shouldLogDetail = isNewPattern || isNewTargetPattern || unresolvedPlayerDamage || _detailedDamageLogCount < DetailedDamageLogLimit;

            if (_damageLogCount % AggregateReportInterval == 0)
            {
                LogAggregateSummary();
            }

            if (!shouldLogDetail)
            {
                if (!_reportedDamageLimit)
                {
                    _reportedDamageLimit = true;
                    Log($"detailed damage logging is now anomaly/new-pattern only after {DetailedDamageLogLimit} detailed events; aggregates continue");
                }

                return;
            }

            _detailedDamageLogCount++;

            var message = new StringBuilder(512);
            message.Append("damage ");
            message.Append("index=").Append(_damageLogCount).Append(' ');
            message.Append("sequence=").Append(record.SequenceId).Append(' ');
            message.Append("detailIndex=").Append(_detailedDamageLogCount).Append(' ');
            message.Append("t=").Append(FormatFloat(Time.time)).Append(' ');
            message.Append("room=").Append(_currentRoom).Append(' ');
            message.Append("class=").Append(damageClass).Append(' ');
            message.Append("newPattern=").Append(isNewPattern).Append(' ');
            message.Append("newTargetPattern=").Append(isNewTargetPattern).Append(' ');
            message.Append("amount=").Append(FormatFloat(info.damage.amount)).Append(' ');
            message.Append("shieldNegated=").Append(FormatFloat(info.negatedAmountByShield)).Append(' ');
            message.Append("discarded=").Append(FormatFloat(info.damage.discardedAmount)).Append(' ');
            message.Append("proc=").Append(FormatFloat(info.damage.procCoefficient)).Append(' ');
            message.Append("type=").Append(info.damage.type).Append(' ');
            message.Append("attrs=").Append(info.damage.attributes).Append(' ');
            message.Append("targetKind=").Append(targetKind).Append(' ');
            message.Append("eligibility=").Append(eligibility).Append(' ');
            message.Append("analyticsEligible=").Append(record.IsEligiblePlayerDamage).Append(' ');
            message.Append("sourceKey=").Append(record.SourceKey).Append(' ');
            message.Append("memoryKey=").Append(FormatOptionalMemoryKey(record.MemoryKey)).Append(' ');
            message.Append("gemKey=").Append(FormatOptionalGemKey(record.GemKey)).Append(' ');
            message.Append("originatingMemoryKey=").Append(FormatOptionalMemoryKey(record.OriginatingMemoryKey)).Append(' ');
            message.Append("procCandidate=").Append(procCandidateKey).Append(' ');
            message.Append("actor=").Append(FormatObject(actor)).Append(' ');
            message.Append("victim=").Append(FormatObject(victim)).Append(' ');
            message.Append("victimHp=").Append(victim != null ? FormatFloat(victim.currentHealth) : "null").Append(' ');
            message.Append("owner=").Append(FormatPlayer(owner)).Append(' ');
            message.Append("caster=").Append(FormatObject(caster)).Append(' ');
            message.Append("casterOwner=").Append(caster != null ? FormatPlayer(caster.owner) : "null").Append(' ');
            message.Append("memory=").Append(FormatSkill(memory, null)).Append(' ');
            message.Append("actorGem=").Append(FormatGem(actorGem, null)).Append(' ');
            message.Append("abilityGem=").Append(FormatGem(abilityGem, null)).Append(' ');
            message.Append("parentChain=").Append(FormatParentChain(actor));

            Log(message.ToString());
            LogHudHierarchy("damage-event");
        }

        private static void LogComparisonSnapshot(string reason, SkillTrigger skill, Gem prevGem, Gem newGem, Hero hero)
        {
            LogNumericSnapshot(reason + ".candidateGem", newGem, hero);
            LogNumericSnapshot(reason + ".previousGem", prevGem, hero);
            LogNumericSnapshot(reason + ".targetSkill", skill, hero);
            Log("comparison-readiness " +
                $"reason={reason} " +
                $"candidate={FormatGem(newGem, hero != null ? hero.Skill : null)} " +
                $"previous={FormatGem(prevGem, hero != null ? hero.Skill : null)} " +
                "damagePerUse=UNKNOWN " +
                "note=structured numeric fields are logged, but no non-live whole-swap evaluator is confirmed");
        }

        private static void LogNumericSnapshot(string label, Actor actor, Hero hero)
        {
            if (actor == null)
            {
                Log($"numeric-snapshot {label} actor=null");
                return;
            }

            var builder = new StringBuilder(512);
            builder.Append("numeric-snapshot ");
            builder.Append(label).Append(' ');
            builder.Append("actor=").Append(FormatObject(actor)).Append(' ');

            var level = ResolveEffectiveLevel(actor);
            builder.Append("level=").Append(level).Append(' ');
            builder.Append("fields=[");
            builder.Append(FormatNumericFields(actor, level, hero));
            builder.Append("] configs=[");
            builder.Append(FormatTriggerConfigs(actor));
            builder.Append(']');

            Log(builder.ToString());
        }

        private static void LogCandidateFlow(string evt, IItem item, HeroSkill heroSkill, string extra)
        {
            var actor = item as Actor;
            if (heroSkill == null)
            {
                heroSkill = ResolveLocalHeroSkill(actor);
            }

            var flowId = GetCandidateFlowId(item);
            var manager = EditSkillManager.instance;
            var local = DewPlayer.local;
            var hero = local != null ? local.hero : null;

            var builder = new StringBuilder(768);
            builder.Append("candidate-flow ");
            builder.Append("event=").Append(evt).Append(' ');
            builder.Append("flow=").Append(FormatFlowId(flowId)).Append(' ');
            builder.Append("t=").Append(FormatFloat(Time.time)).Append(' ');
            builder.Append("player=").Append(FormatPlayer(local)).Append(' ');
            builder.Append("candidate=").Append(FormatItem(item)).Append(' ');
            builder.Append("held=").Append(FormatItem(heroSkill != null ? heroSkill.holdingObject : null)).Append(' ');
            builder.Append("mode=").Append(manager != null ? manager.mode.ToString() : "null").Append(' ');
            builder.Append("provider=").Append(FormatObject(manager != null ? manager.currentProvider : null)).Append(' ');
            builder.Append("selectedSkillSlot=").Append(FormatNullableSkillLocation(manager != null ? manager.selectedSkillSlot : null)).Append(' ');
            builder.Append("selectedGemSlot=").Append(FormatNullableGemLocation(manager != null ? manager.selectedGemSlot : null)).Append(' ');
            builder.Append("currentMemories=[").Append(FormatSkills(heroSkill)).Append("] ");
            builder.Append("currentGems=[").Append(FormatGemSlots(heroSkill)).Append("] ");
            builder.Append("legalSkillTargets=[").Append(FormatLegalSkillTargets(heroSkill)).Append("] ");
            builder.Append("legalGemTargets=[").Append(FormatLegalGemTargets(heroSkill, actor as Gem)).Append("] ");
            builder.Append("heroStats=").Append(FormatHeroStats(hero)).Append(' ');
            if (!string.IsNullOrEmpty(extra))
            {
                builder.Append(extra);
            }

            Log(builder.ToString());
        }

        private static void LogMemoryGemState(
            string trigger,
            MemoryGemStatePhase phase,
            string role,
            Actor subject,
            HeroSkill heroSkill,
            bool isCandidate)
        {
            LogMemoryGemState(GetCandidateFlowId(subject as IItem), trigger, phase, role, subject, heroSkill, isCandidate);
        }

        private static void LogMemoryGemState(
            int flowId,
            string trigger,
            MemoryGemStatePhase phase,
            string role,
            Actor subject,
            HeroSkill heroSkill,
            bool isCandidate)
        {
            if (!Enabled || subject == null)
            {
                return;
            }

            var line = MemoryGemStateDiagnostics.Capture(flowId, trigger, phase, role, subject, heroSkill, isCandidate);
            if (!string.IsNullOrEmpty(line))
            {
                Log(line);
            }
        }

        private static void LogLiveCandidateComparison(string reason, IItem candidate = null)
        {
            var heroSkill = DewPlayer.local != null && DewPlayer.local.hero != null ? DewPlayer.local.hero.Skill : null;
            var held = heroSkill != null ? heroSkill.holdingObject : null;
            var item = held ?? candidate;
            LiveCandidateComparisonSnapshot snapshot;

            if (reason == "HELD_CLEARED" ||
                reason == "DROP_MEMORY" ||
                reason == "DROP_GEM" ||
                reason == "COMMIT_APPLIED_MEMORY" ||
                reason == "COMMIT_APPLIED_GEM" ||
                reason == "CANCEL_REQUEST_MEMORY" ||
                reason == "CANCEL_REQUEST_GEM" ||
                reason == "MERGE_APPLIED_GEM" ||
                reason == "MERGE_CONFIRMED_GEM")
            {
                snapshot = LiveCandidateComparisonCoordinator.Clear(reason);
            }
            else
            {
                snapshot = LiveCandidateComparisonCoordinator.RefreshForHeldCandidate(item, heroSkill, reason);
            }

            Log(
                "live-comparison " +
                $"event={reason} " +
                $"sequence={snapshot.SequenceId} " +
                $"status={snapshot.Status} " +
                $"candidateKind={snapshot.CandidateKind} " +
                $"comparisons={snapshot.Comparisons.Count} " +
                $"contextualEvaluations={snapshot.ContextualEvaluations.Count} " +
                $"reason=\"{snapshot.Reason}\"");
        }

        private static void LogHasteConfigSnapshot(string reason, SkillTrigger candidate = null)
        {
            var local = DewPlayer.local;
            var hero = local != null ? local.hero : null;
            var heroSkill = hero != null ? hero.Skill : null;
            var gameManager = NetworkedManagerBase<GameManager>.instance;
            var floor = gameManager != null ? SafeFloat(() => gameManager.ges.cooldownFloorRatioByAbilityHaste) : "unavailable";
            var abilityHaste = hero != null ? SafeFloat(() => hero.Status.abilityHaste) : "null";

            Log(
                "haste-config-summary " +
                $"reason={reason} " +
                $"t={FormatFloat(Time.time)} " +
                $"player={FormatPlayer(local)} " +
                $"floor={floor} " +
                $"abilityHaste={abilityHaste} " +
                $"currentMemories=[{FormatSkills(heroSkill)}] " +
                $"candidate={FormatSkill(candidate, heroSkill)}");

            if (heroSkill != null)
            {
                foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
                {
                    LogSkillTriggerConfigs(reason, location.ToString(), heroSkill.GetSkill(location));
                }
            }

            if (candidate != null)
            {
                LogSkillTriggerConfigs(reason, "candidate", candidate);
            }
        }

        private static void LogSkillTriggerConfigs(string reason, string slot, SkillTrigger skill)
        {
            if (skill == null)
            {
                return;
            }

            var trigger = skill as AbilityTrigger;
            if (trigger.configs == null)
            {
                Log($"haste-config reason={reason} slot={slot} skill={FormatSkill(skill, null)} configs=null");
                return;
            }

            for (var i = 0; i < trigger.configs.Length; i++)
            {
                var config = trigger.configs[i];
                if (config == null)
                {
                    Log($"haste-config reason={reason} slot={slot} skill={FormatSkill(skill, null)} config={i} configState=null");
                    continue;
                }

                Log(
                    "haste-config " +
                    $"reason={reason} " +
                    $"slot={slot} " +
                    $"skill={FormatSkill(skill, null)} " +
                    $"config={i} " +
                    $"canReceiveCooldownReduction={config.canReceiveCooldownReduction} " +
                    $"cooldown={FormatFloat(config.cooldownTime)} " +
                    $"maxCharges={config.maxCharges} " +
                    $"addedCharges={config.addedCharges} " +
                    $"startCharges={config.startCharges} " +
                    $"minimumDelay={FormatFloat(config.minimumDelay)} " +
                    $"currentCharge={SafeIndexedInt(trigger.currentCharges, i)} " +
                    $"currentUnscaledCooldown={SafeIndexedFloat(trigger.currentUnscaledCooldownTimes, i)} " +
                    $"currentScaledCooldown={SafeFloat(() => trigger.currentUnscaledCooldownTimes[i] * trigger.GetCooldownTimeMultiplier(i))} " +
                    $"maxCooldownScaled={SafeFloat(() => trigger.GetMaxCooldownTime(i))} " +
                    $"maxCooldownUnscaled={SafeFloat(() => trigger.GetMaxCooldownTime(i, scaled: false))} " +
                    $"multiplier={SafeFloat(() => trigger.GetCooldownTimeMultiplier(i))}");
            }
        }

        private static string FormatNumericFields(Actor actor, int level, Hero hero)
        {
            var parts = new List<string>();
            var type = actor.GetType();

            while (type != null && type != typeof(Actor) && parts.Count < NumericFieldLimit)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (parts.Count >= NumericFieldLimit)
                    {
                        break;
                    }

                    if (!IsInterestingNumericField(field))
                    {
                        continue;
                    }

                    try
                    {
                        var value = field.GetValue(actor);
                        parts.Add(FormatFieldValue(field.Name, value, level, hero));
                    }
                    catch (Exception ex)
                    {
                        parts.Add($"{field.Name}=error:{ex.GetType().Name}");
                    }
                }

                type = type.BaseType;
            }

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        private static bool IsInterestingNumericField(FieldInfo field)
        {
            var type = field.FieldType;
            if (type == typeof(ScalingValue) || type == typeof(StatBonus) || type == typeof(SkillBonus) || type == typeof(CastMethodData))
            {
                return true;
            }

            if (type == typeof(float) || type == typeof(int) || type == typeof(bool))
            {
                var name = field.Name.ToLowerInvariant();
                return name.Contains("damage") || name.Contains("dmg") || name.Contains("cooldown") || name.Contains("duration") ||
                    name.Contains("chance") || name.Contains("radius") || name.Contains("range") || name.Contains("count") ||
                    name.Contains("amount") || name.Contains("scale") || name.Contains("multiplier") || name.Contains("proc") ||
                    name.Contains("charge") || name.Contains("projectile") || name.Contains("hit") || name.Contains("amp");
            }

            return false;
        }

        private static string FormatFieldValue(string name, object value, int level, Hero hero)
        {
            if (value == null)
            {
                return $"{name}=null";
            }

            if (value is ScalingValue scaling)
            {
                var raw = scaling.ToString();
                var evaluated = SafeFloat(() => scaling.GetValue(level, hero));
                return $"{name}=ScalingValue(raw:{raw},value:{evaluated})";
            }

            if (value is CastMethodData cast)
            {
                return $"{name}=Cast(type:{cast.type},range:{FormatFloat(SafeFloatRaw(() => cast.pointData.range))},radius:{FormatFloat(SafeFloatRaw(() => cast.pointData.radius))},angle:{FormatFloat(SafeFloatRaw(() => cast.coneData.angle))},width:{FormatFloat(SafeFloatRaw(() => cast.arrowData.width))},length:{FormatFloat(SafeFloatRaw(() => cast.arrowData.length))})";
            }

            if (value is StatBonus stat)
            {
                return $"{name}=StatBonus(adFlat:{FormatFloat(stat.attackDamageFlat)},apFlat:{FormatFloat(stat.abilityPowerFlat)},hasteFlat:{FormatFloat(stat.abilityHasteFlat)},critChanceFlat:{FormatFloat(stat.critChanceFlat)})";
            }

            if (value is SkillBonus skill)
            {
                return $"{name}=SkillBonus({skill})";
            }

            if (value is float f)
            {
                return $"{name}={FormatFloat(f)}";
            }

            return $"{name}={value}";
        }

        private static string FormatTriggerConfigs(Actor actor)
        {
            var trigger = actor as AbilityTrigger;
            if (trigger == null || trigger.configs == null)
            {
                return "none";
            }

            var parts = new List<string>();
            for (var i = 0; i < trigger.configs.Length; i++)
            {
                var config = trigger.configs[i];
                if (config == null)
                {
                    continue;
                }

                parts.Add($"{i}:cooldown={FormatFloat(config.cooldownTime)},charges={config.maxCharges}+{config.addedCharges},mana={FormatFloat(config.manaCost)},range={FormatFloat(config.effectiveRange)}");
            }

            return parts.Count == 0 ? "none" : string.Join("; ", parts);
        }

        private static int ResolveEffectiveLevel(Actor actor)
        {
            if (actor is Gem gem)
            {
                return gem.effectiveLevel;
            }

            if (actor is SkillTrigger skill)
            {
                return skill.level;
            }

            if (actor is AbilityInstance ability)
            {
                return ability.effectiveLevel;
            }

            return 1;
        }

        private static int GetCandidateFlowId(IItem item)
        {
            var actor = item as Actor;
            if (actor == null)
            {
                return 0;
            }

            var key = actor.GetInstanceID();
            if (!CandidateFlowIds.TryGetValue(key, out var flowId))
            {
                flowId = ++_candidateFlowSequence;
                CandidateFlowIds[key] = flowId;
            }

            return flowId;
        }

        private static void ForgetCandidateFlow(IItem item)
        {
            var actor = item as Actor;
            if (actor != null)
            {
                CandidateFlowIds.Remove(actor.GetInstanceID());
            }
        }

        private static string FormatFlowId(int flowId)
        {
            return flowId > 0 ? flowId.ToString(CultureInfo.InvariantCulture) : "none";
        }

        private static HeroSkill ResolveLocalHeroSkill(Actor actor)
        {
            if (DewPlayer.local != null && DewPlayer.local.hero != null)
            {
                return DewPlayer.local.hero.Skill;
            }

            if (actor is SkillTrigger skill && skill.owner != null)
            {
                return skill.owner.Skill;
            }

            if (actor is Gem gem && gem.owner != null)
            {
                return gem.owner.Skill;
            }

            return null;
        }

        private static string FormatItem(IItem item)
        {
            if (item == null)
            {
                return "null";
            }

            if (item is Gem gem)
            {
                return "Gem:" + FormatGem(gem, null);
            }

            if (item is SkillTrigger skill)
            {
                return "Memory:" + FormatSkill(skill, null) + ":level=" + skill.level;
            }

            var actor = item as Actor;
            return actor != null ? FormatObject(actor) : item.GetType().Name;
        }

        private static string FormatHeroStats(Hero hero)
        {
            if (hero == null)
            {
                return "null";
            }

            return
                "abilityHaste=" + SafeFloat(() => hero.Status.abilityHaste) +
                ":attackDamage=" + SafeFloat(() => hero.Status.attackDamage) +
                ":abilityPower=" + SafeFloat(() => hero.Status.abilityPower) +
                ":attackSpeedMultiplier=" + SafeFloat(() => hero.Status.attackSpeedMultiplier);
        }

        private static string FormatNullableSkillLocation(HeroSkillLocation? location)
        {
            return location.HasValue ? location.Value.ToString() : "null";
        }

        private static string FormatNullableGemLocation(GemLocation? location)
        {
            return location.HasValue ? FormatGemLocation(location.Value) : "null";
        }

        private static string FormatGemLocation(GemLocation location)
        {
            return $"{location.skill}:{location.index}";
        }

        private static string FormatGemSlots(HeroSkill heroSkill)
        {
            if (heroSkill == null)
            {
                return "unavailable";
            }

            var parts = new List<string>();
            foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
            {
                var slotParts = new List<string>();
                foreach (var pair in heroSkill.GetGemsPairInSkill(location))
                {
                    slotParts.Add($"{pair.Key.index}:{FormatGem(pair.Value, heroSkill)}");
                }

                parts.Add($"{location}:max={heroSkill.GetMaxGemCount(location)}:gems={(slotParts.Count == 0 ? "empty" : string.Join("|", slotParts))}");
            }

            return string.Join("; ", parts);
        }

        private static string FormatLegalSkillTargets(HeroSkill heroSkill)
        {
            if (heroSkill == null)
            {
                return "unavailable";
            }

            var parts = new List<string>();
            foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
            {
                parts.Add($"{location}:canReplace={SafeBool(() => heroSkill.CanReplaceSkill(location))}:current={FormatSkill(heroSkill.GetSkill(location), heroSkill)}");
            }

            return string.Join("; ", parts);
        }

        private static string FormatLegalGemTargets(HeroSkill heroSkill, Gem candidate)
        {
            if (heroSkill == null)
            {
                return "unavailable";
            }

            var parts = new List<string>();
            foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
            {
                var emptyIndex = SafeInt(() => heroSkill.GetEmptyGemSlot(location));
                parts.Add($"{location}:emptyIndex={emptyIndex}:max={heroSkill.GetMaxGemCount(location)}");
            }

            if (candidate != null && heroSkill.TryGetEquippedGemOfSameType(candidate.GetType(), out var duplicateLocation, out var duplicateGem))
            {
                parts.Add($"duplicate=mergeTarget:{FormatGemLocation(duplicateLocation)}:{FormatGem(duplicateGem, heroSkill)}:projectedQuality={Gem.GetMergedQuality(candidate.quality, duplicateGem.quality)}");
            }
            else if (candidate != null)
            {
                parts.Add("duplicate=none");
            }

            return string.Join("; ", parts);
        }

        private static string SafeIndexedFloat(float[] values, int index)
        {
            try
            {
                return values != null && index >= 0 && index < values.Length ? FormatFloat(values[index]) : "missing";
            }
            catch (Exception ex)
            {
                return "error:" + ex.GetType().Name;
            }
        }

        private static string SafeIndexedInt(int[] values, int index)
        {
            try
            {
                return values != null && index >= 0 && index < values.Length ? values[index].ToString(CultureInfo.InvariantCulture) : "missing";
            }
            catch (Exception ex)
            {
                return "error:" + ex.GetType().Name;
            }
        }

        private static string ClassifyDamage(Actor actor, Gem abilityGem, SkillTrigger memory)
        {
            var typeName = actor != null ? actor.GetType().Name : "";

            if (abilityGem != null || actor is Gem || typeName.StartsWith("Ai_Gem_", StringComparison.Ordinal))
            {
                return "GEM_DIRECT";
            }

            if (typeName.StartsWith("Se_", StringComparison.Ordinal))
            {
                return "STATUS_DOT";
            }

            if (memory != null)
            {
                return "MEMORY_DIRECT";
            }

            if (typeName.Contains("_Atk_") || typeName.Contains("BasicAttack") || typeName.Contains("VesperMace"))
            {
                return "BASIC_ATTACK";
            }

            if (!string.IsNullOrEmpty(typeName))
            {
                return "OTHER_IDENTIFIED";
            }

            return "UNATTRIBUTED";
        }

        private static string ClassifyTarget(Entity victim)
        {
            if (victim == null)
            {
                return "NO_TARGET";
            }

            var typeName = victim.GetType().Name;

            if (victim is Hero)
            {
                return "HERO";
            }

            if (typeName.StartsWith("Mon_", StringComparison.Ordinal))
            {
                return "MONSTER";
            }

            if (typeName.StartsWith("Sum_", StringComparison.Ordinal))
            {
                return "SUMMON";
            }

            if (typeName.StartsWith("PropEnt_", StringComparison.Ordinal))
            {
                return "PROP";
            }

            return "OTHER_ENTITY";
        }

        private static string ClassifyEligibility(DewPlayer owner, Entity victim, string targetKind)
        {
            if (owner != null && targetKind == "MONSTER")
            {
                return "PLAYER_TO_MONSTER";
            }

            if (owner != null && targetKind == "PROP")
            {
                return "PLAYER_TO_PROP";
            }

            if (owner != null && victim != null && victim.owner == owner)
            {
                return "PLAYER_SELF_OR_OWNED";
            }

            if (owner != null && victim != null && victim.owner != null && victim.owner != owner)
            {
                return "PLAYER_TO_OTHER_PLAYER";
            }

            if (owner != null)
            {
                return "PLAYER_TO_OTHER";
            }

            if (targetKind == "HERO")
            {
                return "NONPLAYER_TO_HERO";
            }

            if (targetKind == "SUMMON")
            {
                return "NONPLAYER_TO_SUMMON";
            }

            if (targetKind == "MONSTER")
            {
                return "NONPLAYER_TO_MONSTER";
            }

            return "UNOWNED_OR_UNKNOWN";
        }

        private static string FormatProcCandidateKey(string damageClass, Actor actor, Gem abilityGem, SkillTrigger memory, Entity caster)
        {
            if (damageClass != "GEM_DIRECT")
            {
                return "not-gem";
            }

            return $"{FormatType(abilityGem)}|actor={FormatType(actor)}|memory={FormatType(memory)}|caster={FormatType(caster)}";
        }

        private static void AddDamage(Dictionary<string, DamageBucket> buckets, string key, float amount, string damageClass)
        {
            if (!buckets.TryGetValue(key, out var bucket))
            {
                bucket = new DamageBucket();
                buckets[key] = bucket;
            }

            bucket.Events++;
            bucket.Amount += amount;
            bucket.LastClass = damageClass;
        }

        private static void LogAggregateSummary()
        {
            Log(
                "damage-aggregate " +
                $"t={FormatFloat(Time.time)} " +
                $"room={_currentRoom} " +
                $"totalEvents={_damageLogCount} " +
                $"detailedEvents={_detailedDamageLogCount} " +
                $"byClass=[{FormatBuckets(DamageByClass)}] " +
                $"byOwner=[{FormatBuckets(DamageByOwner)}] " +
                $"byTargetKind=[{FormatBuckets(DamageByTargetKind)}] " +
                $"byEligibility=[{FormatBuckets(DamageByEligibility)}] " +
                $"gemProcCandidates=[{FormatBuckets(DamageByProcCandidate)}]");
            LogAnalyticsSummary("aggregate-interval");
        }

        private static void LogAnalyticsSummary(string reason)
        {
            var current = _analytics.GetCurrentEncounterSnapshot(Time.time);
            var last = _analytics.GetLastEncounterSnapshot();
            var run = _analytics.GetRunSnapshot();

            Log(
                "analytics-summary " +
                $"reason={reason} " +
                $"current=[{FormatEncounterSnapshot(current)}] " +
                $"last=[{FormatEncounterSnapshot(last)}] " +
                $"run=[{FormatRunSnapshot(run)}]");
        }

        private static void TrySubscribeHeroLifecycle(ClientEventManager manager, string reason)
        {
            if (manager == null)
            {
                return;
            }

            if (_heroLifecycleManager != null && _heroLifecycleManager != manager)
            {
                if (_heroLifecycleManager.OnHeroKnockedOut != null)
                {
                    _heroLifecycleManager.OnHeroKnockedOut.Remove(LogHeroKnockedOut);
                }

                if (_heroLifecycleManager.OnHeroRevive != null)
                {
                    _heroLifecycleManager.OnHeroRevive.Remove(LogHeroRevived);
                }
            }

            _heroLifecycleManager = manager;

            if (manager.OnHeroKnockedOut == null || manager.OnHeroRevive == null)
            {
                Log($"hero lifecycle subscription skipped; OnHeroKnockedOut/OnHeroRevive unavailable; reason={reason}");
                return;
            }

            manager.OnHeroKnockedOut.Remove(LogHeroKnockedOut);
            manager.OnHeroKnockedOut.Add(LogHeroKnockedOut);
            manager.OnHeroRevive.Remove(LogHeroRevived);
            manager.OnHeroRevive.Add(LogHeroRevived);
            Log($"subscribed to ClientEventManager hero lifecycle events; reason={reason}");
        }

        private static void LogHeroLifecycle(string evt, Hero hero)
        {
            Log(
                "timeline " +
                $"event={evt} " +
                $"t={FormatFloat(Time.time)} " +
                $"room={_currentRoom} " +
                $"hero={FormatObject(hero)} " +
                $"owner={FormatPlayer(hero != null ? hero.owner : null)} " +
                $"isLocal={SafeBool(() => hero != null && hero.owner == DewPlayer.local)} " +
                $"isKnockedOut={SafeBool(() => hero != null && hero.isKnockedOut)} " +
                $"hp={(hero != null ? SafeFloat(() => hero.currentHealth) : "null")} " +
                $"aliveHeroes={SafeInt(() => Dew.GetAliveHeroCount())} " +
                $"gamePlayers={SafeInt(() => DewPlayer.gamePlayers.Count)}");
        }

        private static void StartRoomScope(Room room)
        {
            _currentRoom = FormatObject(room);
            _roomDamageLogCount = 0;
            _roomHeroCombatTransitions = 0;
            RoomDamageByClass.Clear();
            RoomDamageByEligibility.Clear();
        }

        private static void LogRoomSummary(string reason)
        {
            Log(
                "room-summary " +
                $"reason={reason} " +
                $"t={FormatFloat(Time.time)} " +
                $"room={_currentRoom} " +
                $"damageEvents={_roomDamageLogCount} " +
                $"heroCombatTransitions={_roomHeroCombatTransitions} " +
                $"byClass=[{FormatBuckets(RoomDamageByClass)}] " +
                $"byEligibility=[{FormatBuckets(RoomDamageByEligibility)}]");
        }

        private static string FormatBuckets(Dictionary<string, DamageBucket> buckets)
        {
            var entries = new List<KeyValuePair<string, DamageBucket>>(buckets);
            entries.Sort((left, right) => right.Value.Amount.CompareTo(left.Value.Amount));

            var parts = new List<string>();
            var shown = 0;
            foreach (var pair in entries)
            {
                if (shown >= BucketDisplayLimit)
                {
                    break;
                }

                parts.Add($"{pair.Key}:events={pair.Value.Events}:damage={FormatFloat(pair.Value.Amount)}");
                shown++;
            }

            if (entries.Count > BucketDisplayLimit)
            {
                parts.Add($"...{entries.Count - BucketDisplayLimit} more");
            }

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        private static string FormatEncounterSnapshot(EncounterDamageSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return "none";
            }

            return
                $"id={snapshot.EncounterId}:active={snapshot.IsActive}:completed={snapshot.IsCompleted}:" +
                $"duration={FormatOptionalFloat(snapshot.Duration)}:durationValidated={snapshot.DurationIsValidated}:" +
                $"players=[{FormatPlayerSnapshots(snapshot.Players)}]:" +
                $"coverage=[{FormatCoverage(snapshot.Coverage)}]:rev={snapshot.Revision}";
        }

        private static string FormatRunSnapshot(RunDamageSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return "none";
            }

            return
                $"id={snapshot.RunId}:combatDuration={FormatOptionalFloat(snapshot.CombatDuration)}:" +
                $"durationValidated={snapshot.DurationIsValidated}:encounters={snapshot.EncounterCount}:" +
                $"players=[{FormatPlayerSnapshots(snapshot.Players)}]:" +
                $"coverage=[{FormatCoverage(snapshot.Coverage)}]:rev={snapshot.Revision}";
        }

        private static string FormatPlayerSnapshots(IReadOnlyList<PlayerDamageSnapshot> players)
        {
            if (players == null || players.Count == 0)
            {
                return "none";
            }

            var parts = new List<string>();
            foreach (var player in players)
            {
                parts.Add(
                    $"{player.PlayerKey}:events={player.Aggregate.HitCount}:damage={FormatFloat(player.Aggregate.Damage)}:" +
                    $"dps={FormatOptionalFloat(player.Dps)}:share={FormatOptionalPercent(player.PartyShare)}:" +
                    $"sourceCoverage=[{FormatSourceCoverage(player.SourceCoverage)}]:" +
                    $"memoryGemCoverage=[{FormatMemoryGemCoverage(player.MemoryGemCoverage)}]:" +
                    $"sources=[{FormatSourceSnapshots(player.Sources)}]:" +
                    $"memories=[{FormatMemorySnapshots(player.Memories)}]:" +
                    $"gems=[{FormatGemSnapshots(player.Gems)}]:" +
                    $"memoryPackages=[{FormatMemoryPackageSnapshots(player.MemoryPackages)}]");
            }

            return string.Join(", ", parts);
        }

        private static string FormatMemorySnapshots(IReadOnlyList<MemoryDamageSnapshot> memories)
        {
            if (memories == null || memories.Count == 0)
            {
                return "none";
            }

            var parts = new List<string>();
            foreach (var memory in memories)
            {
                parts.Add(
                    $"{memory.MemoryKey}:events={memory.DirectHitCount}:direct={FormatFloat(memory.DirectDamage)}:" +
                    $"share={FormatOptionalPercent(memory.PlayerShare)}");
            }

            return string.Join(", ", parts);
        }

        private static string FormatGemSnapshots(IReadOnlyList<GemDamageSnapshot> gems)
        {
            if (gems == null || gems.Count == 0)
            {
                return "none";
            }

            var parts = new List<string>();
            foreach (var gem in gems)
            {
                parts.Add(
                    $"{gem.GemKey}:events={gem.DamageEventCount}:direct={FormatFloat(gem.DirectDamage)}:" +
                    $"share={FormatOptionalPercent(gem.PlayerShare)}");
            }

            return string.Join(", ", parts);
        }

        private static string FormatMemoryPackageSnapshots(IReadOnlyList<MemoryPackageDamageSnapshot> packages)
        {
            if (packages == null || packages.Count == 0)
            {
                return "none";
            }

            var parts = new List<string>();
            foreach (var package in packages)
            {
                parts.Add(
                    $"{package.MemoryKey}:directMemory={FormatFloat(package.DirectMemoryDamage)}:" +
                    $"attachedDirectGem={FormatFloat(package.AttachedDirectGemDamage)}:" +
                    $"otherChild={FormatFloat(package.OtherDirectlyAttributedChildDamage)}:" +
                    $"total={FormatFloat(package.TotalPackageDamage)}:" +
                    $"share={FormatOptionalPercent(package.PlayerShare)}:" +
                    $"children=[{FormatMemoryPackageChildren(package.ChildBreakdown)}]");
            }

            return string.Join(", ", parts);
        }

        private static string FormatMemoryPackageChildren(IReadOnlyList<MemoryPackageChildDamageSnapshot> children)
        {
            if (children == null || children.Count == 0)
            {
                return "none";
            }

            var parts = new List<string>();
            foreach (var child in children)
            {
                parts.Add($"{child.SourceKey}:{FormatOptionalGemKey(child.GemKey)}:events={child.DirectAggregate.HitCount}:damage={FormatFloat(child.DirectAggregate.Damage)}");
            }

            return string.Join(", ", parts);
        }

        private static string FormatSourceSnapshots(IReadOnlyList<SourceDamageSnapshot> sources)
        {
            if (sources == null || sources.Count == 0)
            {
                return "none";
            }

            var parts = new List<string>();
            foreach (var source in sources)
            {
                parts.Add(
                    $"{source.SourceKey}:events={source.Aggregate.HitCount}:damage={FormatFloat(source.Aggregate.Damage)}:" +
                    $"share={FormatOptionalPercent(source.PlayerShare)}:unattributed={source.IsUnattributed}");
            }

            return string.Join(", ", parts);
        }

        private static string FormatSourceCoverage(SourceCoverageSnapshot coverage)
        {
            if (coverage == null)
            {
                return "none";
            }

            return
                $"eligible={FormatFloat(coverage.EligibleDamage)}:" +
                $"attributed={FormatFloat(coverage.AttributedDamage)}:" +
                $"unattributed={FormatFloat(coverage.UnattributedDamage)}:" +
                $"ratio={FormatOptionalPercent(coverage.AttributionCoverageRatio)}";
        }

        private static string FormatMemoryGemCoverage(MemoryGemCoverageSnapshot coverage)
        {
            if (coverage == null)
            {
                return "none";
            }

            return
                $"eligible={FormatFloat(coverage.EligibleDamage)}:" +
                $"directMemory={FormatFloat(coverage.DirectMemoryDamage)}:" +
                $"memoryIdentified={FormatFloat(coverage.MemoryIdentifiedDamage)}:" +
                $"memoryRatio={FormatOptionalPercent(coverage.MemoryIdentityCoverageRatio)}:" +
                $"directGem={FormatFloat(coverage.DirectGemDamage)}:" +
                $"gemIdentified={FormatFloat(coverage.GemIdentifiedDamage)}:" +
                $"gemRatio={FormatOptionalPercent(coverage.GemIdentityCoverageRatio)}:" +
                $"packageAssignedGem={FormatFloat(coverage.PackageAssignableGemDamage)}:" +
                $"packageUnknownGem={FormatFloat(coverage.PackageRelationshipUnknownGemDamage)}:" +
                $"packageAssignedChild={FormatFloat(coverage.PackageAssignableChildDamage)}:" +
                $"packageUnknownChild={FormatFloat(coverage.PackageRelationshipUnknownChildDamage)}:" +
                $"packageRatio={FormatOptionalPercent(coverage.PackageAssignmentCoverageRatio)}";
        }

        private static string FormatCoverage(DamageCoverageSnapshot coverage)
        {
            if (coverage == null)
            {
                return "none";
            }

            var hostileCoverageDelta = coverage.EligibleHostileDamage - coverage.PlayerOwnedHostileDamage - coverage.UnknownOwnerHostileDamage;
            return
                $"events={coverage.TotalObservedEvents}:hostileEvents={coverage.EligibleHostileEvents}:" +
                $"observed={FormatFloat(coverage.TotalObservedDamage)}:hostile={FormatFloat(coverage.EligibleHostileDamage)}:" +
                $"playerOwnedHostile={FormatFloat(coverage.PlayerOwnedHostileDamage)}:" +
                $"unknownOwnerHostile={FormatFloat(coverage.UnknownOwnerHostileDamage)}:" +
                $"sourceAttributedHostile={FormatFloat(coverage.SourceAttributedHostileDamage)}:" +
                $"unattributedSourceHostile={FormatFloat(coverage.UnattributedSourceHostileDamage)}:" +
                $"sourceCoverage={FormatOptionalPercent(coverage.SourceAttributionCoverageRatio)}:" +
                $"hostileCoverageDelta={FormatFloat(hostileCoverageDelta)}";
        }

        private static string FormatOptionalFloat(float? value)
        {
            return value.HasValue ? FormatFloat(value.Value) : "unknown";
        }

        private static string FormatOptionalPercent(float? value)
        {
            return value.HasValue ? FormatFloat(value.Value * 100f) + "%" : "unknown";
        }

        private static DewPlayer ResolveOwner(Actor actor)
        {
            var current = actor;
            var hops = 0;

            while (current != null && hops < ParentChainLimit)
            {
                if (current is AbilityInstance ability)
                {
                    var caster = ability.info.caster;
                    if (caster != null && caster.owner != null)
                    {
                        return caster.owner;
                    }
                }

                if (current is Entity entity && entity.owner != null)
                {
                    return entity.owner;
                }

                var firstEntity = current.firstEntity;
                if (firstEntity != null && firstEntity.owner != null)
                {
                    return firstEntity.owner;
                }

                current = current.parentActor;
                hops++;
            }

            return null;
        }

        private static PlayerKey? ResolvePlayerKey(DewPlayer player)
        {
            if (player == null)
            {
                return null;
            }

            var stableId = !string.IsNullOrEmpty(player.guid) ? player.guid : player.playerNameRaw;
            if (string.IsNullOrEmpty(stableId))
            {
                stableId = player.GetInstanceID().ToString(CultureInfo.InvariantCulture);
            }

            return new PlayerKey(stableId, player == DewPlayer.local);
        }

        private static TargetRelationship MapTargetRelationship(string eligibility, string targetKind)
        {
            if (targetKind == "MONSTER")
            {
                return TargetRelationship.Hostile;
            }

            if (eligibility == "PLAYER_SELF_OR_OWNED")
            {
                return TargetRelationship.SelfOrOwned;
            }

            if (eligibility == "PLAYER_TO_OTHER_PLAYER")
            {
                return TargetRelationship.Friendly;
            }

            if (targetKind == "PROP")
            {
                return TargetRelationship.Environment;
            }

            return TargetRelationship.Unknown;
        }

        private static DamageSourceCategory MapSourceCategory(string damageClass)
        {
            switch (damageClass)
            {
                case "MEMORY_DIRECT":
                    return DamageSourceCategory.MemoryDirect;
                case "GEM_DIRECT":
                    return DamageSourceCategory.GemDirect;
                case "BASIC_ATTACK":
                    return DamageSourceCategory.BasicAttack;
                case "STATUS_DOT":
                    return DamageSourceCategory.StatusDot;
                case "OTHER_IDENTIFIED":
                    return DamageSourceCategory.OtherIdentified;
                default:
                    return DamageSourceCategory.Unattributed;
            }
        }

        private static SourceKey ResolveSourceKey(
            string damageClass,
            DamageSourceCategory sourceCategory,
            Actor actor,
            Gem abilityGem,
            SkillTrigger memory)
        {
            if (sourceCategory == DamageSourceCategory.Unattributed)
            {
                return SourceKey.ForCategory(DamageSourceCategory.Unattributed);
            }

            switch (damageClass)
            {
                case "MEMORY_DIRECT":
                    return BuildSourceKey(sourceCategory, memory != null ? memory : actor);
                case "GEM_DIRECT":
                    return BuildSourceKey(sourceCategory, abilityGem != null ? (Actor)abilityGem : actor);
                case "BASIC_ATTACK":
                    return new SourceKey(sourceCategory, "BASIC_ATTACK", "Basic Attack");
                case "STATUS_DOT":
                case "OTHER_IDENTIFIED":
                    return BuildSourceKey(sourceCategory, actor);
                default:
                    return SourceKey.ForCategory(DamageSourceCategory.Unattributed);
            }
        }

        private static MemoryKey? ResolveMemoryKey(DamageSourceCategory sourceCategory, PlayerKey? playerKey, Actor memoryActor)
        {
            if (sourceCategory != DamageSourceCategory.MemoryDirect || !playerKey.HasValue)
            {
                return null;
            }

            return BuildMemoryKey(playerKey.Value, memoryActor);
        }

        private static GemKey? ResolveGemKey(DamageSourceCategory sourceCategory, PlayerKey? playerKey, Actor gemActor)
        {
            if (sourceCategory != DamageSourceCategory.GemDirect || !playerKey.HasValue)
            {
                return null;
            }

            return BuildGemKey(playerKey.Value, gemActor);
        }

        private static MemoryKey? ResolveOriginatingMemoryKey(DamageSourceCategory sourceCategory, PlayerKey? playerKey, SkillTrigger memory)
        {
            if (!playerKey.HasValue || memory == null)
            {
                return null;
            }

            if (sourceCategory != DamageSourceCategory.MemoryDirect &&
                sourceCategory != DamageSourceCategory.GemDirect &&
                sourceCategory != DamageSourceCategory.StatusDot)
            {
                return null;
            }

            return BuildMemoryKey(playerKey.Value, memory);
        }

        private static MemoryKey? BuildMemoryKey(PlayerKey playerKey, Actor actor)
        {
            if (actor == null)
            {
                return null;
            }

            var typeName = actor.GetType().Name;
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            return new MemoryKey(playerKey, typeName, FormatSourceDisplayName(DamageSourceCategory.MemoryDirect, typeName));
        }

        private static GemKey? BuildGemKey(PlayerKey playerKey, Actor actor)
        {
            if (actor == null)
            {
                return null;
            }

            var typeName = actor.GetType().Name;
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            return new GemKey(playerKey, typeName, FormatSourceDisplayName(DamageSourceCategory.GemDirect, typeName));
        }

        private static SourceKey BuildSourceKey(DamageSourceCategory sourceCategory, Actor actor)
        {
            if (actor == null)
            {
                return SourceKey.ForCategory(DamageSourceCategory.Unattributed);
            }

            var typeName = actor.GetType().Name;
            if (string.IsNullOrEmpty(typeName))
            {
                return SourceKey.ForCategory(DamageSourceCategory.Unattributed);
            }

            return new SourceKey(sourceCategory, typeName, FormatSourceDisplayName(sourceCategory, typeName));
        }

        private static string FormatOptionalMemoryKey(MemoryKey? key)
        {
            return key.HasValue ? key.Value.ToString() : "unknown";
        }

        private static string FormatOptionalGemKey(GemKey? key)
        {
            return key.HasValue ? key.Value.ToString() : "unknown";
        }

        private static string FormatSourceDisplayName(DamageSourceCategory sourceCategory, string typeName)
        {
            if (sourceCategory == DamageSourceCategory.Unattributed)
            {
                return "Unattributed";
            }

            if (string.IsNullOrEmpty(typeName))
            {
                return sourceCategory.ToString();
            }

            return HumanizeSourceName(typeName);
        }

        private static string HumanizeSourceName(string typeName)
        {
            var name = typeName;
            string[] prefixes = { "Ai_Gem_", "Skill_", "Mem_", "Gem_", "Se_" };
            for (var i = 0; i < prefixes.Length; i++)
            {
                if (name.StartsWith(prefixes[i], StringComparison.Ordinal))
                {
                    name = name.Substring(prefixes[i].Length);
                    break;
                }
            }

            return name.Replace('_', ' ');
        }

        private static T FindParent<T>(Actor actor) where T : Actor
        {
            var current = actor;
            var hops = 0;

            while (current != null && hops < ParentChainLimit)
            {
                if (current is T typed)
                {
                    return typed;
                }

                current = current.parentActor;
                hops++;
            }

            return null;
        }

        private static string FormatSkill(SkillTrigger skill, HeroSkill heroSkill)
        {
            if (skill == null)
            {
                return "null";
            }

            var location = "unknown";
            if (heroSkill == null && skill.owner != null)
            {
                heroSkill = skill.owner.Skill;
            }

            if (heroSkill != null && heroSkill.TryGetSkillLocation(skill, out var skillLocation))
            {
                location = skillLocation.ToString();
            }

            return $"{skill.GetType().Name}@{location}";
        }

        private static string FormatGem(Gem gem, HeroSkill heroSkill)
        {
            if (gem == null)
            {
                return "null";
            }

            var location = "unknown";
            if (heroSkill != null && heroSkill.TryGetGemLocation(gem, out var gemLocation))
            {
                location = $"{gemLocation.skill}:{gemLocation.index}";
            }
            else if (gem.owner != null && gem.owner.Skill != null && gem.owner.Skill.TryGetGemLocation(gem, out var ownedLocation))
            {
                location = $"{ownedLocation.skill}:{ownedLocation.index}";
            }

            return $"{gem.GetType().Name}@{location}:quality={gem.quality}:effectiveLevel={gem.effectiveLevel}";
        }

        private static string FormatGems(IEnumerable<Gem> gems)
        {
            if (gems == null)
            {
                return "null";
            }

            var parts = new List<string>();
            foreach (var gem in gems)
            {
                parts.Add(FormatGem(gem, null));
            }

            return parts.Count == 0 ? "empty" : string.Join(", ", parts);
        }

        private static string FormatSkills(HeroSkill heroSkill)
        {
            if (heroSkill == null)
            {
                return "unavailable";
            }

            var parts = new List<string>();
            foreach (HeroSkillLocation location in Enum.GetValues(typeof(HeroSkillLocation)))
            {
                parts.Add($"{location}:{FormatSkill(heroSkill.GetSkill(location), heroSkill)}");
            }

            return string.Join(", ", parts);
        }

        private static string FormatPlayer(DewPlayer player)
        {
            if (player == null)
            {
                return "null";
            }

            return $"{player.playerNameRaw}/{player.guid}/{player.state}";
        }

        private static string FormatObject(UnityObject obj)
        {
            if (obj == null)
            {
                return "null";
            }

            return $"{obj.GetType().Name}:{obj.name}";
        }

        private static string FormatType(UnityObject obj)
        {
            return obj != null ? obj.GetType().Name : "null";
        }

        private static string FormatParentChain(Actor actor)
        {
            if (actor == null)
            {
                return "null";
            }

            var parts = new List<string>();
            var current = actor;
            var hops = 0;

            while (current != null && hops < ParentChainLimit)
            {
                parts.Add(FormatObject(current));
                current = current.parentActor;
                hops++;
            }

            if (current != null)
            {
                parts.Add("...");
            }

            return string.Join(" -> ", parts);
        }

        private static void LogObjectsOfType<T>(string label) where T : UnityObject
        {
            var objects = Resources.FindObjectsOfTypeAll(typeof(T));
            if (objects == null || objects.Length == 0)
            {
                Log($"hud {label}: none");
                return;
            }

            foreach (var obj in objects)
            {
                if (obj is Component component)
                {
                    Log($"hud {label}: {FormatComponent(component)}");
                }
                else
                {
                    Log($"hud {label}: {FormatObject(obj)}");
                }
            }
        }

        private static string FormatComponent(Component component)
        {
            var transform = component.transform;
            var builder = new StringBuilder(256);
            builder.Append(component.GetType().Name);
            builder.Append(" path=").Append(GetTransformPath(transform));
            builder.Append(" activeSelf=").Append(component.gameObject.activeSelf);
            builder.Append(" activeInHierarchy=").Append(component.gameObject.activeInHierarchy);

            if (transform is RectTransform rect)
            {
                builder.Append(" anchorMin=").Append(rect.anchorMin);
                builder.Append(" anchorMax=").Append(rect.anchorMax);
                builder.Append(" pivot=").Append(rect.pivot);
                builder.Append(" anchoredPos=").Append(rect.anchoredPosition);
                builder.Append(" sizeDelta=").Append(rect.sizeDelta);
            }

            return builder.ToString();
        }

        private static string GetTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return "null";
            }

            var parts = new List<string>();
            var current = transform;

            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string SafeFloat(Func<float> read)
        {
            try
            {
                return FormatFloat(read());
            }
            catch (Exception ex)
            {
                return "error:" + ex.GetType().Name;
            }
        }

        private static float SafeFloatRaw(Func<float> read)
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

        private static bool SafeBool(Func<bool> read)
        {
            try
            {
                return read();
            }
            catch
            {
                return false;
            }
        }

        private static int SafeInt(Func<int> read)
        {
            try
            {
                return read();
            }
            catch
            {
                return -1;
            }
        }

        private static void Log(string message)
        {
            if (Enabled)
            {
                Debug.Log("[DamageAnalyzer] " + message);
            }
        }
    }

    internal sealed class DamageBucket
    {
        internal int Events;
        internal float Amount;
        internal string LastClass;
    }

    internal sealed class DamageAnalyzerOverlay : MonoBehaviour
    {
        private const float DisplaySeconds = 4f;

        private static DamageAnalyzerOverlay _instance;
        private static string _message = "";
        private static float _expiresAt;
        private static GUIStyle _style;
        private static GUIStyle _boxStyle;

        internal static void EnsureCreated()
        {
            if (_instance != null)
            {
                return;
            }

            var obj = new GameObject("DamageAnalyzerOverlay");
            UnityObject.DontDestroyOnLoad(obj);
            _instance = obj.AddComponent<DamageAnalyzerOverlay>();
        }

        internal static void DestroyOverlay()
        {
            if (_instance == null)
            {
                return;
            }

            var obj = _instance.gameObject;
            _instance = null;
            if (obj != null)
            {
                UnityObject.Destroy(obj);
            }
        }

        internal static void Show(string message)
        {
            EnsureCreated();
            _message = message;
            _expiresAt = Time.unscaledTime + DisplaySeconds;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_message) || Time.unscaledTime > _expiresAt)
            {
                return;
            }

            EnsureStyles();

            var width = Mathf.Min(520f, Screen.width - 32f);
            var height = 72f;
            var rect = new Rect(16f, 16f, width, height);
            GUI.Box(rect, GUIContent.none, _boxStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, rect.height - 20f), _message, _style);
        }

        private static void EnsureStyles()
        {
            if (_style != null && _boxStyle != null)
            {
                return;
            }

            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = true
            };

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = Texture2D.grayTexture }
            };
        }
    }

    [HarmonyPatch(typeof(UI_TooltipManager), nameof(UI_TooltipManager.ShowGemEquipTooltip))]
    internal static class DamageAnalyzerGemTooltipPatch
    {
        private static void Postfix(SkillTrigger skill, Gem prevGem, Gem newGem)
        {
            DamageAnalyzerDiagnostics.LogGemEquipTooltip(skill, prevGem, newGem);
        }
    }

    [HarmonyPatch(typeof(UI_TooltipManager), nameof(UI_TooltipManager.ShowSkillEquipTooltip))]
    internal static class DamageAnalyzerSkillTooltipPatch
    {
        private static void Postfix(SkillTrigger prevSkill, SkillTrigger newSkill)
        {
            DamageAnalyzerDiagnostics.LogSkillEquipTooltip(prevSkill, newSkill);
        }
    }

    [HarmonyPatch(typeof(EditSkillManager), nameof(EditSkillManager.StartEquipGem))]
    internal static class DamageAnalyzerStartEquipGemPatch
    {
        private static void Postfix(Gem gem)
        {
            DamageAnalyzerDiagnostics.LogStartEquipGem(gem);
        }
    }

    [HarmonyPatch(typeof(EditSkillManager), nameof(EditSkillManager.StartEquipSkill))]
    internal static class DamageAnalyzerStartEquipSkillPatch
    {
        private static void Postfix(SkillTrigger skill)
        {
            DamageAnalyzerDiagnostics.LogStartEquipSkill(skill);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "OnHoldingObjectChanged")]
    internal static class DamageAnalyzerHoldingObjectChangedPatch
    {
        private static void Prefix(HeroSkill __instance, IItem oldObject, IItem newObject)
        {
            DamageAnalyzerDiagnostics.LogHoldingObjectChanged(__instance, oldObject, newObject);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "UserCode_RpcInvokeOnSkillPickup__SkillTrigger")]
    internal static class DamageAnalyzerSkillPickupPatch
    {
        private static void Prefix(HeroSkill __instance, SkillTrigger skill)
        {
            DamageAnalyzerDiagnostics.LogSkillPickup(__instance, skill);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "UserCode_RpcInvokeOnGemPickup__Gem")]
    internal static class DamageAnalyzerGemPickupPatch
    {
        private static void Prefix(HeroSkill __instance, Gem gem)
        {
            DamageAnalyzerDiagnostics.LogGemPickup(__instance, gem);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "UserCode_RpcInvokeOnSkillDrop__SkillTrigger")]
    internal static class DamageAnalyzerSkillDropPatch
    {
        private static void Prefix(HeroSkill __instance, SkillTrigger skill)
        {
            DamageAnalyzerDiagnostics.LogSkillDrop(__instance, skill);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "UserCode_RpcInvokeOnGemDrop__Gem")]
    internal static class DamageAnalyzerGemDropPatch
    {
        private static void Prefix(HeroSkill __instance, Gem gem)
        {
            DamageAnalyzerDiagnostics.LogGemDrop(__instance, gem);
        }
    }

    [HarmonyPatch(typeof(EditSkillManager), nameof(EditSkillManager.SetClientState_SetSkillSlot))]
    internal static class DamageAnalyzerSkillSlotClientStatePatch
    {
        private static void Prefix(HeroSkillLocation type, SkillTrigger skill)
        {
            DamageAnalyzerDiagnostics.LogSkillClientSlotSelected(type, skill);
        }
    }

    [HarmonyPatch(typeof(EditSkillManager), nameof(EditSkillManager.SetClientState_SetGemSlot))]
    internal static class DamageAnalyzerGemSlotClientStatePatch
    {
        private static void Prefix(GemLocation loc, Gem gem)
        {
            DamageAnalyzerDiagnostics.LogGemClientSlotSelected(loc, gem);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "UserCode_RpcInvokeOnSkillEquip__SkillTrigger")]
    internal static class DamageAnalyzerSkillEquipAppliedPatch
    {
        private static void Postfix(HeroSkill __instance, SkillTrigger skill)
        {
            DamageAnalyzerDiagnostics.LogSkillEquipApplied(__instance, skill);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "UserCode_RpcInvokeOnGemEquip__Gem")]
    internal static class DamageAnalyzerGemEquipAppliedPatch
    {
        private static void Postfix(HeroSkill __instance, Gem gem)
        {
            DamageAnalyzerDiagnostics.LogGemEquipApplied(__instance, gem);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "UserCode_RpcInvokeOnSkillUnequip__SkillTrigger")]
    internal static class DamageAnalyzerSkillUnequipAppliedPatch
    {
        private static void Postfix(HeroSkill __instance, SkillTrigger skill)
        {
            DamageAnalyzerDiagnostics.LogSkillUnequipApplied(__instance, skill);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), "UserCode_RpcInvokeOnGemUnequip__Gem")]
    internal static class DamageAnalyzerGemUnequipAppliedPatch
    {
        private static void Postfix(HeroSkill __instance, Gem gem)
        {
            DamageAnalyzerDiagnostics.LogGemUnequipApplied(__instance, gem);
        }
    }

    [HarmonyPatch(typeof(EditSkillManager), nameof(EditSkillManager.EndEdit))]
    internal static class DamageAnalyzerEndEditPatch
    {
        private static void Prefix(EditSkillManager __instance)
        {
            DamageAnalyzerDiagnostics.LogEditEnded(__instance);
        }
    }

    [HarmonyPatch(typeof(HeroSkill), nameof(HeroSkill.MergeGem))]
    internal static class DamageAnalyzerGemMergePatch
    {
        private static void Prefix(HeroSkill __instance, Gem victim, Gem receivingGem, ref DamageAnalyzerDiagnostics.MergeGemState __state)
        {
            __state = DamageAnalyzerDiagnostics.LogGemMergeBegin(__instance, victim, receivingGem);
        }

        private static void Postfix(HeroSkill __instance, Gem victim, Gem receivingGem, DamageAnalyzerDiagnostics.MergeGemState __state)
        {
            DamageAnalyzerDiagnostics.LogGemMergeEnd(__instance, victim, receivingGem, __state);
        }
    }

    [HarmonyPatch(typeof(ClientEventManager), "UserCode_InvokeOnGemMergeUpgraded__Hero__Gem")]
    internal static class DamageAnalyzerGemMergeConfirmedPatch
    {
        private static void Postfix(Hero h, Gem g)
        {
            DamageAnalyzerDiagnostics.LogGemMergeClientConfirmed(h, g);
        }
    }

    [HarmonyPatch(typeof(ClientEventManager), "UserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour")]
    internal static class DamageAnalyzerItemUpgradeConfirmedPatch
    {
        private static void Postfix(Hero h, NetworkBehaviour nb)
        {
            DamageAnalyzerDiagnostics.LogItemUpgradeConfirmed(h, nb);
        }
    }

    [HarmonyPatch(typeof(Shrine_UpgradeWell), "OnActivateEditSkill", new Type[] { typeof(DewPlayer), typeof(HeroSkillLocation), typeof(SkillTrigger) })]
    internal static class DamageAnalyzerMemoryUpgradePatch
    {
        private static void Prefix(SkillTrigger target, ref DamageAnalyzerDiagnostics.UpgradeState __state)
        {
            __state = DamageAnalyzerDiagnostics.LogUpgradeBegin(target);
        }

        private static void Postfix(bool __result, DamageAnalyzerDiagnostics.UpgradeState __state)
        {
            if (__result)
            {
                DamageAnalyzerDiagnostics.LogUpgradeEnd(__state);
            }
        }
    }

    [HarmonyPatch(typeof(Shrine_UpgradeWell), "OnActivateEditSkill", new Type[] { typeof(DewPlayer), typeof(GemLocation), typeof(Gem) })]
    internal static class DamageAnalyzerGemUpgradePatch
    {
        private static void Prefix(Gem target, ref DamageAnalyzerDiagnostics.UpgradeState __state)
        {
            __state = DamageAnalyzerDiagnostics.LogUpgradeBegin(target);
        }

        private static void Postfix(bool __result, DamageAnalyzerDiagnostics.UpgradeState __state)
        {
            if (__result)
            {
                DamageAnalyzerDiagnostics.LogUpgradeEnd(__state);
            }
        }
    }

    [HarmonyPatch(typeof(ClientEventManager), "OnStart")]
    internal static class DamageAnalyzerClientEventManagerOnStartPatch
    {
        private static void Postfix(ClientEventManager __instance)
        {
            DamageAnalyzerDiagnostics.TrySubscribeDamage(__instance, "ClientEventManager.OnStart");
        }
    }

    [HarmonyPatch(typeof(UI_InGame_SkillButtons), "Start")]
    internal static class DamageAnalyzerSkillButtonsStartPatch
    {
        private static void Postfix()
        {
            DamageAnalyzerDiagnostics.LogHudHierarchy("UI_InGame_SkillButtons.Start");
        }
    }

    [HarmonyPatch(typeof(UI_InGame_RoomModDisplay), "Awake")]
    internal static class DamageAnalyzerRoomModDisplayAwakePatch
    {
        private static void Postfix()
        {
            DamageAnalyzerDiagnostics.LogHudHierarchy("UI_InGame_RoomModDisplay.Awake");
        }
    }

    [HarmonyPatch(typeof(UI_InGame_HeroInfoBar), "Start")]
    internal static class DamageAnalyzerHeroInfoBarStartPatch
    {
        private static void Postfix()
        {
            DamageAnalyzerDiagnostics.LogHudHierarchy("UI_InGame_HeroInfoBar.Start");
        }
    }

    [HarmonyPatch(typeof(Room), nameof(Room.StartRoom))]
    internal static class DamageAnalyzerRoomStartPatch
    {
        private static void Postfix(Room __instance)
        {
            DamageAnalyzerDiagnostics.LogRoomTimeline("ROOM_START", __instance);
        }
    }

    [HarmonyPatch(typeof(Room), nameof(Room.ClearRoom))]
    internal static class DamageAnalyzerRoomClearPatch
    {
        private static void Prefix(Room __instance)
        {
            DamageAnalyzerDiagnostics.LogRoomTimeline("ROOM_CLEAR_BEGIN", __instance);
        }
    }

    [HarmonyPatch(typeof(Room), nameof(Room.StopRoom))]
    internal static class DamageAnalyzerRoomStopPatch
    {
        private static void Postfix(Room __instance)
        {
            DamageAnalyzerDiagnostics.LogRoomTimeline("ROOM_STOP", __instance);
        }
    }

    [HarmonyPatch(typeof(Room), "UserCode_RpcInvokeRoomStart")]
    internal static class DamageAnalyzerClientRoomStartPatch
    {
        private static void Postfix(Room __instance)
        {
            if (__instance != null && !__instance.isServer)
            {
                DamageAnalyzerDiagnostics.LogRoomTimeline("ROOM_START", __instance);
            }
        }
    }

    [HarmonyPatch(typeof(Room), "UserCode_RpcInvokeOnRoomClear")]
    internal static class DamageAnalyzerClientRoomClearPatch
    {
        private static void Prefix(Room __instance)
        {
            if (__instance != null && !__instance.isServer)
            {
                DamageAnalyzerDiagnostics.LogRoomTimeline("ROOM_CLEAR_BEGIN", __instance);
            }
        }
    }

    [HarmonyPatch(typeof(Room), "UserCode_RpcInvokeRoomStop")]
    internal static class DamageAnalyzerClientRoomStopPatch
    {
        private static void Postfix(Room __instance)
        {
            if (__instance != null && !__instance.isServer)
            {
                DamageAnalyzerDiagnostics.LogRoomTimeline("ROOM_STOP", __instance);
            }
        }
    }

    [HarmonyPatch(typeof(Hero), "OnIsInCombatChanged")]
    internal static class DamageAnalyzerHeroCombatPatch
    {
        private static void Postfix(Hero __instance, bool newVal)
        {
            DamageAnalyzerDiagnostics.LogHeroCombatChanged(__instance, newVal);
        }
    }

    [HarmonyPatch(typeof(GameResultManager), "UserCode_RpcRegisterResult__DewGameResult__Boolean")]
    internal static class DamageAnalyzerGameResultRegisteredPatch
    {
        private static void Postfix(bool didGameEnd)
        {
            DamageAnalyzerDiagnostics.LogGameResultRegistered(didGameEnd);
        }
    }

    [HarmonyPatch(typeof(DewNetworkManager), "StartSession")]
    internal static class DamageAnalyzerNetworkStartSessionPatch
    {
        private static void Prefix()
        {
            DamageAnalyzerDiagnostics.LogGameSessionStarting();
        }
    }

    [HarmonyPatch(typeof(DewInput), nameof(DewInput.IsGameRelatedMouseInputValid))]
    internal static class DamageAnalyzerGameMouseInputPatch
    {
        private static bool Prefix(MouseButton button, ref bool __result)
        {
            if (button != MouseButton.Left && button != MouseButton.Right)
            {
                return true;
            }

            if (!DamageAnalyzerDiagnostics.Enabled || !DamageAnalyticsUiInput.IsPointerOverModPanel())
            {
                return true;
            }

            if (!DamageAnalyticsUiVisibility.ShouldShowAnalyticsUi())
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}

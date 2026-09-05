using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum RunAnalyticsPanelMode
    {
        Memory,
        Gem,
        Package
    }

    internal sealed class RunAnalyticsPanelView
    {
        internal RunAnalyticsPanelView(
            RunDamageSnapshot run,
            AnalyticsPlayerSelection selectedPlayerSelection,
            PlayerDamageSnapshot localPlayer,
            PlayerDamageSnapshot partnerPlayer,
            PlayerDamageSnapshot selectedPlayer,
            RunAnalyticsPanelMode mode)
        {
            Run = run;
            SelectedPlayerSelection = selectedPlayerSelection;
            LocalPlayer = localPlayer;
            PartnerPlayer = partnerPlayer;
            SelectedPlayer = selectedPlayer;
            Mode = mode;
            Memories = selectedPlayer != null && selectedPlayer.Memories != null
                ? selectedPlayer.Memories
                : Array.Empty<MemoryDamageSnapshot>();
            Gems = selectedPlayer != null && selectedPlayer.Gems != null
                ? selectedPlayer.Gems
                : Array.Empty<GemDamageSnapshot>();
            MemoryPackages = selectedPlayer != null && selectedPlayer.MemoryPackages != null
                ? selectedPlayer.MemoryPackages
                : Array.Empty<MemoryPackageDamageSnapshot>();
        }

        internal RunDamageSnapshot Run { get; }
        internal AnalyticsPlayerSelection SelectedPlayerSelection { get; }
        internal PlayerDamageSnapshot LocalPlayer { get; }
        internal PlayerDamageSnapshot PartnerPlayer { get; }
        internal PlayerDamageSnapshot SelectedPlayer { get; }
        internal RunAnalyticsPanelMode Mode { get; }
        internal IReadOnlyList<MemoryDamageSnapshot> Memories { get; }
        internal IReadOnlyList<GemDamageSnapshot> Gems { get; }
        internal IReadOnlyList<MemoryPackageDamageSnapshot> MemoryPackages { get; }
        internal bool HasPartner
        {
            get { return PartnerPlayer != null; }
        }
        internal string SelectedPlayerLabel
        {
            get { return SelectedPlayerSelection == AnalyticsPlayerSelection.Partner ? "Partner" : "You"; }
        }
    }

    internal static class RunAnalyticsPanelPresenter
    {
        internal const string PlayerColumnHeader = "Player";
        internal const string DamageColumnHeader = "Damage";
        internal const string ShareColumnHeader = "Share";
        internal const string DpsColumnHeader = "DPS";
        internal const string HitsColumnHeader = "Hits";
        internal const string MemoryPackageColumnHeader = "Memory Package";
        internal const string MemoryColumnHeader = "Memory";
        internal const string ChildrenColumnHeader = "Children";
        internal const string TotalColumnHeader = "Total";

        internal static RunAnalyticsPanelView BuildView(RunDamageSnapshot run, RunAnalyticsPanelMode mode)
        {
            return BuildView(run, mode, AnalyticsPlayerSelection.Local);
        }

        internal static RunAnalyticsPanelView BuildView(
            RunDamageSnapshot run,
            RunAnalyticsPanelMode mode,
            AnalyticsPlayerSelection selectedPlayerSelection)
        {
            if (run == null)
            {
                return null;
            }

            var local = FindLocalPlayer(run.Players);
            var partner = FindPartnerPlayer(run.Players);
            if (selectedPlayerSelection == AnalyticsPlayerSelection.Partner && partner == null)
            {
                selectedPlayerSelection = AnalyticsPlayerSelection.Local;
            }

            var selectedPlayer = selectedPlayerSelection == AnalyticsPlayerSelection.Partner ? partner : local;
            return new RunAnalyticsPanelView(
                run,
                selectedPlayerSelection,
                local,
                partner,
                selectedPlayer,
                mode);
        }

        internal static string GetMemoryLabel(MemoryDamageSnapshot memory)
        {
            return memory != null ? FormatDisplayName(memory.MemoryKey.DisplayName, "Unknown Memory") : "Unknown Memory";
        }

        internal static string GetGemLabel(GemDamageSnapshot gem)
        {
            return gem != null ? FormatDisplayName(gem.GemKey.DisplayName, "Unknown Gem") : "Unknown Gem";
        }

        internal static string GetPackageLabel(MemoryPackageDamageSnapshot package)
        {
            return package != null ? FormatDisplayName(package.MemoryKey.DisplayName, "Unknown Memory") : "Unknown Memory";
        }

        internal static string GetPackageChildLabel(MemoryPackageChildDamageSnapshot child)
        {
            if (child == null)
            {
                return "Unknown Child";
            }

            if (child.GemKey.HasValue)
            {
                return FormatDisplayName(child.GemKey.Value.DisplayName, "Unknown Gem");
            }

            return FormatDisplayName(child.SourceKey.DisplayName, "Unknown Child");
        }

        internal static string GetCoverageLabel(float? ratio)
        {
            if (!ratio.HasValue)
            {
                return "Unknown";
            }

            return ratio.Value >= 0.999f ? "Verified" : "Partial";
        }

        internal static string FormatDamage(float damage)
        {
            return EncounterAnalyticsPanelPresenter.FormatDamage(damage);
        }

        internal static string FormatPercent(float? ratio)
        {
            return EncounterAnalyticsPanelPresenter.FormatPercent(ratio);
        }

        internal static string FormatDps(float? dps)
        {
            return dps.HasValue ? FormatDamage(dps.Value) + "/s" : "-";
        }

        internal static string FormatDuration(float? duration)
        {
            if (!duration.HasValue)
            {
                return "-";
            }

            var totalSeconds = Math.Max(0, (int)Math.Round(duration.Value));
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;
            return minutes.ToString("0") + ":" + seconds.ToString("00");
        }

        internal static IReadOnlyList<MemoryDamageSnapshot> GetDisplayMemories(IReadOnlyList<MemoryDamageSnapshot> memories, int maxRows)
        {
            if (memories == null || memories.Count == 0 || maxRows <= 0)
            {
                return Array.Empty<MemoryDamageSnapshot>();
            }

            if (memories.Count <= maxRows)
            {
                return memories;
            }

            var result = new List<MemoryDamageSnapshot>(maxRows);
            for (var i = 0; i < maxRows; i++)
            {
                result.Add(memories[i]);
            }

            return result.AsReadOnly();
        }

        internal static IReadOnlyList<GemDamageSnapshot> GetDisplayGems(IReadOnlyList<GemDamageSnapshot> gems, int maxRows)
        {
            if (gems == null || gems.Count == 0 || maxRows <= 0)
            {
                return Array.Empty<GemDamageSnapshot>();
            }

            if (gems.Count <= maxRows)
            {
                return gems;
            }

            var result = new List<GemDamageSnapshot>(maxRows);
            for (var i = 0; i < maxRows; i++)
            {
                result.Add(gems[i]);
            }

            return result.AsReadOnly();
        }

        internal static IReadOnlyList<MemoryPackageDamageSnapshot> GetDisplayPackages(IReadOnlyList<MemoryPackageDamageSnapshot> packages, int maxRows)
        {
            if (packages == null || packages.Count == 0 || maxRows <= 0)
            {
                return Array.Empty<MemoryPackageDamageSnapshot>();
            }

            if (packages.Count <= maxRows)
            {
                return packages;
            }

            var result = new List<MemoryPackageDamageSnapshot>(maxRows);
            for (var i = 0; i < maxRows; i++)
            {
                result.Add(packages[i]);
            }

            return result.AsReadOnly();
        }

        private static string FormatDisplayName(string displayName, string fallback)
        {
            return AnalyticsDisplayNameResolver.ToDisplayName(displayName, fallback);
        }

        internal static string GetTabLabel(RunAnalyticsPanelMode mode)
        {
            switch (mode)
            {
                case RunAnalyticsPanelMode.Memory:
                    return "Memory";
                case RunAnalyticsPanelMode.Gem:
                    return "Direct Gems";
                case RunAnalyticsPanelMode.Package:
                    return "Package";
                default:
                    return mode.ToString();
            }
        }

        internal static string FormatDps(float? dps, bool durationIsValidated)
        {
            if (!dps.HasValue)
            {
                return "-";
            }

            return durationIsValidated ? FormatDamage(dps.Value) : "~" + FormatDamage(dps.Value);
        }

        internal static string FormatCoverageSummary(RunAnalyticsPanelMode mode, PlayerDamageSnapshot selectedPlayer)
        {
            if (selectedPlayer == null)
            {
                return "Coverage Unknown";
            }

            var coverage = selectedPlayer.MemoryGemCoverage;
            float? ratio;
            var incompleteDamage = 0f;
            switch (mode)
            {
                case RunAnalyticsPanelMode.Memory:
                    ratio = coverage.MemoryIdentityCoverageRatio;
                    break;
                case RunAnalyticsPanelMode.Gem:
                    ratio = coverage.GemIdentityCoverageRatio;
                    break;
                default:
                    ratio = coverage.PackageAssignmentCoverageRatio;
                    incompleteDamage += coverage.PackageRelationshipUnknownChildDamage;
                    break;
            }

            if (!ratio.HasValue)
            {
                return "Coverage Unknown";
            }

            var label = "Coverage " + FormatPercent(ratio);
            return incompleteDamage > 0f
                ? label + " - " + FormatDamage(incompleteDamage) + " unattributed"
                : label;
        }

        private static PlayerDamageSnapshot FindLocalPlayer(IReadOnlyList<PlayerDamageSnapshot> players)
        {
            if (players == null)
            {
                return null;
            }

            for (var i = 0; i < players.Count; i++)
            {
                if (players[i].PlayerKey.IsLocalPlayer)
                {
                    return players[i];
                }
            }

            return null;
        }

        private static PlayerDamageSnapshot FindPartnerPlayer(IReadOnlyList<PlayerDamageSnapshot> players)
        {
            if (players == null)
            {
                return null;
            }

            for (var i = 0; i < players.Count; i++)
            {
                if (!players[i].PlayerKey.IsLocalPlayer)
                {
                    return players[i];
                }
            }

            return null;
        }
    }

    internal sealed class DamageAnalyticsRunPanel : MonoBehaviour
    {
        internal const float PanelWidthForLayout = 470f;
        private const float PanelWidth = PanelWidthForLayout;
        private const float RowHeight = 26f;
        private const float SectionGap = 8f;
        private const int MaxRows = 8;
        private const int MaxExpandedChildren = 4;

        private static DamageAnalyticsRunPanel _instance;
        private static GUIStyle _panelStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _mutedStyle;
        private static GUIStyle _rightStyle;
        private static GUIStyle _tabStyle;
        private static Texture2D _panelBackground;
        internal const RunAnalyticsPanelMode DefaultMode = RunAnalyticsPanelMode.Package;
        private static RunAnalyticsPanelMode _mode = DefaultMode;
        private static AnalyticsPlayerSelection _selectedPlayerSelection = AnalyticsPlayerSelection.Local;
        private static string _expandedPackageId;
        private static float _currentPanelHeight;

        internal static void EnsureCreated()
        {
            if (_instance != null)
            {
                return;
            }

            var obj = new GameObject("DamageAnalyticsRunPanel");
            UnityObject.DontDestroyOnLoad(obj);
            _instance = obj.AddComponent<DamageAnalyticsRunPanel>();
        }

        internal static void DestroyPanel()
        {
            if (_instance == null)
            {
                return;
            }

            var obj = _instance.gameObject;
            _instance = null;
            _currentPanelHeight = 0f;
            DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Run);
            if (obj != null)
            {
                UnityObject.Destroy(obj);
            }
        }

        private void OnGUI()
        {
            if (!DamageAnalyticsUiVisibility.ShouldShowAnalyticsUi()
                || DamageAnalyticsNativeUiOcclusion.ShouldHideAnalyticsPanels())
            {
                _currentPanelHeight = 0f;
                DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Run);
                return;
            }

            var view = RunAnalyticsPanelPresenter.BuildView(DamageAnalyzerDiagnostics.GetRunSnapshot(), _mode, _selectedPlayerSelection);
            if (view == null)
            {
                _currentPanelHeight = 0f;
                DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Run);
                return;
            }

            EnsureStyles();
            _selectedPlayerSelection = view.SelectedPlayerSelection;

            var height = CalculateHeight(view);
            _currentPanelHeight = height;
            var layout = DamageAnalyticsUiInput.CalculateMidLeftLayout(
                Screen.width,
                Screen.height,
                DamageAnalyticsEncounterPanel.PanelWidthForLayout,
                DamageAnalyticsEncounterPanel.GetCurrentPanelHeight(),
                PanelWidth,
                height);
            var rect = layout.RunRect;

            DamageAnalyticsUiInput.RegisterPanelRect(DamageAnalyticsPanelKind.Run, rect);
            GUI.Box(rect, GUIContent.none, _panelStyle);
            DrawPanel(rect, view);
            DamageAnalyticsUiInput.ConsumeMouseEventsInside(rect);
        }

        internal static float GetCurrentPanelHeight()
        {
            return _currentPanelHeight;
        }

        private static float CalculateHeight(RunAnalyticsPanelView view)
        {
            var height = 214f + RowHeight;
            if (view.PartnerPlayer != null)
            {
                height += RowHeight * 2f;
            }

            switch (view.Mode)
            {
                case RunAnalyticsPanelMode.Memory:
                    height += Math.Max(1, Math.Min(MaxRows, view.Memories.Count)) * RowHeight;
                    break;
                case RunAnalyticsPanelMode.Gem:
                    height += Math.Max(1, Math.Min(MaxRows, view.Gems.Count)) * RowHeight;
                    break;
                case RunAnalyticsPanelMode.Package:
                    height += Math.Max(1, Math.Min(MaxRows, view.MemoryPackages.Count)) * RowHeight;
                    height += CountExpandedChildRows(view) * RowHeight;
                    break;
            }

            if (view.SelectedPlayer != null)
            {
                height += RowHeight;
                if (view.Mode == RunAnalyticsPanelMode.Package && view.SelectedPlayer.MemoryGemCoverage.PackageRelationshipUnknownChildDamage > 0f)
                {
                    height += RowHeight;
                }
            }

            return height;
        }

        private static int CountExpandedChildRows(RunAnalyticsPanelView view)
        {
            if (string.IsNullOrEmpty(_expandedPackageId))
            {
                return 0;
            }

            var packages = RunAnalyticsPanelPresenter.GetDisplayPackages(view.MemoryPackages, MaxRows);
            for (var i = 0; i < packages.Count; i++)
            {
                var package = packages[i];
                if (package.MemoryKey.StableId == _expandedPackageId && package.ChildBreakdown != null)
                {
                    return Math.Min(MaxExpandedChildren, package.ChildBreakdown.Count);
                }
            }

            return 0;
        }

        private static void DrawPanel(Rect rect, RunAnalyticsPanelView view)
        {
            var x = rect.x + 14f;
            var y = rect.y + 12f;
            var width = rect.width - 28f;

            GUI.Label(new Rect(x, y, width, RowHeight), "RUN ANALYTICS", _titleStyle);
            y += RowHeight;
            GUI.Label(new Rect(x, y, width, RowHeight), "Run " + view.Run.RunId + "  Encounters " + view.Run.EncounterCount, _mutedStyle);
            y += RowHeight;

            DrawRunSummaryHeader(x, y, width);
            y += RowHeight;

            DrawPlayerSummary(x, y, width, view);
            y += view.PartnerPlayer != null ? RowHeight * 2f : RowHeight;

            GUI.Label(
                new Rect(x, y, width, RowHeight),
                "Duration " + RunAnalyticsPanelPresenter.FormatDuration(view.Run.CombatDuration) +
                (view.Run.CombatDuration.HasValue && !view.Run.DurationIsValidated ? "  Provisional" : string.Empty),
                _mutedStyle);
            y += RowHeight + SectionGap;

            if (view.HasPartner)
            {
                DrawPlayerSelector(x, y, width, view);
                y += RowHeight + SectionGap;
            }

            DrawTabs(x, y, width);
            y += RowHeight + SectionGap;

            switch (view.Mode)
            {
                case RunAnalyticsPanelMode.Memory:
                    y = DrawMemoryRows(x, y, width, view);
                    break;
                case RunAnalyticsPanelMode.Gem:
                    y = DrawGemRows(x, y, width, view);
                    break;
                case RunAnalyticsPanelMode.Package:
                    y = DrawPackageRows(x, y, width, view);
                    break;
            }

            y += SectionGap;
            DrawCoverage(x, y, width, view.SelectedPlayer);
        }

        private static void DrawPlayerSummary(float x, float y, float width, RunAnalyticsPanelView view)
        {
            DrawSummaryRow(x, y, width, "You", view.LocalPlayer, view.Run.DurationIsValidated);
            if (view.PartnerPlayer != null)
            {
                DrawSummaryRow(x, y + RowHeight, width, "Partner", view.PartnerPlayer, view.Run.DurationIsValidated);
            }
        }

        private static void DrawSummaryRow(float x, float y, float width, string label, PlayerDamageSnapshot player, bool durationIsValidated)
        {
            GUI.Label(new Rect(x, y, width * 0.34f, RowHeight), label, _labelStyle);
            GUI.Label(new Rect(x + width * 0.34f, y, width * 0.22f, RowHeight), player != null ? RunAnalyticsPanelPresenter.FormatDamage(player.Aggregate.Damage) : "-", _rightStyle);
            GUI.Label(new Rect(x + width * 0.57f, y, width * 0.19f, RowHeight), player != null ? RunAnalyticsPanelPresenter.FormatPercent(player.PartyShare) : "-", _rightStyle);
            GUI.Label(new Rect(x + width * 0.77f, y, width * 0.23f, RowHeight), player != null ? RunAnalyticsPanelPresenter.FormatDps(player.Dps, durationIsValidated) : "-", _rightStyle);
        }

        private static void DrawTabs(float x, float y, float width)
        {
            var tabWidth = width / 3f;
            if (GUI.Button(new Rect(x, y, tabWidth - 4f, RowHeight), RunAnalyticsPanelPresenter.GetTabLabel(RunAnalyticsPanelMode.Memory), _tabStyle))
            {
                SelectMode(RunAnalyticsPanelMode.Memory);
            }

            if (GUI.Button(new Rect(x + tabWidth, y, tabWidth - 4f, RowHeight), RunAnalyticsPanelPresenter.GetTabLabel(RunAnalyticsPanelMode.Gem), _tabStyle))
            {
                SelectMode(RunAnalyticsPanelMode.Gem);
            }

            if (GUI.Button(new Rect(x + tabWidth * 2f, y, tabWidth, RowHeight), RunAnalyticsPanelPresenter.GetTabLabel(RunAnalyticsPanelMode.Package), _tabStyle))
            {
                SelectMode(RunAnalyticsPanelMode.Package);
            }
        }

        internal static void SelectMode(RunAnalyticsPanelMode mode)
        {
            if (_mode != mode)
            {
                _expandedPackageId = null;
            }

            _mode = mode;
        }

        internal static RunAnalyticsPanelMode CurrentModeForValidation
        {
            get { return _mode; }
        }

        private static void DrawPlayerSelector(float x, float y, float width, RunAnalyticsPanelView view)
        {
            var buttonWidth = Mathf.Min(86f, (width - 8f) / 2f);
            var chooseLocal = GUI.Toggle(
                new Rect(x, y, buttonWidth, RowHeight),
                view.SelectedPlayerSelection == AnalyticsPlayerSelection.Local,
                "You",
                _tabStyle);

            var previousEnabled = GUI.enabled;
            GUI.enabled = view.PartnerPlayer != null;
            var choosePartner = GUI.Toggle(
                new Rect(x + buttonWidth + 8f, y, buttonWidth, RowHeight),
                view.SelectedPlayerSelection == AnalyticsPlayerSelection.Partner,
                "Partner",
                _tabStyle);

            GUI.enabled = previousEnabled;
            if (chooseLocal && view.SelectedPlayerSelection != AnalyticsPlayerSelection.Local)
            {
                _selectedPlayerSelection = AnalyticsPlayerSelection.Local;
                _expandedPackageId = null;
            }
            else if (choosePartner && view.SelectedPlayerSelection != AnalyticsPlayerSelection.Partner)
            {
                _selectedPlayerSelection = AnalyticsPlayerSelection.Partner;
                _expandedPackageId = null;
            }
        }

        private static float DrawMemoryRows(float x, float y, float width, RunAnalyticsPanelView view)
        {
            GUI.Label(new Rect(x, y, width, RowHeight), view.SelectedPlayerLabel + " Memory Damage", _titleStyle);
            y += RowHeight;
            GUI.Label(new Rect(x, y, width, RowHeight), "Observed Memory events only", _mutedStyle);
            y += RowHeight;
            DrawFourColumnHeader(x, y, width, "Memory", "Damage", "Share", "DPS");
            y += RowHeight;

            var rows = RunAnalyticsPanelPresenter.GetDisplayMemories(view.Memories, MaxRows);
            if (rows.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), GetEmptyRowsText(view, "Memory"), _mutedStyle);
                return y + RowHeight;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                GUI.Label(new Rect(x, y, width * 0.37f, RowHeight), RunAnalyticsPanelPresenter.GetMemoryLabel(row), _labelStyle);
                GUI.Label(new Rect(x + width * 0.37f, y, width * 0.19f, RowHeight), RunAnalyticsPanelPresenter.FormatDamage(row.DirectDamage), _rightStyle);
                GUI.Label(new Rect(x + width * 0.57f, y, width * 0.13f, RowHeight), RunAnalyticsPanelPresenter.FormatPercent(row.PlayerShare), _rightStyle);
                GUI.Label(new Rect(x + width * 0.71f, y, width * 0.10f, RowHeight), row.DirectHitCount.ToString("0"), _rightStyle);
                GUI.Label(new Rect(x + width * 0.82f, y, width * 0.18f, RowHeight), RunAnalyticsPanelPresenter.FormatDps(row.DirectDps, view.Run.DurationIsValidated), _rightStyle);
                y += RowHeight;
            }

            return y;
        }

        private static float DrawGemRows(float x, float y, float width, RunAnalyticsPanelView view)
        {
            GUI.Label(new Rect(x, y, width, RowHeight), view.SelectedPlayerLabel + " Direct Gem Damage", _titleStyle);
            y += RowHeight;
            GUI.Label(new Rect(x, y, width, RowHeight), "Observed Gem events, not modifier uplift", _mutedStyle);
            y += RowHeight;
            DrawFourColumnHeader(x, y, width, "Direct Gem", "Damage", "Share", "DPS");
            y += RowHeight;

            var rows = RunAnalyticsPanelPresenter.GetDisplayGems(view.Gems, MaxRows);
            if (rows.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), GetEmptyRowsText(view, "Gem"), _mutedStyle);
                return y + RowHeight;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                GUI.Label(new Rect(x, y, width * 0.37f, RowHeight), RunAnalyticsPanelPresenter.GetGemLabel(row), _labelStyle);
                GUI.Label(new Rect(x + width * 0.37f, y, width * 0.19f, RowHeight), RunAnalyticsPanelPresenter.FormatDamage(row.DirectDamage), _rightStyle);
                GUI.Label(new Rect(x + width * 0.57f, y, width * 0.13f, RowHeight), RunAnalyticsPanelPresenter.FormatPercent(row.PlayerShare), _rightStyle);
                GUI.Label(new Rect(x + width * 0.71f, y, width * 0.10f, RowHeight), row.DamageEventCount.ToString("0"), _rightStyle);
                GUI.Label(new Rect(x + width * 0.82f, y, width * 0.18f, RowHeight), RunAnalyticsPanelPresenter.FormatDps(row.DirectDps, view.Run.DurationIsValidated), _rightStyle);
                y += RowHeight;
            }

            return y;
        }

        private static float DrawPackageRows(float x, float y, float width, RunAnalyticsPanelView view)
        {
            GUI.Label(new Rect(x, y, width, RowHeight), view.SelectedPlayerLabel + " Memory Package", _titleStyle);
            y += RowHeight;
            GUI.Label(new Rect(x, y, width, RowHeight), "Memory plus directly attributable children", _mutedStyle);
            y += RowHeight;
            DrawPackageHeader(x, y, width);
            y += RowHeight;

            var rows = RunAnalyticsPanelPresenter.GetDisplayPackages(view.MemoryPackages, MaxRows);
            if (rows.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), GetEmptyPackageRowsText(view), _mutedStyle);
                return y + RowHeight;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var id = row.MemoryKey.StableId;
                if (GUI.Button(new Rect(x, y, width * 0.34f, RowHeight), RunAnalyticsPanelPresenter.GetPackageLabel(row), _tabStyle))
                {
                    _expandedPackageId = _expandedPackageId == id ? null : id;
                }

                GUI.Label(new Rect(x + width * 0.34f, y, width * 0.18f, RowHeight), RunAnalyticsPanelPresenter.FormatDamage(row.DirectMemoryDamage), _rightStyle);
                GUI.Label(new Rect(x + width * 0.53f, y, width * 0.18f, RowHeight), RunAnalyticsPanelPresenter.FormatDamage(row.AttributableChildDamage), _rightStyle);
                GUI.Label(new Rect(x + width * 0.72f, y, width * 0.16f, RowHeight), RunAnalyticsPanelPresenter.FormatDamage(row.TotalPackageDamage), _rightStyle);
                GUI.Label(new Rect(x + width * 0.89f, y, width * 0.11f, RowHeight), RunAnalyticsPanelPresenter.FormatPercent(row.PlayerShare), _rightStyle);
                y += RowHeight;

                if (_expandedPackageId == id && row.ChildBreakdown != null)
                {
                    for (var childIndex = 0; childIndex < row.ChildBreakdown.Count && childIndex < MaxExpandedChildren; childIndex++)
                    {
                        var child = row.ChildBreakdown[childIndex];
                        GUI.Label(new Rect(x + 14f, y, width * 0.43f, RowHeight), RunAnalyticsPanelPresenter.GetPackageChildLabel(child), _mutedStyle);
                        GUI.Label(new Rect(x + width * 0.53f, y, width * 0.18f, RowHeight), RunAnalyticsPanelPresenter.FormatDamage(child.DirectAggregate.Damage), _rightStyle);
                        GUI.Label(new Rect(x + width * 0.72f, y, width * 0.16f, RowHeight), "child", _mutedStyle);
                        GUI.Label(new Rect(x + width * 0.89f, y, width * 0.11f, RowHeight), RunAnalyticsPanelPresenter.FormatPercent(child.PlayerShare), _rightStyle);
                        y += RowHeight;
                    }
                }
            }

            return y;
        }

        private static string GetEmptyRowsText(RunAnalyticsPanelView view, string rowKind)
        {
            if (view.SelectedPlayer == null)
            {
                return view.SelectedPlayerSelection == AnalyticsPlayerSelection.Partner ? "No partner damage yet" : "No run damage yet";
            }

            return "No direct " + rowKind + " damage yet";
        }

        private static string GetEmptyPackageRowsText(RunAnalyticsPanelView view)
        {
            if (view.SelectedPlayer == null)
            {
                return view.SelectedPlayerSelection == AnalyticsPlayerSelection.Partner ? "No partner damage yet" : "No run damage yet";
            }

            return "No package assignments yet";
        }

        private static void DrawCoverage(float x, float y, float width, PlayerDamageSnapshot selectedPlayer)
        {
            if (selectedPlayer == null)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), "Coverage Unknown", _mutedStyle);
                return;
            }

            var coverage = selectedPlayer.MemoryGemCoverage;
            GUI.Label(
                new Rect(x, y, width, RowHeight),
                RunAnalyticsPanelPresenter.FormatCoverageSummary(_mode, selectedPlayer),
                _mutedStyle);

            if (_mode == RunAnalyticsPanelMode.Package && coverage.PackageRelationshipUnknownChildDamage > 0f)
            {
                y += RowHeight;
                GUI.Label(
                    new Rect(x, y, width, RowHeight),
                    "Unknown relationship " + RunAnalyticsPanelPresenter.FormatDamage(coverage.PackageRelationshipUnknownChildDamage),
                    _mutedStyle);
            }
        }

        private static void DrawRunSummaryHeader(float x, float y, float width)
        {
            GUI.Label(new Rect(x, y, width * 0.34f, RowHeight), RunAnalyticsPanelPresenter.PlayerColumnHeader, _mutedStyle);
            GUI.Label(new Rect(x + width * 0.34f, y, width * 0.22f, RowHeight), RunAnalyticsPanelPresenter.DamageColumnHeader, _rightStyle);
            GUI.Label(new Rect(x + width * 0.57f, y, width * 0.19f, RowHeight), RunAnalyticsPanelPresenter.ShareColumnHeader, _rightStyle);
            GUI.Label(new Rect(x + width * 0.77f, y, width * 0.23f, RowHeight), RunAnalyticsPanelPresenter.DpsColumnHeader, _rightStyle);
        }

        private static void DrawFourColumnHeader(float x, float y, float width, string name, string damage, string share, string dps)
        {
            GUI.Label(new Rect(x, y, width * 0.37f, RowHeight), name, _mutedStyle);
            GUI.Label(new Rect(x + width * 0.37f, y, width * 0.19f, RowHeight), damage, _rightStyle);
            GUI.Label(new Rect(x + width * 0.57f, y, width * 0.13f, RowHeight), share, _rightStyle);
            GUI.Label(new Rect(x + width * 0.71f, y, width * 0.10f, RowHeight), RunAnalyticsPanelPresenter.HitsColumnHeader, _rightStyle);
            GUI.Label(new Rect(x + width * 0.82f, y, width * 0.18f, RowHeight), dps, _rightStyle);
        }

        private static void DrawPackageHeader(float x, float y, float width)
        {
            GUI.Label(new Rect(x, y, width * 0.34f, RowHeight), RunAnalyticsPanelPresenter.MemoryPackageColumnHeader, _mutedStyle);
            GUI.Label(new Rect(x + width * 0.34f, y, width * 0.18f, RowHeight), RunAnalyticsPanelPresenter.MemoryColumnHeader, _rightStyle);
            GUI.Label(new Rect(x + width * 0.53f, y, width * 0.18f, RowHeight), RunAnalyticsPanelPresenter.ChildrenColumnHeader, _rightStyle);
            GUI.Label(new Rect(x + width * 0.72f, y, width * 0.16f, RowHeight), RunAnalyticsPanelPresenter.TotalColumnHeader, _rightStyle);
            GUI.Label(new Rect(x + width * 0.89f, y, width * 0.11f, RowHeight), RunAnalyticsPanelPresenter.ShareColumnHeader, _rightStyle);
        }

        private static void EnsureStyles()
        {
            if (_panelStyle != null)
            {
                return;
            }

            _panelBackground = new Texture2D(1, 1);
            _panelBackground.SetPixel(0, 0, new Color(0.025f, 0.03f, 0.035f, 0.72f));
            _panelBackground.Apply();

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = _panelBackground }
            };

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.86f, 0.92f, 1f, 0.92f) },
                clipping = TextClipping.Clip
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                clipping = TextClipping.Clip
            };

            _mutedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = new Color(1f, 1f, 1f, 0.58f) },
                clipping = TextClipping.Clip
            };

            _rightStyle = new GUIStyle(_labelStyle)
            {
                alignment = TextAnchor.MiddleRight
            };

            _tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                clipping = TextClipping.Clip
            };
        }
    }
}

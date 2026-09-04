using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum AnalyticsPlayerSelection
    {
        Local,
        Partner
    }

    internal sealed class EncounterAnalyticsPanelView
    {
        internal EncounterAnalyticsPanelView(
            EncounterDamageSnapshot encounter,
            string stateLabel,
            AnalyticsPlayerSelection selectedPlayerSelection,
            PlayerDamageSnapshot localPlayer,
            PlayerDamageSnapshot partnerPlayer,
            PlayerDamageSnapshot selectedPlayer,
            IReadOnlyList<SourceDamageSnapshot> localSources,
            IReadOnlyList<SourceDamageSnapshot> selectedSources)
        {
            Encounter = encounter;
            StateLabel = stateLabel;
            SelectedPlayerSelection = selectedPlayerSelection;
            LocalPlayer = localPlayer;
            PartnerPlayer = partnerPlayer;
            SelectedPlayer = selectedPlayer;
            LocalSources = localSources;
            SelectedSources = selectedSources;
        }

        internal EncounterDamageSnapshot Encounter { get; }
        internal string StateLabel { get; }
        internal AnalyticsPlayerSelection SelectedPlayerSelection { get; }
        internal PlayerDamageSnapshot LocalPlayer { get; }
        internal PlayerDamageSnapshot PartnerPlayer { get; }
        internal PlayerDamageSnapshot SelectedPlayer { get; }
        internal IReadOnlyList<SourceDamageSnapshot> LocalSources { get; }
        internal IReadOnlyList<SourceDamageSnapshot> SelectedSources { get; }
        internal bool HasPartner
        {
            get { return PartnerPlayer != null; }
        }
        internal string SelectedPlayerLabel
        {
            get { return SelectedPlayerSelection == AnalyticsPlayerSelection.Partner ? "Partner" : "You"; }
        }
    }

    internal static class EncounterAnalyticsPanelPresenter
    {
        internal const string SourceColumnHeader = "Source";
        internal const string DamageColumnHeader = "Damage";
        internal const string ShareColumnHeader = "Share";

        internal static EncounterAnalyticsPanelView BuildView(EncounterDamageSnapshot current, EncounterDamageSnapshot last)
        {
            return BuildView(current, last, AnalyticsPlayerSelection.Local);
        }

        internal static EncounterAnalyticsPanelView BuildView(
            EncounterDamageSnapshot current,
            EncounterDamageSnapshot last,
            AnalyticsPlayerSelection selectedPlayerSelection)
        {
            var encounter = current ?? last;
            if (encounter == null)
            {
                return null;
            }

            var local = FindLocalPlayer(encounter.Players);
            var partner = FindPartnerPlayer(encounter.Players);
            if (selectedPlayerSelection == AnalyticsPlayerSelection.Partner && partner == null)
            {
                selectedPlayerSelection = AnalyticsPlayerSelection.Local;
            }

            var selectedPlayer = selectedPlayerSelection == AnalyticsPlayerSelection.Partner ? partner : local;
            var localSources = local != null && local.Sources != null
                ? local.Sources
                : Array.Empty<SourceDamageSnapshot>();
            var selectedSources = selectedPlayer != null && selectedPlayer.Sources != null
                ? selectedPlayer.Sources
                : Array.Empty<SourceDamageSnapshot>();
            var state = encounter.IsActive ? "Current Encounter" : "Last Encounter";
            return new EncounterAnalyticsPanelView(
                encounter,
                state,
                selectedPlayerSelection,
                local,
                partner,
                selectedPlayer,
                localSources,
                selectedSources);
        }

        internal static IReadOnlyList<SourceDamageSnapshot> GetDisplaySources(IReadOnlyList<SourceDamageSnapshot> sources, int maxRows)
        {
            if (sources == null || sources.Count == 0 || maxRows <= 0)
            {
                return Array.Empty<SourceDamageSnapshot>();
            }

            if (sources.Count <= maxRows)
            {
                return sources;
            }

            var unattributedIndex = -1;
            for (var i = 0; i < sources.Count; i++)
            {
                if (sources[i].IsUnattributed)
                {
                    unattributedIndex = i;
                    break;
                }
            }

            var result = new List<SourceDamageSnapshot>(maxRows);
            var reservedForUnattributed = unattributedIndex >= maxRows && maxRows > 1;
            var normalLimit = reservedForUnattributed ? maxRows - 1 : maxRows;
            for (var i = 0; i < normalLimit; i++)
            {
                result.Add(sources[i]);
            }

            if (reservedForUnattributed)
            {
                result.Add(sources[unattributedIndex]);
            }

            return result.AsReadOnly();
        }

        internal static string GetSourceLabel(SourceDamageSnapshot source)
        {
            if (source == null)
            {
                return "Unattributed";
            }

            if (source.IsUnattributed)
            {
                return "Unattributed";
            }

            if (!string.IsNullOrEmpty(source.DisplayName) && !string.Equals(source.DisplayName, source.Category.ToString(), StringComparison.Ordinal))
            {
                return AnalyticsDisplayNameResolver.ToDisplayName(source.DisplayName, source.DisplayName);
            }

            switch (source.Category)
            {
                case DamageSourceCategory.BasicAttack:
                    return "Basic Attack";
                case DamageSourceCategory.MemoryDirect:
                    return "Memory";
                case DamageSourceCategory.GemDirect:
                    return "Gem";
                case DamageSourceCategory.StatusDot:
                    return "Status/DoT";
                case DamageSourceCategory.CharacterPassive:
                    return "Character Passive";
                case DamageSourceCategory.OtherIdentified:
                    return "Other Identified";
                case DamageSourceCategory.Unattributed:
                    return "Unattributed";
                default:
                    return source.Category.ToString();
            }
        }

        internal static string FormatDamage(float damage)
        {
            var value = Math.Abs(damage);
            if (value >= 1000000f)
            {
                return (damage / 1000000f).ToString("0.0M");
            }

            if (value >= 10000f)
            {
                return (damage / 1000f).ToString("0K");
            }

            if (value >= 1000f)
            {
                return (damage / 1000f).ToString("0.0K");
            }

            return damage.ToString("0");
        }

        internal static string FormatPercent(float? ratio)
        {
            return ratio.HasValue ? (ratio.Value * 100f).ToString("0") + "%" : "-";
        }

        internal static string GetCoverageSummary(SourceCoverageSnapshot coverage)
        {
            if (coverage == null || !coverage.AttributionCoverageRatio.HasValue)
            {
                return "Coverage Unknown";
            }

            var label = "Coverage " + FormatPercent(coverage.AttributionCoverageRatio);
            return coverage.UnattributedDamage > 0f
                ? label + " - " + FormatDamage(coverage.UnattributedDamage) + " unattributed"
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

    internal sealed class DamageAnalyticsEncounterPanel : MonoBehaviour
    {
        internal const float PanelWidthForLayout = 360f;
        private const float PanelWidth = PanelWidthForLayout;
        private const float RowHeight = 26f;
        private const float SectionGap = 10f;
        private const int MaxSourceRows = 8;

        private static DamageAnalyticsEncounterPanel _instance;
        private static GUIStyle _panelStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _mutedStyle;
        private static GUIStyle _rightStyle;
        private static GUIStyle _sourceLabelStyle;
        private static GUIStyle _selectorStyle;
        private static Texture2D _panelBackground;
        private static AnalyticsPlayerSelection _selectedPlayerSelection = AnalyticsPlayerSelection.Local;
        private static float _currentPanelHeight;

        internal static void EnsureCreated()
        {
            if (_instance != null)
            {
                return;
            }

            var obj = new GameObject("DamageAnalyticsEncounterPanel");
            UnityObject.DontDestroyOnLoad(obj);
            _instance = obj.AddComponent<DamageAnalyticsEncounterPanel>();
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
            DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Encounter);
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
                DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Encounter);
                return;
            }

            var view = EncounterAnalyticsPanelPresenter.BuildView(
                DamageAnalyzerDiagnostics.GetCurrentEncounterSnapshot(Time.time),
                DamageAnalyzerDiagnostics.GetLastEncounterSnapshot(),
                _selectedPlayerSelection);
            if (view == null)
            {
                _currentPanelHeight = 0f;
                DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Encounter);
                return;
            }

            EnsureStyles();
            _selectedPlayerSelection = view.SelectedPlayerSelection;

            var displaySources = EncounterAnalyticsPanelPresenter.GetDisplaySources(view.SelectedSources, MaxSourceRows);
            var sourceRows = displaySources.Count;
            var height = 182f + sourceRows * RowHeight;
            if (view.PartnerPlayer != null)
            {
                height += RowHeight * 2f;
            }

            if (view.SelectedPlayer != null && view.SelectedPlayer.SourceCoverage.AttributionCoverageRatio.HasValue)
            {
                height += RowHeight;
            }

            _currentPanelHeight = height;
            var runHeight = DamageAnalyticsRunPanel.GetCurrentPanelHeight();
            var layout = DamageAnalyticsUiInput.CalculateMidLeftLayout(
                Screen.width,
                Screen.height,
                PanelWidth,
                height,
                DamageAnalyticsRunPanel.PanelWidthForLayout,
                runHeight);
            var rect = layout.EncounterRect;

            DamageAnalyticsUiInput.RegisterPanelRect(DamageAnalyticsPanelKind.Encounter, rect);
            DamageAnalyticsUiInput.ConsumeMouseEventsInside(rect);
            GUI.Box(rect, GUIContent.none, _panelStyle);
            DrawPanel(rect, view, displaySources);
        }

        internal static float GetCurrentPanelHeight()
        {
            return _currentPanelHeight;
        }

        private static void DrawPanel(Rect rect, EncounterAnalyticsPanelView view, IReadOnlyList<SourceDamageSnapshot> displaySources)
        {
            var x = rect.x + 14f;
            var y = rect.y + 12f;
            var width = rect.width - 28f;

            GUI.Label(new Rect(x, y, width, RowHeight), view.StateLabel.ToUpperInvariant(), _titleStyle);
            y += RowHeight;
            GUI.Label(new Rect(x, y, width, RowHeight), "Encounter " + view.Encounter.EncounterId, _mutedStyle);
            y += RowHeight + SectionGap;

            DrawPlayerRow(x, y, width, "You", view.LocalPlayer);
            y += RowHeight;

            if (view.PartnerPlayer != null)
            {
                DrawPlayerRow(x, y, width, "Partner", view.PartnerPlayer);
                y += RowHeight;
            }

            y += SectionGap;
            if (view.HasPartner)
            {
                DrawPlayerSelector(x, y, width, view);
                y += RowHeight + SectionGap;
            }

            GUI.Label(new Rect(x, y, width, RowHeight), view.SelectedPlayerLabel + " Sources", _titleStyle);
            y += RowHeight;
            DrawSourceHeader(x, y, width);
            y += RowHeight;

            if (displaySources.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), GetEmptySourceText(view), _mutedStyle);
                y += RowHeight;
            }
            else
            {
                for (var i = 0; i < displaySources.Count; i++)
                {
                    DrawSourceRow(x, y, width, displaySources[i]);
                    y += RowHeight;
                }
            }

            if (view.SelectedPlayer != null && view.SelectedPlayer.SourceCoverage.AttributionCoverageRatio.HasValue)
            {
                y += SectionGap;
                GUI.Label(
                    new Rect(x, y, width, RowHeight),
                    EncounterAnalyticsPanelPresenter.GetCoverageSummary(view.SelectedPlayer.SourceCoverage),
                    _mutedStyle);
            }
        }

        private static void DrawPlayerSelector(float x, float y, float width, EncounterAnalyticsPanelView view)
        {
            var buttonWidth = Mathf.Min(86f, (width - 8f) / 2f);
            var chooseLocal = GUI.Toggle(
                new Rect(x, y, buttonWidth, RowHeight),
                view.SelectedPlayerSelection == AnalyticsPlayerSelection.Local,
                "You",
                _selectorStyle);

            var previousEnabled = GUI.enabled;
            GUI.enabled = view.PartnerPlayer != null;
            var choosePartner = GUI.Toggle(
                new Rect(x + buttonWidth + 8f, y, buttonWidth, RowHeight),
                view.SelectedPlayerSelection == AnalyticsPlayerSelection.Partner,
                "Partner",
                _selectorStyle);

            GUI.enabled = previousEnabled;
            if (chooseLocal && view.SelectedPlayerSelection != AnalyticsPlayerSelection.Local)
            {
                _selectedPlayerSelection = AnalyticsPlayerSelection.Local;
            }
            else if (choosePartner && view.SelectedPlayerSelection != AnalyticsPlayerSelection.Partner)
            {
                _selectedPlayerSelection = AnalyticsPlayerSelection.Partner;
            }
        }

        private static string GetEmptySourceText(EncounterAnalyticsPanelView view)
        {
            if (view.SelectedPlayer == null)
            {
                return view.SelectedPlayerSelection == AnalyticsPlayerSelection.Partner ? "No partner damage yet" : "No local damage yet";
            }

            return view.SelectedPlayerSelection == AnalyticsPlayerSelection.Partner ? "No partner source damage yet" : "No local source damage yet";
        }

        private static void DrawPlayerRow(float x, float y, float width, string label, PlayerDamageSnapshot player)
        {
            var damage = player != null ? EncounterAnalyticsPanelPresenter.FormatDamage(player.Aggregate.Damage) : "-";
            var share = player != null ? EncounterAnalyticsPanelPresenter.FormatPercent(player.PartyShare) : "-";
            GUI.Label(new Rect(x, y, width * 0.45f, RowHeight), label, _labelStyle);
            GUI.Label(new Rect(x + width * 0.43f, y, width * 0.32f, RowHeight), damage, _rightStyle);
            GUI.Label(new Rect(x + width * 0.76f, y, width * 0.24f, RowHeight), share, _rightStyle);
        }

        private static void DrawSourceRow(float x, float y, float width, SourceDamageSnapshot source)
        {
            GUI.Label(new Rect(x, y, width * 0.52f, RowHeight), EncounterAnalyticsPanelPresenter.GetSourceLabel(source), _sourceLabelStyle);
            GUI.Label(new Rect(x + width * 0.53f, y, width * 0.25f, RowHeight), EncounterAnalyticsPanelPresenter.FormatDamage(source.Aggregate.Damage), _rightStyle);
            GUI.Label(new Rect(x + width * 0.79f, y, width * 0.21f, RowHeight), EncounterAnalyticsPanelPresenter.FormatPercent(source.PlayerShare), _rightStyle);
        }

        private static void DrawSourceHeader(float x, float y, float width)
        {
            GUI.Label(new Rect(x, y, width * 0.52f, RowHeight), EncounterAnalyticsPanelPresenter.SourceColumnHeader, _mutedStyle);
            GUI.Label(new Rect(x + width * 0.53f, y, width * 0.25f, RowHeight), EncounterAnalyticsPanelPresenter.DamageColumnHeader, _rightStyle);
            GUI.Label(new Rect(x + width * 0.79f, y, width * 0.21f, RowHeight), EncounterAnalyticsPanelPresenter.ShareColumnHeader, _rightStyle);
        }

        private static void EnsureStyles()
        {
            if (_panelStyle != null)
            {
                return;
            }

            _panelBackground = new Texture2D(1, 1);
            _panelBackground.SetPixel(0, 0, new Color(0.03f, 0.035f, 0.04f, 0.72f));
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
                fontSize = 15,
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

            _sourceLabelStyle = new GUIStyle(_labelStyle)
            {
                fontSize = 14
            };

            _selectorStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                clipping = TextClipping.Clip
            };
        }
    }
}

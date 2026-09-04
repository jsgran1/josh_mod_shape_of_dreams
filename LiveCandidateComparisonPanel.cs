using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal sealed class LiveCandidateComparisonPanelView
    {
        internal LiveCandidateComparisonPanelView(
            long sequenceId,
            LiveCandidateComparisonStatus status,
            string candidateKindLabel,
            string candidateLabel,
            string reason,
            ComparisonPresentationView comparison,
            IReadOnlyList<ComparisonOptionPresentation> options)
        {
            SequenceId = sequenceId;
            Status = status;
            CandidateKindLabel = candidateKindLabel ?? "Candidate";
            CandidateLabel = candidateLabel ?? "Unknown candidate";
            Reason = reason ?? "";
            Comparison = comparison;
            Options = options ?? Array.Empty<ComparisonOptionPresentation>();
        }

        internal long SequenceId { get; }
        internal LiveCandidateComparisonStatus Status { get; }
        internal string CandidateKindLabel { get; }
        internal string CandidateLabel { get; }
        internal string Reason { get; }
        internal ComparisonPresentationView Comparison { get; }
        internal IReadOnlyList<ComparisonOptionPresentation> Options { get; }
        internal bool HasRecommendation
        {
            get
            {
                return Status == LiveCandidateComparisonStatus.Ready
                    && Comparison != null
                    && Comparison.RecommendationState == ComparisonRecommendationState.Recommended
                    && Comparison.RecommendedOption != null;
            }
        }
    }

    internal static class LiveCandidateComparisonPanelPresenter
    {
        internal static LiveCandidateComparisonPanelView BuildView(LiveCandidateComparisonSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Status == LiveCandidateComparisonStatus.Empty)
            {
                return null;
            }

            var comparison = ComparisonPresentationShell.BuildView(snapshot.Comparisons);
            var options = new List<ComparisonOptionPresentation>(
                comparison.RankedDamageOptions.Count + comparison.UnrankedOptions.Count);
            for (var i = 0; i < comparison.RankedDamageOptions.Count; i++)
            {
                options.Add(comparison.RankedDamageOptions[i]);
            }

            for (var i = 0; i < comparison.UnrankedOptions.Count; i++)
            {
                options.Add(comparison.UnrankedOptions[i]);
            }

            return new LiveCandidateComparisonPanelView(
                snapshot.SequenceId,
                snapshot.Status,
                ResolveCandidateKindLabel(snapshot.CandidateKind),
                ResolveCandidateLabel(snapshot, comparison),
                snapshot.Reason,
                comparison,
                options.AsReadOnly());
        }

        internal static string FormatMetricRow(ComparisonPresentationMetricRow row)
        {
            if (row == null)
            {
                return "Unavailable";
            }

            if (row.State == ComparisonPresentationMetricState.Unknown
                || row.State == ComparisonPresentationMetricState.Unsupported
                || row.State == ComparisonPresentationMetricState.NotApplicable
                || row.State == ComparisonPresentationMetricState.Empty)
            {
                return AppendDetail(
                    row.Label + ": " + (!string.IsNullOrEmpty(row.ValueText) ? row.ValueText : row.State.ToString()),
                    row.DetailText);
            }

            if (!string.IsNullOrEmpty(row.BeforeText) || !string.IsNullOrEmpty(row.AfterText))
            {
                return AppendDetail(
                    row.Label + ": " + row.BeforeText + " -> " + row.AfterText +
                        (!string.IsNullOrEmpty(row.DeltaText) ? "  " + row.DeltaText : ""),
                    row.DetailText);
            }

            return AppendDetail(row.Label + (!string.IsNullOrEmpty(row.ValueText) ? ": " + row.ValueText : ""), row.DetailText);
        }

        internal static string TrimForPanel(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || maxLength <= 0)
            {
                return "";
            }

            if (value.Length <= maxLength)
            {
                return value;
            }

            if (maxLength <= 3)
            {
                return value.Substring(0, maxLength);
            }

            return value.Substring(0, maxLength - 3) + "...";
        }

        internal static string FormatStateLabel(ComparisonPresentationMetricState state)
        {
            switch (state)
            {
                case ComparisonPresentationMetricState.Known:
                    return "Damage";
                case ComparisonPresentationMetricState.Estimated:
                    return "Estimated";
                case ComparisonPresentationMetricState.Utility:
                    return "Utility";
                case ComparisonPresentationMetricState.NotApplicable:
                    return "N/A";
                case ComparisonPresentationMetricState.Unknown:
                    return "Unknown";
                case ComparisonPresentationMetricState.Unsupported:
                    return "Unsupported";
                default:
                    return "Info";
            }
        }

        private static string AppendDetail(string text, string detail)
        {
            return string.IsNullOrEmpty(detail) ? text : text + "  [" + detail + "]";
        }

        private static string ResolveCandidateKindLabel(LiveCandidateComparisonCandidateKind kind)
        {
            switch (kind)
            {
                case LiveCandidateComparisonCandidateKind.Gem:
                    return "Gem";
                case LiveCandidateComparisonCandidateKind.Memory:
                    return "Memory";
                default:
                    return "Candidate";
            }
        }

        private static string ResolveCandidateLabel(LiveCandidateComparisonSnapshot snapshot, ComparisonPresentationView comparison)
        {
            if (comparison != null && comparison.HasOptions && !string.IsNullOrEmpty(comparison.CandidateLabel))
            {
                return comparison.CandidateLabel;
            }

            if (snapshot.CandidateGem != null)
            {
                return ResolveDisplayName(snapshot.CandidateGem.GemKey.DisplayName, snapshot.CandidateGem.ContentId);
            }

            if (snapshot.CandidateMemory != null)
            {
                return ResolveDisplayName(snapshot.CandidateMemory.MemoryKey.DisplayName, snapshot.CandidateMemory.ContentId);
            }

            return "Unknown candidate";
        }

        private static string ResolveDisplayName(string displayName, string contentId)
        {
            var fallback = string.IsNullOrEmpty(contentId) ? "Unknown candidate" : contentId;
            return AnalyticsDisplayNameResolver.ToDisplayName(displayName, fallback);
        }
    }

    internal sealed class LiveCandidateComparisonPanel : MonoBehaviour
    {
        internal const float PanelWidth = 470f;
        private const float RowHeight = 22f;
        private const float SectionGap = 7f;
        private const int MaxProjectedRowsPerOption = 3;
        private const int MaxUtilityRowsPerOption = 2;
        private const int MaxObservedRowsPerOption = 2;
        private const int MaxOptions = 5;
        private const int CandidateMaxLength = 52;
        private const int ReplacementMaxLength = 46;
        private const int RowMaxLength = 86;

        private static LiveCandidateComparisonPanel _instance;
        private static GUIStyle _panelStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _mutedStyle;
        private static GUIStyle _sectionStyle;
        private static GUIStyle _projectedStyle;
        private static GUIStyle _utilityStyle;
        private static GUIStyle _recommendationStyle;
        private static GUIStyle _warningStyle;
        private static Texture2D _panelBackground;

        private long _lastSequenceId = long.MinValue;
        private LiveCandidateComparisonPanelView _view;

        internal static void EnsureCreated()
        {
            if (_instance != null)
            {
                return;
            }

            var obj = new GameObject("LiveCandidateComparisonPanel");
            UnityObject.DontDestroyOnLoad(obj);
            _instance = obj.AddComponent<LiveCandidateComparisonPanel>();
        }

        internal static void DestroyPanel()
        {
            if (_instance == null)
            {
                return;
            }

            var obj = _instance.gameObject;
            _instance = null;
            DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Comparison);
            if (obj != null)
            {
                UnityObject.Destroy(obj);
            }
        }

        private void OnGUI()
        {
            if (!DamageAnalyticsUiVisibility.ShouldShowAnalyticsUi())
            {
                ClearRenderedState();
                return;
            }

            var snapshot = DamageAnalyzerDiagnostics.GetLiveCandidateComparisonSnapshot();
            if (snapshot == null || snapshot.SequenceId != _lastSequenceId)
            {
                _lastSequenceId = snapshot != null ? snapshot.SequenceId : long.MinValue;
                _view = LiveCandidateComparisonPanelPresenter.BuildView(snapshot);
            }

            if (_view == null)
            {
                DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Comparison);
                return;
            }

            EnsureStyles();
            var rect = CalculatePanelRect(Screen.width, Screen.height, _view);
            DamageAnalyticsUiInput.RegisterPanelRect(DamageAnalyticsPanelKind.Comparison, rect);
            GUI.Box(rect, GUIContent.none, _panelStyle);
            DrawPanel(rect, _view);
            DamageAnalyticsUiInput.ConsumeMouseEventsInside(rect);
        }

        private void ClearRenderedState()
        {
            _lastSequenceId = long.MinValue;
            _view = null;
            DamageAnalyticsUiInput.ClearPanelRect(DamageAnalyticsPanelKind.Comparison);
        }

        internal static float CalculateHeight(LiveCandidateComparisonPanelView view)
        {
            var rows = 3;
            if (view.HasRecommendation
                || view.Status != LiveCandidateComparisonStatus.Ready
                || (view.Comparison != null && view.Comparison.RecommendationState == ComparisonRecommendationState.Suppressed))
            {
                rows++;
            }

            var visibleOptions = GetVisibleOptionCount(view);
            for (var i = 0; i < visibleOptions; i++)
            {
                var option = view.Options[i];
                rows++;
                if (HasRows(option.ProjectedDamageRows)) rows++;
                if (HasRows(option.UtilityRows)) rows++;
                if (HasObservedRows(option.ObservedContextRows)) rows++;
                rows += Math.Min(MaxProjectedRowsPerOption, option.ProjectedDamageRows.Count);
                rows += Math.Min(MaxUtilityRowsPerOption, option.UtilityRows.Count);
                rows += Math.Min(MaxObservedRowsPerOption, option.ObservedContextRows.Count);
                if (option.ProjectedDamageRows.Count > MaxProjectedRowsPerOption) rows++;
                if (option.UtilityRows.Count > MaxUtilityRowsPerOption) rows++;
                if (option.ObservedContextRows.Count > MaxObservedRowsPerOption) rows++;
            }

            if (visibleOptions < view.Options.Count)
            {
                rows++;
            }

            if (view.Options.Count == 0)
            {
                rows++;
            }

            return 24f + rows * RowHeight + Math.Max(1, visibleOptions) * SectionGap;
        }

        internal static Rect CalculatePanelRect(float screenWidth, float screenHeight, LiveCandidateComparisonPanelView view)
        {
            return DamageAnalyticsUiInput.CalculateTopRightRect(screenWidth, screenHeight, PanelWidth, CalculateHeight(view));
        }

        private static void DrawPanel(Rect rect, LiveCandidateComparisonPanelView view)
        {
            var x = rect.x + 14f;
            var y = rect.y + 10f;
            var width = rect.width - 28f;
            var contentBottom = rect.yMax - 8f;

            GUI.Label(new Rect(x, y, width, RowHeight), "PRE-COMMIT COMPARISON", _titleStyle);
            y += RowHeight;
            GUI.Label(
                new Rect(x, y, width, RowHeight),
                view.CandidateKindLabel + ": " + LiveCandidateComparisonPanelPresenter.TrimForPanel(view.CandidateLabel, CandidateMaxLength),
                _labelStyle);
            y += RowHeight;
            GUI.Label(
                new Rect(x, y, width, RowHeight),
                LiveCandidateComparisonPanelPresenter.TrimForPanel("State: " + view.Status + "  " + view.Reason, RowMaxLength),
                _mutedStyle);
            y += RowHeight;

            if (view.HasRecommendation)
            {
                GUI.Label(
                    new Rect(x, y, width, RowHeight),
                    LiveCandidateComparisonPanelPresenter.TrimForPanel(
                        "BEST DAMAGE: " + FormatActionVerb(view.Comparison.RecommendedOption) + " " + view.Comparison.RecommendedOption.ReplacementLabel,
                        RowMaxLength),
                    _recommendationStyle);
                y += RowHeight;
            }
            else if (view.Comparison != null && view.Comparison.RecommendationState == ComparisonRecommendationState.Suppressed)
            {
                GUI.Label(
                    new Rect(x, y, width, RowHeight),
                    LiveCandidateComparisonPanelPresenter.TrimForPanel("No recommendation: " + view.Comparison.RecommendationText, RowMaxLength),
                    _warningStyle);
                y += RowHeight;
            }
            else if (view.Status != LiveCandidateComparisonStatus.Ready)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), "No recommendation available", _warningStyle);
                y += RowHeight;
            }

            y += SectionGap;
            if (view.Options.Count == 0)
            {
                GUI.Label(
                    new Rect(x, y, width, RowHeight),
                    LiveCandidateComparisonPanelPresenter.TrimForPanel(view.Reason, RowMaxLength),
                    _warningStyle);
                return;
            }

            var visibleOptions = GetVisibleOptionCount(view);
            for (var i = 0; i < visibleOptions && y + RowHeight <= contentBottom; i++)
            {
                var option = view.Options[i];
                var recommended = view.HasRecommendation && ReferenceEquals(view.Comparison.RecommendedOption, option);
                GUI.Label(
                    new Rect(x, y, width, RowHeight),
                    LiveCandidateComparisonPanelPresenter.TrimForPanel(
                        (recommended ? "BEST  " : FormatActionVerb(option) + "  ") + option.ReplacementLabel,
                        ReplacementMaxLength),
                    recommended ? _recommendationStyle : _labelStyle);
                y += RowHeight;

                y = DrawRows(x + 12f, y, width - 12f, contentBottom, "Projected", option.ProjectedDamageRows, MaxProjectedRowsPerOption, _projectedStyle);
                y = DrawRows(x + 12f, y, width - 12f, contentBottom, "Utility", option.UtilityRows, MaxUtilityRowsPerOption, _utilityStyle);
                y = DrawObservedRows(x + 12f, y, width - 12f, contentBottom, option.ObservedContextRows);
                y += SectionGap;
            }

            if (visibleOptions < view.Options.Count && y + RowHeight <= contentBottom)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), "+ " + (view.Options.Count - visibleOptions) + " more replacement targets", _mutedStyle);
            }
        }

        private static float DrawRows(
            float x,
            float y,
            float width,
            float contentBottom,
            string sectionLabel,
            IReadOnlyList<ComparisonPresentationMetricRow> rows,
            int maxRows,
            GUIStyle normalStyle)
        {
            if (!HasRows(rows) || y + RowHeight > contentBottom)
            {
                return y;
            }

            GUI.Label(new Rect(x, y, width, RowHeight), sectionLabel, _sectionStyle);
            y += RowHeight;

            var count = Math.Min(maxRows, rows != null ? rows.Count : 0);
            for (var i = 0; i < count; i++)
            {
                if (y + RowHeight > contentBottom)
                {
                    return y;
                }

                var row = rows[i];
                var style = row.State == ComparisonPresentationMetricState.Unknown
                    || row.State == ComparisonPresentationMetricState.Unsupported
                    || row.State == ComparisonPresentationMetricState.NotApplicable
                    ? _warningStyle
                    : normalStyle;
                var prefix = LiveCandidateComparisonPanelPresenter.FormatStateLabel(row.State) + ": ";
                GUI.Label(
                    new Rect(x, y, width, RowHeight),
                    LiveCandidateComparisonPanelPresenter.TrimForPanel(prefix + LiveCandidateComparisonPanelPresenter.FormatMetricRow(row), RowMaxLength),
                    style);
                y += RowHeight;
            }

            if (rows != null && rows.Count > maxRows && y + RowHeight <= contentBottom)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), "+ " + (rows.Count - maxRows) + " more", _mutedStyle);
                y += RowHeight;
            }

            return y;
        }

        private static string FormatActionVerb(ComparisonOptionPresentation option)
        {
            return option != null
                && option.Comparison != null
                && option.Comparison.ActionKind == CandidateEquipActionKind.EquipIntoEmptySlot
                ? "Equip"
                : "Replace";
        }

        private static float DrawObservedRows(
            float x,
            float y,
            float width,
            float contentBottom,
            IReadOnlyList<ComparisonObservedContextRow> rows)
        {
            if (!HasObservedRows(rows) || y + RowHeight > contentBottom)
            {
                return y;
            }

            GUI.Label(new Rect(x, y, width, RowHeight), "Observed run context", _sectionStyle);
            y += RowHeight;

            var count = Math.Min(MaxObservedRowsPerOption, rows != null ? rows.Count : 0);
            for (var i = 0; i < count; i++)
            {
                if (y + RowHeight > contentBottom)
                {
                    return y;
                }

                GUI.Label(
                    new Rect(x, y, width, RowHeight),
                    LiveCandidateComparisonPanelPresenter.TrimForPanel("Observed: " + FormatObservedRow(rows[i]), RowMaxLength),
                    _mutedStyle);
                y += RowHeight;
            }

            if (rows != null && rows.Count > MaxObservedRowsPerOption && y + RowHeight <= contentBottom)
            {
                GUI.Label(new Rect(x, y, width, RowHeight), "+ " + (rows.Count - MaxObservedRowsPerOption) + " more", _mutedStyle);
                y += RowHeight;
            }

            return y;
        }

        private static string FormatObservedRow(ComparisonObservedContextRow row)
        {
            if (row == null)
            {
                return "Unavailable";
            }

            return row.Label + ": " + (string.IsNullOrEmpty(row.ValueText) ? "Unavailable" : row.ValueText);
        }

        private static int GetVisibleOptionCount(LiveCandidateComparisonPanelView view)
        {
            return view != null && view.Options != null ? Math.Min(MaxOptions, view.Options.Count) : 0;
        }

        private static bool HasRows(IReadOnlyList<ComparisonPresentationMetricRow> rows)
        {
            return rows != null && rows.Count > 0;
        }

        private static bool HasObservedRows(IReadOnlyList<ComparisonObservedContextRow> rows)
        {
            return rows != null && rows.Count > 0;
        }

        private static void EnsureStyles()
        {
            if (_panelStyle != null)
            {
                return;
            }

            _panelBackground = new Texture2D(1, 1);
            _panelBackground.SetPixel(0, 0, new Color(0.025f, 0.03f, 0.035f, 0.86f));
            _panelBackground.Apply();
            _panelStyle = new GUIStyle(GUI.skin.box) { normal = { background = _panelBackground } };
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.86f, 0.92f, 1f, 0.96f) },
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
                fontSize = 12,
                normal = { textColor = new Color(1f, 1f, 1f, 0.58f) },
                clipping = TextClipping.Clip
            };
            _sectionStyle = new GUIStyle(_mutedStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 1f, 1f, 0.7f) }
            };
            _projectedStyle = new GUIStyle(_labelStyle)
            {
                normal = { textColor = new Color(0.55f, 0.9f, 1f, 0.96f) }
            };
            _utilityStyle = new GUIStyle(_labelStyle)
            {
                normal = { textColor = new Color(0.78f, 0.94f, 0.68f, 0.92f) }
            };
            _recommendationStyle = new GUIStyle(_labelStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.75f, 1f, 0.62f, 0.98f) }
            };
            _warningStyle = new GUIStyle(_labelStyle)
            {
                normal = { textColor = new Color(1f, 0.78f, 0.42f, 0.96f) }
            };
        }
    }
}

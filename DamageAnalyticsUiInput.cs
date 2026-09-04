using System.Collections.Generic;
using UnityEngine;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum DamageAnalyticsPanelKind
    {
        Encounter,
        Run,
        Comparison
    }

    internal readonly struct DamageAnalyticsPanelLayout
    {
        internal DamageAnalyticsPanelLayout(Rect encounterRect, Rect runRect)
        {
            EncounterRect = encounterRect;
            RunRect = runRect;
        }

        internal Rect EncounterRect { get; }
        internal Rect RunRect { get; }
    }

    internal static class DamageAnalyticsUiInput
    {
        internal const float PanelMargin = 22f;
        internal const float PanelGap = 12f;

        private static readonly Dictionary<DamageAnalyticsPanelKind, Rect> ActivePanelRects =
            new Dictionary<DamageAnalyticsPanelKind, Rect>();

        internal static DamageAnalyticsPanelLayout CalculateMidLeftLayout(
            float screenWidth,
            float screenHeight,
            float encounterWidth,
            float encounterHeight,
            float runWidth,
            float runHeight)
        {
            var hasEncounter = encounterHeight > 0f;
            var hasRun = runHeight > 0f;
            var groupHeight = encounterHeight + runHeight + (hasEncounter && hasRun ? PanelGap : 0f);
            var groupY = Mathf.Clamp(
                (screenHeight - groupHeight) * 0.5f,
                PanelMargin,
                Mathf.Max(PanelMargin, screenHeight - groupHeight - PanelMargin));
            var x = Mathf.Clamp(
                PanelMargin,
                PanelMargin,
                Mathf.Max(PanelMargin, screenWidth - Mathf.Max(encounterWidth, runWidth) - PanelMargin));

            return new DamageAnalyticsPanelLayout(
                new Rect(x, groupY, encounterWidth, encounterHeight),
                new Rect(x, groupY + (hasEncounter ? encounterHeight + PanelGap : 0f), runWidth, runHeight));
        }

        internal static void RegisterPanelRect(DamageAnalyticsPanelKind kind, Rect rect)
        {
            ActivePanelRects[kind] = rect;
        }

        internal static Rect CalculateTopRightRect(float screenWidth, float screenHeight, float width, float height)
        {
            var clampedWidth = Mathf.Min(width, Mathf.Max(0f, screenWidth - PanelMargin * 2f));
            var clampedHeight = Mathf.Min(height, Mathf.Max(0f, screenHeight - PanelMargin * 2f));
            return new Rect(
                Mathf.Max(PanelMargin, screenWidth - clampedWidth - PanelMargin),
                PanelMargin,
                clampedWidth,
                clampedHeight);
        }

        internal static void ClearPanelRect(DamageAnalyticsPanelKind kind)
        {
            ActivePanelRects.Remove(kind);
        }

        internal static void ClearAllPanelRects()
        {
            ActivePanelRects.Clear();
        }

        internal static bool IsPointerOverModPanel()
        {
            return IsScreenPointOverModPanel(Input.mousePosition);
        }

        internal static bool IsScreenPointOverModPanel(Vector3 mousePosition)
        {
            var guiPoint = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            foreach (var entry in ActivePanelRects)
            {
                if (entry.Value.Contains(guiPoint))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsGuiPointOverModPanel(Vector2 guiPoint)
        {
            foreach (var entry in ActivePanelRects)
            {
                if (entry.Value.Contains(guiPoint))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool ShouldSuppressGameMouseInput(MouseButton button, Vector3 mousePosition, float screenHeight, bool shouldShowAnalyticsUi)
        {
            if (button != MouseButton.Left && button != MouseButton.Right)
            {
                return false;
            }

            if (!shouldShowAnalyticsUi)
            {
                return false;
            }

            var guiPoint = new Vector2(mousePosition.x, screenHeight - mousePosition.y);
            return IsGuiPointOverModPanel(guiPoint);
        }

        internal static void ConsumeMouseEventsInside(Rect rect)
        {
            var evt = Event.current;
            if (evt == null || !rect.Contains(evt.mousePosition))
            {
                return;
            }

            switch (evt.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.MouseDrag:
                case EventType.ScrollWheel:
                    evt.Use();
                    break;
            }
        }
    }
}

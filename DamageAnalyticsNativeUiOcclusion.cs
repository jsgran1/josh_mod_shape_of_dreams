using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal static class DamageAnalyticsNativeUiOcclusion
    {
        internal static bool ShouldHideAnalyticsPanels()
        {
            try
            {
                var tooltipManager = SingletonBehaviour<UI_TooltipManager>.instance;
                if (tooltipManager != null && ShouldHideForNativeTooltipState(
                    tooltipManager.isShowing,
                    IsActive(tooltipManager.skillTooltip),
                    IsActive(tooltipManager.gemTooltip),
                    IsActive(tooltipManager.skillEquipTooltip),
                    IsActive(tooltipManager.gemEquipTooltip),
                    false,
                    tooltipManager.currentObjects))
                {
                    return true;
                }

                var heroDetailWindow = ManagerBase<UI_InGame_HeroDetailWindow>.instance;
                if (heroDetailWindow != null && heroDetailWindow.isShown)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        internal static bool ShouldHideForNativeTooltipState(
            bool isShowing,
            bool skillTooltipActive,
            bool gemTooltipActive,
            bool skillEquipTooltipActive,
            bool gemEquipTooltipActive,
            bool heroDetailShown,
            IEnumerable<object> currentObjects)
        {
            if (heroDetailShown)
            {
                return true;
            }

            if (!isShowing)
            {
                return false;
            }

            if (skillTooltipActive || gemTooltipActive || skillEquipTooltipActive || gemEquipTooltipActive)
            {
                return true;
            }

            if (currentObjects == null)
            {
                return false;
            }

            foreach (var obj in currentObjects)
            {
                if (IsMemoryGemOrBuildObject(obj))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsMemoryGemOrBuildObject(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj is SkillTrigger
                || obj is Gem
                || obj is HeroSkill
                || obj is DewGameResult
                || IsMemoryGemOrBuildTypeName(obj.GetType().Name);
        }

        internal static bool IsMemoryGemOrBuildTypeName(string typeName)
        {
            return !string.IsNullOrEmpty(typeName)
                && (typeName.IndexOf("Skill", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("Gem", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("Build", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("HeroDetail", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsActive(GameObject obj)
        {
            return obj != null && obj.activeInHierarchy;
        }
    }
}

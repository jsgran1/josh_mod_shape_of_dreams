using System;
using UnityEngine.SceneManagement;

namespace ShapeOfDreams.DamageAnalyzer
{
    internal enum DamageAnalyticsUiContext
    {
        NonGameplay,
        ActiveGameplay,
        FinalResults
    }

    internal static class DamageAnalyticsUiVisibility
    {
        private static DamageAnalyticsUiContext _lastGameplayContext = DamageAnalyticsUiContext.NonGameplay;

        internal static void MarkActiveGameplay()
        {
            _lastGameplayContext = DamageAnalyticsUiContext.ActiveGameplay;
        }

        internal static void MarkFinalResults()
        {
            _lastGameplayContext = DamageAnalyticsUiContext.FinalResults;
        }

        internal static void MarkNonGameplay()
        {
            _lastGameplayContext = DamageAnalyticsUiContext.NonGameplay;
            DamageAnalyticsUiInput.ClearAllPanelRects();
        }

        internal static bool ShouldShowAnalyticsUi()
        {
            return ShouldShowAnalyticsUi(ResolveCurrentContext());
        }

        internal static bool ShouldShowAnalyticsUi(DamageAnalyticsUiContext context)
        {
            return context == DamageAnalyticsUiContext.ActiveGameplay
                || context == DamageAnalyticsUiContext.FinalResults;
        }

        internal static DamageAnalyticsUiContext ResolveGameplayContext(
            string activeSceneName,
            bool hasPlayGameManager,
            bool gameSettingsInGame,
            bool gameConcluded,
            bool networkEndingSession,
            bool sessionEnded,
            DamageAnalyticsUiContext lastGameplayContext)
        {
            if (IsKnownNonGameplayScene(activeSceneName) || sessionEnded)
            {
                return DamageAnalyticsUiContext.NonGameplay;
            }

            if (gameConcluded && !networkEndingSession)
            {
                return DamageAnalyticsUiContext.FinalResults;
            }

            if (hasPlayGameManager && gameSettingsInGame)
            {
                return DamageAnalyticsUiContext.ActiveGameplay;
            }

            if (lastGameplayContext == DamageAnalyticsUiContext.FinalResults
                && !networkEndingSession
                && (hasPlayGameManager || gameSettingsInGame))
            {
                return DamageAnalyticsUiContext.FinalResults;
            }

            return DamageAnalyticsUiContext.NonGameplay;
        }

        private static DamageAnalyticsUiContext ResolveCurrentContext()
        {
            try
            {
                var sceneName = SceneManager.GetActiveScene().name;
                var gameSettings = NetworkedManagerBase<GameSettingsManager>.softInstance;
                var gameManager = NetworkedManagerBase<GameManager>.softInstance;
                var networkManager = DewNetworkManager.softInstance;
                var context = ResolveGameplayContext(
                    sceneName,
                    gameManager is PlayGameManager,
                    gameSettings != null && gameSettings.state == GameState.InGame,
                    gameManager != null && gameManager.isGameConcluded,
                    networkManager != null && networkManager.isEndingSession,
                    networkManager != null && networkManager.hasSessionEnded,
                    _lastGameplayContext);

                _lastGameplayContext = context;
                return context;
            }
            catch (Exception)
            {
                return _lastGameplayContext;
            }
        }

        private static bool IsKnownNonGameplayScene(string sceneName)
        {
            return string.Equals(sceneName, "Title", StringComparison.Ordinal)
                || string.Equals(sceneName, "Splash", StringComparison.Ordinal)
                || string.Equals(sceneName, "Intro", StringComparison.Ordinal)
                || string.Equals(sceneName, "PlayLobby", StringComparison.Ordinal)
                || string.Equals(sceneName, "Collectables", StringComparison.Ordinal);
        }
    }
}

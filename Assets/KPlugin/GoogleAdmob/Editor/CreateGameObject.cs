using UnityEditor;
using UnityEngine;

namespace KPlugin.GoogleAdMob.Editor
{
    public class CreateGameObject
    {
        #region Properties
        private const string GAME_OBJECT_NAME_MANAGER = "KPlugin_AdMob_Manager",
            GAME_OBJECT_NAME_APP_OPEN = "KPlugin_AdMob_AppOpen",
            GAME_OBJECT_NAME_BANNER = "KPlugin_AdMob_Banner",
            GAME_OBJECT_NAME_INTERSTITIAL = "KPlugin_AdMob_Interstitial",
            GAME_OBJECT_NAME_REWARDED = "KPlugin_AdMob_Rewarded",
            GAME_OBJECT_NAME_REWARDED_INTERSTITIAL = "KPlugin_AdMob_RewardedInterstitial";
        #endregion

        #region Methods
        [MenuItem("GameObject/KPlugin/Admob/Create Manager", priority = 0)]
        private static void Create_InitManager()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_MANAGER);
            newGO.AddComponent<AdMobManager>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/Admob/Create Ad AppOpen", priority = 1)]
        private static void Create_AdAppOpen()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_APP_OPEN);
            newGO.AddComponent<AdMobAppOpen>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/Admob/Create Ad Banner", priority = 2)]
        private static void Create_AdBanner()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_BANNER);
            newGO.AddComponent<AdMobBanner>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/Admob/Create Ad Interstitial", priority = 3)]
        private static void Create_AdInterstitial()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_INTERSTITIAL);
            newGO.AddComponent<AdMobInterstitial>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/Admob/Create Ad Rewarded", priority = 4)]
        private static void Create_AdRewarded()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_REWARDED);
            newGO.AddComponent<AdMobRewarded>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        [MenuItem("GameObject/KPlugin/Admob/Create Ad Rewarded Interstitial", priority = 5)]
        private static void Create_AdRewardedInterstitial()
        {
            GameObject newGO = new GameObject(GAME_OBJECT_NAME_REWARDED_INTERSTITIAL);
            newGO.AddComponent<AdMobRewardedInterstitial>();
            //
            if (Selection.activeTransform != null)
                newGO.transform.SetParent(Selection.activeTransform);
            Selection.activeGameObject = newGO;
        }
        #endregion
    }
}

using UnityEngine;

namespace KPlugin.AdMob
{
    public class AdMobSetting : ScriptableObject
    {
        #region Properties
        public const string RESOURCES_PATH_FOLDER = "KPlugin/AdMob",
            RESOURCES_PATH_FILE = "AdMobSetting";
        private const string RESOURCES_PATH = RESOURCES_PATH_FOLDER + "/" + RESOURCES_PATH_FILE;

        private static AdMobSetting instance;
        public static AdMobSetting Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<AdMobSetting>(RESOURCES_PATH);
                return instance;
            }
        }

        [SerializeField]
        private AdMobSettingAdId[] appOpenIds,
            bannerIds,
            interstitialIds,
            rewardedIds,
            rewardedInterstitialIds;
        #endregion

        #region Unity Event

        #endregion

        #region Method

        #endregion

        #region Ad
        public int Ad_Count(AdMobAdType adType)
        {
            switch (adType)
            {
                case AdMobAdType.AppOpen:
                    return appOpenIds.Length;
                case AdMobAdType.Banner:
                    return bannerIds.Length;
                case AdMobAdType.Interstitial:
                    return interstitialIds.Length;
                case AdMobAdType.Rewarded:
                    return rewardedIds.Length;
                default:
                    return 0;
            }
        }
        public AdMobSettingAdId Ad_Get(AdMobAdType adType, int index)
        {
            switch (adType)
            {
                case AdMobAdType.AppOpen:
                    return appOpenIds[index];
                case AdMobAdType.Banner:
                    return bannerIds[index];
                case AdMobAdType.Interstitial:
                    return interstitialIds[index];
                case AdMobAdType.Rewarded:
                    return rewardedIds[index];
                default:
                    return null;
            }
        }
        #endregion
    }
}

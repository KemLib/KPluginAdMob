using GoogleMobileAds.Api;
using KTool.Attribute;
using KTool.Init;
using System.Collections.Generic;
using UnityEngine;

namespace KPlugin.GoogleAdMob
{
    public class AdMobManager : MonoBehaviour, IIniter
    {
        #region Properties
        public const string ADMOB_SCOURCE = "GoogleAdMob",
            ADMOB_COUNTRY_CODE = "UnknownCountry";

        public static AdMobManager Instance
        {
            get;
            private set;
        }
        public static bool IsInit
        {
            get;
            private set;
        }

        [SerializeField]
        private string[] testDeviceIds;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobAppOpen[] adAppOpens;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobBanner[] adBanners;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobInterstitial[] adInterstitials;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobRewarded[] adRewardeds;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobRewardedInterstitial[] adRewardedInterstitials;

        private InitTrackingSource initTracking;

        private bool IsIniting => initTracking != null;
        #endregion

        #region Init
        public InitTracking InitBegin()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                return AdMob_Init();
            }
            //
            return InitTracking.Success;
        }

        public void InitEnd()
        {

        }
        #endregion

        #region AdMob
        private InitTracking AdMob_Init()
        {
            if (IsInit || IsIniting)
                return InitTracking.Success;
            //
            initTracking = new InitTrackingSource(true);
            MobileAds.Initialize(AdMob_OnInitComplete);
            return initTracking;
        }
        private void AdMob_OnInitComplete(InitializationStatus initStatus)
        {
            IsInit = true;
            initTracking.CompleteSuccess();
            initTracking = null;
            //
            AdMob_RequestTestDevice();
        }
        private void AdMob_RequestTestDevice()
        {
            List<string> ids = new List<string>();
            foreach (string deviceId in testDeviceIds)
            {
                if (string.IsNullOrEmpty(deviceId))
                    continue;
                ids.Add(deviceId);
            }
            if (ids.Count == 0)
                return;
            //
            RequestConfiguration requestConfiguration = new RequestConfiguration();
            foreach (string deviceId in ids)
                requestConfiguration.TestDeviceIds.Add(deviceId);
            MobileAds.SetRequestConfiguration(requestConfiguration);
        }
        #endregion

        #region AppOpen
        public int AppOpen_Count()
        {
            return adAppOpens.Length;
        }
        public AdMobAppOpen AppOpen_Get(int index)
        {
            if (index < 0 || index >= adAppOpens.Length)
                return null;
            return adAppOpens[index];
        }
        public AdMobAppOpen AppOpen_Get(string adName)
        {
            foreach (var ad in adAppOpens)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion

        #region Banner
        public int Banner_Count()
        {
            return adBanners.Length;
        }
        public AdMobBanner Banner_Get(int index)
        {
            if (index < 0 || index >= adBanners.Length)
                return null;
            return adBanners[index];
        }
        public AdMobBanner Banner_Get(string adName)
        {
            foreach (var ad in adBanners)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion

        #region Interstitial
        public int Interstitial_Count()
        {
            return adInterstitials.Length;
        }
        public AdMobInterstitial Interstitial_Get(int index)
        {
            if (index < 0 || index >= adInterstitials.Length)
                return null;
            return adInterstitials[index];
        }
        public AdMobInterstitial Interstitial_Get(string adName)
        {
            foreach (var ad in adInterstitials)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion

        #region Rewarded
        public int Rewarded_Count()
        {
            return adRewardeds.Length;
        }
        public AdMobRewarded Rewarded_Get(int index)
        {
            if (index < 0 || index >= adRewardeds.Length)
                return null;
            return adRewardeds[index];
        }
        public AdMobRewarded Rewarded_Get(string adName)
        {
            foreach (var ad in adRewardeds)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion

        #region RewardedInterstitial
        public int RewardedInterstitial_Count()
        {
            return adRewardedInterstitials.Length;
        }
        public AdMobRewardedInterstitial RewardedInterstitial_Get(int index)
        {
            if (index < 0 || index >= adRewardedInterstitials.Length)
                return null;
            return adRewardedInterstitials[index];
        }
        public AdMobRewardedInterstitial RewardedInterstitial_Get(string adName)
        {
            foreach (var ad in adRewardedInterstitials)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion
    }
}

using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using KTool.Attribute;
using KTool.Init;
using System.Collections.Generic;
using UnityEngine;

namespace KPlugin.AdMob
{
    public class AdMobManager : MonoBehaviour, IIniter
    {
        #region Properties
        public const string ADMOB_SCOURCE = "GoogleAdMob",
            ADMOB_COUNTRY_CODE = "UnknownCountry";
        public const float VALUE_SCALE = 1000000;
        private const string ERROR_GOOGLE_MOBILE_ADS_INIT_FAIL = "Google Mobile Ads initialization failed.",
            ERROR_GOOGLE_MOBILE_ADS_FAILED_TO_GATHER_CONSENT = "Google Mobile Ads failed to gather consent with error: {0}",
            ERROR_GOOGLE_MOBILE_ADS_CONSENT_UPDATED = "Google Mobile Ads consent updated: {0}";

        public static AdMobManager Instance
        {
            get;
            private set;
        }

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool tagForUnderAgeOfConsent;
        [SerializeField]
        private List<string> testDeviceIds;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobAppOpen[] adAppOpens;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobBanner[] adBanners;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobInterstitial[] adInterstitials;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMobRewarded[] adRewardeds;

        private bool isInit,
            isIniting,
            isConsent;
        private InitTrackingSource initTracking;

        public bool IsInit => isInit;
        private bool CanRequestAds => ConsentInformation.CanRequestAds();
        #endregion

        #region Init
        public IInitTracking InitBegin()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                //
                if (initTracking == null)
                {
                    initTracking = new InitTrackingSource(initIndispensable);
                    AdMob_Init();
                }
                return initTracking;
            }
            //
            return IInitTracking.Success;
        }

        public void InitEnd()
        {

        }
        #endregion

        #region AdMob
        public static bool IsReady()
        {
            return Instance != null && Instance.IsInit;
        }
        private void AdMob_Init()
        {
            if (IsInit || isIniting || isConsent)
                return;
            //
            MobileAds.SetiOSAppPauseOnBackground(true);
            AdMob_RequestTestDevice();
            //
            if (CanRequestAds)
            {
                isIniting = true;
                MobileAds.Initialize(AdMob_OnInitComplete);
            }
            // Ensures that privacy and consent information is up to date.
            Consent_Init();
        }
        private void AdMob_OnInitComplete(InitializationStatus initStatus)
        {
            if (initStatus == null)
                Debug.LogError(ERROR_GOOGLE_MOBILE_ADS_INIT_FAIL);
            //
            isInit = true;
            isIniting = false;
            initTracking.CompleteSuccess();
            initTracking = null;
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

        #region AdMod Consent
        private void Consent_Init()
        {
            isConsent = true;
            ConsentRequestParameters requestParameters = new ConsentRequestParameters
            {
                // False means users are not under age.
                TagForUnderAgeOfConsent = tagForUnderAgeOfConsent
            };
            ConsentInformation.Update(requestParameters, Consent_InfoUpdated);
        }
        private void Consent_InfoUpdated(FormError consentError)
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { });
            //
            if (consentError == null)
            {
                if (CanRequestAds)
                {
                    Consent_OnComplete(string.Empty);
                }
                else
                {
                    isConsent = false;
                    // If the error is null, the consent information state was updated.
                    // You are now ready to check if a form is available.
                    ConsentForm.LoadAndShowConsentFormIfRequired(Consent_OnDismissed);
                }
            }
            else
            {
                // Handle the error.
                Consent_OnComplete(consentError.Message);
            }
        }
        private void Consent_OnDismissed(FormError formError)
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { });
            //
            if (formError == null)
            {
                // Consent has been gathered.
                Consent_OnComplete(string.Empty);
            }
            else
            {
                Consent_OnComplete(formError.Message);
            }
        }
        private void Consent_OnComplete(string error)
        {
            isConsent = false;
            //
            string message;
            if (string.IsNullOrEmpty(error))
            {
                message = string.Format(ERROR_GOOGLE_MOBILE_ADS_CONSENT_UPDATED, ConsentInformation.ConsentStatus);
                Debug.Log(message);
            }
            else
            {
                message = string.Format(ERROR_GOOGLE_MOBILE_ADS_FAILED_TO_GATHER_CONSENT, error);
                Debug.LogError(message);
            }
            //
            if (IsInit || isIniting)
                return;
            if (CanRequestAds)
            {
                MobileAds.Initialize(AdMob_OnInitComplete);
            }
            else
            {
                initTracking.CompleteFail();
                initTracking = null;
            }
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
    }
}

using GoogleMobileAds.Api;
using KTool;
using KTool.Advertisement;
using KTool.Init;
using System;
using System.Collections;
using UnityEngine;

namespace KPlugin.GoogleAdMob
{
    public class AdMobRewardedInterstitial : AdRewarded, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad RewardedInterstitial load fail code: {0}",
            ERROR_DISPLAYED_FAIL = "Ad RewardedInterstitial displayed fail",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad RewardedInterstitial show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad RewardedInterstitial show fail: ad is show";
        private const int AD_EXPIRE_HOUR = 4;

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMobAdType.RewardedInterstitial)]
        private int indexAd = 0;

        private bool isLoading;
        private int attemptLoad;
        private RewardedInterstitialAd adObject;
        private DateTime expireTime;
        private InitTrackingSource initTrackingSource;
        private AdRewardedTrackingSource adTrackingSource;

        public event Action OnAdImpressionRecorded;

        public string AdId
        {
            get
            {
                AdMobSettingAdId settingAdId = AdMobSetting.Instance.Ad_Get(AdMobAdType.RewardedInterstitial, indexAd);
                if (settingAdId != null)
                    return settingAdId.AdID;
                return string.Empty;
            }
        }
        public override bool IsAutoReload
        {
            get => base.IsAutoReload;
            protected set
            {
                if (value == base.IsAutoReload)
                    return;
                base.IsAutoReload = value;
                if (base.IsAutoReload)
                    Load();
            }
        }
        public override bool IsReady => base.IsReady && adObject != null && adObject.CanShowAd();
        #endregion

        #region Unity Event
        private void Update()
        {
            Update_ExpireTime();
        }
        private void OnDestroy()
        {
            if (instance != null && instance.GetInstanceID() == GetInstanceID())
                instance = null;
            Destroy();
        }
        #endregion

        #region Init
        public InitTracking InitBegin()
        {
            initTrackingSource = new InitTrackingSource(initIndispensable);
            Load();
            return initTrackingSource;
        }
        public void InitEnd()
        {

        }
        #endregion

        #region Method
        public override void Init()
        {
            if (IsInited)
                return;
            //
            if (setInstance)
                instance = this;
            IsInited = true;
            PushEvent_Inited();
        }
        public override void Load()
        {
            Init();
            //
            if (IsLoaded)
                return;
            //
            Ad_Create();
        }
        public override void Destroy()
        {
            IsDestroy = true;
            if (!IsShow)
            {
                Ad_Destroy();
                PushEvent_Destroy();
            }
        }
        public override AdRewardedTracking Show()
        {
            if (IsShow)
                return new AdRewardedTrackingSource(ERROR_SHOW_FAIL_AD_IS_SHOWED);
            if (!IsReady)
                return new AdRewardedTrackingSource(ERROR_SHOW_FAIL_AD_NOT_READY);
            //
            adTrackingSource = new AdRewardedTrackingSource(this);
            IsShow = true;
            adObject.Show(Ad_OnReward);
            return adTrackingSource;
        }
        private void Update_ExpireTime()
        {
            if (!IsLoaded || IsShow || DateTime.Now < expireTime)
                return;
            //
            Ad_Destroy();
            if (IsAutoReload)
                Ad_Create();
        }
        #endregion

        #region Ad
        private void Ad_Create()
        {
            if (isLoading)
                return;
            isLoading = true;
            //
            CoroutineManager.Instance.Coroutine_Start(Ad_LoadAd());
        }
        private void Ad_Destroy()
        {
            if (adObject != null)
            {
                adObject.Destroy();
                adObject = null;
            }
            IsLoaded = false;
        }
        private IEnumerator Ad_LoadAd()
        {
            if (attemptLoad > 0)
            {
                float delay = Mathf.Pow(2, attemptLoad);
                yield return new WaitForSecondsRealtime(delay);
            }
            //
            while (!AdMobManager.IsInit)
                yield return new WaitForEndOfFrame();
            //
            AdRequest adRequest = new AdRequest();
            RewardedInterstitialAd.Load(AdId, adRequest, Ad_OnLoadComplete);
        }
        private void Ad_OnLoadComplete(RewardedInterstitialAd adObject, LoadAdError error)
        {
            isLoading = false;
            if (error != null || adObject == null)
            {
                if (error != null)
                    Debug.LogWarning(string.Format(ERROR_LOAD_FAIL, error.GetCode()));
                if (initTrackingSource != null)
                {
                    initTrackingSource.CompleteFail();
                    initTrackingSource = null;
                }
                //
                attemptLoad = Mathf.Min(attemptLoad + 1, 6);
                PushEvent_Loaded(false);
                if (!IsDestroy && IsAutoReload)
                    Ad_Create();
                return;
            }
            //
            if (initTrackingSource != null)
            {
                initTrackingSource.CompleteSuccess();
                initTrackingSource = null;
            }
            attemptLoad = 0;
            this.adObject = adObject;
            expireTime = DateTime.Now + TimeSpan.FromHours(AD_EXPIRE_HOUR);
            Ad_EventRegister();
            //
            IsLoaded = true;
            PushEvent_Loaded(true);
        }
        private void Ad_EventRegister()
        {
            adObject.OnAdFullScreenContentOpened += Ad_OnFullScreenContentOpened;
            adObject.OnAdFullScreenContentFailed += Ad_OnFullScreenContentFailed;
            adObject.OnAdClicked += Ad_OnClicked;
            adObject.OnAdFullScreenContentClosed += Ad_OnFullScreenContentClosed;
            adObject.OnAdPaid += Ad_OnPaid;
            adObject.OnAdImpressionRecorded += Ad_OnImpressionRecorded;
        }
        private void Ad_OnFullScreenContentOpened()
        {
            PushEvent_Displayed(true);
            adTrackingSource.Displayed(true);
        }
        private void Ad_OnFullScreenContentFailed(AdError adError)
        {
            IsShow = false;
            Ad_Destroy();
            //
            Debug.LogWarning(ERROR_DISPLAYED_FAIL);
            PushEvent_Displayed(false);
            adTrackingSource.Displayed(false);
            //
            if (IsDestroy)
                PushEvent_Destroy();
            else if (IsAutoReload)
                Ad_Create();
        }
        private void Ad_OnClicked()
        {
            PushEvent_Clicked();
            adTrackingSource.Clicked();
        }
        private void Ad_OnFullScreenContentClosed()
        {
            IsShow = false;
            Ad_Destroy();
            //
            PushEvent_Hidden();
            adTrackingSource.Hidden();
            //
            if (IsDestroy)
                PushEvent_Destroy();
            else if (IsAutoReload)
                Ad_Create();
        }
        private void Ad_OnPaid(AdValue adValue)
        {
            if (adValue == null)
                return;
            //
            AdRevenuePaid revenuePaid = new AdRevenuePaid(
                AdMobManager.ADMOB_SCOURCE,
                AdMobManager.ADMOB_SCOURCE,
                AdId,
                AdMobManager.ADMOB_COUNTRY_CODE,
                AdType,
                adValue.Value,
                adValue.CurrencyCode);
            //
            PushEvent_RevenuePaid(revenuePaid);
            adTrackingSource.RevenuePaid(revenuePaid);
        }
        private void Ad_OnImpressionRecorded()
        {
            OnAdImpressionRecorded?.Invoke();
        }
        private void Ad_OnReward(Reward reward)
        {
            AdRewardReceived adRewardReceived = new AdRewardReceived(reward.Type, true, reward.Amount);
            PushEvent_ReceivedReward(adRewardReceived);
            adTrackingSource.ReceivedReward(adRewardReceived);
        }
        #endregion
    }
}

using GoogleMobileAds.Api;
using KTool.Advertisement;
using KTool.Cron;
using KTool.Init;
using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace KPlugin.GoogleAdMob
{
    public class AdMobInterstitial : AdInterstitial, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad load fail code: {0}",
            ERROR_DISPLAY_FAIL = "Ad display fail code: {0}",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad show fail: ad is showing";
        private const int AD_EXPIRE_HOUR = 4;

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMobAdType.Interstitial)]
        private int indexAd = 0;

        private bool isLoading;
        private int attemptLoad;
        private InterstitialAd adObject;
        private DateTime expireTime;
        private InitTrackingSource initTrackingSource;
        private AdInterstitialTrackingSource adTrackingSource;
        private string placement;

        public event Action OnAdImpressionRecorded;

        public string AdId
        {
            get
            {
                AdMobSettingAdId settingAdId = AdMobSetting.Instance.Ad_Get(AdMobAdType.Interstitial, indexAd);
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
        public IInitTracking InitBegin()
        {
            if (IsDestroy || IsInited || initTrackingSource != null)
                return IInitTracking.Fail;
            //
            initTrackingSource = new InitTrackingSource(initIndispensable);
            OnAdLoaded += Init_OnAdLoaded;
            Load();
            return initTrackingSource;
        }

        public void InitEnd()
        {

        }
        private void Init_OnAdLoaded(Ad source, bool isSuccess)
        {
            OnAdLoaded -= Init_OnAdLoaded;
            if (isSuccess)
                initTrackingSource.CompleteSuccess();
            else
                initTrackingSource.CompleteFail();
            initTrackingSource = null;
        }
        #endregion

        #region Methods
        public override void Init()
        {
            if (IsDestroy || IsInited)
                return;
            IsInited = true;
            //
            if (setInstance)
                instance = this;
            PushEvent_Inited(true);
        }
        public override void Load()
        {
            if (IsDestroy)
                return;
            Init();
            //
            Ad_Create();
        }
        public override void Destroy()
        {
            if (IsDestroy)
                return;
            IsDestroy = true;
            if (IsInited)
            {
                if (!IsShow)
                {
                    Ad_Destroy();
                }
            }
            else
            {
                PushEvent_Destroy();
            }
        }
        public override IAdTracking Show(string placement = "")
        {
            if (IsShow)
                return new AdInterstitialTrackingSource(this, ERROR_SHOW_FAIL_AD_IS_SHOWED);
            if (!IsReady)
                return new AdInterstitialTrackingSource(this, ERROR_SHOW_FAIL_AD_NOT_READY);
            //
            this.placement = placement;
            adTrackingSource = new AdInterstitialTrackingSource(this);
            IsShow = true;
            adObject.Show();
            return adTrackingSource;
        }
        private void Update_ExpireTime()
        {
            if (!IsLoaded || IsShow || DateTime.Now < expireTime)
                return;
            //
            Load();
        }
        #endregion

        #region Ad
        private void Ad_Create()
        {
            if (IsDestroy || IsLoaded || isLoading)
                return;
            isLoading = true;
            //
            if (adObject != null)
            {
                adObject.Destroy();
                adObject = null;
            }
            //
            float delay = attemptLoad > 0 ? Mathf.Pow(2, attemptLoad) : 0;
            CronObject.Create()
                .Add(ConditionReadTime.Create(delay))
                .Add(ConditionDelegate.Create(AdMobManager.IsReady))
                .Add(CallbackAction.Create(Ad_LoadAd))
                .Run();
        }
        private void Ad_Destroy()
        {
            if (adObject != null)
            {
                adObject.Destroy();
                adObject = null;
            }
            PushEvent_Destroy();
        }
        private void Ad_LoadAd()
        {
            if (IsDestroy)
            {
                isLoading = false;
                return;
            }
            //
            AdRequest adRequest = new AdRequest();
            InterstitialAd.Load(AdId, adRequest, Ad_OnLoadComplete);
        }
        private void Ad_OnLoadComplete(InterstitialAd adObject, LoadAdError error)
        {
            if (error != null || adObject == null)
            {
                isLoading = false;
                if (IsDestroy)
                    return;
                if (IsAutoReload)
                    Ad_Create();
                //
                if (error != null)
                    Debug.LogWarning(string.Format(ERROR_LOAD_FAIL, error.GetCode()));
                //
                attemptLoad = Mathf.Min(attemptLoad + 1, 6);
                PushEvent_Loaded(false);
                //
                if (IsAutoReload)
                    Ad_Create();
            }
            else
            {
                isLoading = false;
                if (IsDestroy)
                {
                    adObject.Destroy();
                    return;
                }
                IsLoaded = true;
                attemptLoad = 0;
                this.adObject = adObject;
                Ad_EventRegister();
                expireTime = DateTime.Now + TimeSpan.FromHours(AD_EXPIRE_HOUR);
                //
                PushEvent_Loaded(true);
            }
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
            adTrackingSource.PushEvent_Displayed(true);
        }
        private void Ad_OnFullScreenContentFailed(AdError adError)
        {
            IsShow = false;
            IsLoaded = false;
            if (IsDestroy)
            {
                Ad_Destroy();
            }
            else
            {
                if (adError != null)
                    Debug.LogWarning(string.Format(ERROR_DISPLAY_FAIL, adError.GetCode()));
                PushEvent_Displayed(false);
                adTrackingSource.PushEvent_Displayed(false);
                //
                if (IsAutoReload)
                    Ad_Create();
            }
        }
        private void Ad_OnClicked()
        {
            PushEvent_Clicked();
            adTrackingSource.PushEvent_Clicked();
        }
        private void Ad_OnFullScreenContentClosed()
        {
            IsShow = false;
            IsLoaded = false;
            //
            if (IsDestroy)
            {
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
                //
                Ad_Destroy();
            }
            else
            {
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
                //
                if (IsAutoReload)
                    Ad_Create();
            }
        }
        private void Ad_OnPaid(AdValue adValue)
        {
            if (adValue == null)
                return;
            //
            AdRevenuePaid revenuePaid = new AdRevenuePaid(
                source: AdMobManager.ADMOB_SCOURCE,
                network_name: AdMobManager.ADMOB_SCOURCE,
                idAd: AdId,
                adType: AdType,
                countryCode: AdMobManager.ADMOB_COUNTRY_CODE,
                placement: placement,
                value: adValue.Value / AdMobManager.VALUE_SCALE,
                currency: adValue.CurrencyCode);
            //
            PushEvent_RevenuePaid(revenuePaid);
            adTrackingSource.PushEvent_RevenuePaid(revenuePaid);
        }
        private void Ad_OnImpressionRecorded()
        {
            OnAdImpressionRecorded?.Invoke();
        }
        #endregion
    }
}

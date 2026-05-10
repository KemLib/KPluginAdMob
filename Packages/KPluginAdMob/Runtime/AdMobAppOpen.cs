using GoogleMobileAds.Api;
using KTool.Advertisement;
using KTool.Cron;
using KTool.Init;
using System;
using UnityEngine;

namespace KPlugin.AdMob
{
    public class AdMobAppOpen : AdAppOpen, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad AppOpen load fail code: {0}",
            ERROR_DISPLAY_FAIL = "Ad AppOpen display fail code: {0}",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad AppOpen show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad AppOpen show fail: ad is showing";
        private const int AD_EXPIRE_HOUR = 4;

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMobAdType.AppOpen)]
        private int indexAd = 0;

        private bool isInit,
            isLoading;
        private int attemptLoad;
        private AppOpenAd adObject;
        private DateTime expireTime;
        private InitTrackingSource initTrackingSource;

        public event Action OnAdImpressionRecorded;

        public string AdId
        {
            get
            {
                AdMobSettingAdId settingAdId = AdMobSetting.Instance.Ad_Get(AdMobAdType.AppOpen, indexAd);
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
            if (IsDestroy || isInit || initTrackingSource != null)
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
        private void Init_OnAdLoaded(AdBase source, bool isSuccess)
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
        public override void Load()
        {
            if (IsDestroy)
                return;
            //
            if (!isInit)
            {
                isInit = true;
                //
                if (setInstance)
                    instance = this;
            }
            //
            Ad_Create();
        }
        public override void Destroy()
        {
            if (IsDestroy)
                return;
            //
            IsDestroy = true;
            if (isInit)
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
        protected override bool OnShow(out string error)
        {
            if (IsShow)
            {
                error = ERROR_SHOW_FAIL_AD_IS_SHOWED;
                return false;
            }
            if (!IsReady)
            {
                error = ERROR_SHOW_FAIL_AD_NOT_READY;
                return false;
            }
            //
            IsShow = true;
            adObject.Show();
            //
            error = string.Empty;
            return true;
        }
        private void Update_ExpireTime()
        {
            if (!IsReady || IsShow || DateTime.Now < expireTime)
                return;
            //
            IsLoaded = false;
            Load();
        }
        #endregion

        #region Ad Event
        private void Ad_EventRegister()
        {
            adObject.OnAdFullScreenContentOpened += Ad_OnFullScreenContentOpened;
            adObject.OnAdFullScreenContentFailed += Ad_OnFullScreenContentFailed;
            adObject.OnAdClicked += Ad_OnClicked;
            adObject.OnAdFullScreenContentClosed += Ad_OnFullScreenContentClosed;
            adObject.OnAdPaid += Ad_OnPaid;
            adObject.OnAdImpressionRecorded += Ad_OnImpressionRecorded;
        }
        private void Ad_EventUnRegister()
        {
            adObject.OnAdFullScreenContentOpened -= Ad_OnFullScreenContentOpened;
            adObject.OnAdFullScreenContentFailed -= Ad_OnFullScreenContentFailed;
            adObject.OnAdClicked -= Ad_OnClicked;
            adObject.OnAdFullScreenContentClosed -= Ad_OnFullScreenContentClosed;
            adObject.OnAdPaid -= Ad_OnPaid;
            adObject.OnAdImpressionRecorded -= Ad_OnImpressionRecorded;
        }
        #endregion

        #region Ad
        private void Ad_Destroy()
        {
            if (adObject != null)
            {
                Ad_EventUnRegister();
                adObject.Destroy();
                adObject = null;
            }
            PushEvent_Destroy();
        }
        private void Ad_Create()
        {
            if (IsDestroy || IsLoaded || isLoading)
                return;
            isLoading = true;
            //
            if (adObject != null)
            {
                Ad_EventUnRegister();
                adObject.Destroy();
                adObject = null;
            }
            //
            float delay = attemptLoad > 0 ? Mathf.Pow(2, attemptLoad) : 0;
            if (delay <= 0 && AdMobManager.IsReady())
            {
                Ad_LoadAd();
            }
            else
            {
                CronObject.Create()
                    .Add(ConditionReadTime.Create(delay))
                    .Add(ConditionDelegate.Create(AdMobManager.IsReady))
                    .Add(CallbackAction.Create(Ad_LoadAd))
                    .Run();
            }
        }
        private void Ad_LoadAd()
        {
            if (IsDestroy)
            {
                isLoading = false;
            }
            else
            {
                AdRequest adRequest = new AdRequest();
                AppOpenAd.Load(AdId, adRequest, Ad_OnLoadComplete);
            }
        }
        private void Ad_OnLoadComplete(AppOpenAd adObject, LoadAdError error)
        {
            isLoading = false;
            if (error != null || adObject == null)
            {
                if (IsDestroy)
                    return;
                //
                attemptLoad = Mathf.Min(attemptLoad + 1, 6);
                if (error != null)
                    Debug.LogWarning(string.Format(ERROR_LOAD_FAIL, error.GetCode()));
                PushEvent_Loaded(false);
                //
                if (IsAutoReload)
                    Ad_Create();
            }
            else
            {
                if (IsDestroy)
                {
                    adObject.Destroy();
                    return;
                }
                //
                IsLoaded = true;
                attemptLoad = 0;
                expireTime = DateTime.Now + TimeSpan.FromHours(AD_EXPIRE_HOUR);
                this.adObject = adObject;
                Ad_EventRegister();
                //
                PushEvent_Loaded(true);
            }
        }
        private void Ad_OnFullScreenContentOpened()
        {
            PushEvent_Displayed(true);
        }
        private void Ad_OnFullScreenContentFailed(AdError adError)
        {
            IsShow = false;
            IsLoaded = false;
            if (adError != null)
                Debug.LogWarning(string.Format(ERROR_DISPLAY_FAIL, adError.GetCode()));
            PushEvent_Displayed(false);
            //
            if (IsDestroy)
            {
                Ad_Destroy();
            }
            else
            {
                if (IsAutoReload)
                    Ad_Create();
            }
        }
        private void Ad_OnClicked()
        {
            PushEvent_Clicked();
        }
        private void Ad_OnFullScreenContentClosed()
        {
            IsShow = false;
            IsLoaded = false;
            PushEvent_Hidden();
            //
            if (IsDestroy)
            {
                Ad_Destroy();
            }
            else
            {
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
                placement: Placement,
                value: adValue.Value / AdMobManager.VALUE_SCALE,
                currency: adValue.CurrencyCode);
            //
            PushEvent_RevenuePaid(revenuePaid);
        }
        private void Ad_OnImpressionRecorded()
        {
            OnAdImpressionRecorded?.Invoke();
        }
        #endregion
    }
}

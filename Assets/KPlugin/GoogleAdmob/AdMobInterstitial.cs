using GoogleMobileAds.Api;
using KTool.Advertisement;
using KTool.Init;
using System;
using System.Collections;
using UnityEngine;

namespace KPlugin.GoogleAdMob
{
    public class AdMobInterstitial : AdInterstitial, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad load fail code: {0}",
            ERROR_DISPLAY_FAIL = "Ad display fail code: {0}",
            ERROR_SHOW_FAIL_AD_IS_DESTROY = "Ad show fail: ad  is destroyed",
            ERROR_SHOW_FAIL_AD_NOT_INIT = "Ad show fail: ad not inited",
            ERROR_SHOW_FAIL_AD_NOT_LOADED = "Ad show fail: ad not loaded",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad show fail: ad is showing";
        private const int AD_EXPIRE_HOUR = 4;

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMobAdType.Interstitial)]
        private int indexAd = 0;

        private bool isIniting,
            isLoading;
        private int attemptLoad;
        private InterstitialAd adObject;
        private DateTime expireTime;
        private InitTrackingSource initTrackingSource;
        private AdInterstitialTrackingSource adTrackingSource;

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
            initTrackingSource = new InitTrackingSource(initIndispensable, true);
            OnAdInited += Init_OnAdInited;
            Init();
            return initTrackingSource;
        }

        public void InitEnd()
        {

        }
        private void Init_OnAdInited(Ad source, bool isSuccess)
        {
            OnAdInited -= Init_OnAdInited;
            if (isSuccess)
            {
                OnAdLoaded += Init_OnLoaded;
                Load();
            }
            else
            {
                initTrackingSource.CompleteFail();
                initTrackingSource = null;
            }
        }
        private void Init_OnLoaded(Ad source, bool isSuccess)
        {
            OnAdLoaded -= Init_OnLoaded;
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
            if (IsInited || isIniting)
                return;
            isIniting = true;
            //
            if (setInstance)
                instance = this;
            StartCoroutine(Ad_Create());
        }
        public override void Load()
        {
            if (!IsInited || IsLoaded || isLoading)
                return;
            isLoading = true;
            //
            StartCoroutine(Ad_Load());
        }
        public override void Destroy()
        {
            IsDestroy = true;
            if (isIniting || isLoading || IsShow)
                return;
            if (IsInited)
                Ad_Destroy();
            else
                PushEvent_Destroy();
        }
        public override IAdTracking Show()
        {
            if (IsDestroy)
                return new AdInterstitialTrackingSource(this, ERROR_SHOW_FAIL_AD_IS_DESTROY);
            if (!IsInited)
                return new AdInterstitialTrackingSource(this, ERROR_SHOW_FAIL_AD_NOT_INIT);
            if (!IsLoaded)
                return new AdInterstitialTrackingSource(this, ERROR_SHOW_FAIL_AD_NOT_LOADED);
            if (!IsReady)
                return new AdInterstitialTrackingSource(this, ERROR_SHOW_FAIL_AD_NOT_READY);
            if (IsShow)
                return new AdInterstitialTrackingSource(this, ERROR_SHOW_FAIL_AD_IS_SHOWED);
            //
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
        private IEnumerator Ad_Create()
        {
            while (!AdMobManager.Instance.IsInit)
                yield return new WaitForEndOfFrame();
            //
            if (IsDestroy)
            {
                isIniting = false;
                PushEvent_Inited(false);
                //
                PushEvent_Destroy();
                yield break;
            }
            //
            IsInited = true;
            isIniting = false;
            PushEvent_Inited(true);
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
        private IEnumerator Ad_Load()
        {
            if (adObject != null)
            {
                adObject.Destroy();
                adObject = null;
            }
            //
            if (attemptLoad > 0)
                yield return new WaitForSecondsRealtime(attemptLoad * 2);
            else
                yield return new WaitForEndOfFrame();
            //
            if (IsDestroy)
            {
                isLoading = false;
                PushEvent_Loaded(false);
                //
                Ad_Destroy();
                yield break;
            }
            //
            AdRequest adRequest = new AdRequest();
            InterstitialAd.Load(AdId, adRequest, Ad_OnLoadComplete);
        }
        private void Ad_OnLoadComplete(InterstitialAd adObject, LoadAdError error)
        {
            if (error != null || adObject == null)
            {
                if (error != null)
                    Debug.LogWarning(string.Format(ERROR_LOAD_FAIL, error.GetCode()));
                //
                attemptLoad = Mathf.Min(attemptLoad + 1, 6);
                IsLoaded = false;
                //
                if (IsDestroy)
                {
                    isLoading = false;
                    PushEvent_Loaded(false);
                    //
                    Ad_Destroy();
                }
                else
                {
                    if (IsAutoReload)
                        StartCoroutine(Ad_Load());
                    else
                        isLoading = false;
                    PushEvent_Loaded(false);
                }
            }
            else
            {
                attemptLoad = 0;
                IsLoaded = true;
                isLoading = false;
                this.adObject = adObject;
                Ad_EventRegister();
                expireTime = DateTime.Now + TimeSpan.FromHours(AD_EXPIRE_HOUR);
                if (IsDestroy)
                {
                    PushEvent_Loaded(true);
                    //
                    Ad_Destroy();
                }
                else
                {
                    PushEvent_Loaded(true);
                }
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
            if (adError != null)
                Debug.LogWarning(string.Format(ERROR_DISPLAY_FAIL, adError.GetCode()));
            //
            IsLoaded = false;
            IsShow = false;
            if (IsDestroy)
            {
                PushEvent_Displayed(false);
                adTrackingSource.PushEvent_Displayed(false);
                //
                Ad_Destroy();
            }
            else
            {
                if (IsAutoReload)
                {
                    isLoading = true;
                    StartCoroutine(Ad_Load());
                }
                PushEvent_Displayed(false);
                adTrackingSource.PushEvent_Displayed(false);
            }
        }
        private void Ad_OnClicked()
        {
            PushEvent_Clicked();
            adTrackingSource.PushEvent_Clicked();
        }
        private void Ad_OnFullScreenContentClosed()
        {
            IsLoaded = false;
            IsShow = false;
            if (IsDestroy)
            {
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
                //
                Ad_Destroy();
            }
            else
            {
                if (IsAutoReload)
                {
                    isLoading = true;
                    StartCoroutine(Ad_Load());
                }
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
            }
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
                adValue.Value / AdMobManager.VALUE_SCALE,
                adValue.CurrencyCode);
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

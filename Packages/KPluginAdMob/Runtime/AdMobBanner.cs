using GoogleMobileAds.Api;
using KTool;
using KTool.Advertisement;
using KTool.Cron;
using KTool.Init;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KPlugin.GoogleAdMob
{
    public class AdMobBanner : AdBanner, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad load fail code: {0}",
            ERROR_IS_DESTROY = "Ad is destroy";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMobAdType.Banner)]
        private int indexAd = 0;

        private bool isLoading;
        private int attemptLoad;
        private bool isCreateAdObject;
        private BannerView adObject;
        private InitTrackingSource initTrackingSource;
        private AdBannerTrackingSource adTrackingSource;

        public event Action OnAdImpressionRecorded;


        public string AdId
        {
            get
            {
                AdMobSettingAdId settingAdId = AdMobSetting.Instance.Ad_Get(AdMobAdType.Banner, indexAd);
                if (settingAdId != null)
                    return settingAdId.AdID;
                return string.Empty;
            }
        }
        public override bool IsAutoReload
        {
            get => true;
            protected set
            {

            }
        }
        public override KTool.Advertisement.AdPosition PositionType
        {
            get => base.PositionType;
            protected set
            {
                base.PositionType = value;
                if (IsInited)
                {
                    adObject.SetPosition(Utility.ConvertPosition(base.PositionType));
                }
            }
        }
        public override Vector2 Position
        {
            get => base.Position;
            protected set
            {
                base.Position = value;
                if (IsInited)
                {
                    Vector2 point = Utility.Convert_UnityToAdMob(base.Position);
                    adObject.SetPosition((int)point.x, (int)point.y);
                }
            }
        }
        #endregion

        #region Methods Unity
        private void OnDestroy()
        {
            if (instance != null && instance.GetInstanceID() == GetInstanceID())
                instance = null;
            Destroy();
        }
        #endregion

        #region Unity Event
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

        #region Method
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
            if (isCreateAdObject)
            {
                if (!IsAutoReload)
                    Ad_LoadAd();
            }
            else
            {
                Ad_Create();
            }
        }
        public override void Destroy()
        {
            if (IsDestroy)
                return;
            IsDestroy = true;
            Ad_Destroy();
            PushEvent_Destroy();
        }
        public override IAdBannerTracking Show()
        {
            if (IsDestroy)
                return new AdBannerTrackingSource(this, ERROR_IS_DESTROY);
            //
            if (IsShow)
            {
                return adTrackingSource;
            }
            else
            {
                adTrackingSource = new AdBannerTrackingSource(this);
                IsShow = true;
                if (IsLoaded)
                {
                    adObject.Show();
                    PushEvent_Displayed(true);
                    adTrackingSource.PushEvent_Displayed(true);
                }
                return adTrackingSource;
            }
        }
        public override void Hide()
        {
            if (IsShow)
            {
                adObject.Hide();
                IsShow = false;
                //
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
                adTrackingSource = null;
            }
            else
            {
                IsShow = false;
            }
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
                PushEvent_Loaded(false);
                //
                Ad_Destroy();
            }
            //
            adObject = Utility.Create_AdBanner(AdId, SizeType, PositionType, Size, Position);
            Ad_EventRegister();
            //
            var adRequest = new AdRequest();
            adObject.LoadAd(adRequest);
        }
        private void Ad_EventRegister()
        {
            adObject.OnBannerAdLoaded += Ad_OnLoaded;
            adObject.OnBannerAdLoadFailed += Ad_OnLoadFailed;
            adObject.OnAdFullScreenContentOpened += Ad_OnFullScreenContentOpened;
            adObject.OnAdFullScreenContentClosed += Ad_OnFullScreenContentClosed;
            adObject.OnAdClicked += Ad_OnClicked;
            adObject.OnAdPaid += Ad_OnPaid;
            adObject.OnAdImpressionRecorded += Ad_OnImpressionRecorded;
        }
        private void Ad_OnLoaded()
        {
            isLoading = false;
            if (IsDestroy)
                return;
            //
            IsLoaded = true;
            attemptLoad = 0;
            //
            if (IsShow)
            {
                adObject.Show();
                PushEvent_Displayed(true);
                adTrackingSource.PushEvent_Displayed(true);
            }
            else
            {
                adObject.Hide();
            }
            PushEvent_Loaded(true);
        }
        private void Ad_OnLoadFailed(LoadAdError error)
        {
            isLoading = false;
            if (IsDestroy)
                return;
            //
            attemptLoad = Mathf.Min(attemptLoad + 1, 6);
            if (error != null)
                Debug.LogWarning(string.Format(ERROR_LOAD_FAIL, error.GetCode()));
            //
            PushEvent_Loaded(false);
        }
        private void Ad_OnFullScreenContentOpened()
        {
            PushEvent_Expanded(true);
            adTrackingSource.PushEvent_Expanded(true);
        }
        private void Ad_OnFullScreenContentClosed()
        {
            PushEvent_Expanded(false);
            adTrackingSource.PushEvent_Expanded(false);
        }
        private void Ad_OnClicked()
        {
            PushEvent_Clicked();
            adTrackingSource.PushEvent_Clicked();
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
                placement: string.Empty,
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

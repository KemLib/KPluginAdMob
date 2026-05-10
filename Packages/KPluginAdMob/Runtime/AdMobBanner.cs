using GoogleMobileAds.Api;
using KTool.Advertisement;
using KTool.Cron;
using KTool.Init;
using System;
using UnityEngine;

namespace KPlugin.AdMob
{
    public class AdMobBanner : AdBanner, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad Banner load fail code: {0}",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad Banner show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad Banner show fail: ad is show";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMobAdType.Banner)]
        private int indexAd = 0;

        private bool isInit,
            isCreating,
            isCreated,
            isLoading;
        private int attemptLoad;
        private BannerView adObject;
        private InitTrackingSource initTrackingSource;

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
                if (isCreated && adObject != null)
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
                if (isCreated && adObject != null && PositionType == KTool.Advertisement.AdPosition.Custom)
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

        #region Method
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
            if (isCreated)
            {
                if (!IsAutoReload)
                    Ad_Load_Begin();
            }
            else
            {
                Ad_Create_Begin();
            }
        }
        public override void Destroy()
        {
            if (IsDestroy)
                return;
            //
            IsDestroy = true;
            Ad_Destroy();
            PushEvent_Destroy();
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
            PushEvent_Displayed(true);
            //
            error = null;
            return true;
        }
        protected override bool OnHide()
        {
            if (!IsShow)
                return false;
            //
            IsShow = false;
            adObject.Hide();
            PushEvent_Hidden();
            //
            return true;
        }
        #endregion

        #region Ad Event
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
        private void Ad_EventUnRegister()
        {
            adObject.OnBannerAdLoaded -= Ad_OnLoaded;
            adObject.OnBannerAdLoadFailed -= Ad_OnLoadFailed;
            adObject.OnAdFullScreenContentOpened -= Ad_OnFullScreenContentOpened;
            adObject.OnAdFullScreenContentClosed -= Ad_OnFullScreenContentClosed;
            adObject.OnAdClicked -= Ad_OnClicked;
            adObject.OnAdPaid -= Ad_OnPaid;
            adObject.OnAdImpressionRecorded -= Ad_OnImpressionRecorded;
        }
        #endregion

        #region Ad
        private void Ad_Destroy()
        {
            if (!isCreated)
                return;
            //
            if (IsLoaded)
            {
                if (IsShow)
                {
                    adObject.Hide();
                    IsShow = false;
                    //
                    PushEvent_Hidden();
                }
                IsLoaded = false;
            }
            //
            if (adObject != null)
            {
                Ad_EventUnRegister();
                adObject.Destroy();
                adObject = null;
            }
            isCreated = false;
        }
        private void Ad_Create_Begin()
        {
            if (isCreated || isCreating)
                return;
            isCreating = true;
            //
            if (adObject != null)
            {
                Ad_EventUnRegister();
                adObject.Destroy();
                adObject = null;
            }
            //
            if (AdMobManager.IsReady())
            {
                Ad_Create();
            }
            else
            {
                CronObject.Create()
                    .Add(ConditionDelegate.Create(AdMobManager.IsReady))
                    .Add(CallbackAction.Create(Ad_Create))
                    .Run();
            }
        }
        private void Ad_Create()
        {
            if (IsDestroy)
            {
                isCreating = false;
                return;
            }
            //
            isCreating = false;
            isCreated = true;
            //
            adObject = Utility.Create_AdBanner(AdId, SizeType, PositionType, Size, Position);
            Ad_EventRegister();
            //
            Ad_Load_Begin();
        }
        private void Ad_Load_Begin()
        {
            if (!isCreated || IsLoaded || isLoading)
                return;
            isLoading = true;
            //
            float delay = attemptLoad > 0 ? Mathf.Pow(2, attemptLoad) : 0;
            if (delay <= 0)
            {
                Ad_Load();
            }
            else
            {
                CronObject.Create()
                    .Add(ConditionReadTime.Create(delay))
                    .Add(CallbackAction.Create(Ad_Load))
                    .Run();
            }
        }
        private void Ad_Load()
        {
            if (IsDestroy)
            {
                isLoading = false;
            }
            else
            {
                var adRequest = new AdRequest();
                adObject.LoadAd(adRequest);
            }
        }
        private void Ad_OnLoaded()
        {
            isLoading = false;
            if (IsDestroy)
                return;
            //
            IsLoaded = true;
            attemptLoad = 0;
            PushEvent_Loaded(true);
            //
            if (!IsShow)
            {
                adObject.Hide();
            }
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
            PushEvent_Loaded(false);
            //
            Ad_Load_Begin();
        }
        private void Ad_OnFullScreenContentOpened()
        {
            PushEvent_Expanded(true);
        }
        private void Ad_OnFullScreenContentClosed()
        {
            PushEvent_Expanded(false);
        }
        private void Ad_OnClicked()
        {
            PushEvent_Clicked();
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
        }
        private void Ad_OnImpressionRecorded()
        {
            OnAdImpressionRecorded?.Invoke();
        }
        #endregion
    }
}

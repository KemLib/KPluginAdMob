using UnityEngine;

namespace KPlugin.GoogleAdMob.Example
{
    public class ExampleAdMob : MonoBehaviour
    {
        #region Properties
        [SerializeField]
        private PanelLog panelLog;
        [SerializeField]
        private PanelAdAppOpen panelAdAppOpen;
        [SerializeField]
        private PanelAdBanner panelAdBanner;
        [SerializeField]
        private PanelAdInterstitial panelAdInterstitial;
        [SerializeField]
        private PanelAdRewarded panelAdRewarded;
        #endregion

        #region Unity Events
        public void Start()
        {
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            panelLog.Init();
            panelAdAppOpen.Init(panelLog);
            panelAdBanner.Init(panelLog);
            panelAdInterstitial.Init(panelLog);
            panelAdRewarded.Init(panelLog);
            //
            panelAdAppOpen.Show();
        }
        #endregion

        #region Ui Event
        public void OnClick_AppOpen()
        {
            panelAdAppOpen.Show();
            panelAdBanner.Hide();
            panelAdInterstitial.Hide();
            panelAdRewarded.Hide();
        }
        public void OnClick_Banner()
        {
            panelAdAppOpen.Hide();
            panelAdBanner.Show();
            panelAdInterstitial.Hide();
            panelAdRewarded.Hide();
        }
        public void OnClick_Interstitial()
        {
            panelAdAppOpen.Hide();
            panelAdBanner.Hide();
            panelAdInterstitial.Show();
            panelAdRewarded.Hide();
        }
        public void OnClick_Rewarded()
        {
            panelAdAppOpen.Hide();
            panelAdBanner.Hide();
            panelAdInterstitial.Hide();
            panelAdRewarded.Show();
        }
        public void OnClick_RewardedInterstitial()
        {
            panelAdAppOpen.Hide();
            panelAdBanner.Hide();
            panelAdInterstitial.Hide();
            panelAdRewarded.Hide();
        }
        #endregion
    }
}

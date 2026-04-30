using KTool.Attribute;
using KTool.Init;
using UnityEngine;

namespace KPlugin.AdMob.Example
{
    public class LoadScene : MonoBehaviour
    {
        #region Properties
        [SerializeField, SelectScene]
        private string sceneName;
        #endregion

        #region Methods Unity

        #endregion

        #region Methods
        public void NextScene()
        {
            InitManager.Instance.LoadScene(sceneName);
        }
        #endregion
    }
}

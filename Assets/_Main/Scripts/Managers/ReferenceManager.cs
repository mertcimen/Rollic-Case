using BaseSystems.Scripts.UI;
using BaseSystems.Scripts.Utilities.Singletons;
using UnityEngine;

namespace _Main.Scripts.Manager
{
    public class ReferenceManager : SingletonInit<ReferenceManager>
    {
        #region Serialized Fields
        [SerializeField] private LoadingPanelController loadingPanelController;
        #endregion

        #region Properties
        public LoadingPanelController LoadingPanelController => loadingPanelController;
        #endregion
    }
}
using ICities;
using UnityEngine;
using FPSCamera.UI;

namespace FPSCamera
{

    public class Mod : IUserMod
    {
        private FPSCameraControlsOptionsUI m_optionsManager = null;

        public string Name
        {
            get { return "First Person Camera v2.0"; }
        }

        public string Description
        {
            get { return "View your city from a different perspective"; }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            if (m_optionsManager == null)
            {
                m_optionsManager = new GameObject("FPSCameraControlsOptionsUI").AddComponent<FPSCameraControlsOptionsUI>();
            }

            m_optionsManager.generateSettings(helper);
        }
    }

    public class ModLoad : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {


        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Message("initializing in: " + mode.ToString());
            FPSCamera.Initialize(mode);
        }

        public override void OnLevelUnloading()
        {
            FPSCamera.Deinitialize();
        }

    }

}

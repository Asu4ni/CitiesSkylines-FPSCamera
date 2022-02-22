using ICities;
using UnityEngine;
using FPSCamera.UI;
using CitiesHarmony.API;

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
        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => HarmonyPatcher.Patch());
        }
        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) HarmonyPatcher.Unpatch();
        }
    }

    public class ModLoad : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {


        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Log.Msg("initializing in: " + mode.ToString());
            FPSCamera.Initialize(mode);
        }

        public override void OnLevelUnloading()
        {
            FPSCamera.Deinitialize();
        }

    }

}

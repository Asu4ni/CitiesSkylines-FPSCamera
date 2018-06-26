using System;
using System.Resources;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;

namespace FPSCamera
{

    public class Mod : IUserMod
    {

        public string Name
        {
            get { return "First Person Camera"; }
        }

        public string Description
        {
            get { return "See your city from a different perspective"; }
        }
    }

    public class ModTerrainUtil : TerrainExtensionBase
    {

        private static ITerrain terrain = null;

        public static float GetHeight(float x, float z)
        {
            if (terrain == null)
            {
                return 0.0f;
            }

            return terrain.SampleTerrainHeight(x, z);
        }

        public override void OnCreated(ITerrain _terrain)
        {
            terrain = _terrain;
        }
    }

    public class ModLoad : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            try
            {
                PublishedFileId originalModId = new PublishedFileId(406255342);

                foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
                {
                    if (plugin.publishedFileID == originalModId)
                    {
                        try
                        {
                            Log.Message("FPSCam: Disabling Original Mod");
                            PlatformService.workshop.Unsubscribe(originalModId);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            FPSCamera.Initialize(mode);
            /*var m_renderingManager = CameraManipulatorManager.instance;
            m_renderingManager.enabled = true;

            if (m_renderingManager != null && !m_renderingManager.m_registered)
            {
                RenderManager.RegisterRenderableManager(m_renderingManager);
                m_renderingManager.m_registered = true;
            }*/

        }

        public override void OnLevelUnloading()
        {
            FPSCamera.Deinitialize();
        }

    }

}

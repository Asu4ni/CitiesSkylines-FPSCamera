using System;
using ICities;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using System.Reflection;
using UnityEngine;

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

    public class ModLoad : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            
         
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            FPSCamera.Initialize(mode);
        }

        public override void OnLevelUnloading()
        {
            FPSCamera.Deinitialize();
        }

    }

}

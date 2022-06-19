namespace FPSCamera
{
    using ICities;
    using UnityEngine;

    public sealed class ThreadingExtension : ThreadingExtensionBase
    {
        public static Controller Controller;
        public override void OnAfterSimulationFrame()
        {
            base.OnAfterSimulationFrame();
            Controller?.SimulationFrame();
        }
    } // end class
}

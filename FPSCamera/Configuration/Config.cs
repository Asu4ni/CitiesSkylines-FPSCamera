namespace FPSCamera.Configuration
{
    using CSkyL.Config;
    using System;
    using UnityEngine;
    using CfFlag = CSkyL.Config.ConfigData<bool>;
    using CfKey = CSkyL.Config.ConfigData<UnityEngine.KeyCode>;
    using Lang = CSkyL.Lang;

    public class Config : Base
    {
        private const string defaultPath = "FPSCameraConfig.xml";
        public static readonly Config G = new Config();  // G: Global config

        public Config() : this(defaultPath) { }
        public Config(string filePath) : base(filePath)
        {
            Lang.LoadFieldNameAttribute(this,
                (Lang.IFieldWithName field, ConfigAttribute attr) => {
                    if (field is IConfigData config)
                        config._set(attr.name, attr.description, attr.detail);
                });
        }

        public static Config Load(string path = defaultPath) => Load<Config>(path);

        /*----------- general config ----------------------------------------*/

        [Config("HideGameUI", "Hide Game's UI")]
        public readonly CfFlag HideGameUI = new CfFlag(false);
        [Config("ShowInfoPanel", "Show Info panel")]
        public readonly CfFlag ShowInfoPanel = new CfFlag(true);
        [Config("InfoPanelHeightScale", "Scaling factor of Info panel's height")]
        public readonly CfFloat InfoPanelHeightScale = new CfFloat(1f, min: .5f, max: 2f);
        [Config("SetBackCamera", "Set camera back after exiting",
                "When exiting FPS Cam, set the camera position \n" +
                "back to where it's left beforehand")]
        public readonly CfFlag SetBackCamera = new CfFlag(true);
        [Config("UseMetricUnit", "Use metric units")]
        public readonly CfFlag UseMetricUnit = new CfFlag(true);

        // camera control
        [Config("MovementSpeed", "Movement/Offset speed")]
        public readonly CfFloat MovementSpeed = new CfFloat(30f, min: 0f, max: 60f);
        [Config("SpeedUpFactor", "Speed-up factor for movement/offset")]
        public readonly CfFloat SpeedUpFactor = new CfFloat(4f, min: 1.25f, max: 10f);

        [Config("InvertRotateHorizontal", "Invert horizontal rotation")]
        public readonly CfFlag InvertRotateHorizontal = new CfFlag(false);
        [Config("InvertRotateVertical", "Invert vertical rotation")]
        public readonly CfFlag InvertRotateVertical = new CfFlag(false);
        [Config("RotateSensitivity", "Camera rotation sensitivity")]
        public readonly CfFloat RotateSensitivity = new CfFloat(5f, min: .25f, max: 10f);
        [Config("RotateKeyFactor", "Rotation speed using keys")]
        public readonly CfFloat RotateKeyFactor = new CfFloat(8f, min: .5f, max: 32f);

        [Config("EnableDOF", "Apply depth-of-field effect")]
        public readonly CfFlag EnableDof = new CfFlag(false);
        [Config("FieldOfView", "Camera Field of View", "Viewing range of the camera (degrees)")]
        public readonly CfFloat CamFieldOfView = new CfFloat(45f, min: 10f, max: 75f);

        // free cam config
        [Config("ShowCursor4Free", "Show cursor in Free-Camera mode")]
        public readonly CfFlag ShowCursor4Free = new CfFlag(false);
        [Config("MaxPitchDeg4Free", "Max vertical viewing angle",
                "The maximum degree to rotate camera up & down.")]
        public readonly CfFloat MaxPitchDeg4Free = new CfFloat(70f, min: 0f, max: 90f);

        public enum GroundClipping { None, PreventClip, SnapToGround }
        [Config("GroundClipping", "Ground clipping option",
                "For Free-Camera Mode:\n-[None] free movement\n" +
                "-[PreventClip] camera always above ground\n" +
                "-[SnapToGround] camera sticks to ground")]
        public readonly ConfigData<GroundClipping> GroundClippingOption
                            = new ConfigData<GroundClipping>(GroundClipping.PreventClip);
        [Config("GroundLevelOffset", "Ground level offset",
                "Vertical offset for ground level for ground clipping option")]
        public readonly CfFloat GroundLevelOffset = new CfFloat(0f, min: -2f, max: 10f);

        // follow config
        [Config("ShowCursor4Follow", "Show cursor in Follow/Walk-Through mode")]
        public readonly CfFlag ShowCursor4Follow = new CfFlag(false);
        [Config("StickToFrontVehicle", "Always follow the front vehicle")]
        public readonly CfFlag StickToFrontVehicle = new CfFlag(true);
        [Config("MaxYawDeg4Follow", "Max horizontal viewing angle")]
        public readonly CfFloat MaxYawDeg4Follow = new CfFloat(60f, min: 0f, max: 180f);
        [Config("MaxPitchDeg4Follow", "Max vertical viewing angle")]
        public readonly CfFloat MaxPitchDeg4Follow = new CfFloat(30f, min: 0f, max: 90f);
        [Config("InstantMoveMax", "Min distance for smooth transition",
                "In Follow Mode, camera needs to move instantly with\n" +
                "the target even when smooth transition is enabled.\n" +
                "This sets the minimum distance to start applying smooth transition.")]
        public readonly CfFloat InstantMoveMax = new CfFloat(15f, min: 5f, max: 50f);

        // walkThru config
        [Config("Period4Walk", "Period (seconds) for each random target")]
        public readonly CfFloat Period4Walk = new CfFloat(20f, min: 5f, max: 300f);
        [Config("ManualSwitch4Walk", "Manual target switch (Secondary Click)",
                "Use secondary mouse click to\nswitch following targets instead.")]
        public readonly CfFlag ManualSwitch4Walk = new CfFlag(false);

        // keys
        [Config("KeyCamToggle", "FPS Camera toggle")]
        public readonly CfKey KeyCamToggle = new CfKey(KeyCode.BackQuote);
        [Config("KeySpeedUp", "Speed up movement/offset")]
        public readonly CfKey KeySpeedUp = new CfKey(KeyCode.CapsLock);
        [Config("KeyCamReset", "Reset Camera offset & rotation")]
        public readonly CfKey KeyCamReset = new CfKey(KeyCode.Backspace);
        [Config("KeyCursorToggle", "Cursor visibility toggle")]
        public readonly CfKey KeyCursorToggle = new CfKey(KeyCode.LeftControl);

        [Config("KeyMoveForward", "Move/Offset forward")]
        public readonly CfKey KeyMoveForward = new CfKey(KeyCode.W);
        [Config("KeyMoveBackward", "Move/Offset backward")]
        public readonly CfKey KeyMoveBackward = new CfKey(KeyCode.S);
        [Config("KeyMoveLeft", "Move/Offset left")]
        public readonly CfKey KeyMoveLeft = new CfKey(KeyCode.A);
        [Config("KeyMoveRight", "Move/Offset right")]
        public readonly CfKey KeyMoveRight = new CfKey(KeyCode.D);
        [Config("KeyMoveUp", "Move/Offset up")]
        public readonly CfKey KeyMoveUp = new CfKey(KeyCode.PageUp);
        [Config("KeyMoveDown", "Move/Offset down")]
        public readonly CfKey KeyMoveDown = new CfKey(KeyCode.PageDown);

        [Config("KeyRotateLeft", "Rotate/Look left")]
        public readonly CfKey KeyRotateLeft = new CfKey(KeyCode.LeftArrow);
        [Config("KeyRotateRight", "Rotate/Look right")]
        public readonly CfKey KeyRotateRight = new CfKey(KeyCode.RightArrow);
        [Config("KeyRotateUp", "Rotate/Look up")]
        public readonly CfKey KeyRotateUp = new CfKey(KeyCode.UpArrow);
        [Config("KeyRotateDown", "Rotate/Look down")]
        public readonly CfKey KeyRotateDown = new CfKey(KeyCode.DownArrow);

        // smooth transition
        [Config("SmoothTransition", "Apply smooth transition",
                "When camera moves, rotates or zooms,\nthe transition could be either" +
                "smooth or instant.\nEnabling the option could make camera look lagging.")]
        public readonly CfFlag SmoothTransition = new CfFlag(true);
        [Config("TransitionRate", "Smooth transition rate")]
        public readonly CfFloat TransRate = new CfFloat(.5f, min: .1f, max: .9f);
        [Config("GiveUpTransitionDistance", "Max distance to transition smoothly",
                "When the camera target position is too far, smooth transition takes too long.\n" +
                "This number sets the distance to give up the transition.")]
        public readonly CfFloat GiveUpTransDistance = new CfFloat(500f, min: 100f, max: 2000f);
        [Config("DeltaPosMin", "Min movement for smooth transition")]
        public readonly CfFloat MinTransMove = new CfFloat(.5f, min: .1f, max: 5f);
        [Config("DeltaPosMax", "Max movement for smooth transition")]
        public readonly CfFloat MaxTransMove = new CfFloat(30f, min: 5f, max: 100f);
        [Config("DeltaRotateMin", "Min rotation for smooth transition", "unit: degree")]
        public readonly CfFloat MinTransRotate = new CfFloat(.1f, min: .05f, max: 5f);
        [Config("DeltaRotateMax", "Max rotation for smooth transition", "unit: degree")]
        public readonly CfFloat MaxTransRotate = new CfFloat(10f, min: 5f, max: 45f);

        // position offset
        [Config("VehicleCamOffset", "Camera offset while following vehicles")]
        public readonly CfOffset VehicleCamOffset = new CfOffset(
            new CfFloat(0f, min: -20f, max: 40f),
            new CfFloat(0f, min: -20f, max: 40f),
            new CfFloat(0f, min: -30f, max: 30f)
        );
        [Config("PedestrianCamOffset", "Camera offset while following pedestrians")]
        public readonly CfOffset PedestrianCamOffset = new CfOffset(
            new CfFloat(0f, min: -20f, max: 40f),
            new CfFloat(0f, min: -20f, max: 40f),
            new CfFloat(0f, min: -30f, max: 30f)
        );


        /*--------- configurable constants ----------------------------------*/

        [Config("MainPanelBtnPos", "In-Game main panel button position")]
        public readonly CfOffset MainPanelBtnPos
                = new CfOffset(new CfFloat(0f, 0f, 0f), new CfFloat(-1f), new CfFloat(-1f));
        //                        always 0 (forward)  |    y-axis (up)  |    x-axis (right)
        // value == -1 : unset

        [Config("CamNearClipPlane", "Camera Near clip plane")]
        public readonly CfFloat CamNearClipPlane = new CfFloat(1f, min: .125f, max: 64f);
        [Config("FoViewScrollfactor", "Field of View scaling factor by scrolling")]
        public readonly CfFloat FoViewScrollfactor = new CfFloat(1.05f, 1.01f, 2f);

        [Config("VehicleFOffsetUp", "Cam fixed offset.up for vehicle")]
        public readonly CfFloat VehicleFOffsetUp = new CfFloat(2f);
        [Config("VehicleFOffsetForward", "Cam fixed offset.forward for vehicle")]
        public readonly CfFloat VehicleFOffsetForward = new CfFloat(3f);
        [Config("MiddleVehicleFOffsetUp", "Cam fixed offset.up for vehicle in the middle")]
        public readonly CfFloat MiddleVehicleFOffsetUp = new CfFloat(3f);
        [Config("PedestrianFOffsetUp", "Cam fixed offset.up for pedestrians")]
        public readonly CfFloat PedestrianFOffsetUp = new CfFloat(2f);

        [Config("MaxExitingDuration", "Max duration for exiting fps cam")]
        public readonly CfFloat MaxExitingDuration = new CfFloat(5f, 0f);
        /*-------------------------------------------------------------------*/

        // Return a ratio[0f, 1f] representing the proportion to advance to the target
        //  *advance ratio per unit(.1 sec): TransRate
        //  *retain ratio per unit: 1f - AdvanceRatioPUnit   *units: elapsedTime / .1f
        //  *retain ratio: RetainRatioPUnit ^ units          *advance ratio: 1f - RetainRatio
        public float GetAdvanceRatio(float elapsedTime)
            => 1f - (float) Math.Pow(1f - TransRate, elapsedTime / .1f);
    }
}

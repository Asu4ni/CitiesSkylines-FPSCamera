namespace FPSCamera.Configuration
{
    using CSkyL.Config;
    using System;
    using UnityEngine;
    using CfFlag = CSkyL.Config.ConfigData<bool>;
    using CfKey = CSkyL.Config.ConfigData<UnityEngine.KeyCode>;

    public class Config : Base
    {
        private const string defaultPath = "FPSCameraConfig.xml";
        public static readonly Config G = new Config();  // G: Global config

        public Config() : this(defaultPath) { }
        public Config(string filePath) : base(filePath) { }

        public static Config Load(string path = defaultPath) => Load<Config>(path);

        /*----------- general config ----------------------------------------*/

        [Config("HideGameUI", "Hide Game's UI")]
        public readonly CfFlag HideGameUI = new CfFlag(false);
        [Config("SetBackCamera", "Set camera back after exiting",
                "When exiting FPS Cam, set the camera position \n" +
                "back to where it's left beforehand")]
        public readonly CfFlag SetBackCamera = new CfFlag(true);
        [Config("UseMetricUnit", "Use metric units")]
        public readonly CfFlag UseMetricUnit = new CfFlag(true);
        [Config("ShowInfoPanel", "Show Info panel")]
        public readonly CfFlag ShowInfoPanel = new CfFlag(true);
        [Config("InfoPanelHeightScale", "Scaling factor of Info panel's height")]
        public readonly CfFloat InfoPanelHeightScale = new CfFloat(1f, min: .5f, max: 2f);
        [Config("MaxPitchDeg", "Max vertical viewing angle",
                "The maximum degree to rotate camera up & down.")]
        public readonly CfFloat MaxPitchDeg = new CfFloat(70f, min: 0f, max: 90f);

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

        public enum GroundClipping { None, AboveGround, SnapToGround, AboveRoad, SnapToRoad }
        [Config("GroundClipping", "Ground clipping option",
                "For Free-Camera Mode:\n-[None] free movement\n" +
                "-[AboveGround] camera always above ground\n" +
                "-[SnapToGround] camera sticks to ground\n" +
                "-[AboveRoad] camera always above the closest road\n" +
                "-[SnapToRoad] camera sticks to the closest road or ground")]
        public readonly ConfigData<GroundClipping> GroundClippingOption
                            = new ConfigData<GroundClipping>(GroundClipping.AboveGround);
        [Config("GroundLevelOffset", "Ground level offset",
                "Vertical offset for ground level for ground clipping option")]
        public readonly CfFloat GroundLevelOffset = new CfFloat(0f, min: -2f, max: 10f);
        [Config("RoadLevelOffset", "Road level offset",
                "Vertical offset for road level for ground clipping option")]
        public readonly CfFloat RoadLevelOffset = new CfFloat(0f, min: -2f, max: 10f);

        // follow config
        [Config("ShowCursor4Follow", "Show cursor in Follow/Walk-Through mode")]
        public readonly CfFlag ShowCursor4Follow = new CfFlag(false);
        [Config("StickToFrontVehicle", "Always follow the front vehicle")]
        public readonly CfFlag StickToFrontVehicle = new CfFlag(true);
        [Config("LookAhead", "Look ahead",
                "Camera looks toward the position the target is going to be.")]
        public readonly CfFlag LookAhead = new CfFlag(false);
        [Config("InstantMoveMax", "Min distance for smooth transition",
                "In Follow Mode, camera needs to move instantly with\n" +
                "the target even when smooth transition is enabled.\n" +
                "This sets the minimum distance to start applying smooth transition.")]
        public readonly CfFloat InstantMoveMax = new CfFloat(15f, min: 5f, max: 50f);
        [Config("FollowCamOffset", "Follow mode universal camera offset")]
        public readonly CfOffset FollowCamOffset = new CfOffset(
            new CfFloat(0f, min: -20f, max: 20f),
            new CfFloat(0f, min: -20f, max: 20f),
            new CfFloat(0f, min: -20f, max: 20f)
        );


        // walkThru config
        [Config("Period4Walk", "Period (seconds) for each random target")]
        public readonly CfFloat Period4Walk = new CfFloat(20f, min: 5f, max: 300f);
        [Config("ManualSwitch4Walk", "Manual target switch (Secondary Click)",
                "Use secondary mouse click to\nswitch following targets instead.")]
        public readonly CfFlag ManualSwitch4Walk = new CfFlag(false);

        [Config("SelectPedestrian", "Walking pedestrians")]
        public readonly CfFlag SelectPedestrian = new CfFlag(true);
        [Config("SelectPassenger", "Pedestrians on public transits")]
        public readonly CfFlag SelectPassenger = new CfFlag(true);
        [Config("SelectWaiting", "Pedestrians waiting for public transits")]
        public readonly CfFlag SelectWaiting = new CfFlag(true);
        [Config("SelectDriving", "Driving/Riding citizens")]
        public readonly CfFlag SelectDriving = new CfFlag(true);
        [Config("SelectPublicTransit", "Public transit vehicles")]
        public readonly CfFlag SelectPublicTransit = new CfFlag(true);
        [Config("SelectService", "Service vehicles")]
        public readonly CfFlag SelectService = new CfFlag(true);
        [Config("SelectCargo", "Cargo vehicles")]
        public readonly CfFlag SelectCargo = new CfFlag(true);

        // keys
        [Config("KeyCamToggle", "FPS Camera toggle")]
        public readonly CfKey KeyCamToggle = new CfKey(KeyCode.BackQuote);
        [Config("KeySpeedUp", "Speed up movement/offset")]
        public readonly CfKey KeySpeedUp = new CfKey(KeyCode.CapsLock);
        [Config("KeyCamReset", "Reset Camera offset & rotation")]
        public readonly CfKey KeyCamReset = new CfKey(KeyCode.Backspace);
        [Config("KeyCursorToggle", "Cursor visibility toggle")]
        public readonly CfKey KeyCursorToggle = new CfKey(KeyCode.LeftControl);
        [Config("KeyAutoMove", "Auto moving toggle in Free-Camera mode",
                "Camera moves forward automatically when it's toggled on.")]
        public readonly CfKey KeyAutoMove = new CfKey(KeyCode.E);
        [Config("KeySaveOffset", "Save the current camera setting as default",
                "In Follow/Walk-Through mode, save the \n" +
                "current camera setting for the followed target")]
        public readonly CfKey KeySaveOffset = new CfKey(KeyCode.Backslash);

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


        /*--------- configurable constants ----------------------------------*/

        [Config("MainPanelBtnPos", "In-Game main panel button position")]
        public readonly CfScreenPosition MainPanelBtnPos
            = new CfScreenPosition(CSkyL.Math.Vec2D.Position(-1f, -1f));
        // (-1, -1): for unset position

        [Config("CamNearClipPlane", "Camera Near clip plane")]
        public readonly CfFloat CamNearClipPlane = new CfFloat(1f, min: .125f, max: 64f);
        [Config("FoViewScrollfactor", "Field of View scaling factor by scrolling")]
        public readonly CfFloat FoViewScrollfactor = new CfFloat(1.05f, 1.01f, 2f);

        [Config("VehicleFixedOffset", "Cam fixed offset for vehicle")]
        public readonly CfOffset VehicleFixedOffset = new CfOffset(
            new CfFloat(3f), new CfFloat(2f), new CfFloat(0f));
        [Config("MidVehFixedOffset", "Cam fixed offset for vehicle in the middle")]
        public readonly CfOffset MidVehFixedOffset = new CfOffset(
            new CfFloat(-2f), new CfFloat(3f), new CfFloat(0f));
        [Config("PedestrianFixedOffset", "Cam fixed offset for pedestrian")]
        public readonly CfOffset PedestrianFixedOffset = new CfOffset(
            new CfFloat(0f), new CfFloat(2f), new CfFloat(0f));

        [Config("MaxExitingDuration", "Max duration for exiting fps cam")]
        public readonly CfFloat MaxExitingDuration = new CfFloat(2f, 0f);
        /*-------------------------------------------------------------------*/

        // Return a ratio[0f, 1f] representing the proportion to advance to the target
        //  *advance ratio per unit(.1 sec): TransRate
        //  *retain ratio per unit: 1f - AdvanceRatioPUnit   *units: elapsedTime / .1f
        //  *retain ratio: RetainRatioPUnit ^ units          *advance ratio: 1f - RetainRatio
        public float GetAdvanceRatio(float elapsedTime)
            => 1f - (float) Math.Pow(1f - TransRate, elapsedTime / .1f);
    }
}

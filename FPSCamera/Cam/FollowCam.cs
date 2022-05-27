namespace FPSCamera.Cam
{
    using Configuration;
    using CSkyL.Game;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Transform;
    using System.Diagnostics;
    using CSkyL.UI;

    public abstract class FollowCam : Base
    {
        public static FollowCam Follow(ObjectID targetID)
        {
            switch (Object.Of(targetID)) {
            case Pedestrian ped: return new PedestrianCam(ped.pedestrianID);
            case Vehicle vehicle:
                if (vehicle is PersonalVehicle car &&
                        Follow(car.GetDriverID()) is FollowCam cam) return cam;
                return new VehicleCam(vehicle.id);
            default: return null;
            }
        }
        public abstract ObjectID TargetID { get; }
        public abstract IObjectToFollow Target { get; }

        public abstract string GetTargetStatus();
        public abstract Utils.Infos GetTargetInfos();

        // return saved entry key, usually PrefabInfo.name
        public abstract string SaveOffset();
    }

    public abstract class FollowCam<IDType, TargetType> : FollowCam
           where IDType : ObjectID where TargetType : class, IObjectToFollow
    {
        const int timeFactor = 3;
        const float angleFactor = .9f;
        protected FollowCam(IDType id)
        {
            _id = id;
            _target = Object.Of(_id) as TargetType;
            if (_target is null) _state = new Finish();
            else _inputOffset = CamOffset.G[_target.GetPrefabName()];
        }

        public override bool Validate()
        {
            if (!IsOperating) return false;

            if (_target == null || !_target.ID.Equals(_id)) {
                _target = Object.Of(_id) as TargetType;
            }

            if (_target is null) {
                _state = new Finish();
                return false;
            }
            return true;
        }

        public override ObjectID TargetID => _id;
        public override IObjectToFollow Target => _target;

        public override float GetSpeed() => _target.GetSpeed();
        public override string GetTargetStatus() => _target.GetStatus();
        public override Utils.Infos GetTargetInfos() => _target.GetInfos();

        public override Positioning GetPositioning()
        { 
            var pos =  _target.GetPositioning();
            pos.position.up += 20; //debug

            return pos
                .Apply(_LocalOffset)
                .Apply(Config.G.FollowCamOffset.AsOffSet)
                .Apply(_inputOffset);
        }

        public override string SaveOffset()
        {
            CamOffset.G[_SavedOffsetKey] = _inputOffset;
            CamOffset.G.Save();
            return _SavedOffsetKey;
        }

        protected virtual Offset _LocalOffset => Offset.None;

        public override void InputOffset(Offset inputOffset)
        {
            inputOffset.movement *= movementFactor;
            inputOffset.movement.up *= heightMovementFactor;
            _inputOffset = _inputOffset.FollowedBy(inputOffset);
            _inputOffset.deltaAttitude = _inputOffset.deltaAttitude.Clamp(pitchRange:
                    new CSkyL.Math.Range(-Config.G.MaxPitchDeg, Config.G.MaxPitchDeg));
        }
        public override void InputReset()
            => _inputOffset = CamOffset.G[_SavedOffsetKey];

        protected virtual bool _SwitchTarget(IDType newID)
        {
            _id = newID;
            if (!Validate()) return false;
            InputReset();
            return true;
        }

        public override void SimulationFrame()=> _target?.SimulationFrame();
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) => _target?.RenderOverlay(cameraInfo);

        protected virtual string _SavedOffsetKey => _target.GetPrefabName();

        protected const float movementFactor = .1f;
        protected const float heightMovementFactor = .2f;

        protected IDType _id;
        protected TargetType _target;
        protected Offset _inputOffset;
    }

    public abstract class FollowCamWithCam<IDType, TargetType, AnotherCam>
            : FollowCam<IDType, TargetType> where IDType : ObjectID
                                          where TargetType : class, IObjectToFollow
            where AnotherCam : Base
    {
        protected FollowCamWithCam(IDType id) : base(id) { }

        public override bool Validate()
        {
            if (!base.Validate()) return false;

            if (_state is UsingOtherCam) {
                if (!_camOther.Validate() || _ReadyToSwitchBack) {
                    _camOther = null;
                    _state = new Normal();
                }
                return true;
            }
            else if (_state is Normal && _ReadyToSwitchToOtherCam) {
                _camOther = _CreateAnotherCam();
                if (_camOther.Validate()) _state = new UsingOtherCam();
                else _camOther = null;
            }
            return true;
        }

        public override Positioning GetPositioning()
            => _state is UsingOtherCam ? _camOther.GetPositioning() : base.GetPositioning();

        public override float GetSpeed()
            => _state is UsingOtherCam ? _camOther.GetSpeed() : base.GetSpeed();

        public override void InputOffset(Offset inputOffset)
        {
            if (_state is UsingOtherCam) _camOther.InputOffset(inputOffset);
            else base.InputOffset(inputOffset);
        }
        public override void InputReset()
        {
            if (_state is UsingOtherCam) _camOther.InputReset();
            else base.InputReset();
        }
        public override string SaveOffset()
            => _state is UsingOtherCam ? (_camOther as FollowCam)?.SaveOffset() :
                                         base.SaveOffset();
        protected abstract bool _ReadyToSwitchToOtherCam { get; }
        protected abstract bool _ReadyToSwitchBack { get; }
        protected abstract AnotherCam _CreateAnotherCam();

        protected class UsingOtherCam : State { }
        protected AnotherCam _camOther = null;

    }
}

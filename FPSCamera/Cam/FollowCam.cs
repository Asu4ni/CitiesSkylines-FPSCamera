namespace FPSCamera.Cam
{
    using CSkyL.Game;
    using CSkyL.Game.ID;
    using CSkyL.Game.Object;
    using CSkyL.Transform;

    public abstract class FollowCam : Base
    {
        public static FollowCam Follow(ObjectID targetID, System.Func<Offset, Offset> handler)
        {
            switch (Object.Of(targetID)) {
            case Pedestrian ped: return new PedestrianCam(ped.pedestrianID, handler);
            case Vehicle vehicle:
                if (vehicle is PersonalVehicle car &&
                        Follow(car.GetDriverID(), handler) is FollowCam cam) return cam;
                return new VehicleCam(vehicle.id, handler);
            default: return null;
            }
        }
        public abstract ObjectID TargetID { get; }

        public abstract string GetTargetStatus();
        public abstract Utils.Infos GetTargetInfos();

        public void SetInputOffsetHandler(System.Func<Offset, Offset> handler)
            => _inputOffsetHandler = handler;

        protected FollowCam(System.Func<Offset, Offset> handler)
        { _inputOffsetHandler = handler; }

        protected System.Func<Offset, Offset> _inputOffsetHandler = null;
    }

    public abstract class FollowCam<IDType, TargetType> : FollowCam
           where IDType : ObjectID where TargetType : class, IObjectToFollow
    {
        protected FollowCam(IDType id, System.Func<Offset, Offset> handler) : base(handler)
        {
            _id = id;
            if (!Validate()) return;
            _inputOffset = Offset.None;
        }

        public override bool Validate()
        {
            if (!IsOperating) return false;

            _target = Object.Of(_id) as TargetType;

            if (_target is null) {
                _state = new Finish();
                return false;
            }
            return true;
        }

        public override ObjectID TargetID => _id;
        public TargetType Target => _target;

        public override float GetSpeed() => _target.GetSpeed();
        public override string GetTargetStatus() => _target.GetStatus();
        public override Utils.Infos GetTargetInfos() => _target.GetInfos();

        public override Positioning GetPositioning()
            => _target.GetPositioning().Apply(_LocalOffset).Apply(_inputOffset);

        protected virtual Offset _LocalOffset => Offset.None;

        public override void InputOffset(Offset inputOffset)
        {
            inputOffset.movement.up *= heightMovementFactor;
            _inputOffset = _inputOffset.FollowedBy(inputOffset);
            if (_inputOffsetHandler is object)
                _inputOffset = _inputOffsetHandler(_inputOffset);
        }
        public override void InputReset() => _inputOffset = Offset.None;

        protected virtual bool _SwitchTarget(IDType newID)
        {
            _id = newID;
            return Validate();
        }

        protected const float heightMovementFactor = .4f;

        protected IDType _id;
        protected TargetType _target;
        protected Offset _inputOffset;
    }

    public abstract class FollowCamWithCam<IDType, TargetType, AnotherCam>
            : FollowCam<IDType, TargetType> where IDType : ObjectID
                                          where TargetType : class, IObjectToFollow
            where AnotherCam : Base
    {
        protected FollowCamWithCam(IDType id, System.Func<Offset, Offset> handler)
            : base(id, handler) { }

        public override bool Validate()
        {
            if (!base.Validate()) return false;

            if (_state is UsingOtherCam && !_camOther.Validate()) {
                _state = new Normal();
            }
            return true;
        }

        public override Positioning GetPositioning()
        {
            if (_state is Normal && _ReadyToSwitchToOtherCam) {
                _camOther = _CreateAnotherCam();
                _state = new UsingOtherCam();
            }
            if (_state is UsingOtherCam) {
                if (!_ReadyToSwitchBack) return _camOther.GetPositioning();
                _state = new Normal();
            }
            return base.GetPositioning();
        }

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

        protected abstract bool _ReadyToSwitchToOtherCam { get; }
        protected abstract bool _ReadyToSwitchBack { get; }
        protected abstract AnotherCam _CreateAnotherCam();

        protected class UsingOtherCam : State { }
        protected AnotherCam _camOther = null;
    }
}

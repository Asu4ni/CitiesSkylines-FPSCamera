namespace FPSCamera.Transform
{
    using Quaternion = UnityEngine.Quaternion;
    using Range = Utils.Range;
    using Vector = UnityEngine.Vector3;
    using Vector2 = UnityEngine.Vector2;

    public class Position
    {
        public static Position Init => new Position { x = 0f, y = 0f, up = 0f };

        public float x, y, up;

        public bool AlmostEquals(Position other) => DisplacementTo(other).AlmostNone;

        public Position Move(Displacement move)
            => new Position { x = x + move.x, y = y + move.y, up = up + move.up };
        public Displacement DisplacementTo(Position target)
            => new Displacement { x = target.x - x, y = target.y - y, up = target.up - up };
        public float DistanceTo(Position target) => DisplacementTo(target).Distance;

        public Position GetNextOfSmoothTrans(Position target,
                                float advanceFactor, Range rangeOfChange)
            => this.GetNextOfSmoothTrans(target, advanceFactor, rangeOfChange,
                (a, b) => (a.DisplacementTo(b)).Distance,
                (a, b, t) => a.Move(a.DisplacementTo(b) * t));

        public Vector2 AsGameGroundPosition => new Vector2(x, y);
        public Vector AsGamePosition => new Vector(x, up, y);
        public static Position FromGame(Vector position)
            => new Position { x = position.x, y = position.z, up = position.y };

        public override string ToString() => $"[x: {x}, y: {y}, u: {up}]";
    }
    public class Displacement
    {
        public static Displacement None => new Displacement { x = 0f, y = 0f, up = 0f };

        public float x, y, up;

        public bool AlmostNone => SqrDistance.AlmostEqual(0f);
        public float SqrDistance => x * x + y * y + up * up;
        public float Distance => (float) System.Math.Sqrt(SqrDistance);

        public LocalMovement AsLocalMovementAt(Angle angle)
            => LocalMovement._FromVec3(angle.RotationTo(Angle.Init)._AsQuat * _AsVec3);

        public static Displacement operator -(Displacement d)
            => new Displacement { x = -d.x, y = -d.y, up = -d.up };
        public static Displacement operator *(Displacement d, float factor)
            => new Displacement { x = d.x * factor, y = d.y * factor, up = d.up * factor };

        internal Vector _AsVec3 => new Vector(x, up, y);
        internal static Displacement _FromVec3(Vector vec)
            => new Displacement { x = vec.x, y = vec.z, up = vec.y };

        public override string ToString() => $"[x: {x}, y: {y}, u: {up}]";
    }
    public class LocalMovement
    {
        public static LocalMovement None
            => new LocalMovement { forward = 0f, up = 0f, right = 0f };

        public float forward, up, right;

        public Displacement AsDisplacementAt(Angle angle)
            => Displacement._FromVec3(angle._AsQuat * _AsVec3);
        public LocalMovement AsLocalMovementAt(Angle angle)
            => _FromVec3(angle._AsQuat * _AsVec3);

        public static LocalMovement operator +(LocalMovement a, LocalMovement b)
            => new LocalMovement
            { forward = a.forward + b.forward, up = a.up + b.up, right = a.right + b.right };
        public static LocalMovement operator *(LocalMovement m, float factor) => new LocalMovement
        { forward = m.forward * factor, up = m.up * factor, right = m.right * factor };

        internal Vector _AsVec3 => new Vector(right, up, forward);
        internal static LocalMovement _FromVec3(Vector vec)
            => new LocalMovement { forward = vec.z, up = vec.y, right = vec.x };

        public override string ToString() => $"[f: {forward}, u: {up}, r: {right}]";
    }

    /*  Angle: object/cam orientation as "attitude" (mostly used for aircraft)
     *  
     *       \       axis        +/- direction    zero state   |    range
     *   yaw  |    vertical    | right /  left | initial front | [-180, 180]
     *  pitch |  left-to-right |   up  /  down |  horizontal   | [-85 , 85 ]
     *   roll |  front-to-back |   clockwise   |  horizontal   | [  0 ,  0 ]
     */
    public class Angle
    {
        public static Angle Init => new Angle(0f, 0f);

        public readonly float yawDegree, pitchDegree;
        public static readonly Range rangePitch = new Range(-85f, 85f);
        public static readonly Range rangeDegree = new Range(-180f, 180f);

        public bool AlmostEquals(Angle other) => RotationTo(other).AlmostNone;

        public Angle Clamp(Range? yawRange = null, Range? pitchRange = null)
        {
            var yaw = yawRange is Range yr ? yawDegree.Clamp(yr) : yawDegree;
            var pitch = pitchRange is Range pr ? pitchDegree.Clamp(pr) : pitchDegree;
            return new Angle(yaw, pitch);
        }
        public Angle GetNextOfSmoothTrans(Angle target,
                            float advanceFactor, Range rangeOfChange)
            => this.GetNextOfSmoothTrans(target, advanceFactor, rangeOfChange,
                                         (a, b) => Quaternion.Angle(a._AsQuat, b._AsQuat),
                                         (a, b, t) => _FromQuat(Quaternion.Slerp(
                                                            a._AsQuat, b._AsQuat, t)));

        public Angle Rotate(DeltaAttitude delta)
            => new Angle(yawDegree + delta.yawDegree, pitchDegree + delta.pitchDegree);
        public Angle Rotate(Rotation rotation) => _FromQuat(rotation._AsQuat * _AsQuat);
        public Rotation RotationTo(Angle target)
            => Rotation._FromQuat(target._AsQuat * Quaternion.Inverse(_AsQuat));

        /*  CameraController.m_targetAngle : Vector2
         *     \       axis        +/- direction    zero state   |    range
         *   x |    vertical    | right /  left | initial front | [-180, 180]
         *   y |  left-to-right | down  /   up  |  horizontal   | [ 0, 90 ] *free [-90, 90]
         */
        public Vector2 AsGameAngle => new Vector2(yawDegree, -pitchDegree);
        public Quaternion AsGameRotation => _AsQuat;
        public static Angle FromGame(Quaternion rotation) => _FromQuat(rotation);

        internal Quaternion _AsQuat => Quaternion.Euler(-pitchDegree, yawDegree, 0f);
        internal static Angle _FromQuat(Quaternion quaternion)
        {
            var vec = quaternion.eulerAngles;
            vec.x = vec.x.Modulus(rangeDegree);
            return new Angle(yawDegree: vec.y, pitchDegree: -vec.x);
        }

        public override string ToString() => $"( yaw : {yawDegree}°,pitch:{pitchDegree}°)";
        public Angle(float yawDegree, float pitchDegree)
        { this.yawDegree = _ToYaw(yawDegree); this.pitchDegree = _ToPitch(pitchDegree); }

        private static float _ToYaw(float angle) => angle.Modulus(rangeDegree);
        private static float _ToPitch(float angle) => angle.Clamp(rangePitch);
    }
    public class DeltaAttitude
    {
        public static DeltaAttitude None => new DeltaAttitude(0f, 0f);

        public readonly float yawDegree, pitchDegree;

        public DeltaAttitude Clamp(Range? yawRange = null, Range? pitchRange = null)
        {
            var yaw = yawRange is Range yr ? yawDegree.Clamp(yr) : yawDegree;
            var pitch = pitchRange is Range pr ? pitchDegree.Clamp(pr) : pitchDegree;
            return new DeltaAttitude(yaw, pitch);
        }

        public static DeltaAttitude operator +(DeltaAttitude a, DeltaAttitude b)
            => new DeltaAttitude(a.yawDegree + b.yawDegree, a.pitchDegree + b.pitchDegree);

        public DeltaAttitude(float yawDegree, float pitchDegree)
        { this.yawDegree = yawDegree; this.pitchDegree = pitchDegree; }
        public override string ToString() => $"( yaw : {yawDegree}°,pitch:{pitchDegree}°)";
    }
    public class Rotation
    {
        public static Rotation None => new Rotation(Quaternion.identity);

        // quaternion.w = cos(theta) = 1 when theta = 0
        public bool AlmostNone => _rotation.w.AlmostEqual(1f, 1f / 16384);

        public static Rotation operator -(Rotation r)
            => new Rotation(Quaternion.Inverse(r._rotation));

        public static Rotation FromRotating(float rightDegree, float upDegree)
            => _FromQuat(Quaternion.Euler(-upDegree, rightDegree, 0f));

        internal Quaternion _AsQuat => _rotation;
        internal static Rotation _FromQuat(Quaternion quaternion) => new Rotation(quaternion);

        public override string ToString()
        { var euler = _rotation.eulerAngles; return $"(right: {euler.y}°,  up :{-euler.x}°)"; }
        private Rotation(Quaternion rotation) { _rotation = rotation; }

        private readonly Quaternion _rotation;
    }

    public class Offset
    {
        public static Offset None
            => new Offset(LocalMovement.None, DeltaAttitude.None);

        public Offset FollowedBy(Offset secondOffset)
            => new Offset(movement + secondOffset.movement
                                        .AsLocalMovementAt(Angle.Init.Rotate(deltaAttitude)),
                          deltaAttitude + secondOffset.deltaAttitude);

        public Offset(LocalMovement movement, DeltaAttitude deltaAttitude)
        { this.movement = movement; this.deltaAttitude = deltaAttitude; }
        public override string ToString() => $"[mov: {movement} del: {deltaAttitude}]";

        public LocalMovement movement;
        public DeltaAttitude deltaAttitude;
    }

    public class Positioning
    {
        public bool AlmostEquals(Positioning other)
            => position.AlmostEquals(other.position) && angle.AlmostEquals(other.angle);

        public Positioning Apply(LocalMovement movement)
            => new Positioning(position.Move(movement.AsDisplacementAt(angle)), angle);
        public Positioning Apply(Offset offset)
            => new Positioning(position.Move(offset.movement.AsDisplacementAt(angle)),
                               angle.Rotate(offset.deltaAttitude));

        public Positioning(Position position, Angle angle)
        { this.position = position; this.angle = angle; }
        public override string ToString() => $"[pos: {position} ang: {angle}]";

        public Position position;
        public Angle angle;
    }
}

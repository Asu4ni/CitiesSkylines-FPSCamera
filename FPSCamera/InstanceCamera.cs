using UnityEngine;

namespace FPSCamera
{
    interface InstanceCamera
    {

        void SetFollowInstance(uint instance);
        void GetInstanceSpeed();
        void StopFollowing();
        Vector3 GetOffset(Vector3 position, Vector3 forward, Vector3 up);
    }
}
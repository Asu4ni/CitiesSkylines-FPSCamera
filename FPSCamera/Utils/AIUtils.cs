namespace FPSCamera.Utils
{
    public class AIUtils
    {
        /**
         * See if the vehicle is reversed (only applicable to trains, metros)
         */
        public static bool GetReversedStatus(VehicleManager vManager, ushort vehicleId)
        {
            return (vManager.m_vehicles.m_buffer[vehicleId].m_flags & Vehicle.Flags.Reversed) == Vehicle.Flags.Reversed;
        }
    }
}

namespace FPSCamera
{
    public class GameUtils
    {
        // check if the vehicle is reversed (only applicable to trains, metros)
        public static bool GetReversedStatus(VehicleManager vManager, ushort vehicleID)
        {
            return (vManager.m_vehicles.m_buffer[vehicleID].m_flags & Vehicle.Flags.Reversed) == Vehicle.Flags.Reversed;
        }
    }
}

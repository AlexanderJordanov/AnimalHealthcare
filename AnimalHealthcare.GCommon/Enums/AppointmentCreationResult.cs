namespace AnimalHealthcare.GCommon.Enums
{
    public enum AppointmentCreationResult
    {
        Success,
        PetNotFound,
        DoctorProcedureMismatch,
        InvalidTimeSlotFormat,
        SlotAlreadyBooked,
        SlotDuringLunch
    }
}

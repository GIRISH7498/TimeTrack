namespace TimeTrack.Domain.Entities.NotificationModels
{
    public class NotificationAddressBook : EntityBase
    {
        public long AddressId { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneE164 { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
    }
}

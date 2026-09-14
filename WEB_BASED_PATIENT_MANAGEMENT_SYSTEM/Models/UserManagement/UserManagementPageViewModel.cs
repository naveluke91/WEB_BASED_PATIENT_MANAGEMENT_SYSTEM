namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    public class UserManagementPageViewModel
    {
        public List<UserAccount> Users { get; set; } = new();

        // The signed-in Admin, who cannot delete their own account or change its role.
        public int CurrentUserId { get; set; }

        // The last Admin account cannot be deleted.
        public int AdminCount { get; set; }
    }
}

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    public class UserManagementPageViewModel
    {
        public List<UserAccount> Users { get; set; } = new();

        // SuperAdmin: Admin ug Staff ang makita ug ma-manage. Admin: Staff ra.
        public bool IsSuperAdmin { get; set; }
    }
}

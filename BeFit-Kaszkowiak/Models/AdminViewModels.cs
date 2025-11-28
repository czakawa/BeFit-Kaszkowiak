namespace BeFit_Kaszkowiak.Models
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class ManageRolesViewModel
    {
        public string UserId { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public IList<RoleCheckbox> Roles { get; set; } = new List<RoleCheckbox>();
    }

    public class RoleCheckbox
    {
        public string RoleName { get; set; } = default!;
        public bool Selected { get; set; }
    }
}

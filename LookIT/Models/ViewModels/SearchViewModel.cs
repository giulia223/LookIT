namespace LookIT.Models.ViewModels
{
    public class SearchViewModel

    {
        public string SearchTerm { get; set; }

        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    }
}

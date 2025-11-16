namespace LookIT.Models
{
    public class GroupMember
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }

        public string MemberId { get; set; }

        public virtual ApplicationUser Member { get; set; }

        public int GroupId { get; set; }

        public virtual Group Group { get; set; }

    }
}

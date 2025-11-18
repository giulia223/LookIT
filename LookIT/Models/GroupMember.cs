namespace LookIT.Models
{
    public class GroupMember
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }

        // am pus UserId nvarchar(50) 
        //100 bytes + 4 bytes = 104 bytes < 900 bytes
        public string MemberId { get; set; }

        public virtual ApplicationUser Member { get; set; }

        public int GroupId { get; set; }

        public virtual Group Group { get; set; }

    }
}

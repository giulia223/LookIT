namespace LookIT.Models
{
    public class FollowRequest
    {
        public string Status { get; set; }

        public DateTime Date {  get; set; }

        // am pus UserId nvarchar(50) 
        //100 bytes + 100 bytes = 200 bytes < 900 bytes
        public string FollowerId { get; set; }

        public string FollowingId { get; set; }

        public virtual ApplicationUser Follower { get; set; }

        public virtual ApplicationUser Following { get; set; }

    }
}

namespace LookIT.Models
{
    public class FollowRequest
    {
        public string Status { get; set; }

        public DateTime Date {  get; set; }

        public string FollowerId { get; set; }

        public string FollowingId { get; set; }

        public virtual ApplicationUser Follower { get; set; }

        public virtual ApplicationUser Following { get; set; }

    }
}

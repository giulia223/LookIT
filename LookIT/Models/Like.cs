namespace LookIT.Models
{
    public class Like
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int PostId{ get; set; }
        public virtual Post Post { get; set; }

    }
}

namespace LookIT.Models
{
    public class Like
    {
        //aici ar trebui modificat ca PK sa fie LikeId pentru ca UserId (nvarchar(450))
        //are 900 bytes, PostId (int) are 4 bytes = 904 bytes > 900 bytes (max key size for SQL Server)
        //ne lamurim mai tarziu cum rezolvam asta
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int PostId{ get; set; }
        public virtual Post Post { get; set; }

    }
}

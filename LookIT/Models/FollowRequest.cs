namespace LookIT.Models
{
    public class FollowRequest
    {
        public string Status { get; set; }

        public DateTime Date {  get; set; }

        //aici ar trebui modificat ca PK sa fie _ pentru ca FollowerId (nvarchar(450))
        //are 900 bytes, FollowingId (nvarchar(450)) are 900 bytes = 1800 bytes > 900 bytes
        //(max key size for SQL Server)
        //ne lamurim mai tarziu cum rezolvam asta
        public string FollowerId { get; set; }

        public string FollowingId { get; set; }

        public virtual ApplicationUser Follower { get; set; }

        public virtual ApplicationUser Following { get; set; }

    }
}

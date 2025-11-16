namespace LookIT.Models
{
    public class GroupMember
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }

        //aici ar trebui modificat ca PK sa fie _ pentru ca MemberId (nvarchar(450))
        //are 900 bytes, GroupId (int) are 4 bytes = 904 bytes > 900 bytes (max key size for SQL Server)
        //ne lamurim mai tarziu cum rezolvam asta
        public string MemberId { get; set; }

        public virtual ApplicationUser Member { get; set; }

        public int GroupId { get; set; }

        public virtual Group Group { get; set; }

    }
}

namespace DataBase
{
    public class User
    {
        public int Id { get; set; }
        public long TID { get; set; }
        public string UserName { get; set; }
        public override string ToString() => $"{UserName} ";
    }

}

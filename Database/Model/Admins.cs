namespace DataBase
{
    public class Admins
    {
        public int Id { get; set; }
        public long TID { get; set; }
        public string UserName { get; set; }
        public int Rang { get; set; }
        public override string ToString() => $"{UserName} [{Rang}]";
    }

}

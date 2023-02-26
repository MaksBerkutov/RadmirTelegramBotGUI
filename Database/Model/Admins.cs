namespace DataBase
{
    public class Admins: User
    {

        public int Rang { get; set; }
        public override string ToString() => $"{UserName} [{Rang}]";
    }

}

namespace DataBase
{
    public class ChatLS
    {
        public int Id { get; set; }
        public long ID_Chat { get; set; }
        public string UserName { get; set; }
        public override string ToString() => UserName;
    }


}

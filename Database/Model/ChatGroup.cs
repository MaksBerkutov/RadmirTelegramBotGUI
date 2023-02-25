namespace DataBase
{
    public class ChatGroup
    {
        public int Id { get; set; }
        public long ID_Chat { get; set; }
        public string GroupName { get; set; }
        public override string ToString() => GroupName;
    }

}

namespace DataBase
{
    public class User
    {
        public int Id { get; set; }
        public long TID { get; set; }
        public string UserName { get; set; }
        public string Nicname { get; set; }
        public string FirstName { get; set; }
        public string GetNicks()
        {
            if (Nicname != null && Nicname.Length != 0) return Nicname;
            else if (UserName != null && UserName.Length != 0) return $"@{UserName}";
            else if (FirstName != null && FirstName.Length != 0) return $"@{FirstName}";
            else return $"[{TID}]";
        }
        public override string ToString()
        {
            if (UserName != null && UserName.Length != 0) return $"@{UserName}";
            else if (FirstName != null && FirstName.Length != 0) return $"@{FirstName}";
            else return $"[{TID}]";
        }
    }

}

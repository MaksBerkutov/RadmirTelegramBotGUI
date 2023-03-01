namespace DataBase
{
    public class ItemSupriseDonate:ItemSuprise
    {
       public int MinDonate { get; set; }
        public string SynbscribeCheck {get; set;}
       public override string ToString() => $"{base.ToString()}  [{MinDonate}$]";
       
    }

}

using System;

namespace DataBase
{
    public class ItemSuprise
    {
        public bool Started { get; set; }
        public bool Closed { get; set; }


        public int Id { get; set; }
        public string Name { get; set; }
        public string Desription { get; set; }
        public int Price { get; set; }
        public long ChatID { get; set; }
        public bool Fake { get; set; }
        public string Image { get; set; }
        public override string ToString() => $"{Name} {Price}$";
        public long FalkeID { get; set; }

        public string Subscribers { get; set; }
        public DateTime DeteEnd { get; set; }
        public DateTime DateStart { get; set; }
    }

}

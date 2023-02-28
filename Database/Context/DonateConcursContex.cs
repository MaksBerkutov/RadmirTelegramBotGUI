using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class DonateConcursContex: DbContext
    {
        public DbSet<DonateConcurs> Donate { get; set; }
        public static ObservableCollection<DonateConcurs> StaticItems { get; set; } = new ObservableCollection<DonateConcurs>();
        public static void Init()
        {
            using (var context = new DonateConcursContex())
            {
                StaticItems = new ObservableCollection<DonateConcurs>(context.Donate.ToList());
            }
        }
        public DonateConcursContex() : base("DbConnection")
        { }

    }

}

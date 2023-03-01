using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class TunelContext : DbContext
    {
        public DbSet<Tunel> Tunels { get; set; }
        public static ObservableCollection<Tunel> StaticItems { get; set; } = new ObservableCollection<Tunel>();
        public static void Init()
        {
            using (var context = new TunelContext())
            {
                StaticItems = new ObservableCollection<Tunel>(context.Tunels.ToList());
            }
        }
        public TunelContext() : base("DbConnection")
        { }

    }

}

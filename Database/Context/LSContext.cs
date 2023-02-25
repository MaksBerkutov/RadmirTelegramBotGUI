using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class LSContext : DbContext
    {
        public DbSet<ChatLS> LS { get; set; }
        public static ObservableCollection<ChatLS> StaticItems = new ObservableCollection<ChatLS>();
        public static void Init()
        {
            using (var context = new LSContext())
            {
                StaticItems = new ObservableCollection<ChatLS>(context.LS.ToList());
            }
        }
        public LSContext() : base("DbConnection")
        { }

    }

}

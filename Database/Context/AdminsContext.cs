using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class AdminsContext : DbContext
    {
        public DbSet<Admins> Admins { get; set; }
        public static ObservableCollection<Admins> StaticItems { get; set; } = new ObservableCollection<Admins>();
        public static void Init()
        {
            using (var context = new AdminsContext())
            {
                StaticItems = new ObservableCollection<Admins>(context.Admins.ToList());
            }
        }
        public AdminsContext() : base("DbConnection")
        { }

    }

}

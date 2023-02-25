using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class UsersContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public static ObservableCollection<User> StaticItems { get; set; } = new ObservableCollection<User>();
        public static void Init()
        {
            using (var context = new UsersContext())
            {
                StaticItems = new ObservableCollection<User>(context.Users.ToList());
            }
        }
        public UsersContext() : base("DbConnection")
        { }

    }

}

using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class ItemwWinnerContext : DbContext
    {
        public DbSet<ItemsWinner> Items { get; set; }
        public static ObservableCollection<ItemsWinner> StaticItems { get; set; } = new ObservableCollection<ItemsWinner>();
        public static void Init()
        {
            using (var context = new ItemwWinnerContext())
            {
                StaticItems = new ObservableCollection<ItemsWinner>(context.Items.ToList());
            }
        }
        public ItemwWinnerContext() : base("DbConnection")
        { }

    }

}

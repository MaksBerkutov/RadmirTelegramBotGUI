using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class ItemwWinnerDonateContext : DbContext
    {
        public DbSet<ItemsWinnerDonate> Items { get; set; }
        public static ObservableCollection<ItemsWinnerDonate> StaticItems { get; set; } = new ObservableCollection<ItemsWinnerDonate>();
        public static void Init()
        {
            using (var context = new ItemwWinnerDonateContext())
            {
                StaticItems = new ObservableCollection<ItemsWinnerDonate>(context.Items.ToList());
            }
        }
        public ItemwWinnerDonateContext() : base("DbConnection")
        { }

    }

}

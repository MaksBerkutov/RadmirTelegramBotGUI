using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class ItemsSurpriceDonateContext : DbContext
    {
        public DbSet<ItemSupriseDonate> Items { get; set; }
        public static ObservableCollection<ItemSupriseDonate> StaticItems { get; set; } = new ObservableCollection<ItemSupriseDonate>();
        public static void Init()
        {
            using (var context = new ItemsSurpriceDonateContext())
            {
                StaticItems = new ObservableCollection<ItemSupriseDonate>(context.Items.ToList());
            }
        }
        public ItemsSurpriceDonateContext() : base("DbConnection")
        { }

    }

}

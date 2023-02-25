using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class ItemsSurpriceContext : DbContext
    {
        public DbSet<ItemSuprise> Items { get; set; }
        public static ObservableCollection<ItemSuprise> StaticItems { get; set; } = new ObservableCollection<ItemSuprise>();
        public static void Init()
        {
            using (var context = new ItemsSurpriceContext())
            {
                StaticItems = new ObservableCollection<ItemSuprise>(context.Items.ToList());
            }
        }
        public ItemsSurpriceContext() : base("DbConnection")
        { }

    }

}

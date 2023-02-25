using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace DataBase
{
    public class ChatGroupContext : DbContext
    {
        public DbSet<ChatGroup> GROUP { get; set; }
        public static ObservableCollection<ChatGroup> StaticItems { get; set; } = new ObservableCollection<ChatGroup>();
        public static void Init()
        {
            using (var context = new ChatGroupContext())
            {
                StaticItems = new ObservableCollection<ChatGroup>(context.GROUP.ToList());
            }
        }
        public ChatGroupContext() : base("DbConnection")
        { }

    }

}

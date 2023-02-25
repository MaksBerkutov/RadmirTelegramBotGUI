using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DataBase
{
    public class MessageContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }
        public static ObservableCollection<Message> StaticItems { get; set; } = new ObservableCollection<Message>();
        public static async Task Init()
        {
            await Task.Run(() =>
            {
                using (var context = new MessageContext())
                {
                    StaticItems = new ObservableCollection<Message>(context.Messages.ToList());
                }
            });
        }
        public MessageContext() : base("DbConnection")
        { }

    }

}

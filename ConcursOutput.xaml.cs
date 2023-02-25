using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RadmirTelegramBotGUI
{
    /// <summary>
    /// Логика взаимодействия для ConcursOutput.xaml
    /// </summary>
    public partial class ConcursOutput : Window
    {
        public ConcursOutput(DataBase.ItemSuprise obj)
        {
            InitializeComponent();
            var imageData = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(obj.Image);
            if (imageData != null)
            {
                BitmapImage image = new BitmapImage();

                using (MemoryStream memoryStream = new MemoryStream(imageData))
                {
                    memoryStream.Position = 0;
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = memoryStream;
                    image.EndInit();
                }
                IMG.Source = image;
            }
            this.DataContext = obj;
        }
    }
}

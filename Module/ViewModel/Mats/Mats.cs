using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadmirTelegramBotGUI.Module
{
    class Mats
    {
        private static string[] _baseData;
        public static async Task Load()
        {
            await Task.Run(() =>
            {
                _baseData = System.IO.File.ReadAllLines(@"C:\Users\User\source\repos\RadmirTelegramBotGUI\Module\ViewModel\Mats\TextFile1.txt");
                
            });
        }
        public static bool check(string str)
        {
            try
            {
                foreach (var i in _baseData)
                    if (str.ToLower().Contains(i.ToLower())) return true;
                return false;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return false;
           
        }
    }
}

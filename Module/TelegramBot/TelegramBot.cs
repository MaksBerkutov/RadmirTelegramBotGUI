/*
 /Concurs [Nissan Slivia S15] [Это лучший автомобиль для дрифта для новечка] [15000] [0] [0] [2023-02-20 21:30:00] [2023-02-20 20:30:00]
 /dconcurs [Nissan Slivia S15] [Это лучший автомобиль для дрифта для новечка] [15000] [0] [0] [2023-02-20 21:30:00] [2023-02-20 20:30:00] [donateSum]
 
 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using System.Timers;
using System.Text.RegularExpressions;
using System.Configuration;

namespace RadmitTelegramBot
{
    public class TBot
    {
        private static System.Timers.Timer DelMsgTimer = new System.Timers.Timer();
        private static System.Timers.Timer CheckCocnc = new System.Timers.Timer();
        private static readonly TimeSpan messageTime = new TimeSpan(0, 0, 0, 1, 0);
        private static Dictionary<long, (DateTime, int)> tmpValueTime = new Dictionary<long, (DateTime, int)>();
        private static Dictionary<long, DateTime> Muted = new Dictionary<long, DateTime>();
        private static ITelegramBotClient botClient;
        static readonly DateTime StartedTime = DateTime.UtcNow;
        private static readonly Regex _SyntaxCommand = new Regex(@"\[(.*?)\]");
        public static int IWINChooseHandler(DataBase.User usr,DataBase.ItemSuprise conc, DataBase.ItemsWinner win)
        {
            var image = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(conc.Image);
            List<InlineKeyboardButton> tmpKey = new List<InlineKeyboardButton>() { new InlineKeyboardButton(conc.Name) { CallbackData = $"WIN_ITEM_{win.ID_Concurs}" }, new InlineKeyboardButton($"{conc.Price}$") { CallbackData = $"WIN_MONEY_{win.ID_Concurs}" } };
            var Caption = $"Поздравляем {usr.GetNicks()}\n У вас есть выбор {conc.Name} или {conc.Price}$ ниже кнопки для выбора)";
            if (image != null)
            {
                using (var stream = new MemoryStream(image))
                {
                    var photo = new InputOnlineFile(stream);
                    return  botClient.SendPhotoAsync(usr.TID, photo, Caption, ParseMode.Html, null, true, false, null, false, new InlineKeyboardMarkup(tmpKey.ToArray())).Result.MessageId;

                }
            }

            else return  botClient.SendTextMessageAsync(usr.TID, Caption, ParseMode.Html, null, true, false, null, 0, null, new InlineKeyboardMarkup(tmpKey.ToArray())).Result.MessageId;

        }
        public static int IWINChooseHandler(DataBase.User usr, DataBase.ItemSupriseDonate conc, DataBase.ItemsWinnerDonate win)
        {
            var image = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(conc.Image);
            List<InlineKeyboardButton> tmpKey = new List<InlineKeyboardButton>() { new InlineKeyboardButton(conc.Name) { CallbackData = $"DWIN_ITEM_{win.ID_Concurs}" }, new InlineKeyboardButton($"{conc.Price}$") { CallbackData = $"WIN_MONEY_{win.ID_Concurs}" } };
            var Caption = $"Поздравляем {usr.GetNicks()}\n У вас есть выбор {conc.Name} или {conc.Price}$ ниже кнопки для выбора)";
            if (image != null)
            {
                using (var stream = new MemoryStream(image))
                {
                    var photo = new InputOnlineFile(stream);
                    return botClient.SendPhotoAsync(usr.TID, photo, Caption, ParseMode.Html, null, true, false, null, false, new InlineKeyboardMarkup(tmpKey.ToArray())).Result.MessageId;

                }
            }

            else return botClient.SendTextMessageAsync(usr.TID, Caption, ParseMode.Html, null, true, false, null, 0, null, new InlineKeyboardMarkup(tmpKey.ToArray())).Result.MessageId;

        }
        private static async Task AccCabHandler(ITelegramBotClient botClient, Update update)
        {
            await botClient.DeleteMessageAsync(update.Message.From.Id,update.Message.MessageId);
            var param = _SyntaxCommand.Matches(update.Message.Type == MessageType.Photo ? update.Message.Caption : update.Message.Text);
            string cmd = update.Message.Type == MessageType.Photo ? update.Message.Caption.Split(' ')[0] : update.Message.Text.Split(' ')[0];

            //var arr = reg.Matches(input);
            try
            {
                switch (cmd.ToLower())
                {
                    case "/cdonate":
                        {
                            // /cdonate [ID_CONCURS] [SUM]
                            if (DataBase.Manager.TryGetConcurs(int.Parse(param[0].Groups[1].Value), out var conc))
                                if (DataBase.Manager.TryGetUser(update.Message.From.Id, out var user))
                                    await DataBase.Manager.CreateReqestDonate(conc, user, int.Parse(param[1].Groups[1].Value));
                            break;
                        }
                    
                    case "/win":
                        {
                            await DataBase.Manager.IWINHandler(update.Message.From.Id, update.Message.From.Id);
                            break;
                        }
                    case "/cdonatc":
                        {
                            if(DataBase.Manager.TryGetUser(update.Message.From.Id,out var ACC))
                                await DataBase.Manager.ReqestToSybscribe(int.Parse(param[0].Groups[1].Value),ACC);
                            break;
                        }
                    case "/dwin":
                        {
                            await DataBase.Manager.IDWINHandler(update.Message.From.Id, update.Message.From.Id);
                            break;
                        }
                    case "/nicks":
                        string nick = param[0].Groups[1].Value;
                        if (DataBase.Manager.TryGetUser(update.Message.From.Id, out var Acc))
                            await DataBase.Manager.SetNick(Acc, nick);
                        else if (DataBase.Manager.TryGetAdmins(update.Message.From.Id, out var Adm))
                            await DataBase.Manager.SetNick(Adm, nick);
                        else return;
                        await botClient.SendTextMessageAsync(update.Message.From.Id, $"Успешно установлен никнейм {nick}");
                        break;
                    default:
                        if(DataBase.Manager.TryGetAdmins(update.Message.From.Id,out var ADM))
                        {
                            switch (cmd.ToLower())
                            {
                                case "/cleardatabase":
                                    await DataBase.Manager.ClearAllDatabases();
                                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Все бд были очишены администратором \n@{update.Message.From.Username}");
                                    break;
                                case "/cend":
                                    if (update.Message.Type == MessageType.Photo)
                                    {
                                        byte[] imageBytes = null;
                                        using (var stream = new MemoryStream())
                                        {
                                            var file = await botClient.GetInfoAndDownloadFileAsync(update.Message.Photo.Last().FileId, stream);
                                            FileStream fileStream = new FileStream("image.png", FileMode.Create);
                                            await botClient.DownloadFileAsync(file.FilePath, fileStream);
                                            fileStream.Close();
                                            imageBytes = stream.ToArray();
                                        }
                                        await DataBase.Manager.CloseConcurs(ADM, int.Parse(param[0].Groups[1].Value),imageBytes);
                                    }
                                    else await DataBase.Manager.CloseConcurs(ADM, int.Parse(param[0].Groups[1].Value));
                                    break;
                    case "/dcend":
                        if (update.Message.Type == MessageType.Photo)
                        {
                            byte[] imageBytes = null;
                            using (var stream = new MemoryStream())
                            {
                                var file = await botClient.GetInfoAndDownloadFileAsync(update.Message.Photo.Last().FileId, stream);
                                FileStream fileStream = new FileStream("image.png", FileMode.Create);
                                await botClient.DownloadFileAsync(file.FilePath, fileStream);
                                fileStream.Close();
                                imageBytes = stream.ToArray();
                            }
                            await DataBase.Manager.CloseConcursDonate(ADM, int.Parse(param[0].Groups[1].Value), imageBytes);
                        }
                        else await DataBase.Manager.CloseConcursDonate(ADM, int.Parse(param[0].Groups[1].Value));
                        break;
                    case "/ctunel":
                                    {
                                        if (DataBase.Manager.TryGetTunel(int.Parse(param[0].Groups[1].Value), out var tun))
                                            await DataBase.Manager.ConnectAdminToTunel(ADM, tun);
                                        break;
                                    }
                                case "/etunel":
                                    {
                                        if (DataBase.Manager.TryGetTunel(int.Parse(param[0].Groups[1].Value), out var tun))
                                            await DataBase.Manager.CloseTunel( tun, ADM);
                                        break;
                                    }
                                case "/sdconcurs":
                                    {

                                            await DataBase.Manager.AcceptDonateConcurs(ADM, int.Parse(param[0].Groups[1].Value), int.Parse(param[1].Groups[1].Value));
                                        break;
                                    }
                                case "/edonate":
                                    {
                                        // /edonate [ID_DONATE]
                                        if (DataBase.Manager.TryGetDonateConcurs(int.Parse(param[0].Groups[1].Value), out var donate))
                                            await DataBase.Manager.AnswerReqestDonate(donate,ADM);

                                        break;
                                    }
                            }
                        }
                        else await botClient.SendTextMessageAsync(update.Message.From.Id, $"Не найденна комманда {cmd}");
                        break;
                }
                
            }
            catch (Exception ex)
            {

                await botClient.SendTextMessageAsync(update.Message.From.Id, "Допущенна ошибка в ситаксе команды");
                Console.WriteLine(ex.Message);
            }
        }
        private static async Task PerconalCabdinet(ITelegramBotClient botClient, Update update)
        {
            if (update.Message != null)
                if (!(update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group || update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup || update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Channel))
                    if (update.Message.Type == MessageType.Text)
                    {
                        if (update.Message.Text.Contains("/")) await AccCabHandler(botClient, update);
                        else if (DataBase.Manager.TryGetAdmins(update.Message.From.Id, out var admins))
                        {
                            if (DataBase.Manager.CheckTunel(admins.Id, out var tunel))
                                await DataBase.Manager.SendTunel(admins, tunel, update.Message.Text);
                        }    
                            
                        else if(DataBase.Manager.TryGetUser(update.Message.From.Id, out var users))
                        {
                            if (DataBase.Manager.CheckTunel(users.Id, out var tunel))
                                await DataBase.Manager.SendTunel(users, tunel, update.Message.Text);
                        }
                               
                    }
                    else if (update.Message.Type == MessageType.Photo) if (update.Message.Caption.Contains("/")) await AccCabHandler(botClient, update);

        }
        public static async Task Start()

        {
            await RadmirTelegramBotGUI.Module.Mats.Load();
            await Task.Run(() =>
            {



                // 6206777282:AAEPm0ib-NWEkUsL8vw13EnZ_RNneUqoyBs
                botClient = new TelegramBotClient(ConfigurationManager.AppSettings["ApiKey"]);
                DelMsgTimer.Interval = 10000;
                DelMsgTimer.Elapsed += DelMsgTimer_Tick1; DelMsgTimer.Start();
                CheckCocnc.Interval = 30000;
                CheckCocnc.Elapsed += CheckCocnc_Tick; CheckCocnc.Start();

                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { }, // receive all update types
                };
                botClient.StartReceiving(
                    HandleUpdateAsync,
                    HandleErrorAsync,

                    receiverOptions,
                    cancellationToken
                );
                Console.ReadLine();
            });



        }

        private static void CheckCocnc_Tick(object sender, ElapsedEventArgs e)
        {
            DataBase.Manager.ChechConck();
        }

        private static void DelMsgTimer_Tick1(object sender, ElapsedEventArgs e)
        {
            try
            {
                DataBase.Manager.RemoveMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        

        public static async Task DeleteMSG(long chat_id, int msg_id) => await botClient.DeleteMessageAsync(chat_id, msg_id);
        public static async Task WinnerConcur(DataBase.User winner, DataBase.ItemSuprise conc, bool nowiner = false)
        {
            var image = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(conc.Image);
            var Caption = string.Empty;
            if (nowiner) Caption = $"Не кто не захотел участвовать в конкурсе\n Ну ладно заберу себе {conc.Name}";
            else Caption = "Ну что же подошло время огласить победителя! \n" +
                $"Им Выступает {winner.GetNicks()} [{winner.TID}]\n" +
                $"Он(а) выиграл этот приз {conc.Name}\n" +
                $"Если вы не хотите забирать приз то можете забрать сумму в размере {conc.Price}$\n " +
                $"Отпишите мне в лс комманду /win\n" +
                $"Участвовало: {Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(conc.Subscribers).Count} людей";
            if (image != null)
            {
                using (var stream = new MemoryStream(image))
                {
                    var photo = new InputOnlineFile(stream);
                    await botClient.SendPhotoAsync(conc.ChatID, photo, Caption);

                }
            }

            else await botClient.SendTextMessageAsync(conc.ChatID, Caption);

        }
        public static async Task WinnerConcur(DataBase.User winner, DataBase.ItemSupriseDonate conc, bool nowiner = false)
        {
            var image = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(conc.Image);
            var Caption = string.Empty;
            if (nowiner) Caption = $"Не кто не захотел участвовать в конкурсе\n Ну ладно заберу себе {conc.Name}";
            else Caption = "Ну что же подошло время огласить победителя! \n" +
                $"Им Выступает {winner.GetNicks()} [{winner.TID}]\n" +
                $"Он(а) выиграл этот приз {conc.Name}\n" +
                $"Если вы не хотите забирать приз то можете забрать сумму в размере {conc.Price}$\n " +
                $"Отпишите мне в лс комманду /dwin\n" +
                $"Участвовало: {Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(conc.Subscribers).Count} людей";
            if (image != null)
            {
                using (var stream = new MemoryStream(image))
                {
                    var photo = new InputOnlineFile(stream);
                    await botClient.SendPhotoAsync(conc.ChatID, photo, Caption);

                }
            }

            else await botClient.SendTextMessageAsync(conc.ChatID, Caption);

        }
        public static async Task ChannelHandler(ITelegramBotClient botClient, Update update)
        {
            string cmd = update.ChannelPost.Type == MessageType.Photo ? update.ChannelPost.Caption : update.ChannelPost.Text;
            switch (cmd.Split(' ')[0])
            {
                case "/Concurs":
                    if (update.ChannelPost.Type == MessageType.Photo)
                    {
                        byte[] imageBytes = null;
                        //var file = await botClient.GetFileAsync(update.Message.Photo.First().FileId);


                        using (var stream = new MemoryStream())
                        {
                            var file = await botClient.GetInfoAndDownloadFileAsync(update.ChannelPost.Photo.Last().FileId, stream);
                            FileStream fileStream = new FileStream("image.png", FileMode.Create);
                            await botClient.DownloadFileAsync(file.FilePath, fileStream);
                            fileStream.Close();
                            imageBytes = stream.ToArray();
                        }
                        DataBase.Manager.AddConcurs(update.ChannelPost.Caption + $" [{update.ChannelPost.SenderChat.Id}]", imageBytes);
                    }
                    else DataBase.Manager.AddConcurs(update.ChannelPost.Text + $" [{update.Message.SenderChat.Id}]");


                    break;
                case "/dconcurs":
                    if (update.ChannelPost.Type == MessageType.Photo)
                    {
                        byte[] imageBytes = null;
                        //var file = await botClient.GetFileAsync(update.Message.Photo.First().FileId);


                        using (var stream = new MemoryStream())
                        {
                            var file = await botClient.GetInfoAndDownloadFileAsync(update.ChannelPost.Photo.Last().FileId, stream);
                            FileStream fileStream = new FileStream("image.png", FileMode.Create);
                            await botClient.DownloadFileAsync(file.FilePath, fileStream);
                            fileStream.Close();
                            imageBytes = stream.ToArray();
                        }
                        DataBase.Manager.AddConcursDonate(update.ChannelPost.Caption + $" [{update.ChannelPost.SenderChat.Id}]", imageBytes);
                    }
                    else DataBase.Manager.AddConcursDonate(update.ChannelPost.Text + $" [{update.ChannelPost.SenderChat.Id}]");


                    break;
            }
        }
        public static async Task HandlerCMDGroup(ITelegramBotClient botClient, Update update)
        {
            Message input_msg;
            string cmd = update.Message.Type == MessageType.Photo ? update.Message.Caption : update.Message.Text;
            try
            {
                switch (cmd.Split(' ')[0])
                {
                    case "/SetAdmin":
                        //@UNAME RANG
                        if (cmd.Split(' ')[1].StartsWith("@"))
                        {
                            if (DataBase.Manager.TryGetUser(cmd.Split(' ')[1].Remove(0, 1), out var users))
                            {
                                if (int.TryParse(cmd.Split(' ')[2], out var lvl))
                                {
                                    await DataBase.Manager.AddAdmin(users.TID, users.UserName, lvl);
                                    input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Успешно добавлен Администратор @{ users.UserName} \nАдминистратором @{update.Message.From.Username}");
                                    DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                                }
                            }
                        }
                        // TID UNAME RANG
                        else if (long.TryParse(cmd.Split(' ')[1], out long value))
                        {
                            if (int.TryParse(cmd.Split(' ')[3], out var lvl))
                            {
                                DataBase.Manager.AddAdmin(value, cmd.Split(' ')[2], lvl);
                                input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Успешно добавлен Администратор @{cmd.Split(' ')[2]} \nАдминистратором @{update.Message.From.Username}");
                                DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                            }


                            return;
                        }
                        break;
                    case "/RemoveAdmin":
                        //@UNAME RANG
                        if (cmd.Split(' ')[1].StartsWith("@"))
                        {
                            if (DataBase.Manager.TryGetAdmins(cmd.Split(' ')[1].Remove(0, 1), out var users))
                            {
                                if (DataBase.Manager.TryGetAdmins(cmd.Split(' ')[1].Remove(0, 1), out var Main))
                                {
                                    if (Main.Rang < users.Rang)
                                    {
                                        input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"У администратора которого вы хотите удалить уровень выше вашего @{update.Message.From.Username}");
                                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"@{users.UserName} вас пытался удалить администратор с уровнем ниже вашего ! [@{update.Message.From.Username}]");
                                    }
                                    else
                                    {
                                        DataBase.Manager.RemoveAdmin(users.TID);
                                        input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Успешно удалён Администратор @{ users.UserName} \nАдминистратором @{update.Message.From.Username}");
                                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                                    }
                                }


                                //if (int.TryParse(cmd.Split(' ')[2], out var lvl))
                                //{
                                //    DataBase.Manager.AddAdmin(users.TID, users.UserName, lvl);
                                //    input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Успешно добавлен Администратор @{ users.UserName} \nАдминистратором @{update.Message.From.Username}");
                                //    DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                                //}
                            }
                        }
                        // TID UNAME 
                        else if (long.TryParse(cmd.Split(' ')[1], out long value))
                        {
                            //if (int.TryParse(cmd.Split(' ')[3], out var lvl))
                            //{
                            //    DataBase.Manager.AddAdmin(value, cmd.Split(' ')[2], lvl);
                            //    input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Успешно добавлен Администратор @{cmd.Split(' ')[2]} \nАдминистратором @{update.Message.From.Username}");
                            //    DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                            //}


                            return;
                        }
                        break;
                    case "/cleardatabase":
                        await DataBase.Manager.ClearAllDatabases();
                        input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Все бд были очишены администратором \n@{update.Message.From.Username}");
                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                        break;
                    case "/Concurs":
                        if (update.Message.Type == MessageType.Photo)
                        {
                            byte[] imageBytes = null;
                            //var file = await botClient.GetFileAsync(update.Message.Photo.First().FileId);


                            using (var stream = new MemoryStream())
                            {
                                var file = await botClient.GetInfoAndDownloadFileAsync(update.Message.Photo.Last().FileId, stream);
                                FileStream fileStream = new FileStream("image.png", FileMode.Create);
                                await botClient.DownloadFileAsync(file.FilePath, fileStream);
                                fileStream.Close();
                                imageBytes = stream.ToArray();
                            }
                            DataBase.Manager.AddConcurs(update.Message.Caption + $" [{update.Message.Chat.Id}]", imageBytes);
                        }
                        else DataBase.Manager.AddConcurs(update.Message.Text + $" [{update.Message.Chat.Id}]");


                        break;
                    case "/dconcurs":
                        if (update.Message.Type == MessageType.Photo)
                        {
                            byte[] imageBytes = null;
                            //var file = await botClient.GetFileAsync(update.Message.Photo.First().FileId);


                            using (var stream = new MemoryStream())
                            {
                                var file = await botClient.GetInfoAndDownloadFileAsync(update.Message.Photo.Last().FileId, stream);
                                FileStream fileStream = new FileStream("image.png", FileMode.Create);
                                await botClient.DownloadFileAsync(file.FilePath, fileStream);
                                fileStream.Close();
                                imageBytes = stream.ToArray();
                            }
                            DataBase.Manager.AddConcursDonate(update.Message.Caption + $" [{update.Message.Chat.Id}]", imageBytes);
                        }
                        else DataBase.Manager.AddConcursDonate(update.Message.Text + $" [{update.Message.Chat.Id}]");


                        break;
                    default:
                        input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Err not found {cmd}");
                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);

                        break;
                }
            }
            catch (Exception ex)
            {

                input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, ex.Message);
                DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);

            }
        }
        public static async Task HandleUpdateAsync(List<DataBase.Message> orDel)
        {
            try
            {
                var saveList = new List<DataBase.Message>(orDel.ToArray());
                foreach (var item in saveList)
                    try
                    {
                        await botClient.DeleteMessageAsync(item.CahtID, item.MessageID);
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.Message);
                        continue;
                    }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }
        public static async Task SendPool(string name, string[] items, long chatid,bool isAnonimous) => await botClient.SendPollAsync(chatid, name, items,isAnonimous);
        public static async Task SendPhoto(InputOnlineFile file,long chat_id,string msg)=>await botClient.SendPhotoAsync(chat_id,file, msg);
        public static async Task SendMSG(string msg, long ChatId,bool delete = true)
        {
            var input_msg = await botClient.SendTextMessageAsync(ChatId, msg);
            if(delete) DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);

        }
        public static void SetMute(int MuteTimeFromSecond,long TID)
        {
            tmpValueTime[TID] = (DateTime.Now, 0);
            Muted[TID] = DateTime.Now.AddSeconds(MuteTimeFromSecond);
        }
        public static async Task<Message> StartConcurs(DataBase.ItemSuprise conc)
        {
            var image = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(conc.Image);
            List<InlineKeyboardButton> tmpKey = new List<InlineKeyboardButton>() { new InlineKeyboardButton("Подписаться") { CallbackData = $"CONC_{conc.Id}" } };
            var Caption = "Сейчас начнёться конкурс \n" +
                   $"Призом выступет {conc.Name}\n" +
                   $"Кратакое описание {conc.Desription}\n" +
                   $"Примерная цена {conc.Price}\n " +
                   $"Дата окончания {conc.DeteEnd.ToLongDateString()} {conc.DeteEnd.ToLongTimeString()}";
            if (image != null)
            {
                using (var stream = new MemoryStream(image))
                {
                    var photo = new InputOnlineFile(stream);
                    return await botClient.SendPhotoAsync(conc.ChatID, photo, Caption, ParseMode.Html, null, true, false, null, false, new InlineKeyboardMarkup(tmpKey.ToArray()));

                }
            }

            else return await botClient.SendTextMessageAsync(conc.ChatID, Caption, ParseMode.Html, null, true, false, null, 0, null, new InlineKeyboardMarkup(tmpKey.ToArray()));

        }
        public static async Task<Message> StartConcurs(DataBase.ItemSupriseDonate conc)
        {
            var image = Newtonsoft.Json.JsonConvert.DeserializeObject<byte[]>(conc.Image);
            var Caption = "Сейчас начнёться конкурс \n" +
                   $"Призом выступет {conc.Name}\n" +
                   $"Кратакое описание {conc.Desription}\n" +
                   $"Примерная цена {conc.Price}\n " +
                   $"Цена участия {conc.MinDonate}\n " +
                   $"Для участи напишите мне в лс комманду '/cdonatc [{conc.Id}]'\n " +
                   $"Дата окончания {conc.DeteEnd.ToLongDateString()} {conc.DeteEnd.ToLongTimeString()}";
            if (image != null)
            {
                using (var stream = new MemoryStream(image))
                {
                    var photo = new InputOnlineFile(stream);
                    return await botClient.SendPhotoAsync(conc.ChatID, photo, Caption, ParseMode.Html);

                }
            }

            else return await botClient.SendTextMessageAsync(conc.ChatID, Caption, ParseMode.Html);

        }
        public static async Task CheckMute(ITelegramBotClient botClient, Update update,bool noRecursions = true)
        {
            
            if (Muted.TryGetValue(update.Message.From.Id, out var Muteds))
            {
                if (DateTime.Now > Muteds)
                {
                    Muted.Remove(update.Message.From.Id);
                    var input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"[@{update.Message.From.Username}]\nРазмучен!");
                    DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                }     
                else await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                return;
            }
            else
            {
                if (DataBase.Manager.TryGetAdmins(update.Message.From.Id, out var adm)) return;
                if (tmpValueTime.TryGetValue(update.Message.From.Id, out var value)&&noRecursions)
                {

                    if (DateTime.Now.Ticks - value.Item1.Ticks < messageTime.Ticks)
                    {

                        tmpValueTime[update.Message.From.Id] = (DateTime.Now, value.Item2 + 1);
                        var input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"[@{update.Message.From.Username}]\nХватит спамить в группе!\n");
                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                        await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);

                    }
                    if (RadmirTelegramBotGUI.Module.Mats.check(update.Message.Text))
                    {
                        tmpValueTime[update.Message.From.Id] = (DateTime.Now, value.Item2 + 1);
                        var input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"[@{update.Message.From.Username}]\nВыбирай выражения в группе!\n");
                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                        await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                    }
                    tmpValueTime[update.Message.From.Id] = (DateTime.Now, tmpValueTime[update.Message.From.Id].Item2);
                    if (value.Item2 > 3)
                    {
                        var input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"[@{update.Message.From.Username}]\nМут на 15 минут!");
                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                        Muted[update.Message.From.Id] = DateTime.Now.AddMinutes(15);

                        tmpValueTime[update.Message.From.Id] = (DateTime.Now, 0);

                    }

                }
                else
                {
                    tmpValueTime.Add(update.Message.From.Id, (DateTime.Now, 0));
                    await  CheckMute(botClient,update, false);
                }
            }
        }
        
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            try
            {
                if (update.Message != null)
                {
                    if (update.Message.Type == MessageType.ChatMembersAdded)
                    {

                        var chatId = update.Message.Chat.Id;
                        var username = update.Message.NewChatMembers.FirstOrDefault()?.Username;
                        await DataBase.Manager.TryAddGroup(chatId, username);
                        var input_msg = await botClient.SendTextMessageAsync(chatId, $"Привет, @{username}!\nДобро пожаловать берлогу)");
                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                        return;
                    }
                    if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                    {
                        if (update.Message.Date < StartedTime) return;
                        
                        if (update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group || update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
                        {
                            await DataBase.Manager.TryAddGroup(update.Message.Chat.Id, update.Message.Chat.Title);
                            await DataBase.Manager.TryAddUsers(update);
                            await CheckMute(botClient, update);
                            if (update.Message.Type == MessageType.Text)
                            {
                               
                                if (update.Message.Text.StartsWith("/"))
                                {
                                    if (!DataBase.Manager.ThisIsAdmin(update.Message.From.Id))
                                    {
                                        var input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"[@{update.Message.From.Username}]\nВы не являетесь администратором и не можете использовать какие либо команды!");
                                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                                        await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                                        return;
                                    }

                                    await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                                    await HandlerCMDGroup(botClient, update);
                                }
                                return;
                            }
                            if (update.Message.Type == MessageType.Photo)
                            {
                                if (update.Message.Caption == null) return;
                                if (update.Message.Caption.StartsWith("/"))
                                {
                                    if (!DataBase.Manager.ThisIsAdmin(update.Message.From.Id))
                                    {
                                        var input_msg = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"[@{update.Message.From.Username}]\nВы не являетесь администратором и не можете использовать какие либо команды!");
                                        DataBase.Manager.AddMessage(input_msg.Chat.Id, input_msg.MessageId);
                                        await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                                        return;
                                    }
                                    await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                                    //await botClient.DeleteChatPhotoAsync(update.Message.Chat.Id);
                                    await HandlerCMDGroup(botClient, update);
                                }
                            }
                        }
                        if (update.Message.Type == MessageType.Text || update.Message.Type == MessageType.Photo) 
                        {
                            await DataBase.Manager.TryAddLS(update.Message.From.Id, update.Message.From.Username);
                            await DataBase.Manager.TryAddUsers(update);
                            await PerconalCabdinet(botClient, update);
                           //if (DataBase.Manager.ThisIsUserOrAdmin(update.Message.From.Id))
                           //{
                           //    if (update.Message.Text == "/id") await botClient.SendTextMessageAsync(update.Message.From.Id, $"Username: {update.Message.From.Username}\nTID: {update.Message.From.Id}");
                           //
                           //    if (DataBase.Manager.ThisIsAdmin(update.Message.From.Id))
                           //    {
                           //        await botClient.SendTextMessageAsync(update.Message.From.Id, ProgrammHandler(update.Message.Text)); return;
                           //    }
                           //
                           //}
                           //else await botClient.SendTextMessageAsync(update.Message.From.Id, "Вы не вгруппе");
                        }
                    }
                }
                if (update.Type == UpdateType.ChannelPost)
                {
                    await DataBase.Manager.TryAddGroup(update.ChannelPost.SenderChat.Id, update.ChannelPost.SenderChat.Title);
                    if (update.ChannelPost.Type == MessageType.Photo)
                    {
                        if (update.ChannelPost.Caption.StartsWith("/"))
                        {
                            await ChannelHandler(botClient, update);
                            await botClient.DeleteMessageAsync(update.ChannelPost.SenderChat.Id, update.ChannelPost.MessageId);
                        }
                    }
                    else if (update.ChannelPost.Text.StartsWith("/"))
                    {
                        await ChannelHandler(botClient, update);
                        await botClient.DeleteMessageAsync(update.ChannelPost.SenderChat.Id, update.ChannelPost.MessageId);
                    }
                }
                if (update.Type == UpdateType.CallbackQuery)
                {
                    CallbackQuery callbackQuery = update.CallbackQuery;
                    switch (callbackQuery.Data.Split('_')[0])
                    {
                        case "CONC":
                            await DataBase.Manager.CallBackSubsckriber(update,int.Parse(callbackQuery.Data.Split('_')[1]), callbackQuery.Message.Chat.Id);
                            break;
                        case "WIN":
                            await DataBase.Manager.IWINButtPress(int.Parse(callbackQuery.Data.Split('_')[2]), callbackQuery.Data.Split('_')[1]);
                            break;
                        case "DWIN":
                            await DataBase.Manager.IDWINButtPress(int.Parse(callbackQuery.Data.Split('_')[2]), callbackQuery.Data.Split('_')[1]);
                            break;

                    }

                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex);
            }

        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await Task.Run(() => Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception)));
        }

    }
}

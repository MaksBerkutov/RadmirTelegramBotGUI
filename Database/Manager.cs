using RadmirTelegramBotGUI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataBase
{
    public static class Manager
    {
        private static Random rnd = new Random();
        //=====
      
        public static async Task UpdateDateInItemsWinner(ItemsWinner obj,int _messageChooseId)
        {
            using (var context = new ItemwWinnerContext())
            {
                var res = context.Items.Where(u =>u.Id==obj.Id).ToList();
                if (res.Any())
                {

                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemwWinnerContext.StaticItems.ToList().Find(u => u.Id == obj.Id).Id_Message_LS = _messageChooseId;
                    });
                    res[0].Id_Message_LS = _messageChooseId;
                    await context.SaveChangesAsync();
                }
            }
        }
        public static async Task SendAllAdminMessage(string msg)
        {
            foreach(var adm in AdminsContext.StaticItems)
                try
                {
                    await RadmitTelegramBot.TBot.SendMSG(msg, adm.TID, false);
                }
                catch (Exception)
                {
                    continue;
                }
        }
        public static async Task CloseConcurs(Admins ADM, int conc_id, byte[] img = null)
        {
            if(TryGetConcurs(conc_id,out var conc))
            {
                if(TryGetItemsWinner(conc_id,out var winner))
                {
                    if(img != null)
                    {
                        using (var stream = new MemoryStream(img))
                        {
                            var photo = new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream);
                            await RadmitTelegramBot.TBot.SendPhoto(photo, conc.ChatID, $"Приз в конкурсе {conc.Name} был выдан администраторм\n [@{ADM.UserName}]");

                        }
                    }
                    else await RadmitTelegramBot.TBot.SendMSG($"Приз в конкурсе {conc.Name} был выдан администраторм\n [@{ADM.UserName}]",conc.ChatID,false);
                    await RemoveConcursAsync(conc);
                    await RemoveWinnersAsync(winner);

                }
            }
        }
        public static async Task IWINButtPress(int id_conc,string Choose)
        {
            if(TryGetItemsWinner(id_conc, out var obj))
                if (TryGetUser(obj.Id_Winner, out var acc))
                    if (TryGetConcurs(obj.ID_Concurs, out var concurs))
                    {
                        await RadmitTelegramBot.TBot.DeleteMSG(acc.TID, obj.Id_Message_LS);
                        switch (Choose)
                        {
                            case "MONEY":
                                await SetInfoChooseWinItem(1, obj);
                                await SendAllAdminMessage($"\t[{id_conc}]\nПользователь {acc.GetNicks()} выбрал забрать сумму в размере {concurs.Price}$");
                                await RadmitTelegramBot.TBot.SendMSG($"Вы выбрали забрать сумму в размере {concurs.Price}$", acc.TID, false);
                                break;
                            case "ITEM":
                                await SetInfoChooseWinItem(0, obj);
                               await SendAllAdminMessage($"\t[{id_conc}]\nПользователь {acc.GetNicks()} выбрал забрать приз\n{concurs.Name}");
                                await RadmitTelegramBot.TBot.SendMSG($"Вы выбрали забрать приз\n{concurs.Name}", acc.TID, false);
                                break;
                        }
                       
                        
                
                    }
        }
        public static async Task IWINHandler(long UserId, long chat_ID)
        {
            if (TryGetUser(UserId, out var acc))
            {
                var WinConc = ItemwWinnerContext.StaticItems.ToList().FindAll(x => x.Id_Winner == acc.Id&&x.Id_Message_LS==-1);
                if (WinConc.Any())
                {
                    foreach (var item in WinConc)
                        if (TryGetConcurs(item.ID_Concurs, out var concurs))
                            await UpdateDateInItemsWinner(item, RadmitTelegramBot.TBot.IWINChooseHandler(acc, concurs, item));
                    }
                else await RadmitTelegramBot.TBot.SendMSG("К сожелению вы не чего не выграли", chat_ID);
            }
        }
        //===
        private static async Task HandlerWinnerChoose(ItemSuprise FindedEnd, ItemsWinner items)
        {
            var Subscribers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(FindedEnd.Subscribers);
            if (Subscribers.Count == 0)
            {
                await RadmitTelegramBot.TBot.WinnerConcur(null, FindedEnd, true);
                await RadmitTelegramBot.TBot.DeleteMSG(FindedEnd.ChatID, items.Id_Post);
                if (TryGetItemsWinner(FindedEnd.Id, out var itemsWinner)) await RemoveWinnersAsync(itemsWinner);
                await RemoveConcursAsync(FindedEnd);
                return;
            }
            var winner = Subscribers[rnd.Next(0, Subscribers.Count - 1)];
            if (TryGetUser(winner, out User user))
            {
                await RadmitTelegramBot.TBot.WinnerConcur(user, FindedEnd);
                await RadmitTelegramBot.TBot.DeleteMSG(FindedEnd.ChatID, items.Id_Post);
                await SetInfo(user.Id, items);
                //Out winner User
            }
        }
        public static async Task HandlerEndCocurs(ItemSuprise FindedEnd)
        {
            if (!TryGetItemsWinner(FindedEnd.Id, out var items)) return;
            if (FindedEnd.Fake)
            {
                if (TryGetUser(FindedEnd.FalkeID, out User user))
                {
                    //Out winner User
                    await RadmitTelegramBot.TBot.WinnerConcur(user, FindedEnd);
                    await RadmitTelegramBot.TBot.DeleteMSG(FindedEnd.ChatID, items.Id_Post);
                    await SetInfo(user.Id, items);


                }
                else await HandlerWinnerChoose(FindedEnd, items);
            }
            else await HandlerWinnerChoose(FindedEnd, items);
         
        }
        public static async Task CreateItemsWinner(ItemsWinner obj)
        {
            await Task.Run(() =>
            {
                using (var context = new ItemwWinnerContext())
                {
                    context.Items.Add(obj);
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemwWinnerContext.StaticItems.Add(obj);
                    });
                    context.SaveChanges();
                }
            });
        }
        public static async Task SetInfoChooseWinItem(short item, ItemsWinner obj)
        {
            await Task.Run(() =>
            {
                using (var context = new ItemwWinnerContext())
                {
                    var finded = context.Items.ToList().Find(x => x.Id == obj.Id);
                    if (finded != null)
                    {
                        finded.SetPrise = item;
                    }
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemwWinnerContext.StaticItems.ToList().Find(x => x.Id == obj.Id).SetPrise = item;
                    });
                    context.SaveChanges();
                }
            });
        }
        public static async Task SetInfo(int U_id, ItemsWinner obj)
        {
            await Task.Run(() =>
            {
                using (var context = new ItemwWinnerContext())
                {
                    var finded = context.Items.ToList().Find(x => x.Id == obj.Id);
                    if (finded != null)
                    {
                        finded.Id_Winner = U_id;
                    }
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemwWinnerContext.StaticItems.ToList().Find(x => x.Id == obj.Id).Id_Winner = U_id;
                    });
                    context.SaveChanges();
                }
            });
        }
        public static async Task ClearDatabase<T>() where T : DbContext, new()
        {
            await Task.Run(() =>
            {
                using (var context = new T())
                {
                    var tableNames = context.Database.SqlQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '%Migration%'").ToList();
                    foreach (var tableName in tableNames)
                    {
                        context.Database.ExecuteSqlCommand(string.Format("DELETE FROM {0}", tableName));
                    }

                    context.SaveChanges();
                }
            });

        }
        public static async Task RemoveMessage()
        {
            using (var context = new MessageContext())
            {
                var res = context.Messages.Where(u => u.DeteEnd < DateTime.Now).ToList();
                if (res.Any())
                {
                    await RadmitTelegramBot.TBot.HandleUpdateAsync(res);
                    context.Messages.RemoveRange(res);
                    context.SaveChanges();
                }
            }
        }
        public static void AddMessage(long id, int MesageId,double add_sec=30)
        {
            using (var context = new MessageContext())
            {
                var user = new Message
                {
                    MessageID = MesageId,
                    CahtID = id,
                    DeteEnd = DateTime.Now.AddSeconds(add_sec)
                };
                context.Messages.Add(user);
                context.SaveChanges();
            }
        }
        public static bool TryGetItemsWinner(int id_conc, out ItemsWinner itmes)
        {
            itmes = null;
            var users = ItemwWinnerContext.StaticItems.Where(u => u.ID_Concurs == id_conc).ToList();
            if (users.Any()) { itmes = users[0]; return true; }
            return false;
        }
        public static async Task ClearAllDatabases()
        {
            await ClearDatabase<ItemwWinnerContext>();
            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
            {
                ItemwWinnerContext.StaticItems.Clear();
            });
            await ClearDatabase<LSContext>();
            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
            {
                LSContext.StaticItems.Clear();
            });
            await ClearDatabase<ChatGroupContext>();
            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
            {
                ChatGroupContext.StaticItems.Clear();
            });
            await ClearDatabase<MessageContext>();
            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
            {
                MessageContext.StaticItems.Clear();
            });
            await ClearDatabase<UsersContext>();
            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
            {
                UsersContext.StaticItems.Clear();
            });
            await ClearDatabase<AdminsContext>();
            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
            {
                AdminsContext.StaticItems.Clear();
            });
            await ClearDatabase<ItemsSurpriceContext>();
            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
            {
                ItemsSurpriceContext.StaticItems.Clear();
            });


        }
        public static void AddGroup(long ID, string GroupName)
        {
            using (var context = new ChatGroupContext())
            {
                var user = new ChatGroup()
                {
                    GroupName = GroupName,
                    ID_Chat = ID

                };
                RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                {
                    ChatGroupContext.StaticItems.Add(user);
                });
                context.GROUP.Add(user);
                context.SaveChanges();
            }
        }
        public static void AddLS(long ID, string UName)
        {
            using (var context = new LSContext())
            {
                var user = new ChatLS()
                {
                    UserName = UName,
                    ID_Chat = ID

                };
                RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                {
                    LSContext.StaticItems.Add(user);
                });
                context.LS.Add(user);
                context.SaveChanges();
            }
        }
        public static async Task TryAddUsers(Telegram.Bot.Types.Update upd)
        {
            await Task.Run(() =>
            {
                if (!ThisIsUserOrAdmin(upd.Message.From.Id)) AddUsers(upd.Message.From.Id, upd.Message.From.Username, upd.Message.From.FirstName);
            });
        }
        public static bool TryFindLSStatic(long ID, out ChatLS chat)
        {
            chat = null;
            var res = LSContext.StaticItems.ToList().FindAll(x => x.ID_Chat == ID);
            if (res.Any()) { chat = res[0]; return true; }
            return false;
        }
        public static bool TryFindGroupStatic(long ID, out ChatGroup chat)
        {
            chat = null;
            var res = ChatGroupContext.StaticItems.ToList().FindAll(x => x.ID_Chat == ID);
            if (res.Any()) { chat = res[0]; return true; }
            return false;
        }
        public static async Task TryAddLS(long ID, string UName)
        {
            await Task.Run(() => {
                if (!LSContext.StaticItems.ToList().FindAll(x => x.ID_Chat == ID).Any()) AddLS(ID, UName);

            });
        }
        public static async Task TryAddGroup(long ID, string GroupName)
        {
            await Task.Run(() => {
                if (!ChatGroupContext.StaticItems.ToList().FindAll(x => x.ID_Chat == ID).Any()) AddGroup(ID, GroupName);

            });
        }

        public static async Task RemoveAdmin(long id)
        {
            await Task.Run(() =>
            {
                if (ThisIsUser(id)) return;
                using (var context = new AdminsContext())
                {
                    if (TryGetAdmins(id, out var Delete))
                    {
                        AddUsers(Delete.TID, Delete.UserName,Delete.FirstName,Delete.Nicname);
                        context.Admins.Remove(context.Admins.ToList().Find(x => x.Id == Delete.Id));
                        RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                        {
                            AdminsContext.StaticItems.Remove(Delete);
                        });
                        context.SaveChanges();
                    }

                }
            }
            );
        }
        public static async Task AddAdmin(long id, string UserName, int LVL)
        {
            await Task.Run(() =>
            {
               // if (ThisIsUser(id)) RemoveUser(id);
                if (ThisIsAdmin(id)) return;
                using (var context = new AdminsContext())
                {
                    Admins user;
                    if(TryGetUser(id,out var tmp)){
                        user = new Admins
                        {
                            UserName = tmp.UserName,
                            Nicname = tmp.Nicname,
                            FirstName = tmp.FirstName,
                            TID = id,
                            Rang = LVL

                        };
                        RemoveUser(id);
                    }
                    else user = new Admins
                    {
                        UserName = UserName,
                        TID = id,
                        Rang = LVL

                    };
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        AdminsContext.StaticItems.Add(user);
                    });
                    context.Admins.Add(user);
                    context.SaveChanges();
                }
            });
        }
        public static void AddUsers(long id, string UserName,string FirstName,string Nickname = null)
        {

            using (var context = new UsersContext())
            {
                var user = new User
                {
                    UserName = UserName,
                    TID = id,
                    FirstName = FirstName,
                    Nicname = Nickname
                };
                RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                {
                    UsersContext.StaticItems.Add(user);
                });
                context.Users.Add(user);
                context.SaveChanges();
            }


        }
        public static void AddUsers(Telegram.Bot.Types.User usr)
        {

            using (var context = new UsersContext())
            {
                var user = new User
                {
                    UserName = usr.Username,
                    TID = usr.Id,
                    FirstName = usr.FirstName,
                    Nicname = null
                };
                RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                {
                    UsersContext.StaticItems.Add(user);
                });
                context.Users.Add(user);
                context.SaveChanges();
            }


        }
        public static void RemoveUser(long id)
        {
            using (var context = new UsersContext())
            {
                var finded = context.Users.ToList().Find(x => x.TID == id);
                RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                {
                    UsersContext.StaticItems.Remove(UsersContext.StaticItems.ToList().Find(x => x.Id == finded.Id));
                });
                context.Users.Remove(finded);
                context.SaveChanges();
            }
        }
        public static bool ThisIsAdmin(long id)
        {
            var users = AdminsContext.StaticItems.Where(u => u.TID == id).ToList();
            if (users.Any()) return true;
            return false;
        }
        public static bool TryGetAdmins(long id, out Admins usr)
        {
            usr = null;
            var users = AdminsContext.StaticItems.Where(u => u.TID == id).ToList();
            if (users.Any()) { usr = users[0]; return true; }
            return false;
        }
        public static bool TryGetAdmins(string Uname, out Admins usr)
        {
            usr = null;
            var users = AdminsContext.StaticItems.Where(u => u.UserName == Uname).ToList();
            if (users.Any()) { usr = users[0]; return true; }
            return false;
        }
        public static bool ThisIsUser(long id)
        {
            var users = UsersContext.StaticItems.Where(u => u.TID == id).ToList();
            if (users.Any()) return true;
            return false;
        }
        public static bool TryGetUser(long id, out User usr)
        {
            usr = null;
            var users = UsersContext.StaticItems.Where(u => u.TID == id).ToList();
            if (users.Any()) { usr = users[0]; return true; }
            return false;
        }
        public static bool TryGetUser(string username, out User usr)
        {
            usr = null;
            var users = UsersContext.StaticItems.Where(u => u.UserName.Equals(username)).ToList();
            if (users.Any()) { usr = users[0]; return true; }
            return false;
        }
        public static bool TryGetUser(int id, out User usr)
        {
            usr = null;
            var users = UsersContext.StaticItems.Where(u => u.Id == id).ToList();
            if (users.Any()) { usr = users[0]; return true; }
            return false;
        }
        public static bool TryGetConcurs(int id, out ItemSuprise item)
        {
            item = null;
            var users = ItemsSurpriceContext.StaticItems.Where(u => u.Id == id).ToList();
            if (users.Any()) { item = users[0]; return true; }
            return false;
        }
        public static async Task Sybscribe(int idCon, int Id_Acc)
        {
            await Task.Run(() =>
            {
                using (var context = new ItemsSurpriceContext())
                {
                    var users = context.Items.ToList().Find(x => x.Id == idCon);
                    if (users.Subscribers == null) users.Subscribers = Newtonsoft.Json.JsonConvert.SerializeObject(new List<int>());
                    var tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(users.Subscribers);
                    tmp.Add(Id_Acc);
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemsSurpriceContext.StaticItems.ToList().Find(x => x.Id == idCon).Subscribers = Newtonsoft.Json.JsonConvert.SerializeObject(tmp);
                    });
                    users.Subscribers = Newtonsoft.Json.JsonConvert.SerializeObject(tmp);
                    context.SaveChanges();
                }
            });
        }
        public static async Task ChechConck()
        {
            await Task.Run(async () =>
            {
                using (var context = new ItemsSurpriceContext())
                {
                    //var tmp = context.Items.Where(u =>u.Closed ).ToList();tmp[0].Closed = false; context.SaveChanges();

                    var res = context.Items.Where(u =>
                    !u.Started && !u.Closed && u.DateStart <= DateTime.Now).ToList();
                    var End = context.Items.Where(u =>
                    u.Started && !u.Closed && u.DeteEnd <= DateTime.Now).ToList();
                    if (res.Any())
                    {

                        foreach (var items in res)
                        {
                            var Winner = new ItemsWinner()
                            {
                                id_Admins = -1,
                                ID_Concurs = items.Id,
                                Id_Post = RadmitTelegramBot.TBot.StartConcurs(items).Result.MessageId,
                                Id_Winner = -1,
                                Id_Message_LS = -1,
                                SetPrise = -1
                            };
                            CreateItemsWinner(Winner);
                            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                            {
                                ItemsSurpriceContext.StaticItems.ToList().Find(u => u.Id == items.Id).Started = true;
                            });
                            items.Started = true;

                        }
                        context.SaveChanges();



                    }
                    if (End.Any())
                    {
                        foreach (var items in End)
                        {
                            //RadmitTelegramBot.TBot.StartConcurs(items);
                            HandlerEndCocurs(items);
                            RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                            {
                                ItemsSurpriceContext.StaticItems.ToList().Find(u => u.Id == items.Id).Closed = true;
                            });
                            items.Closed = true;
                            //RemoveConcursAsync(items);

                        }
                        context.SaveChanges();
                    }

                }
            });
        }
        public static bool ThisIsUserOrAdmin(long id)
        {
            try
            {
                return ThisIsUser(id) || ThisIsAdmin(id);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return false;

        }
        public static bool TryFindConc(int id, out ItemSuprise item)
        {
            item = null;
            var users = ItemsSurpriceContext.StaticItems.Where(u => u.Id == id).ToList();
            if (users.Any()) { item = users[0]; return true; }
            return false;
        }
        private static bool HelpConn(User usr, ItemSuprise conc)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(conc.Subscribers).FindAll(x => x == usr.Id).Any();
        }
        public static async Task CallBackSubsckriber(Telegram.Bot.Types.Update upd,int idCon,long chatId)
        {
            await Task.Run(() =>
            {
                if (!ThisIsUserOrAdmin(upd.CallbackQuery.From.Id)) AddUsers(upd.CallbackQuery.From);
                if (TryGetUser(upd.CallbackQuery.From.Id, out var Acc))
                    if (TryGetConcurs(idCon, out var Conc))
                        if (HelpConn(Acc, Conc)) RadmitTelegramBot.TBot.SendMSG($"Уважаемый {Acc.GetNicks()}\n Вы уже подписанны на данный конкурс!", chatId);
                        else { Sybscribe(idCon, Acc.Id); RadmitTelegramBot.TBot.SendMSG($"Уважаемый {Acc.GetNicks()}\n Вы успешно подписались на кокурс он оканчиваеться\n{Conc.DeteEnd.ToLongDateString()} {Conc.DeteEnd.ToLongTimeString()}", chatId); }
                    else RadmitTelegramBot.TBot.SendMSG($"Уважаемый {Acc.GetNicks()}\n На конкурс так как он не был найден)", chatId);
                else if(TryGetAdmins(upd.CallbackQuery.From.Id, out var ADM)) RadmitTelegramBot.TBot.SendMSG($"Уважаемый {ADM.GetNicks()}\n Вы являетесь администратором и не можете подписываться на конкурс", chatId);

            });
        }
        public static void AddConcurs(string input, byte[] image = null)
        {
            //  string   string       int    byte    def 0 long          D  M   Y   H  M  S                                      long
            // [Name] [Description] [Price] [Fake] [idWinerr]   [DateEnd{00-00-0000 00:00:00}] [DateStart{00-00-0000 00:00:00}] [ChatID]
            Regex reg = new Regex(@"\[(.*?)\]");
            var arr = reg.Matches(input);
            if (arr.Count != 7)
            {


                string name = arr[0].Groups[1].Value;
                string desc = arr[1].Groups[1].Value;
                int Price = 0; int.TryParse(arr[2].Groups[1].Value, out Price);
                bool Fake = byte.TryParse(arr[3].Groups[1].Value, out var res) ? res == 0 ? false : true : false;
                long WinId = Fake ? long.TryParse(arr[4].Groups[1].Value, out var resWin) ? resWin : 0 : 0;

                DateTime DateEnd = DateTime.TryParse(arr[5].Groups[1].Value, out var resdate) ? resdate : DateTime.Now.AddHours(1.30);
                DateTime DateSatrt = DateTime.TryParse(arr[6].Groups[1].Value, out var resdate1) ? resdate1 : DateTime.Now;
                long ChatID = long.TryParse(arr[7].Groups[1].Value, out var resChat) ? resChat : 0;
                if (ChatID == 0) return;

                using (var context = new ItemsSurpriceContext())
                {
                    var items = new ItemSuprise()
                    {
                        Started = false,
                        Closed = false,

                        Name = name,
                        Desription = desc,
                        Price = Price,
                        Fake = Fake,
                        FalkeID = WinId,
                        ChatID = ChatID,
                        DeteEnd = DateEnd,
                        DateStart = DateSatrt,
                        Subscribers = Newtonsoft.Json.JsonConvert.SerializeObject(new List<int>()),
                        Image = Newtonsoft.Json.JsonConvert.SerializeObject(image)
                    };

                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemsSurpriceContext.StaticItems.Add(items);
                    });
                    context.Items.Add(items);
                    context.SaveChanges();
                }
            }

        }
        public static async Task RemoveConcursAsync(ItemSuprise conc)
        {
            await Task.Run(() =>
            {
                using (var context = new ItemsSurpriceContext())
                {
                    var finded = context.Items.ToList().Find(x => x.Id == conc.Id);
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemsSurpriceContext.StaticItems.Remove(ItemsSurpriceContext.StaticItems.ToList().Find(x => x.Id == conc.Id));
                    });
                    context.Items.Remove(finded);
                    context.SaveChanges();
                }
            });

        }
        public static async Task RemoveWinnersAsync(ItemsWinner conc)
        {
            await Task.Run(() =>
            {
                using (var context = new ItemwWinnerContext())
                {
                    var finded = context.Items.ToList().Find(x => x.Id == conc.Id);
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemsSurpriceContext.StaticItems.Remove(ItemsSurpriceContext.StaticItems.ToList().Find(x => x.Id == conc.Id));
                    });
                    context.Items.Remove(finded);
                    context.SaveChanges();
                }
            });

        }
        public static void init()
        {
            LSContext.Init();
            ChatGroupContext.Init();
            MessageContext.Init();
            UsersContext.Init();
            AdminsContext.Init();
            ItemsSurpriceContext.Init();
            ItemwWinnerContext.Init();
        }

        //setNick
        public static async Task SetNick(User usr,string Nick)
        {
            await Task.Run(() =>
            {
                using (var context = new UsersContext())
                {
                    var finded = context.Users.ToList().Find(x => x.Id == usr.Id);
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        UsersContext.StaticItems[UsersContext.StaticItems.ToList().FindIndex(x => x.Id == finded.Id)].Nicname = Nick;
                    });
                    finded.Nicname = Nick;
                    context.SaveChanges();
                }
            });
           
        }
        public static async Task SetNick(Admins usr, string Nick)
        {
            await Task.Run(() =>
            {
                using (var context = new AdminsContext())
                {
                    var finded = context.Admins.ToList().Find(x => x.Id == usr.Id);
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        UsersContext.StaticItems[UsersContext.StaticItems.ToList().FindIndex(x => x.Id == finded.Id)].Nicname = Nick;
                    });
                    finded.Nicname = Nick;
                    context.SaveChanges();
                }
            });

        }
        //SetWinner
        public static async Task SetWinner(ItemSuprise conc,User usr)
        {
            await Task.Run(() =>
            {
                using (var concContext = new ItemsSurpriceContext())
                {
                    var ConcBD = concContext.Items.Where(p => p.Id == conc.Id).ToList()[0];
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        conc.Fake = true;
                        conc.FalkeID = usr.TID;
                    });
                    ConcBD.Fake = true;
                    ConcBD.FalkeID = usr.TID;
                    concContext.SaveChanges();
                }
                  
            });
        }
        //Start End Concurs 
        public static async Task StartConcurs(ItemSuprise conc)
        {
            if (conc.Started) return;
            await Task.Run(() =>
            {
                using (var concContext = new ItemsSurpriceContext())
                {
                    
                    var ConcBD = concContext.Items.Where(p => p.Id == conc.Id).ToList()[0];
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        conc.Started = true;
                    });
                    ConcBD.Started = true;
                    var Winner = new ItemsWinner()
                    {
                        id_Admins = -1,
                        ID_Concurs = conc.Id,
                        Id_Post = RadmitTelegramBot.TBot.StartConcurs(ConcBD).Result.MessageId,
                        Id_Winner = -1,
                        Id_Message_LS = -1,
                        SetPrise = -1
                    };
                    CreateItemsWinner(Winner);
                    RadmirTelegramBotGUI.MainWindow.instance.Invoke(() =>
                    {
                        ItemsSurpriceContext.StaticItems.ToList().Find(u => u.Id == ConcBD.Id).Started = true;
                    });
                    concContext.SaveChanges();

                }

            });
        }
        public static async Task EndConcurs(ItemSuprise conc)
        {
            if (conc.Closed) return;
            await Task.Run(() =>
            {
                using (var concContext = new ItemsSurpriceContext())
                {

                    var ConcBD = concContext.Items.Where(p => p.Id == conc.Id).ToList()[0];
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        conc.Closed = true;
                    });
                    ConcBD.Closed = true;
                    HandlerEndCocurs(conc);
                    concContext.SaveChanges();
                }

            });
        }
    }

}

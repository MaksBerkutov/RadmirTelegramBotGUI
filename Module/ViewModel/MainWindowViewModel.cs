using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadmirTelegramBotGUI.Module.ViewModel
{
    class PoolItems
    {
        public string Items { get; set; }   
        public static string[] ToArray(PoolItems[] obj)
        {
            List<string> list = new List<string>();
            foreach(var i in obj)list.Add(i.Items);
            return list.ToArray();
        }

    }
    partial class MainWindowViewModel : Base.ViewModel
    {
        //Commands
        public Base.Command SendToGroup { get; }
        public Base.Command SendToLs { get; }
        public Base.Command ToAdmin { get; }
        public Base.Command RemoveAdmin { get; }
        public Base.Command InfoConcurs { get; }
        //Poll
        public Base.Command AddElemetToPoll { get; }
        public Base.Command RemoveElemetOfPoll { get; }
        public Base.Command SendPoll { get; }

        public MainWindowViewModel()
        {
            SendToGroup = new Base.Command(SendToGroupHandler, CanSendToGroup);
            SendToLs = new Base.Command(SendToLsHandler, CanSendToLs);
            ToAdmin = new Base.Command(ToAdminHandler, CanToAdmin);
            RemoveAdmin = new Base.Command(RemoveAdminHandler, CanRemoveAdmin);
            InfoConcurs = new Base.Command(InfoConcursHandler, CanInfoConcurs);
            AddElemetToPoll = new Base.Command(AddElemetToPollHandler) ;
            RemoveElemetOfPoll = new Base.Command(RemoveElemetOfPollHandler,CanRemoveElemetOfPoll) ;
            SendPoll = new Base.Command(SendPollHandler,CanSendPoll) ;

            this.Constructor();
        }
        //ComandsHandler
        private async void SendToGroupHandler(object obj)
        {
            if (_selectedGroup!=null)
            {
                await RadmitTelegramBot.TBot.SendMSG(_sendText, _selectedGroup.ID_Chat);
            }
        }
        private async void SendToLsHandler(object obj)
        {
            if (_selectedChat != null)
            {
                await RadmitTelegramBot.TBot.SendMSG(_sendText, _selectedChat.ID_Chat);
            }
        }
        private async void RemoveAdminHandler(object obj)
        {
            if (_selectedAdmins != null)
            {
                await DataBase.Manager.RemoveAdmin(_selectedAdmins.TID);
            }
        }
        private async void ToAdminHandler(object obj)
        {
            if (_selectedUsers != null)
            {
                await DataBase.Manager.AddAdmin(_selectedUsers.TID,_selectedUsers.UserName, _adminLVL);
            }
        }
        private async void InfoConcursHandler(object obj)
        {
            if (_selectedCocncurs != null)
            {
                await Task.Run(() =>new ConcursOutput(_selectedCocncurs).Show());
            }
        }
        private void AddElemetToPollHandler(object obj)
        {
            _pollItems.Add(new PoolItems() { Items = "Text Poll" });
        }
        private void RemoveElemetOfPollHandler(object obj)
        {
            if (_pollSelectedItems != null)
            {
                PollItems.Remove(_pollSelectedItems);
            }
        }
        private async void SendPollHandler(object obj)
        {
            await RadmitTelegramBot.TBot.SendPool(_pollText, PoolItems.ToArray(_pollItems.ToArray()), _selectedGroup.ID_Chat,_isAnonimous);
        }
        //Check
        private bool CanSendToGroup(object obj) => _selectedGroup != null;
        private bool CanSendToLs(object obj) => _selectedChat != null;
        private bool CanToAdmin(object obj) => _selectedUsers != null;
        private bool CanRemoveAdmin(object obj) => _selectedAdmins != null;
        private bool CanInfoConcurs(object obj) => _selectedCocncurs != null;
        private bool CanRemoveElemetOfPoll(object obj) => _pollSelectedItems != null;
        private bool CanSendPoll(object obj) => _pollItems.Count>=2&&_pollText.Replace(" ","").Any()&&_selectedGroup!=null;


    }
     partial class MainWindowViewModel: Base.ViewModel
    {
        
        //Private Object
        DataBase.Admins _selectedAdmins;
        DataBase.User _selectedUsers;
        DataBase.ChatLS _selectedChat;
        DataBase.ChatGroup _selectedGroup;
        DataBase.ItemSuprise _selectedCocncurs;
        private int _adminLVL = 12;
        string _sendText;
        ObservableCollection<PoolItems> _pollItems;
        PoolItems _pollSelectedItems;
        string _pollText;
        bool _isAnonimous = true;
        private void Constructor()
        {
            _pollItems = new ObservableCollection<PoolItems>(); 
        }
        public PoolItems PollSelectedItems
        {
            get => _pollSelectedItems;
            set
            {
                if(value!= _pollSelectedItems)
                {
                    _pollSelectedItems = value;
                    OnPropertyChanged(nameof(PollSelectedItems));
                }
            }
        }
        public string PollText
        {
            get => _pollText;
            set
            {
                _pollText = value;
                OnPropertyChanged(nameof(PollText));
            }
        }
        public bool IsAnonimous
        {
            get => _isAnonimous;
            set
            {
                _isAnonimous = value;
                OnPropertyChanged(nameof(IsAnonimous));
            }
        }
        public ObservableCollection<PoolItems> PollItems
        {
            get => _pollItems;
            set
            {
                _pollItems = value;
                OnPropertyChanged(nameof(PollItems));
            }
        }
        public DataBase.Admins SelectedAdmins
        {
            get => _selectedAdmins;
            set
            {
                if(_selectedAdmins != value)
                {
                    _selectedAdmins = value;
                    OnPropertyChanged(nameof(SelectedAdmins));
                }
                    
            }
        }
        public DataBase.User SelectedUsers
        {
            get => _selectedUsers;
            set
            {
                if (_selectedUsers != value)
                {
                    _selectedUsers = value;
                    OnPropertyChanged(nameof(SelectedUsers));
                }

            }
        }
        public DataBase.ChatLS SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (_selectedChat != value)
                {
                    _selectedChat = value;
                    OnPropertyChanged(nameof(SelectedChat));
                }

            }
        }
        public DataBase.ChatGroup SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    OnPropertyChanged(nameof(SelectedGroup));
                }

            }
        }
        public DataBase.ItemSuprise SelectedCocncurs
        {
            get => _selectedCocncurs;
            set
            {
                if (_selectedCocncurs != value)
                {
                    _selectedCocncurs = value;
                    OnPropertyChanged(nameof(SelectedCocncurs));
                }

            }
        }
        public string SendText
        {
            get => _sendText;
            set
            {
                _sendText = value;
                OnPropertyChanged(nameof(SendText));
            }
        }

        public ObservableCollection<DataBase.Admins> AllAdmis => DataBase.AdminsContext.StaticItems;
        public ObservableCollection<DataBase.User> AllUsers => DataBase.UsersContext.StaticItems;
        public ObservableCollection<DataBase.ChatGroup> AllChats => DataBase.ChatGroupContext.StaticItems;
        public ObservableCollection<DataBase.ChatLS> AllLs => DataBase.LSContext.StaticItems;
        public ObservableCollection<DataBase.ItemSuprise> AllConcurs => DataBase.ItemsSurpriceContext.StaticItems;





    }
}

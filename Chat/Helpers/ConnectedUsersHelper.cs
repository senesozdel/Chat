using Chat.Entities;

namespace Chat.Helpers
{
    public static class ConnectedUsersHelper
    {
        private static User _currentReceiver;
        private static User _currentSender;

        public static void SetMessageUsers(User receiver, User sender)
        {
            _currentReceiver = receiver;
            _currentSender = sender;
        }

        public static (User receiver, User sender) GetMessageUsers()
        {
            return (_currentReceiver, _currentSender);
        }

        public static User GetReceiver() => _currentReceiver;
        public static User GetSender() => _currentSender;
    }
}

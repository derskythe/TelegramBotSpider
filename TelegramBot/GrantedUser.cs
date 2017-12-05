namespace TelegramBot
{
    class GrantedUser
    {
        public string Username { get; set; }
        public AccessType AccessType { get; set; }

        public GrantedUser()
        {
        }

        public GrantedUser(string username, AccessType accessType)
        {
            Username = username;
            AccessType = accessType;
        }

        public override string ToString()
        {
            return string.Format("Username: {0}, AccessType: {1}", Username, AccessType);
        }
    }
}

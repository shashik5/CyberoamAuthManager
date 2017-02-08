namespace CyberoamAuthManager
{
    class Program
    {
        static void Main(string[] args)
        {
            User user = UserDetails.GetUser(args.Length > 1 ? args[1] : string.Empty);
            if (args.Length > 0)
            {
                var operation = args[0];
                switch (operation)
                {
                    case "login":
                        AuthManager.Login(user.Username, user.Password);
                        break;
                    case "logout":
                        AuthManager.Logout(user.Username);
                        break;
                }
            }
            else
            {
                AuthManager.Login(user.Username, user.Password);
            }
        }
    }
}

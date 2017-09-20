namespace CyberoamAuthManager
{
    class Program
    {
        static void Main(string[] args)
        {
            User user = UserDetails.GetUser(args.Length > 1 ? args[1] : string.Empty);

            if (!string.IsNullOrEmpty(user.Username))
            {
                if (args.Length > 0)
                {
                    var operation = args[0];
                    switch (operation.ToLower())
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
            else
            {
                AuthManager.ShowNotification("User details not configured.");
            }
            
        }
    }
}

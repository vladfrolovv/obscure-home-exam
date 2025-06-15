using UnityEngine;
namespace OGServer.Utils
{
    public static class PlayerDataGenerator
    {

        private const string UsernameToken = "USERNAME";
        private const string PasswordToken = "PASSWORD";
        private const string Token = "TOKEN";

        public static string TryToCreateOrGetUsername()
        {
            if (PlayerPrefs.HasKey(UsernameToken))
            {
                return PlayerPrefs.GetString(UsernameToken);
            }

            string username = GenerateUsername();
            PlayerPrefs.SetString(UsernameToken, username);

            return username;
        }

        public static string TryToCreateOrGetPassword()
        {
            if (PlayerPrefs.HasKey(PasswordToken))
            {
                return PlayerPrefs.GetString(PasswordToken);
            }

            string password = GenerateString(Random.Range(8, 16));
            PlayerPrefs.SetString(PasswordToken, password);

            return password;
        }

        public static string TryToCreateOrGetToken()
        {
            if (PlayerPrefs.HasKey(Token))
            {
                return PlayerPrefs.GetString(Token);
            }

            string token = GenerateString(32);
            PlayerPrefs.SetString(Token, token);

            return token;
        }

        private static string GenerateUsername()
        {
            string username = $"GUEST{GenerateString(Random.Range(3, 6))}";
            PlayerPrefs.SetString(UsernameToken, username);

            return username;
        }

        private static string GenerateString(int length = 10)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string str = "";

            for (int i = 0; i < length; i++)
            {
                str += chars[Random.Range(0, chars.Length)];
            }

            return str;
        }

    }
}

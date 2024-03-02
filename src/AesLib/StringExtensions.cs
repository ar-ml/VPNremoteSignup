namespace ARaction.AesLib
{
    public static class StringExtensions
    {
        public static string Encrypt(this string source, string key)
        {
            return AesOperation.EncryptString(key, source);
        }

        public static string Decrypt(this string source, string key)
        {
            return AesOperation.DecryptString(key, source);
        }
    }
}

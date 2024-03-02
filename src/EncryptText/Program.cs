using ARaction.AesLib;
using System;

namespace ARaction.EncryptText
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var key = args[0];
            var text = args[1];

            Console.WriteLine($"Text: '{text}'");
            Console.WriteLine($"Encrypted: '{text.Encrypt(key)}'");
        }
    }
}

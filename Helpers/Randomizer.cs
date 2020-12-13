using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Helpers
{
    public static class Randomizer
    {
        private static Random random = new Random();
        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

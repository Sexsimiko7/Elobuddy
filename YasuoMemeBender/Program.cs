using System;
using EloBuddy.SDK.Events;

namespace YasuoMemeBender
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                new YasuoMemeBender();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to load Yasuo - The Memebender: " + exception);
            }
        }
    }
}

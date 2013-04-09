using System;

namespace Theremin
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ThereminGame game = new ThereminGame())
            {
                game.Run();
            }
        }
    }
#endif
}


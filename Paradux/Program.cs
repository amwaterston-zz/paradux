using System;

namespace Paradux
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (AppFramework game = new AppFramework())
            {
                game.Run();
            }
//             bool bEditMode = false;
//             string map = null;
//             foreach (string arg in args)
//             {
//                 if (arg == "edit")
//                 {
//                     bEditMode = true;
//                 }
//                 else
//                 {
//                     map = arg;
//                     if (!map.EndsWith(".dux"))
//                     {
//                         map += ".dux";
//                     }
//                 }
//             }
//             if (bEditMode)
//             {
//                 using (DuxEd game = new DuxEd(map))
//                 {
//                     game.Run();
//                 }
//             }
//             else
//             {
//                 using (Paradux game = new Paradux(map))
//                 {
//                     game.Run();
//                 }
//             }
        }
    }
}


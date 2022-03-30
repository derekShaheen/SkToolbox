using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkToolbox.Commands;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox.SkModules
{
    class ModTestMenu : SkBaseModule, IModule
    {
        public bool drawLines = false;
        internal Texture2D textureBlue;
        CMainObject cmo;

        public ModTestMenu() : base()
        {
            base.ModuleName = "Test";
            base.Loading();
            SkCommandProcessor.Instance.AddCommand(new CmdSetPoints());
            base.CallerEntry = new SkMenuItem("Test Menu\t►", () => base.SkMC.RequestSubMenu(base.FlushMenu()), "Test Menu");

        }

        public void Start()
        {
            cmo = FindObjectOfType<CMainObject>();
            textureBlue = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            textureBlue.SetPixel(0, 0, Color.white);
            textureBlue.SetPixel(1, 0, Color.blue);
            textureBlue.SetPixel(0, 1, Color.blue);
            textureBlue.SetPixel(1, 1, Color.blue);
            textureBlue.Apply();

            BeginMenu();
            base.Ready();
        }

        public void Update()
        {

        }

        public void BeginMenu()
        {
            SkMenu MenuOpt = new SkMenu();
            for(int x = 0; x < 35; x++)
            {
                MenuOpt.AddItemToggle("[" + x + "] Test1Menu" + x, ref drawLines, new Action<string>(Test), "Long time, lol. yup this is a long one for sure!");
                MenuOpt.AddItem("Reset Points", new Action(ResetPoints), "Reset Points");
                MenuOpt.AddItem("Add Generic", new Action(Test3));
            }
            MenuOptions = MenuOpt;
        }

        public void BeginMenu2()
        {
            SkMenu MenuOpt = new SkMenu();
            MenuOpt.AddItem("Test5Menu1", new Action<string>(Test), "Long time, lol. yup this is a long one for sure!");
            MenuOpt.AddItem("Test7Menu1", new Action(ResetPoints), "Long time, lol. yup this for sure!");
            MenuOpt.AddItem("Test4Menu2", new Action(Test3), "Long timer sure!");
            MenuOptions = MenuOpt;
        }

        public void Test(string ln)
        {
            SkUtilities.Logz("TEST1: " + ln);
            drawLines = !drawLines;
            BeginMenu();
            SkMC.RequestSubMenu(base.FlushMenu()); // When this option is selected in the menu, display the submenu again.

        }

        public void ResetPoints()
        {
            SkUtilities.Logz("Points: " + cmo.points + " ->  0");
            cmo.points = 0;
        }

        public void Test3()
        {
            //SkModuleController.Instance.AddModule<SkModules.ModGeneric>();
            SkUtilities.Logz("Added");
        }

        public void OnGUI()
        {
            if (drawLines)
            {
                DrawLine();
            }
        }

        public void DrawLine()
        {
            Circle.Draw(500, 500, 50f, Color.green);
            Text.Draw(new Rect(10f, 10f, 500f, 500f), "TEST", Color.yellow);
            Box.Draw(50f, 50f, 50f, 50f, textureBlue);
        }

        class CmdSetPoints : Commands.SkCommand
        {
            public override string Command => "SetPoints";

            public override string Description => "[points] - Set the points shown on screen";

            public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

            public override bool Enabled => true;

            public override void Execute(string[] args)
            {
                int points = 0;
                if(args.Length > 0 && int.TryParse(args[0], out points))
                {
                    CMainObject cmo;
                    cmo = FindObjectOfType<CMainObject>();
                    if (cmo != null)
                    {
                        cmo.points = int.Parse(args[0]);
                        Utility.SkUtilities.Logz(new string[] { "SetPoints" }, new string[] { "Set to: " + cmo.points });
                        return;
                    }
                }
                Utility.SkUtilities.Logz(new string[] { "SetPoints", "ERR" }, new string[] { "Could not set points." });
            }
        }
    }
}

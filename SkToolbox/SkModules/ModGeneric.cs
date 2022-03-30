using SkToolbox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkToolbox.SkModules
{
    class ModGeneric : SkBaseModule, IModule
    {

        public ModGeneric() : base()
        {
            base.ModuleName = "Generic";
            base.Loading();
        }

        public void Start()
        {
            BeginMenu();
            base.Ready(); // Must be called when the module has completed initialization. // End of Start
            base.CallerEntry = new SkMenuItem("Generic Menu\t►", () => base.SkMC.RequestSubMenu(base.FlushMenu()), "Empty Menu with a long context tip");

        }

        public void BeginMenu()
        {
            SkMenu GenericMenu = new SkMenu();
                GenericMenu.AddItem("Timescale\t►", new Action(BeginMenu));
                GenericMenu.AddItem("Gravity\t►", new Action(BeginMenu));
            MenuOptions = GenericMenu;
        }

        //public void BeginTimescaleMenu()
        //{
        //    SkMenu GenericMenu = new SkMenu();
        //    GenericMenu.AddItem("0.5", new Action(UnloadMenu));
        //    GenericMenu.AddItem("0.75", new Action(UnloadMenu));
        //    GenericMenu.AddItem("1.0", new Action(toggleWriteFile));
        //    GenericMenu.AddItem("1.5", new Action(OpenLogFolder));
        //    GenericMenu.AddItem("2", new Action(ReloadMenu));
        //    base.RequestMenu(GenericMenu);
        //}
        ////public void BeginGravityMenu()
        //{
        //    SkMenu GenericMenu = new SkMenu();
        //    GenericMenu.AddItem("Timescale >", new Action(toggleWriteFile), "Write log output to file?");
        //    GenericMenu.AddItem("Open Log Folder", new Action(OpenLogFolder));
        //    GenericMenu.AddItem("Reload Menu", new Action(ReloadMenu));
        //    GenericMenu.AddItem("Unload Toolbox", new Action(UnloadMenu));
        //    base.RequestMenu(GenericMenu);
        //}


    }
}

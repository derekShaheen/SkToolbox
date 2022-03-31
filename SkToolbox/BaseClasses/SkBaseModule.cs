using System.Collections.Generic;
using UnityEngine;
using SkToolbox.Utility;

namespace SkToolbox.SkModules
{
    /// <summary>
    /// All modules must inerhit from this base class. Within those modules, base.Ready() must be called when the module is ready for use, and this must happen within 3 frames of module initialization.
    /// </summary>
    public class SkBaseModule : MonoBehaviour
    {
        internal SkMenuController SkMC;
        public SkMenu MenuOptions { get; set; } = new SkMenu();
        public SkMenuItem CallerEntry { get; set; } = new SkMenuItem();
        public SkUtilities.Status ModuleStatus { get; set; } = SkUtilities.Status.Initialized;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if(IsEnabled && ModuleStatus != SkUtilities.Status.Ready) // If the module is ready, then it is loaded and running.
                {   // To disable, set the status to "Unload" so it properly unloads.
                    isEnabled = value;
                }
                if(!IsEnabled)
                {
                    IsEnabled = value;
                }
            }
        }

        public string ModuleName = "UNNAMED";

        private bool isEnabled = true;

        public SkBaseModule()
        {
            //Loading();
            //

            //Ready();
        }


        public void Awake()
        {
            SkMC = GetComponent<SkMenuController>();
        }

        public List<SkMenuItem> FlushMenu()
        {
          return MenuOptions.FlushMenu();
        }

        public void RequestMenu()
        {
            SkMC.RequestSubMenu(MenuOptions.FlushMenu());
        }

        public void RequestMenu(SkMenu Menu)
        {
            SkMC.RequestSubMenu(Menu);
        }

        public void RemoveModule()
        {

            Destroy(this);
        }

        public void Ready()
        {
            ModuleStatus = SkUtilities.Status.Ready;
        }
        public void Loading()
        {
            ModuleStatus = SkUtilities.Status.Loading;
        }
        public void Error()
        {
            ModuleStatus = SkUtilities.Status.Error;
        }
        public void Unload()
        {
            ModuleStatus = SkUtilities.Status.Unload;
        }
    }
}

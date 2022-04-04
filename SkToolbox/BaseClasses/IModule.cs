using SkToolbox.Utility;
using System.Collections.Generic;

namespace SkToolbox.SkModules
{
    public interface IModule
    {
        void BeginMenu();
        void Start();

        void RequestMenu();

        void RequestMenu(SkMenu Menu);

        List<SkMenuItem> FlushMenu();

        void Ready();

        void Loading();

        void Error();

        void Unload();

        string ModuleName
        {
            get; set;
        }

        bool IsEnabled
        {
            get; set;
        }

        SkUtilities.Status ModuleStatus
        {
            get; set;
        }

        SkMenu MenuOptions
        {
            get; set;
        }

        SkMenuItem CallerEntry
        {
            get; set;
        }
    }
}
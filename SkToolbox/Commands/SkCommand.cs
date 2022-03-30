using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkToolbox.Commands
{
    public abstract class SkCommand
    {
        /// <summary>
        ///     The command that the user will type to run the command.
        /// </summary>
        public abstract string Command { get; }

        /// <summary>
        ///     Is this command enabled? 
        /// </summary>
        public abstract bool Enabled { get; }

        /// <summary>
        ///     The help text or description text associated with this command.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        ///     Should this command be shown when 'help' is typed? How about when 'help cmdname' is typed, specifically searching for this command?
        /// </summary>
        public abstract SkCommandEnum.VisiblityFlag VisibilityFlag { get; }

        /// <summary>
        ///     The function that will be called when the command is executed, with coma-delimited arguments.
        /// </summary>
        /// <param name="args">The arguments the user types, with spaces being the delimiter.</param>
        public abstract void Execute(string[] args);
    }

    public class SkCommandEnum
    {
        public enum VisiblityFlag
        {
            Visible = 0,    //Visible
            Hidden = 1,     //Hidden from 'help'
            FullHidden = 2  //Hidden from 'help cmdname' and suggestions
        };
    }
}

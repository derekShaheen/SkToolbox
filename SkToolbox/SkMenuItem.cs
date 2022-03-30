using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkToolbox
{
    public class SkMenuItem
    {
        private string itemText;
        private Action itemClass;
        private Action<string> itemClassStr;
        private string itemTip;

        public SkMenuItem()
        {

        }

        public SkMenuItem(string itemText, Action itemClass, string itemTip = "")
        {
            ItemText = itemText; // Text to show in the menu for this item
            ItemClass = itemClass; // Method to call when chosen
            ItemTip = itemTip; // Text to show in the tooltip when this item is selected
        }

        public SkMenuItem(string itemText, Action<string> itemClassStr, string itemTip = "")
        {
            ItemText = itemText; // Text to show in the menu for this item
            ItemClassStr = itemClassStr; // Method to call when chosen
            ItemTip = itemTip; // Text to show in the tooltip when this item is selected
        }
        /// <summary>
        /// This text will be displayed in the menu
        /// </summary>
        public string ItemText { get => itemText; set => itemText = value; }

        /// <summary>
        /// Class to call upon selection
        /// </summary>
        public Action ItemClass { get => itemClass; set => itemClass = value; }

        /// <summary>
        /// Contextual tooltip text
        /// </summary>
        public string ItemTip { get => itemTip; set => itemTip = value; }
        public Action<string> ItemClassStr { get => itemClassStr; set => itemClassStr = value; }

        /// <summary>
        /// Compares whether the menu items contain the same property values. <strong>ItemTip is not evaluated.</strong>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if menu items contain same values</returns>
        public bool Compare(object obj)
        {
            if (obj == null) return false;

            if (obj is SkMenuItem othSkMenuItem)
            {
                return this.ItemText.Equals(othSkMenuItem?.ItemText) && this.ItemClass.Equals(othSkMenuItem?.ItemClass);
            }
            return false;
        }
    }
}

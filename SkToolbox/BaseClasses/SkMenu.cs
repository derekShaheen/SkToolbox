using System;
using System.Collections.Generic;
using System.Linq;

namespace SkToolbox
{
     public class SkMenu
    {
        private List<SkMenuItem> listItems = new List<SkMenuItem>();
        private readonly string toggleStrOn = "[ON]";
        private readonly string toggleStrOff = "[OFF]";

        /// <summary>
        /// Designed to create the menu items that will be passed to the menu controller
        /// </summary>
         public SkMenu()
        {

        }

        /// <summary>
        /// Add an item to the current menu build.
        /// </summary>
        /// <param name="inText">Display text for this menu item</param>
        /// <param name="outMethod">Method to call if item selected</param>
        /// <param name="inTip">Tip to show when item is highlighted</param>
        public void AddItem(string inText, Action outMethod, string inTip = null) 
                            => listItems.Add(new SkMenuItem(inText, outMethod, inTip));
        public void AddItem(string inText, Action<string> outMethod, string inTip = null)
                            => listItems.Add(new SkMenuItem(inText, outMethod, inTip));
        public void AddItem(SkMenuItem menuItem) 
                            => listItems.Add(menuItem);


        /// <summary>
        /// Add an item with a toggle variable to the current menu build.
        /// </summary>
        /// <param name="inText">Display text for this menu item</param>
        /// <param name="inToggleVar">Bool to check</param>
        /// <param name="outMethod">Method to call if item selected</param>
        /// <param name="inTip">Tip to show when item is highlighted</param>
        public void AddItemToggle(string inText, ref bool inToggleVar, Action outMethod, string inTip = null) 
            => listItems.Add(new SkMenuItem(inText + " " + (inToggleVar ? toggleStrOn : toggleStrOff), outMethod, inTip));
        public void AddItemToggle(string inText, ref bool inToggleVar, Action<string> outMethod, string inTip = null)
                            => listItems.Add(new SkMenuItem(inText + " " + (inToggleVar ? toggleStrOn : toggleStrOff), outMethod, inTip));

        /// <summary>
        /// Removes items from the list. If multiple items match, each will be removed.
        /// </summary>
        /// <param name="menuItem"></param>
        /// <returns>Number of items removed, or -1 if error</returns>
        public int RemoveItem(SkMenuItem menuItem)
        {
            int numItems = 0;
            try
            {
                foreach (SkMenuItem sMi in listItems)
                {
                    if (sMi.Compare(menuItem))
                    {
                        listItems.Remove(menuItem);
                        numItems++;
                    }
                }

                return numItems;
            } catch(Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns list ready to pass to the menu controller. <strong>This also clears the list after generating the menu array.</strong>
        /// </summary>
        /// <param name="disableClear">Set True to keep the menu in memory after execution</param>
        /// <returns>String array ready to pass into the menu controller</returns>
        public List<SkMenuItem> FlushMenu()
        {
            //var rtnList = listItems.ToList();

            //if (!disableClear) listItems.Clear();
            return listItems;
            //return rtnList;
        }

        /// <summary>
        /// Clears the list of current menu items
        /// </summary>
        public void ClearItems() => listItems.Clear();

        /// <summary>
        /// Returns the number of menu items currently in the list
        /// </summary>
        /// <returns>number of menu items currently in the list</returns>
        public int Count() => listItems.Count();

    }
}

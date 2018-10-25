using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Pokemod
{
    public class ModEntry : Mod
    {
        int trigger = 0;

        /*********
        ** Public methods
        *********/
        public override void Entry(IModHelper helper)
        {
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;

            // Change Marnie's purchase stock
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;
        }

        /*********
        ** Private methods
        *********/
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            this.Monitor.Log("Welcome to Pokémod!");
            Game1.activeClickableMenu = (IClickableMenu)new MyPurchaseAnimalsMenu(getMyAnimalStock(), this.Helper);
        }

        // Change Marnie's purchase stock
        public void MenuEvents_MenuChanged(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is PurchaseAnimalsMenu animalsMenu && trigger == 0)
            {
                this.Monitor.Log("Changing Marnie's stock.");
                Game1.activeClickableMenu = (IClickableMenu)new MyPurchaseAnimalsMenu(getMyAnimalStock(), this.Helper);
                trigger = 1;

                // TODO für neue animals -> PurchaseAnimalsMenu überschreiben, sodass receiveLeftClick statt
                // this.animalBeingPurchased = new FarmAnimal(textureComponent.hoverText, Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
                // ... new MyFarmAnimal aufruft (und eigene Strings in getAnimalDescription  / getAnimaTitle aufruft)
                // -> MyFarmAnimal als Kopie von FarmAnimal schreiben, sodass im Konstruktor und weiteren Verlauf eigene animals eingebaut werden können
            }
            else
                trigger = 0;            
        }

        private List<StardewValley.Object> getMyAnimalStock()
        {
            List<StardewValley.Object> objectList = new List<StardewValley.Object>();

            StardewValley.Object object1 = new StardewValley.Object(100, 1, false, 12, 0);
            object1.Name = "My Chicken";
            object1.Type = null;
            object1.displayName = "Mein Chicken";
            objectList.Add(object1);

            return objectList;
        }
    }
}

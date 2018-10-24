using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pokemod
{
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        public override void Entry(IModHelper helper)
        {
            TimeEvents.AfterDayStarted += this.TimeEvents_AfterDayStarted;
        }

        /*********
        ** Private methods
        *********/
        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            if(!Context.IsWorldReady)
                return;
                        
            this.Monitor.Log("Welcome to Pokémod!");            

            
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Menus;
using xTile.Dimensions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;


namespace Pokemod
{
    class MyPurchaseAnimalsMenu : IClickableMenu
    {        
        public static int menuHeight = 320;
        public static int menuWidth = 448;
        public List<ClickableTextureComponent> animalsToPurchase = new List<ClickableTextureComponent>();
        public const int region_okButton = 101;
        public const int region_doneNamingButton = 102;
        public const int region_randomButton = 103;
        public const int region_namingBox = 104;
        public ClickableTextureComponent okButton;
        public ClickableTextureComponent doneNamingButton;
        public ClickableTextureComponent randomButton;
        public ClickableTextureComponent hovered;
        public ClickableComponent textBoxCC;

        // mine
        private IModHelper helper;

        private bool onFarm;
        private bool namingAnimal;
        private bool freeze;
        private MyFarmAnimal animalBeingPurchased;
        private TextBox textBox;
        private TextBoxEvent e;
        private Building newAnimalHome;
        private int priceOfAnimal;

        // mine
        public MyPurchaseAnimalsMenu(List<StardewValley.Object> stock, IModHelper helper)
          : base(Game1.viewport.Width / 2 - MyPurchaseAnimalsMenu.menuWidth / 2 - IClickableMenu.borderWidth * 2, Game1.viewport.Height / 2 - MyPurchaseAnimalsMenu.menuHeight - IClickableMenu.borderWidth * 2, MyPurchaseAnimalsMenu.menuWidth + IClickableMenu.borderWidth * 2, MyPurchaseAnimalsMenu.menuHeight + IClickableMenu.borderWidth, false)
        {

            // mine
            this.helper = helper;
            
            this.height += 64;

            // Iterate through the stock and create the list of ClickableTextureComponents animalsToPurchase
            for (int index = 0; index < stock.Count; ++index)
            {
                List<ClickableTextureComponent> animalsToPurchase = this.animalsToPurchase;

                ClickableTextureComponent textureComponent = new ClickableTextureComponent(string.Concat((object)stock[index].salePrice()), 
                    new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth + index % 3 * 64 * 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + index / 3 * 85, 128, 64), 
                    (string)null, 
                    stock[index].Name, 
                    // Texture file to load
                    Game1.mouseCursors, 
                    /* Source rectangle of the texture file to load
                     * 
                     *   The base game's code assumes that animals in the purchase menu are always in the same order,
                     *   and finds the appropriate texture by index
                     *
                     *   --> TODO: change this to check for the stock[index].Name and load the appropriate texture
                    */
                    new Microsoft.Xna.Framework.Rectangle(index % 3 * 16 * 2, 448 + index / 3 * 16, 32, 16), 
                    4f, 
                    stock[index].Type == null);

                textureComponent.item = (Item)stock[index];
                textureComponent.myID = index;
                textureComponent.rightNeighborID = index % 3 == 2 ? -1 : index + 1;
                textureComponent.leftNeighborID = index % 3 == 0 ? -1 : index - 1;
                textureComponent.downNeighborID = index + 3;
                textureComponent.upNeighborID = index - 3;
                animalsToPurchase.Add(textureComponent);
            }

            // OK Button
            ClickableTextureComponent textureComponent1 = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false);
            textureComponent1.myID = 101;
            textureComponent1.upNeighborID = 103;
            textureComponent1.leftNeighborID = 103;
            this.okButton = textureComponent1;

            // Random Button
            ClickableTextureComponent textureComponent2 = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width + 51 + 64, Game1.viewport.Height / 2, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f, false);
            textureComponent2.myID = 103;
            textureComponent2.downNeighborID = 101;
            textureComponent2.rightNeighborID = 101;
            this.randomButton = textureComponent2;

            MyPurchaseAnimalsMenu.menuHeight = 320;
            MyPurchaseAnimalsMenu.menuWidth = 448;
            this.textBox = new TextBox((Texture2D)null, (Texture2D)null, Game1.dialogueFont, Game1.textColor);
            this.textBox.X = Game1.viewport.Width / 2 - 192;
            this.textBox.Y = Game1.viewport.Height / 2;
            this.textBox.Width = 256;
            this.textBox.Height = 192;
            this.e = new TextBoxEvent(this.textBoxEnter);
            this.textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X, this.textBox.Y, 192, 48), "")
            {
                myID = 104,
                rightNeighborID = 102,
                downNeighborID = 101
            };

            ClickableTextureComponent textureComponent3 = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X + this.textBox.Width + 64 + 48 - 8, Game1.viewport.Height / 2 + 4, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f, false);
            textureComponent3.myID = 103;
            textureComponent3.leftNeighborID = 102;
            textureComponent3.downNeighborID = 101;
            textureComponent3.rightNeighborID = 101;
            this.randomButton = textureComponent3;

            ClickableTextureComponent textureComponent4 = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X + this.textBox.Width + 32 + 4, Game1.viewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
            textureComponent4.myID = 102;
            textureComponent4.rightNeighborID = 103;
            textureComponent4.leftNeighborID = 104;
            textureComponent4.downNeighborID = 101;
            this.doneNamingButton = textureComponent4;

            if (!Game1.options.SnappyMenus)
                return;
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public void textBoxEnter(TextBox sender)
        {
            if (!this.namingAnimal)
                return;
            if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is MyPurchaseAnimalsMenu))
            {
                this.textBox.OnEnterPressed -= this.e;
            }
            else
            {
                if (sender.Text.Length < 1)
                    return;
                if (Utility.areThereAnyOtherAnimalsWithThisName(sender.Text))
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MyPurchaseAnimalsMenu.cs.11308"));
                }
                else
                {
                    this.textBox.OnEnterPressed -= this.e;
                    this.animalBeingPurchased.Name = sender.Text;
                    this.animalBeingPurchased.displayName = sender.Text;
                    this.animalBeingPurchased.home = this.newAnimalHome;
                    this.animalBeingPurchased.homeLocation.Value = new Vector2((float)(int)((NetFieldBase<int, NetInt>)this.newAnimalHome.tileX), (float)(int)((NetFieldBase<int, NetInt>)this.newAnimalHome.tileY));
                    this.animalBeingPurchased.setRandomPosition((GameLocation)((NetFieldBase<GameLocation, NetRef<GameLocation>>)this.animalBeingPurchased.home.indoors));
                    // TODO: MyAnimalHouse
                    //(this.newAnimalHome.indoors.Value as AnimalHouse).animals.Add((long)((NetFieldBase<long, NetLong>)this.animalBeingPurchased.myID), this.animalBeingPurchased);
                    (this.newAnimalHome.indoors.Value as AnimalHouse).animalsThatLiveHere.Add((long)((NetFieldBase<long, NetLong>)this.animalBeingPurchased.myID));
                    this.newAnimalHome = (Building)null;
                    this.namingAnimal = false;
                    Game1.player.money -= this.priceOfAnimal;
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForReturnAfterPurchasingAnimal), 0.02f);
                }
            }
        }

        public void setUpForReturnAfterPurchasingAnimal()
        {
            LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop", false);
            locationRequest.OnWarp += (LocationRequest.Callback)(() =>
            {
                this.onFarm = false;
                this.okButton.bounds.X = this.xPositionOnScreen + this.width + 4;
                Game1.displayHUD = true;
                Game1.displayFarmer = true;
                this.freeze = false;
                this.textBox.OnEnterPressed -= this.e;
                this.textBox.Selected = false;
                Game1.viewportFreeze = false;
                this.marnieAnimalPurchaseMessage();
            });
            Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), (int)((NetFieldBase<int, NetInt>)Game1.player.facingDirection));
        }

        public void marnieAnimalPurchaseMessage()
        {
            this.exitThisMenu(true);
            Game1.player.forceCanMove();
            this.freeze = false;
            Game1.drawDialogue(Game1.getCharacterFromName("Marnie", false), this.animalBeingPurchased.isMale() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311", (object)this.animalBeingPurchased.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", (object)this.animalBeingPurchased.displayName));
        }

        public void setUpForAnimalPlacement()
        {
            Game1.displayFarmer = false;
            Game1.currentLocation = Game1.getLocationFromName("Farm");
            Game1.currentLocation.resetForPlayerEntry();
            Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f);
            this.onFarm = true;
            this.freeze = false;
            this.okButton.bounds.X = Game1.viewport.Width - 128;
            this.okButton.bounds.Y = Game1.viewport.Height - 128;
            Game1.displayHUD = false;
            Game1.viewportFreeze = true;
            Game1.viewport.Location = new Location(3136, 320);
            Game1.panScreen(0, 0);
        }

        public void setUpForReturnToShopMenu()
        {
            this.freeze = false;
            Game1.displayFarmer = true;
            LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop", false);
            locationRequest.OnWarp += (LocationRequest.Callback)(() =>
            {
                Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f);
                this.onFarm = false;
                this.okButton.bounds.X = this.xPositionOnScreen + this.width + 4;
                this.okButton.bounds.Y = this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth;
                Game1.displayHUD = true;
                Game1.viewportFreeze = false;
                this.namingAnimal = false;
                this.textBox.OnEnterPressed -= this.e;
                this.textBox.Selected = false;
                if (!Game1.options.SnappyMenus)
                    return;
                this.snapToDefaultClickableComponent();
            });
            Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), (int)((NetFieldBase<int, NetInt>)Game1.player.facingDirection));
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Game1.globalFade || this.freeze)
                return;
            if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
            {
                if (this.onFarm)
                {
                    Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForReturnToShopMenu), 0.02f);
                    Game1.playSound("smallSelect");
                }
                else
                {
                    Game1.exitActiveMenu();
                    Game1.playSound("bigDeSelect");
                }
            }
            if (this.onFarm)
            {
                Building buildingAt = (Game1.getLocationFromName("Farm") as Farm).getBuildingAt(new Vector2((float)((x + Game1.viewport.X) / 64), (float)((y + Game1.viewport.Y) / 64)));
                if (buildingAt != null && !this.namingAnimal)
                {
                    if (buildingAt.buildingType.Value.Contains(this.animalBeingPurchased.buildingTypeILiveIn.Value))
                    {
                        if ((buildingAt.indoors.Value as AnimalHouse).isFull())
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11321"));
                        else if ((int)(byte)((NetFieldBase<byte, NetByte>)this.animalBeingPurchased.harvestType) != 2)
                        {
                            this.namingAnimal = true;
                            this.newAnimalHome = buildingAt;
                            if (this.animalBeingPurchased.sound.Value != null && Game1.soundBank != null)
                            {
                                Cue cue = Game1.soundBank.GetCue(this.animalBeingPurchased.sound.Value);
                                cue.SetVariable("Pitch", (float)(1200 + Game1.random.Next(-200, 201)));
                                cue.Play();
                            }
                            this.textBox.OnEnterPressed += this.e;
                            this.textBox.Text = this.animalBeingPurchased.displayName;
                            Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)this.textBox;
                            if (Game1.options.SnappyMenus)
                            {
                                this.currentlySnappedComponent = this.getComponentWithID(104);
                                this.snapCursorToCurrentSnappedComponent();
                            }
                        }
                        else if (Game1.player.money >= this.priceOfAnimal)
                        {
                            this.newAnimalHome = buildingAt;
                            this.animalBeingPurchased.home = this.newAnimalHome;
                            this.animalBeingPurchased.homeLocation.Value = new Vector2((float)(int)((NetFieldBase<int, NetInt>)this.newAnimalHome.tileX), (float)(int)((NetFieldBase<int, NetInt>)this.newAnimalHome.tileY));
                            this.animalBeingPurchased.setRandomPosition((GameLocation)((NetFieldBase<GameLocation, NetRef<GameLocation>>)this.animalBeingPurchased.home.indoors));
                            // TODO: MyAnimalHouse
                            //(this.newAnimalHome.indoors.Value as AnimalHouse).animals.Add((long)((NetFieldBase<long, NetLong>)this.animalBeingPurchased.myID), this.animalBeingPurchased);
                            (this.newAnimalHome.indoors.Value as AnimalHouse).animalsThatLiveHere.Add((long)((NetFieldBase<long, NetLong>)this.animalBeingPurchased.myID));
                            this.newAnimalHome = (Building)null;
                            this.namingAnimal = false;
                            if (this.animalBeingPurchased.sound.Value != null && Game1.soundBank != null)
                            {
                                Cue cue = Game1.soundBank.GetCue(this.animalBeingPurchased.sound.Value);
                                cue.SetVariable("Pitch", (float)(1200 + Game1.random.Next(-200, 201)));
                                cue.Play();
                            }
                            Game1.player.money -= this.priceOfAnimal;
                            Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11324", (object)this.animalBeingPurchased.displayType), Color.LimeGreen, 3500f));
                            //this.animalBeingPurchased = new FarmAnimal((string)((NetFieldBase<string, NetString>)this.animalBeingPurchased.type), Game1.multiplayer.getNewID(), (long)((NetFieldBase<long, NetLong>)Game1.player.uniqueMultiplayerID));

                            // TODO: Replace with new MyFarmAnimal
                            this.animalBeingPurchased = new MyFarmAnimal((string)((NetFieldBase<string, NetString>)this.animalBeingPurchased.type), helper.Multiplayer.GetNewID(), (long)((NetFieldBase<long, NetLong>)Game1.player.uniqueMultiplayerID));




                        }
                        else if (Game1.player.money < this.priceOfAnimal)
                            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                    }
                    else
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11326", (object)this.animalBeingPurchased.displayType));
                }
                if (!this.namingAnimal)
                    return;
                if (this.doneNamingButton.containsPoint(x, y))
                {
                    this.textBoxEnter(this.textBox);
                    Game1.playSound("smallSelect");
                }
                else if (this.namingAnimal && this.randomButton.containsPoint(x, y))
                {
                    this.animalBeingPurchased.Name = Dialogue.randomName();
                    this.animalBeingPurchased.displayName = this.animalBeingPurchased.Name;
                    this.textBox.Text = this.animalBeingPurchased.displayName;
                    this.randomButton.scale = this.randomButton.baseScale;
                    Game1.playSound("drumkit6");
                }
                this.textBox.Update();
            }
            else
            {
                foreach (ClickableTextureComponent textureComponent in this.animalsToPurchase)
                {
                    if (textureComponent.containsPoint(x, y) && (textureComponent.item as StardewValley.Object).Type == null)
                    {
                        int num = textureComponent.item.salePrice();
                        if (Game1.player.money >= num)
                        {
                            Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForAnimalPlacement), 0.02f);
                            Game1.playSound("smallSelect");
                            this.onFarm = true;
                            //this.animalBeingPurchased = new FarmAnimal(textureComponent.hoverText, Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);

                            // TODO: Replace with new MyFarmAnimal
                            this.animalBeingPurchased = new MyFarmAnimal(textureComponent.hoverText, helper.Multiplayer.GetNewID(), Game1.player.UniqueMultiplayerID);
                            this.priceOfAnimal = num;
                        }
                        else
                            Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"), Color.Red, 3500f));
                    }
                }
            }
        }

        public override bool overrideSnappyMenuCursorMovementBan()
        {
            if (this.onFarm)
                return !this.namingAnimal;
            return false;
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b != Buttons.B || Game1.globalFade || (!this.onFarm || !this.namingAnimal))
                return;
            Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForReturnToShopMenu), 0.02f);
            Game1.playSound("smallSelect");
        }

        public override void receiveKeyPress(Keys key)
        {
            if (Game1.globalFade || this.freeze)
                return;
            if (!Game1.globalFade && this.onFarm)
            {
                if (!this.namingAnimal)
                {
                    if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
                    {
                        Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForReturnToShopMenu), 0.02f);
                    }
                    else
                    {
                        if (Game1.options.SnappyMenus)
                            return;
                        if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                            Game1.panScreen(0, 4);
                        else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                            Game1.panScreen(4, 0);
                        else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
                        {
                            Game1.panScreen(0, -4);
                        }
                        else
                        {
                            if (!Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                                return;
                            Game1.panScreen(-4, 0);
                        }
                    }
                }
                else
                {
                    if (!Game1.options.SnappyMenus)
                        return;
                    if (!this.textBox.Selected && Game1.options.doesInputListContain(Game1.options.menuButton, key))
                    {
                        Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForReturnToShopMenu), 0.02f);
                        Game1.playSound("smallSelect");
                    }
                    else
                    {
                        if (this.textBox.Selected && Game1.options.doesInputListContain(Game1.options.menuButton, key))
                            return;
                        base.receiveKeyPress(key);
                    }
                }
            }
            else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.globalFade)
            {
                if (!this.readyToClose())
                    return;
                Game1.player.forceCanMove();
                Game1.exitActiveMenu();
                Game1.playSound("bigDeSelect");
            }
            else
            {
                if (!Game1.options.SnappyMenus)
                    return;
                base.receiveKeyPress(key);
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (!this.onFarm || this.namingAnimal)
                return;
            int num1 = Game1.getOldMouseX() + Game1.viewport.X;
            int num2 = Game1.getOldMouseY() + Game1.viewport.Y;
            if (num1 - Game1.viewport.X < 64)
                Game1.panScreen(-8, 0);
            else if (num1 - (Game1.viewport.X + Game1.viewport.Width) >= -64)
                Game1.panScreen(8, 0);
            if (num2 - Game1.viewport.Y < 64)
                Game1.panScreen(0, -8);
            else if (num2 - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
                Game1.panScreen(0, 8);
            foreach (Keys pressedKey in Game1.oldKBState.GetPressedKeys())
                this.receiveKeyPress(pressedKey);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
            this.hovered = (ClickableTextureComponent)null;
            if (Game1.globalFade || this.freeze)
                return;
            if (this.okButton != null)
            {
                if (this.okButton.containsPoint(x, y))
                    this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
                else
                    this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
            }
            if (this.onFarm)
            {
                if (!this.namingAnimal)
                {
                    Vector2 tile = new Vector2((float)((x + Game1.viewport.X) / 64), (float)((y + Game1.viewport.Y) / 64));
                    Farm locationFromName = Game1.getLocationFromName("Farm") as Farm;
                    foreach (Building building in locationFromName.buildings)
                        building.color.Value = Color.White;
                    Building buildingAt = locationFromName.getBuildingAt(tile);
                    if (buildingAt != null)
                    {
                        if (buildingAt.buildingType.Value.Contains(this.animalBeingPurchased.buildingTypeILiveIn.Value) && !(buildingAt.indoors.Value as AnimalHouse).isFull())
                            buildingAt.color.Value = Color.LightGreen * 0.8f;
                        else
                            buildingAt.color.Value = Color.Red * 0.8f;
                    }
                }
                if (this.doneNamingButton != null)
                {
                    if (this.doneNamingButton.containsPoint(x, y))
                        this.doneNamingButton.scale = Math.Min(1.1f, this.doneNamingButton.scale + 0.05f);
                    else
                        this.doneNamingButton.scale = Math.Max(1f, this.doneNamingButton.scale - 0.05f);
                }
                this.randomButton.tryHover(x, y, 0.5f);
            }
            else
            {
                foreach (ClickableTextureComponent textureComponent in this.animalsToPurchase)
                {
                    if (textureComponent.containsPoint(x, y))
                    {
                        textureComponent.scale = Math.Min(textureComponent.scale + 0.05f, 4.1f);
                        this.hovered = textureComponent;
                    }
                    else
                        textureComponent.scale = Math.Max(4f, textureComponent.scale - 0.025f);
                }
            }
        }

        public static string getAnimalTitle(string name)
        {
            switch (name)
            {
                case "Chicken":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5922");
                case "Dairy Cow":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927");
                case "Duck":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5937");
                case "Goat":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5933");
                case "Pig":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5948");
                case "Rabbit":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5945");
                case "Sheep":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5942");
                case "My Chicken":
                    return "My Chicken";
                default:
                    return "Nothing";
            }
        }

        public static string getAnimalDescription(string name)
        {
            switch (name)
            {
                case "Chicken":                    
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
                case "Dairy Cow":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
                case "Duck":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11337") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
                case "Goat":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11349") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
                case "Pig":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11346") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
                case "Rabbit":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11340") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
                case "Sheep":
                    return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11352") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
                case "My Chicken":
                    return "This looks like a regular chicken but something is a little off...";
                default:
                    return "Nothing";
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!this.onFarm && !Game1.dialogueUp && !Game1.globalFade)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11354"), this.xPositionOnScreen + 96, this.yPositionOnScreen, "", 1f, -1);
                Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, (string)null, false);
                Game1.dayTimeMoneyBox.drawMoneyBox(b, -1, -1);
                foreach (ClickableTextureComponent textureComponent in this.animalsToPurchase)
                    textureComponent.draw(b, (textureComponent.item as StardewValley.Object).Type != null ? Color.Black * 0.4f : Color.White, 0.87f);
            }
            else if (!Game1.globalFade && this.onFarm)
            {
                string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11355", (object)this.animalBeingPurchased.displayHouse, (object)this.animalBeingPurchased.displayType);
                SpriteText.drawStringWithScrollBackground(b, s, Game1.viewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16, "", 1f, -1);
                if (this.namingAnimal)
                {
                    b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                    Game1.drawDialogueBox(Game1.viewport.Width / 2 - 256, Game1.viewport.Height / 2 - 192 - 32, 512, 192, false, true, (string)null, false);
                    Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11357"), Game1.dialogueFont, new Vector2((float)(Game1.viewport.Width / 2 - 256 + 32 + 8), (float)(Game1.viewport.Height / 2 - 128 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
                    this.textBox.Draw(b, true);
                    this.doneNamingButton.draw(b);
                    this.randomButton.draw(b);
                }
            }
            if (!Game1.globalFade && this.okButton != null)
                this.okButton.draw(b);
            if (this.hovered != null)
            {
                if ((this.hovered.item as StardewValley.Object).Type != null)
                {
                    IClickableMenu.drawHoverText(b, Game1.parseText((this.hovered.item as StardewValley.Object).Type, Game1.dialogueFont, 320), Game1.dialogueFont, 0, 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
                }
                else
                {
                    string animalTitle = MyPurchaseAnimalsMenu.getAnimalTitle(this.hovered.hoverText);
                    SpriteText.drawStringWithScrollBackground(b, animalTitle, this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 64, this.yPositionOnScreen + this.height - 32 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "Truffle Pig", 1f, -1);
                    SpriteText.drawStringWithScrollBackground(b, "$" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", (object)this.hovered.item.salePrice()), this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128, this.yPositionOnScreen + this.height + 64 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "$99999999g", Game1.player.Money >= this.hovered.item.salePrice() ? 1f : 0.5f, -1);
                    string animalDescription = MyPurchaseAnimalsMenu.getAnimalDescription(this.hovered.hoverText);
                    IClickableMenu.drawHoverText(b, Game1.parseText(animalDescription, Game1.smallFont, 320), Game1.smallFont, 0, 0, -1, animalTitle, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
                }
            }
            Game1.mouseCursorTransparency = 1f;
            this.drawMouse(b);
        }
    }
}

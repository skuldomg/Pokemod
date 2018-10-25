using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System.Xml.Serialization;
using StardewValley;

namespace Pokemod
{
    class MyFarmAnimal : Character
    {
        [XmlElement("defaultProduceIndex")]
        public readonly NetInt defaultProduceIndex = new NetInt();
        [XmlElement("deluxeProduceIndex")]
        public readonly NetInt deluxeProduceIndex = new NetInt();
        [XmlElement("currentProduce")]
        public readonly NetInt currentProduce = new NetInt();
        [XmlElement("friendshipTowardFarmer")]
        public readonly NetInt friendshipTowardFarmer = new NetInt();
        [XmlElement("daysSinceLastFed")]
        public readonly NetInt daysSinceLastFed = new NetInt();
        public int uniqueFrameAccumulator = -1;
        [XmlElement("age")]
        public readonly NetInt age = new NetInt();
        [XmlElement("meatIndex")]
        public readonly NetInt meatIndex = new NetInt();
        [XmlElement("health")]
        public readonly NetInt health = new NetInt();
        [XmlElement("price")]
        public readonly NetInt price = new NetInt();
        [XmlElement("produceQuality")]
        public readonly NetInt produceQuality = new NetInt();
        [XmlElement("daysToLay")]
        public readonly NetByte daysToLay = new NetByte();
        [XmlElement("daysSinceLastLay")]
        public readonly NetByte daysSinceLastLay = new NetByte();
        [XmlElement("ageWhenMature")]
        public readonly NetByte ageWhenMature = new NetByte();
        [XmlElement("harvestType")]
        public readonly NetByte harvestType = new NetByte();
        [XmlElement("happiness")]
        public readonly NetByte happiness = new NetByte();
        [XmlElement("fullness")]
        public readonly NetByte fullness = new NetByte();
        [XmlElement("happinessDrain")]
        public readonly NetByte happinessDrain = new NetByte();
        [XmlElement("fullnessDrain")]
        public readonly NetByte fullnessDrain = new NetByte();
        [XmlElement("wasPet")]
        public readonly NetBool wasPet = new NetBool();
        [XmlElement("showDifferentTextureWhenReadyForHarvest")]
        public readonly NetBool showDifferentTextureWhenReadyForHarvest = new NetBool();
        [XmlElement("allowReproduction")]
        public readonly NetBool allowReproduction = new NetBool(true);
        [XmlElement("sound")]
        public readonly NetString sound = new NetString();
        [XmlElement("type")]
        public readonly NetString type = new NetString();
        [XmlElement("buildingTypeILiveIn")]
        public readonly NetString buildingTypeILiveIn = new NetString();
        [XmlElement("toolUsedForHarvest")]
        public readonly NetString toolUsedForHarvest = new NetString();
        [XmlElement("frontBackBoundingBox")]
        public readonly NetRectangle frontBackBoundingBox = new NetRectangle();
        [XmlElement("sidewaysBoundingBox")]
        public readonly NetRectangle sidewaysBoundingBox = new NetRectangle();
        [XmlElement("frontBackSourceRect")]
        public readonly NetRectangle frontBackSourceRect = new NetRectangle();
        [XmlElement("sidewaysSourceRect")]
        public readonly NetRectangle sidewaysSourceRect = new NetRectangle();
        [XmlElement("myID")]
        public readonly NetLong myID = new NetLong();
        [XmlElement("ownerID")]
        public readonly NetLong ownerID = new NetLong();
        [XmlElement("parentId")]
        public readonly NetLong parentId = new NetLong(-1L);
        [XmlIgnore]
        private readonly NetBuildingRef netHome = new NetBuildingRef();
        [XmlElement("homeLocation")]
        public readonly NetVector2 homeLocation = new NetVector2();
        [XmlElement("moodMessage")]
        public readonly NetInt moodMessage = new NetInt();
        [XmlElement("isEating")]
        private readonly NetBool isEating = new NetBool();
        [XmlIgnore]
        private readonly NetEvent1Field<int, NetInt> doFarmerPushEvent = new NetEvent1Field<int, NetInt>();
        public const byte eatGrassBehavior = 0;
        public const short newHome = 0;
        public const short happy = 1;
        public const short neutral = 2;
        public const short unhappy = 3;
        public const short hungry = 4;
        public const short disturbedByDog = 5;
        public const short leftOutAtNight = 6;
        public const int hitsTillDead = 3;
        public const double chancePerUpdateToChangeDirection = 0.007;
        public const byte fullnessValueOfGrass = 60;
        public const int noWarpTimerTime = 3000;
        public new const double chanceForSound = 0.002;
        public const double chanceToGoOutside = 0.002;
        public const int uniqueDownFrame = 16;
        public const int uniqueRightFrame = 18;
        public const int uniqueUpFrame = 20;
        public const int uniqueLeftFrame = 22;
        public const int pushAccumulatorTimeTillPush = 40;
        public const int timePerUniqueFrame = 500;
        public const byte layHarvestType = 0;
        public const byte grabHarvestType = 1;
        public int pushAccumulator;
        [XmlIgnore]
        public int noWarpTimer;
        [XmlIgnore]
        public int hitGlowTimer;
        [XmlIgnore]
        public int pauseTimer;
        private string _displayHouse;
        private string _displayType;

        [XmlIgnore]
        public Building home
        {
            get
            {
                return this.netHome.Value;
            }
            set
            {
                this.netHome.Value = value;
            }
        }

        [XmlIgnore]
        public string displayHouse
        {
            get
            {
                if (this._displayHouse == null)
                {
                    string str;
                    Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals").TryGetValue((string)((NetFieldBase<string, NetString>)this.type), out str);
                    this._displayHouse = (string)((NetFieldBase<string, NetString>)this.buildingTypeILiveIn);
                    if (str != null && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
                        this._displayHouse = str.Split('/')[26];
                }
                return this._displayHouse;
            }
            set
            {
                this._displayHouse = value;
            }
        }

        [XmlIgnore]
        public string displayType
        {
            get
            {
                if (this._displayType == null)
                {
                    string str;
                    Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals").TryGetValue((string)((NetFieldBase<string, NetString>)this.type), out str);
                    if (str != null)
                        this._displayType = str.Split('/')[25];
                }
                return this._displayType;
            }
            set
            {
                this._displayType = value;
            }
        }

        public MyFarmAnimal()
        {
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields((INetSerializable)this.defaultProduceIndex, (INetSerializable)this.deluxeProduceIndex, (INetSerializable)this.currentProduce, (INetSerializable)this.friendshipTowardFarmer, (INetSerializable)this.daysSinceLastFed, (INetSerializable)this.age, (INetSerializable)this.meatIndex, (INetSerializable)this.health, (INetSerializable)this.price, (INetSerializable)this.produceQuality, (INetSerializable)this.daysToLay, (INetSerializable)this.daysSinceLastLay, (INetSerializable)this.ageWhenMature, (INetSerializable)this.harvestType, (INetSerializable)this.happiness, (INetSerializable)this.fullness, (INetSerializable)this.happinessDrain, (INetSerializable)this.fullnessDrain, (INetSerializable)this.wasPet, (INetSerializable)this.showDifferentTextureWhenReadyForHarvest, (INetSerializable)this.allowReproduction, (INetSerializable)this.sound, (INetSerializable)this.type, (INetSerializable)this.buildingTypeILiveIn, (INetSerializable)this.toolUsedForHarvest, (INetSerializable)this.frontBackBoundingBox, (INetSerializable)this.sidewaysBoundingBox, (INetSerializable)this.frontBackSourceRect, (INetSerializable)this.sidewaysSourceRect, (INetSerializable)this.myID, (INetSerializable)this.ownerID, (INetSerializable)this.parentId, (INetSerializable)this.netHome.NetFields, (INetSerializable)this.homeLocation, (INetSerializable)this.moodMessage, (INetSerializable)this.isEating, (INetSerializable)this.doFarmerPushEvent);
            this.position.Field.AxisAlignedMovement = true;
            this.doFarmerPushEvent.onEvent += new AbstractNetEvent1<int>.Event(this.doFarmerPush);
        }

        public MyFarmAnimal(string type, long id, long ownerID)
          : base((AnimatedSprite)null, new Vector2((float)(64 * Game1.random.Next(2, 9)), (float)(64 * Game1.random.Next(4, 8))), 2, type)
        {
            this.ownerID.Value = ownerID;
            this.health.Value = 3;
            if (type.Contains("Chicken") && !type.Equals("Void Chicken"))
            {
                type = Game1.random.NextDouble() < 0.5 || type.Contains("Brown") ? "Brown Chicken" : "White Chicken";
                if (Game1.player.eventsSeen.Contains(3900074) && Game1.random.NextDouble() < 0.25)
                    type = "Blue Chicken";
            }
            if (type.Contains("Cow"))
                type = type.Contains("White") || Game1.random.NextDouble() >= 0.5 && !type.Contains("Brown") ? "White Cow" : "Brown Cow";
            Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals");
            this.myID.Value = id;
            string key = type;
            string str;
            dictionary.TryGetValue(key, out str);
            if (str != null)
            {
                string[] strArray = str.Split('/');
                this.daysToLay.Value = Convert.ToByte(strArray[0]);
                this.ageWhenMature.Value = Convert.ToByte(strArray[1]);
                this.defaultProduceIndex.Value = Convert.ToInt32(strArray[2]);
                this.deluxeProduceIndex.Value = Convert.ToInt32(strArray[3]);
                this.sound.Value = strArray[4].Equals("none") ? (string)null : strArray[4];
                this.frontBackBoundingBox.Value = new Microsoft.Xna.Framework.Rectangle(Convert.ToInt32(strArray[5]), Convert.ToInt32(strArray[6]), Convert.ToInt32(strArray[7]), Convert.ToInt32(strArray[8]));
                this.sidewaysBoundingBox.Value = new Microsoft.Xna.Framework.Rectangle(Convert.ToInt32(strArray[9]), Convert.ToInt32(strArray[10]), Convert.ToInt32(strArray[11]), Convert.ToInt32(strArray[12]));
                this.harvestType.Value = Convert.ToByte(strArray[13]);
                this.showDifferentTextureWhenReadyForHarvest.Value = Convert.ToBoolean(strArray[14]);
                this.buildingTypeILiveIn.Value = strArray[15];
                int int32 = Convert.ToInt32(strArray[16]);
                this.Sprite = new AnimatedSprite("Animals\\" + ((int)(byte)((NetFieldBase<byte, NetByte>)this.ageWhenMature) > 0 ? "Baby" : "") + (type.Equals("Duck") ? "White Chicken" : type), 0, int32, Convert.ToInt32(strArray[17]));
                this.frontBackSourceRect.Value = new Microsoft.Xna.Framework.Rectangle(0, 0, Convert.ToInt32(strArray[16]), Convert.ToInt32(strArray[17]));
                this.sidewaysSourceRect.Value = new Microsoft.Xna.Framework.Rectangle(0, 0, Convert.ToInt32(strArray[18]), Convert.ToInt32(strArray[19]));
                this.fullnessDrain.Value = Convert.ToByte(strArray[20]);
                this.happinessDrain.Value = Convert.ToByte(strArray[21]);
                this.happiness.Value = byte.MaxValue;
                this.fullness.Value = byte.MaxValue;
                this.toolUsedForHarvest.Value = strArray[22].Length > 0 ? strArray[22] : "";
                this.meatIndex.Value = Convert.ToInt32(strArray[23]);
                this.price.Value = Convert.ToInt32(strArray[24]);
            }
            this.type.Value = type;
            this.Name = Dialogue.randomName();
            this.displayName = (string)((NetFieldBase<string, NetString>)this.name);
        }

        public string shortDisplayType()
        {
            switch (LocalizedContentManager.CurrentLanguageCode)
            {
                case LocalizedContentManager.LanguageCode.en:
                    return ((IEnumerable<string>)this.displayType.Split(' ')).Last<string>();
                case LocalizedContentManager.LanguageCode.ja:
                    if (this.displayType.Contains("トリ"))
                        return "トリ";
                    if (this.displayType.Contains("ウシ"))
                        return "ウシ";
                    if (!this.displayType.Contains("ブタ"))
                        return this.displayType;
                    return "ブタ";
                case LocalizedContentManager.LanguageCode.ru:
                    if (this.displayType.ToLower().Contains("курица"))
                        return "Курица";
                    if (!this.displayType.ToLower().Contains("корова"))
                        return this.displayType;
                    return "Корова";
                case LocalizedContentManager.LanguageCode.zh:
                    if (this.displayType.Contains("鸡"))
                        return "鸡";
                    if (this.displayType.Contains("牛"))
                        return "牛";
                    if (!this.displayType.Contains("猪"))
                        return this.displayType;
                    return "猪";
                case LocalizedContentManager.LanguageCode.pt:
                case LocalizedContentManager.LanguageCode.es:
                    return ((IEnumerable<string>)this.displayType.Split(' ')).First<string>();
                case LocalizedContentManager.LanguageCode.de:
                    return ((IEnumerable<string>)((IEnumerable<string>)this.displayType.Split(' ')).Last<string>().Split('-')).Last<string>();
                default:
                    return this.displayType;
            }
        }

        public bool isCoopDweller()
        {
            if (this.home != null)
                return this.home is Coop;
            return false;
        }

        public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
        {
            return new Microsoft.Xna.Framework.Rectangle((int)((double)this.Position.X + (double)(this.Sprite.getWidth() * 4 / 2) - 32.0 + 8.0), (int)((double)this.Position.Y + (double)(this.Sprite.getHeight() * 4) - 64.0 + 8.0), 48, 48);
        }

        public void reload(Building home)
        {
            string str = (string)((NetFieldBase<string, NetString>)this.type);
            if ((int)((NetFieldBase<int, NetInt>)this.age) < (int)(byte)((NetFieldBase<byte, NetByte>)this.ageWhenMature))
                str = "Baby" + (this.type.Value.Equals("Duck") ? "White Chicken" : this.type.Value);
            else if ((bool)((NetFieldBase<bool, NetBool>)this.showDifferentTextureWhenReadyForHarvest) && (int)((NetFieldBase<int, NetInt>)this.currentProduce) <= 0)
                str = "Sheared" + this.type.Value;
            this.Sprite = new AnimatedSprite("Animals\\" + str, 0, this.frontBackSourceRect.Width, this.frontBackSourceRect.Height);
            this.home = home;
        }

        public void pet(Farmer who)
        {
            if (who.FarmerSprite.PauseForSingleAnimation)
                return;
            who.Halt();
            who.faceGeneralDirection(this.Position, 0, false);
            if (Game1.timeOfDay >= 1900 && !this.isMoving())
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\FarmAnimals:TryingToSleep", (object)this.displayName));
            }
            else
            {
                this.Halt();
                this.Sprite.StopAnimation();
                this.uniqueFrameAccumulator = -1;
                switch (Game1.player.FacingDirection)
                {
                    case 0:
                        this.Sprite.currentFrame = 0;
                        break;
                    case 1:
                        this.Sprite.currentFrame = 12;
                        break;
                    case 2:
                        this.Sprite.currentFrame = 8;
                        break;
                    case 3:
                        this.Sprite.currentFrame = 4;
                        break;
                }
                if (!(bool)((NetFieldBase<bool, NetBool>)this.wasPet))
                {
                    this.wasPet.Value = true;
                    this.friendshipTowardFarmer.Value = Math.Min(1000, (int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) + 15);
                    if (who.professions.Contains(3) && !this.isCoopDweller() || who.professions.Contains(2) && this.isCoopDweller())
                    {
                        this.friendshipTowardFarmer.Value = Math.Min(1000, (int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) + 15);
                        this.happiness.Value = (byte)Math.Min((int)byte.MaxValue, (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) + Math.Max(5, 40 - (int)(byte)((NetFieldBase<byte, NetByte>)this.happinessDrain)));
                    }
                    this.doEmote((int)((NetFieldBase<int, NetInt>)this.moodMessage) == 4 ? 12 : 20, true);
                    this.happiness.Value = (byte)Math.Min((int)byte.MaxValue, (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) + Math.Max(5, 40 - (int)(byte)((NetFieldBase<byte, NetByte>)this.happinessDrain)));
                    this.makeSound();
                    who.gainExperience(0, 5);
                }
                else if (who.ActiveObject == null || (int)((NetFieldBase<int, NetInt>)who.ActiveObject.parentSheetIndex) != 178)
                    // TODO: MyAnimalQueryMenu
                    // TODO: Game1.activeClickableMenu = (IClickableMenu)new AnimalQueryMenu(this);
                    return;
                if (!this.type.Value.Equals("Sheep") || (int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) < 900)
                    return;
                this.daysToLay.Value = (byte)2;
            }
        }

        public void farmerPushing()
        {
            ++this.pushAccumulator;
            if (this.pushAccumulator <= 40)
                return;
            this.doFarmerPushEvent.Fire(Game1.player.FacingDirection);
            Game1.player.temporaryImpassableTile = this.GetBoundingBox();
            this.pushAccumulator = 0;
        }

        private void doFarmerPush(int direction)
        {
            if (!Game1.IsMasterGame)
                return;
            switch (direction)
            {
                case 0:
                    this.Halt();
                    this.SetMovingUp(true);
                    break;
                case 1:
                    this.Halt();
                    this.SetMovingRight(true);
                    break;
                case 2:
                    this.Halt();
                    this.SetMovingDown(true);
                    break;
                case 3:
                    this.Halt();
                    this.SetMovingLeft(true);
                    break;
            }
        }

        public void setRandomPosition(GameLocation location)
        {
            string[] strArray = location.getMapProperty("ProduceArea").Split(' ');
            int int32_1 = Convert.ToInt32(strArray[0]);
            int int32_2 = Convert.ToInt32(strArray[1]);
            int int32_3 = Convert.ToInt32(strArray[2]);
            int int32_4 = Convert.ToInt32(strArray[3]);
            this.Position = new Vector2((float)(Game1.random.Next(int32_1, int32_1 + int32_3) * 64), (float)(Game1.random.Next(int32_2, int32_2 + int32_4) * 64));
            int num = 0;
            while (this.Position.Equals(Vector2.Zero) || location.Objects.ContainsKey(this.Position) || location.isCollidingPosition(this.GetBoundingBox(), Game1.viewport, false, 0, false, (Character)this))
            {
                this.Position = new Vector2((float)Game1.random.Next(int32_1, int32_1 + int32_3), (float)Game1.random.Next(int32_2, int32_2 + int32_4)) * 64f;
                ++num;
                if (num > 64)
                    break;
            }
        }

        public void dayUpdate(GameLocation environtment)
        {
            this.controller = (PathFindController)null;
            this.health.Value = 3;
            bool flag1 = false;
            if (this.home != null && !(this.home.indoors.Value as AnimalHouse).animals.ContainsKey((long)((NetFieldBase<long, NetLong>)this.myID)) && environtment is Farm)
            {
                if (!(bool)((NetFieldBase<bool, NetBool>)this.home.animalDoorOpen))
                {
                    this.moodMessage.Value = 6;
                    flag1 = true;
                    this.happiness.Value /= (byte)2;
                }
                else
                {
                    // TODO: MyAnimalHouse
                    //(environtment as Farm).animals.Remove((long)((NetFieldBase<long, NetLong>)this.myID));
                    //(this.home.indoors.Value as AnimalHouse).animals.Add((long)((NetFieldBase<long, NetLong>)this.myID), this); 
                    if (Game1.timeOfDay > 1800 && this.controller == null)
                        this.happiness.Value /= (byte)2;
                    environtment = (GameLocation)((NetFieldBase<GameLocation, NetRef<GameLocation>>)this.home.indoors);
                    this.setRandomPosition(environtment);
                    return;
                }
            }
            ++this.daysSinceLastLay.Value;
            if (!(bool)((NetFieldBase<bool, NetBool>)this.wasPet))
            {
                this.friendshipTowardFarmer.Value = Math.Max(0, (int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) - (10 - (int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) / 200));
                this.happiness.Value = (byte)Math.Max(0, (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) - (int)(byte)((NetFieldBase<byte, NetByte>)this.happinessDrain) * 5);
            }
            this.wasPet.Value = false;
            if (((int)(byte)((NetFieldBase<byte, NetByte>)this.fullness) < 200 || Game1.timeOfDay < 1700) && environtment is AnimalHouse)
            {
                for (int index = environtment.objects.Count() - 1; index >= 0; --index)
                {
                    KeyValuePair<Vector2, StardewValley.Object> keyValuePair = environtment.objects.Pairs.ElementAt<KeyValuePair<Vector2, StardewValley.Object>>(index);
                    if (keyValuePair.Value.Name.Equals("Hay"))
                    {
                        OverlaidDictionary<Vector2, StardewValley.Object> objects = environtment.objects;
                        keyValuePair = environtment.objects.Pairs.ElementAt<KeyValuePair<Vector2, StardewValley.Object>>(index);
                        Vector2 key = keyValuePair.Key;
                        objects.Remove(key);
                        this.fullness.Value = byte.MaxValue;
                        break;
                    }
                }
            }
            Random random = new Random((int)(long)((NetFieldBase<long, NetLong>)this.myID) / 2 + (int)Game1.stats.DaysPlayed);
            if ((int)(byte)((NetFieldBase<byte, NetByte>)this.fullness) > 200 || random.NextDouble() < (double)((int)(byte)((NetFieldBase<byte, NetByte>)this.fullness) - 30) / 170.0)
            {
                ++this.age.Value;
                if ((int)((NetFieldBase<int, NetInt>)this.age) == (int)(byte)((NetFieldBase<byte, NetByte>)this.ageWhenMature))
                {
                    this.Sprite.LoadTexture("Animals\\" + this.type.Value);
                    if (this.type.Value.Contains("Sheep"))
                        this.currentProduce.Value = (int)((NetFieldBase<int, NetInt>)this.defaultProduceIndex);
                    this.daysSinceLastLay.Value = (byte)99;
                }
                this.happiness.Value = (byte)Math.Min((int)byte.MaxValue, (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) + (int)(byte)((NetFieldBase<byte, NetByte>)this.happinessDrain) * 2);
            }
            if ((int)this.fullness.Value < 200)
            {
                this.happiness.Value = (byte)Math.Max(0, (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) - 100);
                this.friendshipTowardFarmer.Value = Math.Max(0, (int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) - 20);
            }
            bool flag2 = (int)(byte)((NetFieldBase<byte, NetByte>)this.daysSinceLastLay) >= (int)(byte)((NetFieldBase<byte, NetByte>)this.daysToLay) - (!this.type.Value.Equals("Sheep") || !Game1.getFarmer((long)((NetFieldBase<long, NetLong>)this.ownerID)).professions.Contains(3) ? 0 : 1) && random.NextDouble() < (double)(byte)((NetFieldBase<byte, NetByte>)this.fullness) / 200.0 && random.NextDouble() < (double)(byte)((NetFieldBase<byte, NetByte>)this.happiness) / 70.0;
            int parentSheetIndex;
            if (!flag2 || (int)((NetFieldBase<int, NetInt>)this.age) < (int)(byte)((NetFieldBase<byte, NetByte>)this.ageWhenMature))
            {
                parentSheetIndex = -1;
            }
            else
            {
                parentSheetIndex = (int)((NetFieldBase<int, NetInt>)this.defaultProduceIndex);
                if (random.NextDouble() < (double)(byte)((NetFieldBase<byte, NetByte>)this.happiness) / 150.0)
                {
                    float num1 = (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) > 200 ? (float)(byte)((NetFieldBase<byte, NetByte>)this.happiness) * 1.5f : ((int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) <= 100 ? (float)((int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) - 100) : 0.0f);
                    if (this.type.Value.Equals("Duck") && random.NextDouble() < ((double)(int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) + (double)num1) / 5000.0 + Game1.dailyLuck + (double)Game1.player.LuckLevel * 0.01)
                        parentSheetIndex = (int)((NetFieldBase<int, NetInt>)this.deluxeProduceIndex);
                    else if (this.type.Value.Equals("Rabbit") && random.NextDouble() < ((double)(int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) + (double)num1) / 5000.0 + Game1.dailyLuck + (double)Game1.player.LuckLevel * 0.02)
                        parentSheetIndex = (int)((NetFieldBase<int, NetInt>)this.deluxeProduceIndex);
                    this.daysSinceLastLay.Value = (byte)0;
                    switch (parentSheetIndex)
                    {
                        case 176:
                            ++Game1.stats.ChickenEggsLayed;
                            break;
                        case 180:
                            ++Game1.stats.ChickenEggsLayed;
                            break;
                        case 440:
                            ++Game1.stats.RabbitWoolProduced;
                            break;
                        case 442:
                            ++Game1.stats.DuckEggsLayed;
                            break;
                    }
                    if (random.NextDouble() < ((double)(int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) + (double)num1) / 1200.0 && !this.type.Value.Equals("Duck") && (!this.type.Value.Equals("Rabbit") && (int)((NetFieldBase<int, NetInt>)this.deluxeProduceIndex) != -1) && (int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) >= 200)
                        parentSheetIndex = (int)((NetFieldBase<int, NetInt>)this.deluxeProduceIndex);
                    double num2 = (double)(int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) / 1000.0 - (1.0 - (double)(byte)((NetFieldBase<byte, NetByte>)this.happiness) / 225.0);
                    if (!this.isCoopDweller() && Game1.getFarmer((long)((NetFieldBase<long, NetLong>)this.ownerID)).professions.Contains(3) || this.isCoopDweller() && Game1.getFarmer((long)((NetFieldBase<long, NetLong>)this.ownerID)).professions.Contains(2))
                        num2 += 0.33;
                    if (num2 >= 0.95 && random.NextDouble() < num2 / 2.0)
                        this.produceQuality.Value = 4;
                    else if (random.NextDouble() < num2 / 2.0)
                        this.produceQuality.Value = 2;
                    else if (random.NextDouble() < num2)
                        this.produceQuality.Value = 1;
                    else
                        this.produceQuality.Value = 0;
                }
            }
            if ((int)(byte)((NetFieldBase<byte, NetByte>)this.harvestType) == 1 & flag2)
            {
                this.currentProduce.Value = parentSheetIndex;
                parentSheetIndex = -1;
            }
            if (parentSheetIndex != -1 && this.home != null && !this.home.indoors.Value.Objects.ContainsKey(this.getTileLocation()))
                this.home.indoors.Value.Objects.Add(this.getTileLocation(), new StardewValley.Object(Vector2.Zero, parentSheetIndex, (string)null, false, true, false, true)
                {
                    Quality = (int)((NetFieldBase<int, NetInt>)this.produceQuality)
                });
            if (!flag1)
            {
                if ((int)(byte)((NetFieldBase<byte, NetByte>)this.fullness) < 30)
                    this.moodMessage.Value = 4;
                else if ((int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) < 30)
                    this.moodMessage.Value = 3;
                else if ((int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) < 200)
                    this.moodMessage.Value = 2;
                else
                    this.moodMessage.Value = 1;
            }
            if (Game1.timeOfDay < 1700)
                this.fullness.Value = (byte)Math.Max(0, (int)(byte)((NetFieldBase<byte, NetByte>)this.fullness) - (int)(byte)((NetFieldBase<byte, NetByte>)this.fullnessDrain) * (1700 - Game1.timeOfDay) / 100);
            this.fullness.Value = (byte)0;
            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
                this.fullness.Value = (byte)250;
            this.reload(this.home);
        }

        public int getSellPrice()
        {
            return (int)((double)(int)((NetFieldBase<int, NetInt>)this.price) * ((double)(int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) / 1000.0 + 0.3));
        }

        public bool isMale()
        {
            string str = this.type.Value;
            if (str == "Rabbit")
                return (long)((NetFieldBase<long, NetLong>)this.myID) % 2L == 0L;
            if (str == "Truffle Pig" || str == "Hog" || str == "Pig")
                return (long)((NetFieldBase<long, NetLong>)this.myID) % 2L == 0L;
            return false;
        }

        public string getMoodMessage()
        {
            if ((int)(byte)((NetFieldBase<byte, NetByte>)this.harvestType) == 2)
                this.Name = "It";
            string str = this.isMale() ? "Male" : "Female";
            switch (this.moodMessage.Value)
            {
                case 0:
                    if ((long)((NetFieldBase<long, NetLong>)this.parentId) != -1L)
                        return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_NewHome_Baby_" + str, (object)this.displayName);
                    return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_NewHome_Adult_" + str + "_" + (object)(Game1.dayOfMonth % 2 + 1), (object)this.displayName);
                case 4:
                    return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_" + (((long)Game1.dayOfMonth + (long)((NetFieldBase<long, NetLong>)this.myID)) % 2L == 0L ? "Hungry1" : "Hungry2"), (object)this.displayName);
                case 5:
                    return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_DisturbedByDog_" + str, (object)this.displayName);
                case 6:
                    return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_LeftOutsideAtNight_" + str, (object)this.displayName);
                default:
                    if ((int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) < 30)
                        this.moodMessage.Value = 3;
                    else if ((int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) < 200)
                        this.moodMessage.Value = 2;
                    else
                        this.moodMessage.Value = 1;
                    switch ((int)((NetFieldBase<int, NetInt>)this.moodMessage))
                    {
                        case 1:
                            return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Happy", (object)this.displayName);
                        case 2:
                            return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Fine", (object)this.displayName);
                        case 3:
                            return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Sad", (object)this.displayName);
                        default:
                            return "";
                    }
            }
        }

        public bool isBaby()
        {
            return (int)((NetFieldBase<int, NetInt>)this.age) < (int)(byte)((NetFieldBase<byte, NetByte>)this.ageWhenMature);
        }

        public void warpHome(Farm f, MyFarmAnimal a)
        {
            if (this.home == null)
                return;
            // TODO: MyAnimalHouse
            //(this.home.indoors.Value as AnimalHouse).animals.Add((long)((NetFieldBase<long, NetLong>)this.myID), this);
            f.animals.Remove((long)((NetFieldBase<long, NetLong>)this.myID));
            this.controller = (PathFindController)null;
            this.setRandomPosition((GameLocation)((NetFieldBase<GameLocation, NetRef<GameLocation>>)this.home.indoors));
            ++this.home.currentOccupants.Value;
        }

        public override void draw(SpriteBatch b)
        {
            if (this.isCoopDweller())
                this.Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, this.Position - new Vector2(0.0f, 24f)), this.isBaby() ? 3f : 4f);
            this.Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.Position - new Vector2(0.0f, 24f)), (float)(((double)(this.GetBoundingBox().Center.Y + 4) + (double)this.Position.X / 1000.0) / 10000.0), 0, 0, this.hitGlowTimer > 0 ? Color.Red : Color.White, this.FacingDirection == 3, 4f, 0.0f, false);
            if (!this.isEmoting)
                return;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, this.Position + new Vector2((float)(this.frontBackSourceRect.Width / 2 * 4 - 32), this.isCoopDweller() ? -96f : -64f));
            b.Draw(Game1.emoteSpriteSheet, local, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.GetBoundingBox().Bottom / 10000f);
        }

        public virtual void updateWhenNotCurrentLocation(Building currentBuilding, GameTime time, GameLocation environment)
        {
            this.doFarmerPushEvent.Poll();
            if (!Game1.shouldTimePass())
                return;
            this.update(time, environment, (long)((NetFieldBase<long, NetLong>)this.myID), false);
            if (!Game1.IsMasterGame)
                return;
            if (currentBuilding != null && Game1.random.NextDouble() < 0.002 && ((bool)((NetFieldBase<bool, NetBool>)currentBuilding.animalDoorOpen) && Game1.timeOfDay < 1630) && (!Game1.isRaining && !Game1.currentSeason.Equals("winter") && environment.getFarmersCount() == 0))
            {
                // TODO: MyFarm
                Farm locationFromName = (Farm)Game1.getLocationFromName("Farm");
                if (locationFromName.isCollidingPosition(new Microsoft.Xna.Framework.Rectangle(((int)((NetFieldBase<int, NetInt>)currentBuilding.tileX) + currentBuilding.animalDoor.X) * 64 + 2, ((int)((NetFieldBase<int, NetInt>)currentBuilding.tileY) + currentBuilding.animalDoor.Y) * 64 + 2, (this.isCoopDweller() ? 64 : 128) - 4, 60), Game1.viewport, false, 0, false, (Character)this, false, false, false) || locationFromName.isCollidingPosition(new Microsoft.Xna.Framework.Rectangle(((int)((NetFieldBase<int, NetInt>)currentBuilding.tileX) + currentBuilding.animalDoor.X) * 64 + 2, ((int)((NetFieldBase<int, NetInt>)currentBuilding.tileY) + currentBuilding.animalDoor.Y + 1) * 64 + 2, (this.isCoopDweller() ? 64 : 128) - 4, 60), Game1.viewport, false, 0, false, (Character)this, false, false, false))
                    return;
                if (locationFromName.animals.ContainsKey((long)((NetFieldBase<long, NetLong>)this.myID)))
                {
                    for (int index = locationFromName.animals.Count() - 1; index >= 0; --index)
                    {
                        /*if (locationFromName.animals.Pairs.ElementAt<KeyValuePair<long, MyFarmAnimal>>(index).Key.Equals((long)((NetFieldBase<long, NetLong>)this.myID)))
                        {
                            locationFromName.animals.Remove((long)((NetFieldBase<long, NetLong>)this.myID));
                            break;
                        }*/
                    }
                }
              (currentBuilding.indoors.Value as AnimalHouse).animals.Remove((long)((NetFieldBase<long, NetLong>)this.myID));
                // TODO: MyFarm
                //locationFromName.animals.Add((long)((NetFieldBase<long, NetLong>)this.myID), this);
                this.faceDirection(2);
                this.SetMovingDown(true);
                this.Position = new Vector2((float)currentBuilding.getRectForAnimalDoor().X, (float)(((int)((NetFieldBase<int, NetInt>)currentBuilding.tileY) + currentBuilding.animalDoor.Y) * 64 - (this.Sprite.getHeight() * 4 - this.GetBoundingBox().Height) + 32));
                this.controller = new PathFindController((Character)this, (GameLocation)locationFromName, new PathFindController.isAtEnd(MyFarmAnimal.grassEndPointFunction), Game1.random.Next(4), false, new PathFindController.endBehavior(MyFarmAnimal.behaviorAfterFindingGrassPatch), 200, Point.Zero);
                if (this.controller == null || this.controller.pathToEndPoint == null || this.controller.pathToEndPoint.Count < 3)
                {
                    this.SetMovingDown(true);
                    this.controller = (PathFindController)null;
                }
                else
                {
                    this.faceDirection(2);
                    this.Position = new Vector2((float)(this.controller.pathToEndPoint.Peek().X * 64), (float)(this.controller.pathToEndPoint.Peek().Y * 64 - (this.Sprite.getHeight() * 4 - this.GetBoundingBox().Height) + 16));
                    if (!this.isCoopDweller())
                        this.position.X -= 32f;
                }
                this.noWarpTimer = 3000;
                --currentBuilding.currentOccupants.Value;
                if (Utility.isOnScreen(this.getTileLocationPoint(), 192, (GameLocation)locationFromName))
                    locationFromName.localSound("sandyStep");
                if (environment.isTileOccupiedByFarmer(this.getTileLocation()) != null)
                    environment.isTileOccupiedByFarmer(this.getTileLocation()).temporaryImpassableTile = this.GetBoundingBox();
            }
            this.behaviors(time, environment);
        }

        public static void behaviorAfterFindingGrassPatch(Character c, GameLocation environment)
        {
            if ((int)(byte)((NetFieldBase<byte, NetByte>)((MyFarmAnimal)c).fullness) >= (int)byte.MaxValue)
                return;
            ((MyFarmAnimal)c).eatGrass(environment);
        }

        public static bool animalDoorEndPointFunction(PathNode currentPoint, Point endPoint, GameLocation location, Character c)
        {
            Vector2 vector2 = new Vector2((float)currentPoint.x, (float)currentPoint.y);
            foreach (Building building in ((BuildableGameLocation)location).buildings)
            {
                if (building.animalDoor.X >= 0 && (double)(building.animalDoor.X + (int)((NetFieldBase<int, NetInt>)building.tileX)) == (double)vector2.X && ((double)(building.animalDoor.Y + (int)((NetFieldBase<int, NetInt>)building.tileY)) == (double)vector2.Y && building.buildingType.Value.Contains((string)((NetFieldBase<string, NetString>)((MyFarmAnimal)c).buildingTypeILiveIn))) && (int)((NetFieldBase<int, NetInt>)building.currentOccupants) < (int)((NetFieldBase<int, NetInt>)building.maxOccupants))
                {
                    ++building.currentOccupants.Value;
                    location.playSound("dwop");
                    return true;
                }
            }
            return false;
        }

        public static bool grassEndPointFunction(PathNode currentPoint, Point endPoint, GameLocation location, Character c)
        {
            Vector2 key = new Vector2((float)currentPoint.x, (float)currentPoint.y);
            if (!location.terrainFeatures.ContainsKey(key) || !(location.terrainFeatures[key] is Grass))
                return false;
            // TODO: MyFarm
           /* foreach (KeyValuePair<long, MyFarmAnimal> pair in ((Farm)location).animals.Pairs)
            {
                if ((double)pair.Value.getTileLocation().X == (double)currentPoint.x)
                {
                    if ((double)pair.Value.getTileLocation().Y == (double)currentPoint.y)
                        break;
                }
            }*/
            return true;
        }

        public virtual void updatePerTenMinutes(int timeOfDay, GameLocation environment)
        {
            if (timeOfDay >= 1800)
            {
                if (environment.IsOutdoors && timeOfDay > 1900 || !environment.IsOutdoors && (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) > 150 && Game1.currentSeason.Equals("winter") || ((bool)((NetFieldBase<bool, NetBool>)environment.isOutdoors) && Game1.isRaining || (bool)((NetFieldBase<bool, NetBool>)environment.isOutdoors) && Game1.currentSeason.Equals("winter")))
                    this.happiness.Value = (byte)Math.Min((int)byte.MaxValue, Math.Max(0, (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) - (environment.numberOfObjectsWithName("Heater") <= 0 || !Game1.currentSeason.Equals("winter") ? (int)(byte)((NetFieldBase<byte, NetByte>)this.happinessDrain) : (int)-(byte)((NetFieldBase<byte, NetByte>)this.happinessDrain))));
                else if (environment.IsOutdoors)
                    this.happiness.Value = (byte)Math.Min((int)byte.MaxValue, (int)(byte)((NetFieldBase<byte, NetByte>)this.happiness) + (int)(byte)((NetFieldBase<byte, NetByte>)this.happinessDrain));
            }
            if (environment.isTileOccupiedByFarmer(this.getTileLocation()) == null)
                return;
            environment.isTileOccupiedByFarmer(this.getTileLocation()).temporaryImpassableTile = this.GetBoundingBox();
        }

        public void eatGrass(GameLocation environment)
        {
            // TODO: Find out what to do with this
            /*
            Vector2 index;
            // ISSUE: explicit reference operation
            // ISSUE: variable of a reference type
            
           Vector2& local = @index;
            Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
            double num1 = (double)(boundingBox.Center.X / 64);
            boundingBox = this.GetBoundingBox();
            double num2 = (double)(boundingBox.Center.Y / 64);
      // ISSUE: explicit reference operation
      ^ local = new Vector2((float)num1, (float)num2);
            if (!environment.terrainFeatures.ContainsKey(index) || !(environment.terrainFeatures[index] is Grass))
                return;
            this.isEating.Value = true;
            if (((Grass)environment.terrainFeatures[index]).reduceBy(this.isCoopDweller() ? 2 : 4, index, environment.Equals(Game1.currentLocation)))
                environment.terrainFeatures.Remove(index);
            this.Sprite.loop = false;
            this.fullness.Value = byte.MaxValue;
            if ((int)((NetFieldBase<int, NetInt>)this.moodMessage) == 5 || (int)((NetFieldBase<int, NetInt>)this.moodMessage) == 6 || Game1.isRaining)
                return;
            this.happiness.Value = byte.MaxValue;
            this.friendshipTowardFarmer.Value = Math.Min(1000, (int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) + 8);*/
        }

        public override void performBehavior(byte which)
        {
            if ((int)which != 0)
                return;
            this.eatGrass(Game1.currentLocation);
        }

        private bool behaviors(GameTime time, GameLocation location)
        {
            if (this.home == null)
                return false;
            if ((bool)((NetFieldBase<bool, NetBool>)this.isEating))
            {
                if (this.home != null && this.home.getRectForAnimalDoor().Intersects(this.GetBoundingBox()))
                {
                    MyFarmAnimal.behaviorAfterFindingGrassPatch((Character)this, location);
                    this.isEating.Value = false;
                    this.Halt();
                    return false;
                }
                if (this.buildingTypeILiveIn.Contains("Barn"))
                {
                    this.Sprite.Animate(time, 16, 4, 100f);
                    if (this.Sprite.currentFrame >= 20)
                    {
                        this.isEating.Value = false;
                        this.Sprite.loop = true;
                        this.Sprite.currentFrame = 0;
                        this.faceDirection(2);
                    }
                }
                else
                {
                    this.Sprite.Animate(time, 24, 4, 100f);
                    if (this.Sprite.currentFrame >= 28)
                    {
                        this.isEating.Value = false;
                        this.Sprite.loop = true;
                        this.Sprite.currentFrame = 0;
                        this.faceDirection(2);
                    }
                }
                return true;
            }
            if (!Game1.IsClient)
            {
                if (this.controller != null)
                    return true;
                if (location.IsOutdoors && (int)(byte)((NetFieldBase<byte, NetByte>)this.fullness) < 195 && Game1.random.NextDouble() < 0.002)
                    this.controller = new PathFindController((Character)this, location, new PathFindController.isAtEnd(MyFarmAnimal.grassEndPointFunction), -1, false, new PathFindController.endBehavior(MyFarmAnimal.behaviorAfterFindingGrassPatch), 200, Point.Zero);
                if (Game1.timeOfDay >= 1700 && location.IsOutdoors && (this.controller == null && Game1.random.NextDouble() < 0.002))
                {
                    this.controller = new PathFindController((Character)this, location, new PathFindController.isAtEnd(PathFindController.isAtEndPoint), 0, false, (PathFindController.endBehavior)null, 200, new Point((int)((NetFieldBase<int, NetInt>)this.home.tileX) + this.home.animalDoor.X, (int)((NetFieldBase<int, NetInt>)this.home.tileY) + this.home.animalDoor.Y));
                    if (location.getFarmersCount() == 0)
                    {
                        (location as Farm).animals.Remove((long)((NetFieldBase<long, NetLong>)this.myID));
                        // TODO: MyAnimalHouse
                        //(this.home.indoors.Value as AnimalHouse).animals.Add((long)((NetFieldBase<long, NetLong>)this.myID), this);
                        this.setRandomPosition((GameLocation)((NetFieldBase<GameLocation, NetRef<GameLocation>>)this.home.indoors));
                        this.faceDirection(Game1.random.Next(4));
                        this.controller = (PathFindController)null;
                        return true;
                    }
                }
                if (location.IsOutdoors && !Game1.isRaining && (!Game1.currentSeason.Equals("winter") && (int)((NetFieldBase<int, NetInt>)this.currentProduce) != -1) && ((int)((NetFieldBase<int, NetInt>)this.age) >= (int)(byte)((NetFieldBase<byte, NetByte>)this.ageWhenMature) && this.type.Value.Contains("Pig") && Game1.random.NextDouble() < 0.0002))
                {
                    Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
                    for (int corner = 0; corner < 4; ++corner)
                    {
                        Vector2 cornersOfThisRectangle = Utility.getCornersOfThisRectangle(ref boundingBox, corner);
                        Vector2 key = new Vector2((float)(int)((double)cornersOfThisRectangle.X / 64.0), (float)(int)((double)cornersOfThisRectangle.Y / 64.0));
                        if (location.terrainFeatures.ContainsKey(key) || location.objects.ContainsKey(key))
                            return false;
                    }
                    if (Game1.player.currentLocation.Equals(location))
                    {
                        DelayedAction.playSoundAfterDelay("dirtyHit", 450, (GameLocation)null);
                        DelayedAction.playSoundAfterDelay("dirtyHit", 900, (GameLocation)null);
                        DelayedAction.playSoundAfterDelay("dirtyHit", 1350, (GameLocation)null);
                    }
                    if (location.Equals(Game1.currentLocation))
                    {
                        switch (this.FacingDirection)
                        {
                            case 0:
                                this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(9, 250),
                  new FarmerSprite.AnimationFrame(11, 250),
                  new FarmerSprite.AnimationFrame(9, 250),
                  new FarmerSprite.AnimationFrame(11, 250),
                  new FarmerSprite.AnimationFrame(9, 250),
                  new FarmerSprite.AnimationFrame(11, 250, false, false, new AnimatedSprite.endOfAnimationBehavior(this.findTruffle), false)
                });
                                break;
                            case 1:
                                this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(5, 250),
                  new FarmerSprite.AnimationFrame(7, 250),
                  new FarmerSprite.AnimationFrame(5, 250),
                  new FarmerSprite.AnimationFrame(7, 250),
                  new FarmerSprite.AnimationFrame(5, 250),
                  new FarmerSprite.AnimationFrame(7, 250, false, false, new AnimatedSprite.endOfAnimationBehavior(this.findTruffle), false)
                });
                                break;
                            case 2:
                                this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(1, 250),
                  new FarmerSprite.AnimationFrame(3, 250),
                  new FarmerSprite.AnimationFrame(1, 250),
                  new FarmerSprite.AnimationFrame(3, 250),
                  new FarmerSprite.AnimationFrame(1, 250),
                  new FarmerSprite.AnimationFrame(3, 250, false, false, new AnimatedSprite.endOfAnimationBehavior(this.findTruffle), false)
                });
                                break;
                            case 3:
                                this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(5, 250, false, true, (AnimatedSprite.endOfAnimationBehavior) null, false),
                  new FarmerSprite.AnimationFrame(7, 250, false, true, (AnimatedSprite.endOfAnimationBehavior) null, false),
                  new FarmerSprite.AnimationFrame(5, 250, false, true, (AnimatedSprite.endOfAnimationBehavior) null, false),
                  new FarmerSprite.AnimationFrame(7, 250, false, true, (AnimatedSprite.endOfAnimationBehavior) null, false),
                  new FarmerSprite.AnimationFrame(5, 250, false, true, (AnimatedSprite.endOfAnimationBehavior) null, false),
                  new FarmerSprite.AnimationFrame(7, 250, false, true, new AnimatedSprite.endOfAnimationBehavior(this.findTruffle), false)
                });
                                break;
                        }
                        this.Sprite.loop = false;
                    }
                    else
                        this.findTruffle(Game1.player);
                }
            }
            return false;
        }

        private void findTruffle(Farmer who)
        {
            Utility.spawnObjectAround(Utility.getTranslatedVector2(this.getTileLocation(), this.FacingDirection, 1f), new StardewValley.Object(this.getTileLocation(), 430, 1), (GameLocation)Game1.getFarm());
            if (new Random((int)(long)((NetFieldBase<long, NetLong>)this.myID) / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay).NextDouble() <= (double)(int)((NetFieldBase<int, NetInt>)this.friendshipTowardFarmer) / 1500.0)
                return;
            this.currentProduce.Value = -1;
        }

        public void hitWithWeapon(MeleeWeapon t)
        {
        }

        public void makeSound()
        {
            if (this.sound.Value == null || Game1.soundBank == null || this.currentLocation != Game1.currentLocation)
                return;
            Cue cue = Game1.soundBank.GetCue(this.sound.Value);
            cue.SetVariable("Pitch", (float)(1200 + Game1.random.Next(-200, 201)));
            cue.Play();
        }

        public virtual bool updateWhenCurrentLocation(GameTime time, GameLocation location)
        {
            if (!Game1.shouldTimePass())
                return false;
            if (this.health.Value <= 0)
                return true;
            if (this.hitGlowTimer > 0)
                this.hitGlowTimer -= time.ElapsedGameTime.Milliseconds;
            if (this.Sprite.CurrentAnimation != null)
            {
                if (this.Sprite.animateOnce(time))
                    this.Sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
                return false;
            }
            this.update(time, location, (long)((NetFieldBase<long, NetLong>)this.myID), false);
            if (Game1.IsMasterGame && this.behaviors(time, location) || this.Sprite.CurrentAnimation != null)
                return false;
            if (this.controller != null && this.controller.timerSinceLastCheckPoint > 10000)
            {
                this.controller = (PathFindController)null;
                this.Halt();
            }
            if (location is Farm && this.noWarpTimer <= 0 && (this.home != null && Game1.IsMasterGame))
            {
                Microsoft.Xna.Framework.Rectangle rectForAnimalDoor = this.home.getRectForAnimalDoor();
                // ISSUE: explicit reference operation
                // ISSUE: variable of a reference type
                // TODO: Find out what to do about this
                //Microsoft.Xna.Framework.Rectangle & local = @rectForAnimalDoor;
                Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
                int x = boundingBox.Center.X;
                boundingBox = this.GetBoundingBox();
                int top = boundingBox.Top;
                // ISSUE: explicit reference operation
               /* if ((^ local).Contains(x, top))
                {
                    ((Farm)location).animals.Remove((long)((NetFieldBase<long, NetLong>)this.myID));
                    (this.home.indoors.Value as AnimalHouse).animals[(long)((NetFieldBase<long, NetLong>)this.myID)] = this;
                    this.setRandomPosition((GameLocation)((NetFieldBase<GameLocation, NetRef<GameLocation>>)this.home.indoors));
                    this.faceDirection(Game1.random.Next(4));
                    this.controller = (PathFindController)null;
                    if (Utility.isOnScreen(this.getTileLocationPoint(), 192, location))
                        location.localSound("dwoop");
                    return true;
                }*/
            }
            this.noWarpTimer = Math.Max(0, this.noWarpTimer - time.ElapsedGameTime.Milliseconds);
            if (this.pauseTimer > 0)
                this.pauseTimer -= time.ElapsedGameTime.Milliseconds;
            if (Game1.timeOfDay >= 2000)
            {
                this.Sprite.currentFrame = this.buildingTypeILiveIn.Contains("Coop") ? 16 : 12;
                this.Sprite.UpdateSourceRect();
                if (!this.isEmoting && Game1.random.NextDouble() < 0.002)
                    this.doEmote(24, true);
            }
            else if (this.pauseTimer <= 0)
            {
                if (Game1.random.NextDouble() < 0.001 && (int)((NetFieldBase<int, NetInt>)this.age) >= (int)(byte)((NetFieldBase<byte, NetByte>)this.ageWhenMature) && ((int)Game1.gameMode == 3 && this.sound.Value != null) && Utility.isOnScreen(this.Position, 192))
                    this.makeSound();
                if (!Game1.IsClient && Game1.random.NextDouble() < 0.007 && this.uniqueFrameAccumulator == -1)
                {
                    int direction = Game1.random.Next(5);
                    if (direction != (this.FacingDirection + 2) % 4)
                    {
                        if (direction < 4)
                        {
                            int facingDirection = this.FacingDirection;
                            this.faceDirection(direction);
                            if (!(bool)((NetFieldBase<bool, NetBool>)location.isOutdoors) && location.isCollidingPosition(this.nextPosition(direction), Game1.viewport, (Character)this))
                            {
                                this.faceDirection(facingDirection);
                                return false;
                            }
                        }
                        switch (direction)
                        {
                            case 0:
                                this.SetMovingUp(true);
                                break;
                            case 1:
                                this.SetMovingRight(true);
                                break;
                            case 2:
                                this.SetMovingDown(true);
                                break;
                            case 3:
                                this.SetMovingLeft(true);
                                break;
                            default:
                                this.Halt();
                                this.Sprite.StopAnimation();
                                break;
                        }
                    }
                    else if (this.noWarpTimer <= 0)
                    {
                        this.Halt();
                        this.Sprite.StopAnimation();
                    }
                }
                if (!Game1.IsClient && this.isMoving() && (Game1.random.NextDouble() < 0.014 && this.uniqueFrameAccumulator == -1))
                {
                    this.Halt();
                    this.Sprite.StopAnimation();
                    if (Game1.random.NextDouble() < 0.75)
                    {
                        this.uniqueFrameAccumulator = 0;
                        if (this.buildingTypeILiveIn.Contains("Coop"))
                        {
                            switch (this.FacingDirection)
                            {
                                case 0:
                                    this.Sprite.currentFrame = 20;
                                    break;
                                case 1:
                                    this.Sprite.currentFrame = 18;
                                    break;
                                case 2:
                                    this.Sprite.currentFrame = 16;
                                    break;
                                case 3:
                                    this.Sprite.currentFrame = 22;
                                    break;
                            }
                        }
                        else if (this.buildingTypeILiveIn.Contains("Barn"))
                        {
                            switch (this.FacingDirection)
                            {
                                case 0:
                                    this.Sprite.currentFrame = 15;
                                    break;
                                case 1:
                                    this.Sprite.currentFrame = 14;
                                    break;
                                case 2:
                                    this.Sprite.currentFrame = 13;
                                    break;
                                case 3:
                                    this.Sprite.currentFrame = 14;
                                    break;
                            }
                        }
                    }
                    this.Sprite.UpdateSourceRect();
                }
                if (this.uniqueFrameAccumulator != -1 && !Game1.IsClient)
                {
                    this.uniqueFrameAccumulator += time.ElapsedGameTime.Milliseconds;
                    if (this.uniqueFrameAccumulator > 500)
                    {
                        if (this.buildingTypeILiveIn.Contains("Coop"))
                            this.Sprite.currentFrame = this.Sprite.currentFrame + 1 - this.Sprite.currentFrame % 2 * 2;
                        else if (this.Sprite.currentFrame > 12)
                        {
                            this.Sprite.currentFrame = (this.Sprite.currentFrame - 13) * 4;
                        }
                        else
                        {
                            switch (this.FacingDirection)
                            {
                                case 0:
                                    this.Sprite.currentFrame = 15;
                                    break;
                                case 1:
                                    this.Sprite.currentFrame = 14;
                                    break;
                                case 2:
                                    this.Sprite.currentFrame = 13;
                                    break;
                                case 3:
                                    this.Sprite.currentFrame = 14;
                                    break;
                            }
                        }
                        this.uniqueFrameAccumulator = 0;
                        if (Game1.random.NextDouble() < 0.4)
                            this.uniqueFrameAccumulator = -1;
                    }
                }
                else if (!Game1.IsClient)
                    this.MovePosition(time, Game1.viewport, location);
            }
            if (!Game1.IsClient && !this.isMoving() && (location is Farm && this.controller == null))
            {
                this.Halt();
                Microsoft.Xna.Framework.Rectangle boundingBox1 = this.GetBoundingBox();
                // TODO: MyFarm
                /*foreach (KeyValuePair<long, MyFarmAnimal> pair in (location as Farm).animals.Pairs)
                {
                    if (!pair.Value.Equals((object)this))
                    {
                        Microsoft.Xna.Framework.Rectangle boundingBox2 = pair.Value.GetBoundingBox();
                        if (boundingBox2.Intersects(boundingBox1))
                        {
                            int x1 = boundingBox1.Center.X;
                            boundingBox2 = pair.Value.GetBoundingBox();
                            int x2 = boundingBox2.Center.X;
                            int num1 = x1 - x2;
                            int y1 = boundingBox1.Center.Y;
                            boundingBox2 = pair.Value.GetBoundingBox();
                            int y2 = boundingBox2.Center.Y;
                            int num2 = y1 - y2;
                            if (num1 > 0 && Math.Abs(num1) > Math.Abs(num2))
                                this.SetMovingUp(true);
                            else if (num1 < 0 && Math.Abs(num1) > Math.Abs(num2))
                                this.SetMovingDown(true);
                            else if (num2 > 0)
                                this.SetMovingLeft(true);
                            else
                                this.SetMovingRight(true);
                        }
                    }
                }*/
            }
            return false;
        }

        public override bool shouldCollideWithBuildingLayer(GameLocation location)
        {
            return true;
        }

        public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            if (this.pauseTimer > 0 || Game1.IsClient)
                return;
            if (this.moveUp)
            {
                if (!currentLocation.isCollidingPosition(this.nextPosition(0), Game1.viewport, false, 0, false, (Character)this, false, false, false))
                {
                    this.position.Y -= (float)this.speed;
                    this.Sprite.AnimateUp(time, 0, "");
                }
                else
                {
                    this.Halt();
                    this.Sprite.StopAnimation();
                    if (Game1.random.NextDouble() < 0.6)
                        this.SetMovingDown(true);
                }
                this.faceDirection(0);
            }
            else if (this.moveRight)
            {
                if (!currentLocation.isCollidingPosition(this.nextPosition(1), Game1.viewport, false, 0, false, (Character)this))
                {
                    this.position.X += (float)this.speed;
                    this.Sprite.AnimateRight(time, 0, "");
                }
                else
                {
                    this.Halt();
                    this.Sprite.StopAnimation();
                    if (Game1.random.NextDouble() < 0.6)
                        this.SetMovingLeft(true);
                }
                this.faceDirection(1);
            }
            else if (this.moveDown)
            {
                if (!currentLocation.isCollidingPosition(this.nextPosition(2), Game1.viewport, false, 0, false, (Character)this))
                {
                    this.position.Y += (float)this.speed;
                    this.Sprite.AnimateDown(time, 0, "");
                }
                else
                {
                    this.Halt();
                    this.Sprite.StopAnimation();
                    if (Game1.random.NextDouble() < 0.6)
                        this.SetMovingUp(true);
                }
                this.faceDirection(2);
            }
            else
            {
                if (!this.moveLeft)
                    return;
                if (!currentLocation.isCollidingPosition(this.nextPosition(3), Game1.viewport, false, 0, false, (Character)this))
                {
                    this.position.X -= (float)this.speed;
                    this.Sprite.AnimateRight(time, 0, "");
                }
                else
                {
                    this.Halt();
                    this.Sprite.StopAnimation();
                    if (Game1.random.NextDouble() < 0.6)
                        this.SetMovingRight(true);
                }
                this.FacingDirection = 3;
                if (this.isCoopDweller() || this.Sprite.currentFrame <= 7)
                    return;
                this.Sprite.currentFrame = 4;
            }
        }

        public override void animateInFacingDirection(GameTime time)
        {
            if (this.FacingDirection == 3)
                this.Sprite.AnimateRight(time, 0, "");
            else
                base.animateInFacingDirection(time);
        }
    }
}

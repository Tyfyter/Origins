using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.World;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Testing {
	public class Blood_Mushroom_Soup : ModItem {
        int mode;
        const int modeCount = 10;
        long packedMode => (long)mode|((long)parameters.Count<<32);
        LinkedQueue<object> parameters = new LinkedQueue<object>();
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Worldgen Testing Item");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
			Item.width = 16;
			Item.height = 26;
			Item.value = 25000;
			Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 10;
            Item.useTime = 10;
		}
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool? UseItem(Player player)/* Suggestion: Return null instead of false */ {
            if(Main.myPlayer == player.whoAmI){
                if(player.altFunctionUse == 2) {
                    parameters.Clear();
                    if(player.controlSmart) {
                        mode = (mode + modeCount - 1)%modeCount;
                    } else {
                        mode = (mode + 1)%modeCount;
                    }
                } else {
                    if(player.controlSmart) {
                        Apply();
                    } else if(player.controlDown) {
                        if(parameters.Count>0)parameters.RemoveAt(parameters.Count-1);
                    } else {
                        SetParameter();
                    }
                }
                return true;
            }
            return false;
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if(Main.LocalPlayer.HeldItem.type==Item.type)Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, GetMouseText(), Main.MouseScreen.X, Math.Max(Main.MouseScreen.Y-24, 18), Colors.RarityNormal, Color.Black, new Vector2(0f));
        }
        const long p0 = (0L << 32);
        const long p1 = (1L << 32);
        const long p2 = (2L << 32);
        const long p3 = (3L << 32);
        const long p4 = (4L << 32);
        const long p5 = (5L << 32);
        const long p6 = (6L << 32);
        const long p7 = (7L << 32);
        void SetParameter() {
            Point mousePos = new Point((int)(Main.MouseScreen.X / 16), (int)(Main.MouseScreen.Y / 16));
            int mousePacked = mousePos.X + (Main.screenWidth / 16) * mousePos.Y;
            double mousePackedDouble = (Main.MouseScreen.X/16d + (Main.screenWidth/16d) * Main.MouseScreen.Y/16d)/16d;
            Tile mouseTile  = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
            Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.MountedCenter;
            switch(packedMode) {
                case 6|p0:
                parameters.Enqueue(Main.MouseWorld.X/16);
                parameters.Enqueue(Main.MouseWorld.Y/16);
                Apply();
                break;
                case 2|p0:
                case 3|p0:
                case 4|p0:
                case 5|p0:
                parameters.Enqueue(Player.tileTargetX);
                parameters.Enqueue(Player.tileTargetY);
                Apply();
                break;
                case 1|p0:
                case 0|p0:
                parameters.Enqueue(Player.tileTargetX);
                parameters.Enqueue(Player.tileTargetY);
                break;
                case 1|p1:
                case 0|p1:
                parameters.Enqueue(Player.tileTargetY);
                break;
                case 1|p2:
                case 0|p2:
                parameters.Enqueue(Math.Sqrt(mousePackedDouble / 16));
                break;
                case 1|p3:
                case 0|p3:
                parameters.Enqueue(diffFromPlayer / 16);
                break;
                case 1|p4:
                case 0|p4:
                parameters.Enqueue(mousePackedDouble);
                break;
                case 1|p5:
                case 0|p5:
                parameters.Enqueue(Main.LocalPlayer.controlUp?0:diffFromPlayer.ToRotation());
                break;
                case 1|p6:
                case 0|p6:
                parameters.Enqueue(Main.MouseScreen.Y > Main.screenHeight / 2f);
                break;
                case 1|p7:
                parameters.Enqueue((byte)((mousePacked/16)%256));
                break;
            }
        }
        string GetMouseText() {
            Point mousePos = new Point((int)(Main.MouseScreen.X / 16), (int)(Main.MouseScreen.Y / 16));
            int mousePacked = mousePos.X + (Main.screenWidth / 16) * mousePos.Y;
            double mousePackedDouble = (Main.MouseScreen.X/16d + (Main.screenWidth/16d) * Main.MouseScreen.Y/16d)/16d;
            Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.MountedCenter;
            switch(packedMode) {
                case 6|p0:
                return "place defiled stone ring";
                case 5|p0:
                return "place defiled start";
                case 4|p0:
                return "place brine pool";
                case 3|p0:
                return "place riven cave";
                case 2|p0:
                return "place riven start";
                case 1|p0:
                case 0|p0:
                return $"i,j: {Player.tileTargetX}, {Player.tileTargetY}";
                case 1|p1:
                case 0|p1:
                return $"j: {Player.tileTargetY}";
                case 1|p2:
                case 0|p2:
                return $"strength: {mousePackedDouble / 16}";
                case 1|p3:
                case 0|p3:
                return $"speed: {diffFromPlayer / 16}";
                case 1|p4:
                case 0|p4:
                return $"length: {mousePackedDouble}";
                case 1|p5:
                case 0|p5:
                return $"twist: {(Main.LocalPlayer.controlUp?0:(double)diffFromPlayer.ToRotation())}";
                case 1|p6:
                case 0|p6:
                return $"random twist: {Main.MouseScreen.Y>Main.screenHeight/2f}";
                case 1|p7:
                return $"branch count (optional): {(byte)((mousePacked/16)%256)}";
                //return $":{}";
            }
            return "";
        }
        void Apply() {
            switch(mode) {
                case 0: {
                    GenRunners.VeinRunner(
                        i: (int)parameters.Dequeue(),
                        j: (int)parameters.Dequeue(),
                        strength: (double)parameters.Dequeue(),
                        speed: (Vector2)parameters.Dequeue(),
                        length: (double)parameters.Dequeue(),
                        twist: (float)parameters.Dequeue(),
                        randomtwist: (bool)parameters.Dequeue());
                    break;
                }
                case 1: {
                    int i = (int)parameters.Dequeue();
                    int j = (int)parameters.Dequeue();
                    double strength = (double)parameters.Dequeue();
                    Vector2 speed = (Vector2)parameters.Dequeue();
                    Stack<((Vector2, Vector2), byte)> veins = new Stack<((Vector2, Vector2), byte)>();
                    double length = (double)parameters.Dequeue();
                    float twist = (float)parameters.Dequeue();
                    bool twistRand = (bool)parameters.Dequeue();
                    veins.Push(((new Vector2(i, j), speed), (parameters.Count > 0 ? (byte)parameters.Dequeue() : (byte)10)));
                    ((Vector2 p, Vector2 v) v, byte count) curr;
                    (Vector2 p, Vector2 v) ret;
                    byte count;
                    while (veins.Count > 0) {
                        curr = veins.Pop();
                        count = curr.count;
                        ret = GenRunners.VeinRunner(
                            i: (int)curr.v.p.X,
                            j: (int)curr.v.p.Y,
                            strength: strength,
                            speed: curr.v.v,
                            length: length,
                            twist: twist,
                            randomtwist: twistRand);
                        if (count > 0 && Main.rand.NextBool(3)) {
                            veins.Push(((ret.p, ret.v.RotatedBy(Main.rand.NextBool() ? -1 : 1)), (byte)Main.rand.Next(--count)));
                        }
                        if (count > 0) {
                            veins.Push(((ret.p, ret.v.RotatedByRandom(0.05)), --count));
                        }
                    }
                    break;
                }
                case 2:
                World.BiomeData.RivenHive.Gen.StartHive((int)parameters.Dequeue(), (int)parameters.Dequeue());
                break;
                case 3:
                World.BiomeData.RivenHive.Gen.HiveCave((int)parameters.Dequeue(), (int)parameters.Dequeue());
                break;
                case 4:
                World.BiomeData.BrinePool.Gen.BrineStart((int)parameters.Dequeue(), (int)parameters.Dequeue());
                break;
                case 5:
                World.BiomeData.DefiledWastelands.Gen.StartDefiled((int)parameters.Dequeue(), (int)parameters.Dequeue());
                break;
                case 6: {
                    Vector2 a = new Vector2((float)parameters.Dequeue(), (float)parameters.Dequeue());
                    World.BiomeData.DefiledWastelands.Gen.DefiledRibs((int)a.X, (int)a.Y);
                    for (int i = (int)a.X - 1; i < (int)a.X + 3; i++) {
                        for (int j = (int)a.Y - 2; j < (int)a.Y + 2; j++) {
                            Main.tile[i, j].SetActive(false);
                        }
                    }
                    TileObject.CanPlace((int)a.X, (int)a.Y, (ushort)ModContent.TileType<Tiles.Defiled.Defiled_Heart>(), 0, 1, out var data);
                    TileObject.Place(data);
                    break;
                }
            }
        }
    }
}

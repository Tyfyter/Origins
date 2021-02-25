using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Testing {
	public class Blood_Mushroom_Soup : ModItem {
        int mode;
        const int modeCount = 10;
        long packedMode => (long)mode|((long)p.Count<<32);
        LinkedQueue<object> p = new LinkedQueue<object>();
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Worldgen Testing Item");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
			item.width = 16;
			item.height = 26;
			item.value = 25000;
			item.rare = 2;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.useAnimation = 10;
            item.useTime = 10;
		}
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool UseItem(Player player) {
            if(Main.myPlayer == player.whoAmI){
                if(player.altFunctionUse == 2) {
                    p.Clear();
                    if(player.controlSmart) {
                        mode = (mode + modeCount - 1)%modeCount;
                    } else {
                        mode = (mode + 1)%modeCount;
                    }
                } else {
                    if(player.controlSmart) {
                        Apply();
                    } else if(player.controlDown) {
                        p.RemoveAt(p.Count-1);
                    } else {
                        SetParameter();
                    }
                }
                return true;
            }
            return false;
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if(Main.LocalPlayer.HeldItem.type==item.type)Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, GetMouseText(), Main.MouseScreen.X, Main.MouseScreen.Y-24, Colors.RarityNormal, Color.Black, new Vector2(0f));
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
            Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.Center;
            switch(packedMode) {
                case 0|p0:
                p.Q = Player.tileTargetX;
                p.Q = Player.tileTargetY;
                break;
                case 0|p1:
                p.Q = Player.tileTargetY;
                break;
                case 0|p2:
                p.Q = Math.Sqrt(mousePackedDouble / 16);
                break;
                case 0|p3:
                p.Q = diffFromPlayer / 16;
                break;
                case 0|p4:
                p.Q = mousePackedDouble;
                break;
                case 0|p5:
                p.Q = Main.LocalPlayer.controlUp?0:diffFromPlayer.ToRotation();
                break;
                case 0|p6:
                p.Q = Main.MouseScreen.Y > Main.screenHeight / 2f;
                break;
            }
        }
        string GetMouseText() {
            double mousePackedDouble = (Main.MouseScreen.X/16d + (Main.screenWidth/16d) * Main.MouseScreen.Y/16d)/16d;
            Vector2 diffFromPlayer = Main.MouseWorld - Main.LocalPlayer.Center;
            switch(packedMode) {
                case 0|p0:
                return $"i,j: {Player.tileTargetX}, {Player.tileTargetY}";
                case 0|p1:
                return $"j: {Player.tileTargetY}";
                case 0|p2:
                return $"strength: {mousePackedDouble / 16}";
                case 0|p3:
                return $"speed: {diffFromPlayer / 16}";
                case 0|p4:
                return $"length: {mousePackedDouble}";
                case 0|p5:
                return $"twist: {(Main.LocalPlayer.controlUp?0:(double)diffFromPlayer.ToRotation())}";
                case 0|p6:
                return $"random twist: {Main.MouseScreen.Y>Main.screenHeight/2f}";
                //return $":{}";
            }
            return "";
        }
        void Apply() {
            switch(mode) {
                case 0:
                GenRunners.VeinRunner(
                    i:(int)p.DQ,
                    j:(int)p.DQ,
                    strength:(double)p.DQ,
                    speed:(Vector2)p.DQ,
                    length:(double)p.DQ,
                    twist:(float)p.DQ,
                    randomtwist:(bool)p.DQ);
                break;
            }
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI.Chat;

namespace Origins.Items.Other.Testing {
	public class TileEntityDebugger : TestingItem, ICustomDrawItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.SpectreGoggles;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 26;
			Item.value = Item.sellPrice(platinum: 9001);
			Item.rare = ItemRarityID.Green;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
			Item.useAnimation = 10;
			Item.useTime = 10;
			Item.color = Color.Beige;
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			for (int i = 0; i < TESystem.TESystemCount; i++) {
				TESystem system = TESystem.Get(i);
				for (int j = 0; j < system.tileEntityLocations.Count; j++) {
					Vector2 pos = system.tileEntityLocations[j].ToWorldCoordinates() - Main.screenPosition;
					if (pos.X >= 0 && pos.Y >= 0 && pos.X <= Main.screenWidth && pos.Y <= Main.screenHeight) {
						ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, i + "", pos, Color.Beige, 0, Vector2.Zero, Vector2.One);
					}
				}
			}
		}
	}
}

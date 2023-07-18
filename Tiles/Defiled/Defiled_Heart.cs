using Microsoft.Xna.Framework;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Heart : ModTile {
		public int heartBroken = 0;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileLighted[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
			TileObjectData.newTile.Origin = new Point16(1, 2);
			TileObjectData.newTile.AnchorWall = false;
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.addTile(Type);
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("{$Defiled} Heart");
			AddMapEntry(new Color(50, 50, 50), name);
			//disableSmartCursor = true;
			AdjTiles = new int[] { TileID.ShadowOrbs };
			ID = Type;
			HitSound = Origins.Sounds.DefiledIdle;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 9) {
				frameCounter = 0;
				frame = ++frame % 4;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			// Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting
			int uniqueAnimationFrame = Main.tileFrame[Type] + i;
			if (i % 2 == 0)
				uniqueAnimationFrame += 3;
			if (i % 3 == 0)
				uniqueAnimationFrame += 3;
			if (i % 4 == 0)
				uniqueAnimationFrame += 3;
			uniqueAnimationFrame %= 4;

			frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
		public override bool CreateDust(int i, int j, ref int type) {
			type = Defiled_Wastelands.DefaultTileDust;
			return true;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.3f;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {

			}
			ModContent.GetInstance<OriginSystem>().Defiled_Hearts.Add(new Point(i, j));
		}
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			heartBroken++;
        }
    }
	public class Defiled_Heart_Item : ModItem {
		public override string Texture => "Origins/Tiles/Defiled/Defiled_Heart";
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Heart (Debugging Item)");
		}
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Defiled_Heart>();
		}
	}
}

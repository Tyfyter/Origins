using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Riven {
	public class Riven_Dungeon_Chest : ModChest, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		protected override bool CanBeLocked => true;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color.DoFancyGlow(new Vector3(0f, 0.25f, 0.5f), tile.TileColor);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			base.SetStaticDefaults();
			Main.tileLighted[Type] = true;
			AddMapEntry(new Color(75, 96, 161), CreateMapEntryName(), MapChestName);
			AddMapEntry(new Color(75, 96, 161), Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.{Name}_Locked.MapEntry")), MapChestName);
			AdjTiles = [TileID.Containers];
			keyItem = ModContent.ItemType<Riven_Key>();
			DustType = Riven_Hive.DefaultTileDust;
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override bool CanUnlockChest(int i, int j) => NPC.downedPlantBoss;
		public override bool LockChest(int i, int j, ref short frameXAdjustment, ref bool manual) => NPC.downedPlantBoss && base.LockChest(i, j, ref frameXAdjustment, ref manual);
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawChestGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0f;
			g = 0.05f;
			b = 0.1f;
		}
		public override void Load() {
			base.Load();
			this.SetupGlowKeys();
		}
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}

using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Flesh_Shredder : ModTile, IGlowingModTile {
		public const int BaseTileID = TileID.FleshCloningVat;
		public static int ID { get; private set; }
		public override void Load() {
			 new TileItem(this)
			.WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.FleshCloningVaat);
				item.createTile = Type;
			})
			.RegisterItem();
			this.SetupGlowKeys();
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;

			TileID.Sets.DisableSmartCursor[Type] = TileID.Sets.DisableSmartCursor[BaseTileID];
			TileID.Sets.AvoidedByNPCs[Type] = TileID.Sets.AvoidedByNPCs[BaseTileID];

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(BaseTileID, 0));
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.SetHeight(3);
			TileObjectData.newTile.SetOriginBottomCenter();
			AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum() + 2 * TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);

			AddMapEntry(new(75, 96, 161), TileItem.Get(this).DisplayName);
			AdjTiles = [BaseTileID, Type];
			HitSound = SoundID.NPCHit18;
			DustType = Ashen_Biome.DefaultTileDust;
			ID = Type;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 7) {
				frameCounter = 0;
				frame = ++frame % 6;
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public void FancyLightingGlowColor(Tile tile, int x, int y, ref Vector3 color) {
			switch (tile.TileFrameY) {
				case 0:
				if (tile.TileFrameX is 0 or (18 * 2)) return;
				break;
				case 18:
				if (tile.TileFrameX == 18) return;
				break;
			}
			color.DoFancyGlow(Riven_Hive.ColoredGlow(0.8f), tile.TileColor);
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new(Riven_Hive.NormalGlowValue.Value, Riven_Hive.NormalGlowValue.Value, Riven_Hive.NormalGlowValue.Value, Riven_Hive.NormalGlowValue.Value);
	}
}

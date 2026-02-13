using Origins.Items.Other.Critters;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Peppered_Moth_Lamp : LightFurnitureBase {
		public override int BaseTileID => TileID.Lamps;
		public override Color MapColor { get; }
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddRecipeGroup(OriginSystem.LampRecipeGroup)
				.AddIngredient<Peppered_Moth_Item>()
				.Register();
			};
			Item.ExtraStaticDefaults += item => {
				if (!OriginsModIntegrations.CheckAprilFools()) OriginSystem.LampRecipeGroup.ValidItems.Remove(item.type);
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileLighted[Type] = true;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void ModifyTileData() {
			TileObjectData.newTile.SetHeight(4);
			TileObjectData.newTile.Origin = new(0, TileObjectData.newTile.Height - 1);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Framing.GetTileSafely(i, j).TileFrameY > 0) return;
			if (IsOn(Main.tile[i, j])) {
				(r, g, b) = FromHexRGB(0xf99e6f).ToVector3() * 0.7f;//#f99e6f
			}
		}
		public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible) {
			if (tileFrameY == 0 && IsOn(tile) && Main.rand.NextBool(100)) {
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.PooFly, new ParticleOrchestraSettings {
					PositionInWorld = new Vector2(i * 16 + 8, j * 16 + 8)
				});
			}
		}
	}
}

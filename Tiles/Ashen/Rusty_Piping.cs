using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Rusty_Piping : OriginTile, IAshenTile {
		public override void Load() {
			new TileItem(this)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type, 5)
				.AddRecipeGroup(ALRecipeGroups.CopperBars)
				.AddIngredient<Scrap>(5)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
		}
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.Stone[Type] = false;
			TileID.Sets.Conversion.Stone[Type] = false;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			AddMapEntry(FromHexRGB(0x6c452c));

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
	}
}

using Origins.Items.Weapons.Ammo;
using Origins.Tiles.Other;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Scrap_Heap : ComplexFrameTile, IAshenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[Type][ModContent.TileType<Ashen_Grass>()] = true;
			Main.tileMerge[ModContent.TileType<Ashen_Grass>()][Type] = true;
			Main.tileMerge[Type][ModContent.TileType<Ashen_Jungle_Grass>()] = true;
			Main.tileMerge[ModContent.TileType<Ashen_Jungle_Grass>()][Type] = true;
			AddMapEntry(FromHexRGB(0x854A4A));
			DustType = DustID.Copper;
			HitSound = SoundID.NPCHit42.WithPitch(1.5f).WithVolume(0.5f);
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay(merge + "Sludge_Overlay", ModContent.TileType<Super_Sludge>());
			yield return new TileMergeOverlay(merge + "Murk_Overlay", ModContent.TileType<Murky_Sludge>());
			yield return new TileMergeOverlay(merge + "Murk_Overlay", ModContent.TileType<Ashen_Murky_Sludge_Grass>());
		}
	}
	public class Scrap_Heap_Item : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ExtractinatorMode[Type] = Type;
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Scrap_Heap>());
		}
		public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) {
			resultType = ModContent.ItemType<Scrap>();
			resultStack = Main.rand.Next(7, 14);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Scrap>(15)
			.AddTile(ModContent.TileType<Metal_Presser>())
			.Register();
	}
}

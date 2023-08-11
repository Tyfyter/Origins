using AltLibrary.Common.AltBiomes;
using Origins.Items.Materials;
using Origins.Tiles.Ashen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.World {
	public class AshenBiomeData {
	}
	public class Ashen_Alt_Biome : AltBiome {
		AltMaterialContext materialContext;
		public override AltMaterialContext MaterialContext => materialContext ??= new AltMaterialContext()
			.SetEvilOre(ModContent.ItemType<Sanguinite_Ore_Item>())
			.SetEvilBar(ModContent.ItemType<Sanguinite_Bar>())
			.SetEvilHerb(ModContent.ItemType<Surveysprout>())
			.SetVileComponent(ModContent.ItemType<Respyrite>())
			.SetVileInnard(ModContent.ItemType<Biocomponent10>())
			.SetEvilBossDrop(ModContent.ItemType<NE8>());
		public override void SetStaticDefaults() {
			Selectable = false;
			BiomeOre = ModContent.TileType<Sanguinite_Ore>();
			BiomeOreItem = ModContent.ItemType<Sanguinite_Ore_Item>();
		}
	}
}

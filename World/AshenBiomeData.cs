using AltLibrary.Common.AltBiomes;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Tiles.Ashen;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Items.Weapons.Magic;

namespace Origins.World {
    public class AshenBiomeData : ModBiome {
		public static IItemDropRule FirstOrbDropRule;
		public static IItemDropRule OrbDropRule;
		public override void Load() {
			FirstOrbDropRule = ItemDropRule.Common(ModContent.ItemType<Neural_Network>());
			FirstOrbDropRule.OnSuccess(ItemDropRule.Common(ItemID.MusketBall, 1, 100, 100));

			OrbDropRule = new OneFromRulesRule(1,
				FirstOrbDropRule,
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Area_Denial>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Outreach>()),
				//ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Smiths_Hammer>()),
				ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Cinder_Seal>())
			);
		}
		public override void Unload() {
			FirstOrbDropRule = null;
			OrbDropRule = null;
		}
	}
	public class Ashen_Alt_Biome : AltBiome {
		AltMaterialContext materialContext;
		public override AltMaterialContext MaterialContext => materialContext ??= new AltMaterialContext()
			.SetEvilSword(ModContent.ItemType<Switchblade_Broadsword>())
			.SetEvilOre(ModContent.ItemType<Sanguinite_Ore_Item>())
			.SetEvilBar(ModContent.ItemType<Sanguinite_Bar>())
			.SetEvilHerb(ModContent.ItemType<Surveysprout_Item>())
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

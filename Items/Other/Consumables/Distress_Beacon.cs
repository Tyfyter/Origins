using Origins.Items.Materials;
using Origins.NPCs.Ashen.Boss;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	[LegacyName("Broken_Record")]
	public class Distress_Beacon : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.rare = ItemRarityID.Blue;
			Item.useLimitPerAnimation = 1;
		}
		public override bool CanUseItem(Player player) {
			return player.InModBiome<Ashen_Biome>() && !NPC.AnyNPCs(ModContent.NPCType<Trenchmaker>());
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				SoundEngine.PlaySound(SoundID.Roar, player.Center);
				new Spawn_Trenchmaker_Action(player).Perform();
			}
			return true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Biocomponent10>(), 15)
			.AddIngredient(ModContent.ItemType<Ash_Urn>(), 30)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
	}
}

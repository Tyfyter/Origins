using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs.Riven.World_Cracker;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Sus_Ice_Cream : ModItem {
		static short glowmask;
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
			Item.useLimitPerAnimation = 1;
		}
		public override bool CanUseItem(Player player) {
			return player.InModBiome<Riven_Hive>() && !NPC.AnyNPCs(ModContent.NPCType<World_Cracker_Head>());
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				SoundEngine.PlaySound(SoundID.Roar, player.Center);
				player.SpawnBossOn(ModContent.NPCType<World_Cracker_Head>());
			}
			return true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Sentient_Powder>(30)
			.AddIngredient<Bud_Barnacle>(15)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
	}
}

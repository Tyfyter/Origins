using Origins.Items.Materials;
using Origins.NPCs.Defiled.Boss;
using Origins.Projectiles.Misc;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Nerve_Impulse_Manipulator : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
		}
		public override bool CanUseItem(Player player) {
			if (Defiled_Amalgamation.spawnDA || player.ownedProjectileCounts[ModContent.ProjectileType<Defiled_Wastelands_Signal>()] > 0) return false;
			return player.InModBiome<Defiled_Wastelands>() && !NPC.AnyNPCs(ModContent.NPCType<Defiled_Amalgamation>());
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				SoundEngine.PlaySound(
					new SoundStyle("Origins/Sounds/Custom/Defiled_Idle3") {
						Pitch = -1,
						Volume = 0.66f
					}, player.itemLocation
				);
				Projectile.NewProjectile(
					player.GetSource_ItemUse(Item),
					player.itemLocation,
					default,
					ModContent.ProjectileType<Defiled_Wastelands_Signal>(),
					0,
					0,
					Main.myPlayer,
					ai0: 2,
					ai1: player.whoAmI
				);
			}
			return true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Dreadful_Powder>(30)
			.AddIngredient<Strange_String>(15)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
	}
}

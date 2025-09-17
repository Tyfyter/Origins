using Origins.NPCs.MiscB.Shimmer_Construct;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class Aether_Orb : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = ItemRarityID.Blue;
			Item.consumable = false;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			if (!NetmodeActive.MultiplayerClient && Item.shimmerWet && !Item.shimmered) {
				if (!NPC.AnyNPCs(ModContent.NPCType<Shimmer_Construct>())) {
					Item.shimmerTime += 0.02f;
					if (Item.shimmerTime > 0.9f) {
						Item.shimmerTime = 0.9f;
						SoundEngine.PlaySound(SoundID.Roar, Item.Center);
						NPC.SpawnBoss((int)Item.Center.X, (int)Item.Center.Y, ModContent.NPCType<Shimmer_Construct>(), Main.myPlayer);
						Item.stack--;
						if (Item.stack <= 0) Item.active = false;
					}
				}
				if (Item.active && NPC.AnyNPCs(ModContent.NPCType<Shimmer_Construct>())) {
					Item.shimmered = true;
				}
			}
		}
	}
}

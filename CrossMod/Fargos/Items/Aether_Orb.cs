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
			if (Item.shimmerWet && !Item.shimmered && Main.netMode != NetmodeID.MultiplayerClient && !NPC.AnyNPCs(ModContent.NPCType<Shimmer_Construct>())) {
				SoundEngine.PlaySound(SoundID.Roar, Item.Center);
				NPC.SpawnBoss((int)Item.Center.X, (int)Item.Center.Y, ModContent.NPCType<Shimmer_Construct>(), Main.myPlayer);
				Item.stack--;
				if (Item.stack <= 0) Item.active = false;
				if (Item.active) {
					Item.shimmered = true;
					Item.shimmerTime = 1;
				}
			}
		}
	}
}

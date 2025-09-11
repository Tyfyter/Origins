using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Accessories;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	public class Fae_Nymph : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 4, 38, 54);
		public int AnimationFrames => 36;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPCID.Nymph] = Type;
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Nymph];
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Nymph);
			NPC.lifeMax = 300;
			NPC.defense = 16;
			NPC.damage = 35;
			NPC.width = 28;
			NPC.height = 46;
			NPC.friendly = false;
			NPC.DeathSound = SoundID.Zombie113.WithPitch(1f).WithVolume(2f);
			NPC.value = Item.buyPrice(gold: 2);
			AIType = NPCID.Nymph;
			AnimationType = NPCID.Nymph;
			Banner = Item.NPCtoBanner(NPCID.Nymph);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<Fairy_Lotus>(), 2, 1));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_1");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 20, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_2");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 20, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_2");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 34, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_3");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 34, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_3");
			}
		}
	}
	public class Whimsical_Girl : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 4, 38, 54);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPCID.LostGirl] = Type;
			Main.npcFrameCount[Type] = 9;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.LostGirl);
			NPC.aiStyle = 0;
			NPC.lifeMax = 250;
			NPC.defense = 30;
			NPC.damage = 10;
			NPC.width = 28;
			NPC.height = 46;
			NPC.friendly = false;
			NPC.value = Item.buyPrice(gold: 2);
			AIType = NPCID.LostGirl;
			AnimationType = NPCID.LostGirl;
			Banner = Item.NPCtoBanner(NPCID.LostGirl);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.Player.ZoneShimmer) {
				return 0.0085f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.ai[0] == 0f) {
				if (NPC.target >= 0) {
					Player target = Main.player[NPC.target];
					if (NPC.Center.WithinRange(target.Center, 200f) && Collision.CanHit(NPC.position, NPC.width, NPC.height, target.position, target.width, target.height))
						NPC.ai[0] = 1f;
				}

				if (NPC.velocity.X != 0f || NPC.velocity.Y < 0f || NPC.velocity.Y > 2f || NPC.life != NPC.lifeMax)
					NPC.ai[0] = 1f;
			} else {
				NPC.ai[0] += 1f;
				if (NPC.ai[0] >= 27f) {
					NPC.ai[0] = 27f;
					NPC.Transform(ModContent.NPCType<Fae_Nymph>());
				}
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<Fairy_Lotus>(), 2, 1));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_1");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 20, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_2");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 20, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_2");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 34, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_3");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + Vector2.UnitY * 34, NPC.velocity, $"Gores/NPC/{nameof(Fae_Nymph)}_Gore_3");
			}
		}
	}
}

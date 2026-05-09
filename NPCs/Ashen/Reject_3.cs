using Origins.Core;
using Origins.Dev;
using Origins.Events;
using Origins.Items.Armor.Ashen;
using Origins.Items.Other.Consumables.Food;
using Origins.LootConditions;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	public class Reject_3 : ModNPC, IWikiNPC, IAshenEnemy, IReplaceAITypeSounds {
		public Rectangle DrawRect => new(0, 0, 34, 46);
		public int AnimationFrames => 3;
		public int FrameDuration => 3;
		public static int PowerUpTime => 18;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 11;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			GetInstance<Smog_Storm.SpawnRates>().AddSpawn(Type, Smog_Storm.SpawnRates.Reject_3);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 140;
			NPC.defense = 7;
			NPC.damage = 12;
			NPC.width = 72;
			NPC.height = 70;
			NPC.value = 230;
			NPC.knockBackResist = 0.5f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			AIType = NPCID.WalkingAntlion;
			SpawnModBiomes = [
				GetInstance<Smog_Storm>().Type,
			];
		}
		public override bool PreAI() {
			NPC.spriteDirection = NPC.direction;
			if (NPC.ai[2] < PowerUpTime) {
				if (NPC.ai[2] <= 0 && NPC.life >= NPC.lifeMax) {
					TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, SearchFilters.OnlyPlayersInCertainDistance(NPC.Center, 16 * 15));
					if (searchResults.FoundTarget) {
						NPC.target = searchResults.NearestTargetIndex;
						NPC.targetRect = searchResults.NearestTargetHitbox;
					} else {
						NPC.target = -1;
						NPC.targetRect = NPC.Hitbox.Add(128 * NPC.direction * Vector2.UnitX);
					}
					if (!NPC.HasValidTarget) return false;
				}
				NPC.ai[2]++;
				return false;
			}
			DrawOffsetY = 0;
			return true;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			DrawOffsetY = 0;
			if (NPC.ai[2] < PowerUpTime) {
				NPC.frame.Y = NPC.frame.Height * ((int)(NPC.ai[2] * 3) / PowerUpTime);
				if (NPC.frame.Y == 0) DrawOffsetY = 10;
				return;
			}
			if (NPC.velocity.Y != 0) {
				NPC.frame.Y = NPC.frame.Height * 3;
				NPC.frameCounter = 0;
				return;
			}
			NPC.DoFrames(4, 3.., NPC.velocity.X * NPC.direction);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 5, amountDroppedMaximum: 11));
			npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Greaves>(), 525));
		}
		public override void HitEffect(NPC.HitInfo hit) {
		}

		public bool PlaySound() {
			if (Main.rand.NextBool(500)) SoundEngine.PlaySound(SoundID.Zombie99 with { Pitch = -0.1f }, NPC.Center);
			else if (Main.rand.NextBool(500)) SoundEngine.PlaySound(SoundID.Zombie124 with { Pitch = -1 }, NPC.Center);
			return true;
		}
	}
}

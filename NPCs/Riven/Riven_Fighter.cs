using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Armor.Riven;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Journal;
using Origins.NPCs.Defiled;
using Origins.World.BiomeData;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Riven_Fighter : ModNPC, IRivenEnemy, IWikiNPC, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Riven_Protoform_Entry).Name;
		public class Riven_Protoform_Entry : JournalEntry {
			public override string TextKey => "Riven_Protoform";
			public override JournalSortIndex SortIndex => new("Riven", 6);
		}
		public Rectangle DrawRect => new(0, 6, 68, 56);
		public int AnimationFrames => 32;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.09f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 8;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 81;
			NPC.defense = 10;
			NPC.damage = 33;
			NPC.width = 40;
			NPC.height = 46;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath24.WithPitch(0.6f);
			NPC.value = 90;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public new virtual float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Fighter;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 16));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Symbiote_Skull>(), 40));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override bool PreAI() {
			NPC.localAI[0] += Main.rand.NextFloat(0, 1);
			if (NPC.localAI[0] > 210 && (NPC.collideY || NPC.velocity.Y == 0)) {
				switch (NPC.aiAction) {
					case 0:
					NPC.aiAction = (int)(Main.GlobalTimeWrappedHourly % 2 + 1);
					if (NPC.aiAction == 2) {
						NPC.velocity = ((NPC.GetTargetData().Center - new Vector2(0, 16) - NPC.Center) * new Vector2(1, 4)).WithMaxLength(12);
						NPC.collideX = false;
						NPC.collideY = false;
					}
					NPC.netUpdate = true;
					break;
					case 1:
					case 2:
					NPC.aiAction = 0;
					break;
				}
				NPC.localAI[0] = 0;
			}
			if (NPC.aiAction != 0) {
				if (NPC.aiAction == 1) {
					NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X + NPC.direction * 0.15f, -6, 6);
					NPC.localAI[0] += NPC.collideX ? 9 : 3;
				} else if (NPC.collideX || NPC.collideY) {
					NPC.localAI[0] = 300;
				}
				return false;

			}
			return true;
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}

		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(NPC.localAI[0]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.localAI[0] = reader.ReadSingle();
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(NPC.aiAction == 0 ? 7 : 0);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
			NPC.frame.Y = NPC.frame.Height * 0;
			NPC.frameCounter = 0;
		}
		public static AutoLoadingAsset<Texture2D> glowTexture = typeof(Riven_Fighter).GetDefaultTMLName() + "_Glow";
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			SpriteEffects effects = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.DrawGlowingNPCPart(
				texture,
				glowTexture,
				NPC.Center - Vector2.UnitY * 2 - screenPos,
				NPC.frame,
				drawColor,
				Riven_Hive.GetGlowAlpha(drawColor),
				NPC.rotation,
				new Vector2(22, 27).Apply(effects, texture.Size()),
				NPC.scale,
				effects
			);
			return false;
		}
	}
}

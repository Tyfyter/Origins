using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Gores.NPCs;
using Origins.Items.Armor.Riven;
using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Amebeye : ModNPC, IRivenEnemy {
		public static int ID { get; private set; }
		public override void Load() => this.AddBanner();
		public AutoLoadingAsset<Texture2D> glowTexture = typeof(Amebeye).GetDefaultTMLName() + "_Glow";
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.IchorSticker);
			NPC.aiStyle = NPCAIStyleID.Hovering;
			NPC.lifeMax = 300;
			NPC.defense = 60;
			NPC.damage = 14;
			NPC.width = 72;
			NPC.height = 64;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath15;
			NPC.knockBackResist = 0.75f;
			NPC.value = 76;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Riven_Hive.SpawnRates.Amebeye;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Alkahest>(), 1, 2, 5));
		}
		public override bool PreAI() {
			NPC.aiStyle = NPCAIStyleID.Hovering;
			if (NPC.ai[3] != 0) {
				NPC.defense = 0;
				bool isAggro = NPC.ai[3] == -1;
				if (!isAggro) {
					NPC bubble = Main.npc[(int)NPC.ai[3] - 1];
					if (!bubble.active || bubble.type != Amebeye_P.ID) {
						NPC.ai[3] = -1;
						isAggro = true;
					} else if (bubble.ai[0] != 0) {
						NPC.target = (int)bubble.ai[0] - 1;
						isAggro = true;
					}
				}
				if (isAggro) {
					NPC.aiStyle = NPCAIStyleID.None;
				}
			}
			return true;
		}
		public override void AI() {
			const float attack_range = 16 * 16;
			if (NPC.ai[3] == 0) {
				NPC.defense = NPC.defDefense;
				NPC.TargetClosestUpgraded();
				NPCAimedTarget target = NPC.GetTargetData();
				if (!target.Invalid) {
					if (NPC.DistanceSQ(target.Center) < attack_range * attack_range) {
						NPC.ai[3] = 1 + NPC.NewNPC(
							NPC.GetSource_FromAI(),
							(int)NPC.Center.X,
							(int)NPC.Center.Y,
							ModContent.NPCType<Amebeye_P>(),
							ai3: NPC.whoAmI
						);
					}
				}
			} else if (NPC.aiStyle == NPCAIStyleID.None) {
				NPC.rotation = NPC.velocity.X * 0.1f;
				NPCAimedTarget target = NPC.GetTargetData();
				Vector2 vectorToTargetPosition = target.Center - NPC.Center;
				float speed = 8f;
				float inertia = 16f;
				vectorToTargetPosition.Normalize();
				vectorToTargetPosition *= speed;
				NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 68) % 272, 72, 66);
				NPC.frameCounter = 0;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) {
					Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
				}
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
			NPC.frameCounter = 0;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Glowing_Mod_NPC.DrawGlow(spriteBatch, screenPos, glowTexture, NPC, Riven_Hive.GetGlowAlpha(drawColor));
		}
	}
	public class Amebeye_P : ModNPC, IRivenEnemy {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.ProjectileNPC[Type] = true;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			ID = Type;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.IchorSticker);
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 90;
			NPC.defense = 0;
			NPC.damage = 14;
			NPC.width = 58;
			NPC.height = 58;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath15;
			NPC.knockBackResist = 0.01f;
			NPC.value = 0;
		}
		public override void AI() {
			NPC parent = Main.npc[(int)NPC.ai[3]];
			NPCAimedTarget target;
			bool returning = false;
			if (!parent.active || parent.type != Amebeye.ID) {
				if (Main.netMode != NetmodeID.MultiplayerClient) NPC.StrikeInstantKill();
				return;
			} else if (NPC.ai[0] != 0) {
				NPC.target = (int)NPC.ai[0] - 1;
				target = NPC.GetTargetData();
				if (NPC.ai[1] < 1) NPC.ai[1] += 0.05f;
				NPC.Center = Vector2.Lerp(NPC.Center, target.Center, NPC.ai[1]);
				if (target.Type == NPCTargetType.Player) {
					Player playerTarget =  Main.player[NPC.target];
					playerTarget.GetModPlayer<OriginPlayer>().RivenAssimilation += 0.05f / 60f;
					playerTarget.velocity *= 0.95f;
				}
				NPC.defense = 40;
				return;
			}
			NPC.defense = NPC.defDefense;

			target = NPC.GetTargetData();
			Vector2 targetPosition = target.Center;
			NPC.ai[1] = 0;
			if (++NPC.ai[2] >= 450) {
				targetPosition = parent.Center;
				returning = true;
			}
			Vector2 vectorToTargetPosition = targetPosition - NPC.Center;
			if (returning && vectorToTargetPosition.LengthSquared() < 16 * 16) {
				parent.ai[3] = 0;
				NPC.active = false;
			}
			float speed = 8f;
			float inertia = 18f;
			vectorToTargetPosition.Normalize();
			vectorToTargetPosition *= speed;
			NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;

			if (++NPC.frameCounter > 6) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 60) % 240, 58, 58);
				NPC.frameCounter = 0;
			}
			NPC.spriteDirection = 1;
		}
		public override Color? GetAlpha(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (NPC.ai[0] == 0) {
				NPC.ai[0] = target.whoAmI + 1;
			}
		}
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			return target.whoAmI + 1 != NPC.ai[0];
		}
		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = NPC.life < 0 ? 6 : 2; i-->0;) {
				Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
			}
		}
	}
}

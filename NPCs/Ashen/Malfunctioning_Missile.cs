using CalamityMod.Projectiles.Pets;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Ashen {
	public class Malfunctioning_Missile : Glowing_Mod_NPC, IAshenEnemy, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 2, 76, 76);
		public int AnimationFrames => 4;
		public int FrameDuration => 5;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				Scale = 0.85f,
				PortraitScale = 1,
				Rotation = MathHelper.PiOver4 * 3,
				Position = new Vector2(-3, 8),
				PortraitPositionXOverride = 0,
				PortraitPositionYOverride = 0
			};
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.CursedHammer);
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 175;
			NPC.defense = 16;
			NPC.damage = 85;
			NPC.width = 40;
			NPC.height = 40;
			NPC.knockBackResist = 0.35f;
			NPC.value = 1000;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Ashen_Biome>().Type
			];
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.MeatGrinder, 200));
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Nazar, 100));
		}
		public override void AI() {
			if (NPC.ai[0] <= 1) {
				Dust dust = Dust.NewDustPerfect(
					25 * NPC.rotation.ToRotationVector2() + NPC.Center,
					DustID.Torch,
					default,
					120,
					Color.Orange,
					1.25f
				);
			}
			NPCAimedTarget target = NPC.GetTargetData();
			if (!NPC.HasValidTarget) {
				NPC.TargetClosest();
				target = NPC.GetTargetData();
			}

			switch ((int)NPC.ai[0]) {
				case 0: {
					SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot.WithPitch(-1f), NPC.Center);
					float speed = 11f;
					Vector2 diff = (target.Center - NPC.Center).Normalized(out _);
					NPC.velocity = diff * speed;
					NPC.rotation = NPC.velocity.ToRotation() + MathHelper.Pi;
					NPC.ai[0] = 1f;
					NPC.ai[1] = 0f;
					NPC.netUpdate = true;
					break;
				}
				case 1: {
					if (NPC.justHit) {
						NPC.ai[0] = 2f;
						NPC.ai[1] = 0f;
					}

					NPC.ai[1] += 1f;
					if (NPC.ai[1] >= 60f) {
						NPC.velocity *= 0.97f;
						if (NPC.ai[1] >= 100f) {
							SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen.WithPitch(2f), NPC.Center);
							SoundEngine.PlaySound(SoundID.Item66.WithPitch(-0.5f).WithVolume(0.2f), NPC.Center);
							NPC.netUpdate = true;
							NPC.ai[0] = 2f;
							NPC.ai[1] = 0f;
							NPC.ai[2] = Main.rand.Next(10000);
							NPC.velocity.X = 0f;
							NPC.velocity.Y = 0f;
						}
					}
					NPC.rotation = NPC.velocity.ToRotation() + MathHelper.Pi;
					break;
				}
				case 2: {
					if (NPC.justHit) {
						NPC.ai[0] = 3f;
						NPC.ai[1] = 0f;
						NPC.ai[2] = Main.rand.Next(10000);
						NPC.netUpdate = true;
						break;
					}

					NPC.velocity *= 0.96f;
					NPC.ai[1] += 1f;
					float squiggle = GetSquiggle() * (NPC.ai[1] / 120f);
					squiggle = 0.1f + squiggle * 0.4f;
					NPC.rotation = (target.Center - NPC.Center).ToRotation() + squiggle + MathHelper.Pi;
					if (NPC.ai[1] >= 120f) {
						NPC.netUpdate = true;
						NPC.ai[0] = 0f;
						NPC.ai[1] = 0f;
					}
					break;
				}
				case 3: {
					float speed = 13f;
					NPC.velocity = (NPC.rotation + MathHelper.Pi).ToRotationVector2() * speed;
					NPC.ai[0] = 4f;
					NPC.ai[1] = 0f;
					NPC.netUpdate = true;
					break;
				}
				case 4: {
					SoundEngine.PlaySound(Origins.Sounds.RepairboyDeath.WithPitch(1.8f).WithVolume(0.2f), NPC.Center);
					if (MathUtils.LinearSmoothing(ref NPC.ai[3], 0, 0.01f)) NPC.ai[3] = GetSquiggle() * 0.1f;
					NPC.rotation += NPC.ai[3];
					NPC.velocity = NPC.velocity.RotatedBy(NPC.ai[3]);
					NPC.ai[1] += 1f;
					if (NPC.ai[1] >= 60f) {
						NPC.velocity *= 0.97f;
						if (NPC.ai[1] >= 100f) {
							NPC.netUpdate = true;
							NPC.ai[0] = 2f;
							NPC.ai[1] = 0f;
							NPC.ai[2] = Main.rand.Next(10000);
							NPC.velocity.X = 0f;
							NPC.velocity.Y = 0f;
						}
					}
					break;
				}
			}
		}
		public float GetSquiggle() {
			FastRandom rand = new((int)NPC.ai[2]);
			NPC.ai[2] = rand.Next(10000);
			return rand.NextFloat() * (rand.Next(2) > 0).ToDirectionInt();
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.ai[0] == 2) {
				NPC.frameCounter = 0.0;
				NPC.frame.Y = 38 * 3;
				return;
			}
			NPC.DoFrames(4, ..3);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			SpriteEffects effects = NPC.rotation is >= MathHelper.PiOver2 and < MathHelper.PiOver2 * 3 ? SpriteEffects.FlipVertically : SpriteEffects.None;

			spriteBatch.DrawGlowingNPCPart(
				texture,
				GlowTexture,
				NPC.Center - screenPos,
				NPC.frame,
				NPC.GetTintColor(drawColor),
				GetGlowColor(drawColor),
				NPC.rotation,
				NPC.frame.Size() * 0.5f,
				NPC.scale,
				effects);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
	}
}

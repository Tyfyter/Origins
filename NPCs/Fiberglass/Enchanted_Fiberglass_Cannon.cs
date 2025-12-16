using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Ranged;
using Origins.LootConditions;
using Origins.Projectiles;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using PegasusLib;
using Mono.Cecil;
using ThoriumMod.Empowerments;
using Terraria.Audio;
using Origins.Dusts;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Tiles.Other;
using Terraria.GameContent.Bestiary;

namespace Origins.NPCs.Fiberglass {
	public class Enchanted_Fiberglass_Cannon : ModNPC, IWikiNPC {
		readonly Color[] oldColor = new Color[10];
		readonly int[] oldDir = new int[10];
		public Rectangle DrawRect => new(0, 0, 52, 26);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.TrailingMode[NPC.type] = 3;
		}
		public override void SetDefaults() {
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;
			NPC.damage = 10;
			NPC.life = NPC.lifeMax = 57;
			NPC.defense = 10;
			NPC.noGravity = true;
			NPC.width = NPC.height = 27;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0.4f;
			NPC.value = 500f;
			SpawnModBiomes = [
				ModContent.GetInstance<Fiberglass_Undergrowth>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void AI() {
			NPC.velocity *= 0.85f;
			NPC.TargetClosest();
			NPC.spriteDirection = NPC.direction;
			float? angle = GeometryUtils.AngleToTarget(Main.player[NPC.target].Center - NPC.Center, 11f, grav: 0.08f);
			if (NPC.localAI[0] > 120 || (angle.HasValue && Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1) && Main.netMode != NetmodeID.MultiplayerClient)) {
				NPC.rotation = angle ?? NPC.rotation;
				NPC.localAI[0] += 1f;
				if (NPC.rotation == NPC.oldRot[0]) NPC.localAI[0] += 2f;
				if (NPC.localAI[0] >= 360f) {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						Vector2 velocity = GeometryUtils.Vec2FromPolar(11, NPC.rotation);
						int type = ModContent.ProjectileType<Enchanted_Fiberglass_Cannon_P>();
						for (int i = 4; i-- > 0;) {
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								NPC.Center,
								velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.9f, 1f),
								type,
								(int)(60 * ContentExtensions.DifficultyDamageMultiplier),
								1
							);
						}
						NPC.life = 0;
						NPC.value = 0;
						NPC.checkDead();
					}
					SoundEngine.PlaySound(SoundID.Item62.WithPitch(0.4f), NPC.Center);
				}
				if (angle.HasValue && NPC.spriteDirection != 1) NPC.rotation += MathHelper.Pi;
			} else NPC.localAI[0] = 0f;
			if (NPC.localAI[0] > 120) {
				Dust.NewDustPerfect(
					NPC.Center + new Vector2(-14 * NPC.direction, -2).RotatedBy(NPC.rotation, Vector2.UnitY * 2),
					ModContent.DustType<Fuse_Dust>(),
					Main.rand.NextVector2Circular(2, 2),
					newColor: new Color(50, 180, 255, 0),
					Scale: 0.5f
				);
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.ByCondition(new AnyPlayerInteraction(), ModContent.ItemType<Shaped_Glass>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Item>(), 10, 3, 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.Vine, 7));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Shard>(), 1, 1, 7));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			NPC.velocity.X += hit.HitDirection * 3;
			if (hit.Damage > NPC.life * 2f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
			if (NPC.life <= 0) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG1_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG2_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG3_Gore");
			} else if (hit.Damage > NPC.lifeMax * 0.5f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			for (int i = NPC.oldPos.Length - 1; i > 0; i--) {
				spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.oldPos[i] + new Vector2(13.5f, 19) - Main.screenPosition, new Rectangle(0, 0, 38, 22), oldColor[i].MultiplyRGBA(new Color(new Vector4(1 - i / 10f))), NPC.oldRot[i], new Vector2(19, 11), 1f, oldDir[i] != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1f);
				oldDir[i] = oldDir[i - 1];
				oldColor[i] = oldColor[i - 1];
			}
			oldDir[0] = NPC.spriteDirection;
			oldColor[0] = drawColor;
			return true;
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this);
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
	}
	public class Enchanted_Fiberglass_Cannon_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.timeLeft = 120;
			Projectile.scale = 0.85f;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 0;
			Projectile.friendly = false;
			Projectile.hostile = true;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.08f;
			Projectile.rotation += Projectile.velocity.X * 0.05f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Projectile.timeLeft > 0) {
				target.immune = true;
				Projectile.playerImmune[target.whoAmI] += 1;
				Projectile.Kill();
			}
		}
		public override void OnKill(int timeLeft) {
			Projectile.hostile = true;
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, sound: SoundID.Item62);
		}
	}
	public class Fuse_Dust : ModDust {
		public override string Texture => typeof(Flare_Dust).GetDefaultTMLName();
		public override bool Update(Dust dust) {
			dust.fadeIn--;
			if (!dust.noLight && !dust.noLightEmittence) {
				float scale = dust.scale;
				if (scale > 1f) scale = 1f;
				Lighting.AddLight(dust.position, dust.color.ToVector3() * scale * 0.25f);
			}
			if (dust.noGravity) {
				dust.velocity *= 0.93f;
				if (dust.fadeIn == 0f) {
					dust.scale += 0.0025f;
				}
			} else {
				float speed = dust.velocity.Length();
				dust.velocity -= (dust.velocity / speed) * Math.Max(speed, 2);
				dust.scale -= 0.001f;
				dust.velocity.Y += 0.02f;
				dust.scale *= Math.Min(speed * 3, 1);
			}
			if (WorldGen.SolidTile(Framing.GetTileSafely(dust.position)) && !dust.noGravity) {
				dust.scale *= 0.9f;
				dust.velocity *= 0.25f;
			}
			dust.position += dust.velocity;
			return true;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) {
			return dust.color with { A = 25 };
		}
		public override bool PreDraw(Dust dust) {
			float trail = Math.Abs(dust.velocity.X) + Math.Abs(dust.velocity.Y);
			trail *= 0.3f;
			trail *= 5f;
			if (trail > 5f) trail = 5f;
			if (trail > -dust.fadeIn) trail = -dust.fadeIn;
			Vector2 origin = new(4f, 4f);
			Color color = dust.GetAlpha(Lighting.GetColor((int)(dust.position.X + 4f) / 16, (int)(dust.position.Y + 4f) / 16));
			for (int k = 0; k < trail; k++) {
				Vector2 pos = dust.position - dust.velocity * k;
				float scale = dust.scale * (1f - k / 10f);
				Main.spriteBatch.Draw(TextureAssets.Dust.Value, pos - Main.screenPosition, dust.frame, color, dust.rotation, origin, scale, SpriteEffects.None, 0f);
			}
			return false;
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Armor.Riven;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs.Brine.Boss;
using Origins.NPCs.Brine;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Flagellant : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC {
		public static int SpinDuration => 60;
		public Rectangle DrawRect => new(0, 0, 56, 60);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public AssimilationAmount? Assimilation => 0.11f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 5;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BloodJelly);
			NPC.lifeMax = 135;
			NPC.defense = 14;
			NPC.damage = 35;
			NPC.width = 50;
			NPC.height = 74;
			NPC.value = 500;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type,
				ModContent.GetInstance<Riven_Hive_Ocean>().Type
			];
		}
		public override bool PreAI() {
			if (NPC.ai[1] != 0) {
				int direction = Math.Sign(NPC.ai[1]);
				float rot = MathHelper.TwoPi / SpinDuration;
				NPC.rotation += direction * rot;
				NPC.ai[1] -= direction;
				if (direction != Math.Sign(NPC.ai[1])) NPC.ai[1] = 0;
				return false;
			} else if (NPC.wet) {
				Player target = Main.player[NPC.target];
				if (NPC.HasValidTarget && target.Center.IsWithin(NPC.Center, 150f) && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height)) {
					NPC.ai[2]++;
				} else if (NPC.ai[2] > 0) {
					NPC.ai[2] -= 0.125f;
				} else {
					NPC.ai[2] = 0;
				}
				if (NPC.ai[2] > 60) {
					NPC.ai[1] = SpinDuration * Math.Sign(target.Center.X - NPC.Center.X);
					NPC.ai[2] = 0;

					const float range_multiplier = 2.5f;
					const int delay = 5;
					NPC.SpawnProjectile(null,
						NPC.Center,
						Vector2.UnitX * range_multiplier,
						ModContent.ProjectileType<Flagellant_Whip>(),
						0,
						0,
						-delay,
						ai2: NPC.whoAmI
					);
					NPC.SpawnProjectile(null,
						NPC.Center,
						Vector2.UnitX * range_multiplier,
						ModContent.ProjectileType<Flagellant_Whip>(),
						0,
						0,
						ai2: NPC.whoAmI
					);
					NPC.SpawnProjectile(null,
						NPC.Center,
						Vector2.UnitX * range_multiplier,
						ModContent.ProjectileType<Flagellant_Whip>(),
						0,
						0,
						delay,
						ai2: NPC.whoAmI
					);
				}
			}
			NPC.DoJellyfishAI(canDoZappy: false);
			return false;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			if (NPC.ai[1] == 0) {
				NPC.DoFrames(6, ..4);
			} else {
				NPC.frame.Y = NPC.frame.Height * 4;
			}
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.Water) return 0f;
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Flajelly;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 17));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Flagellash>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 180, 240, targetSeverity: 0.4f);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
	}
	public class Flagellant_Whip : ModProjectile {
		public static int UseTime => Flagellant.SpinDuration;
		public override string Texture => typeof(Flagellash_P).GetDefaultTMLName();
		public NPC Owner => Main.npc[(int)Projectile.ai[2]];
		public override void SetStaticDefaults() {
			Amebic_Vial.CanBeDeflected[Type] = false;
		}
		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.aiStyle = -1;
			Projectile.ownerHitCheck = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.WhipSettings.Segments = 20;
			Projectile.WhipSettings.RangeMultiplier = 0.9f * Projectile.scale;
			Projectile.direction = 0;
			Projectile.manualDirectionChange = true;
		}
		public override void AI() {
			NPC owner = Owner;
			if (Projectile.direction == 0) Projectile.direction = owner.direction;
			Projectile.velocity = (owner.rotation + MathHelper.PiOver2).ToRotationVector2() * Projectile.velocity.Length();
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.ai[0] += 1f;
			float timeToFlyOut = UseTime * Projectile.MaxUpdates;
			Vector2 handPosition = owner.Center;
			Projectile.Center = handPosition + Projectile.velocity * (Projectile.ai[0] - 1f);
			Projectile.spriteDirection = Projectile.direction;
			if (Projectile.ai[0] >= timeToFlyOut) {
				Projectile.Kill();
				return;
			}
			if (Projectile.ai[0] >= (int)(timeToFlyOut / 2f) && Projectile.soundDelay >= 0) {
				Projectile.soundDelay = -1;
				Projectile.WhipPointsForCollision.Clear();
				FillWhipControlPoints(Projectile, handPosition, Projectile.WhipPointsForCollision);
				Vector2 vector = Projectile.WhipPointsForCollision[^1];
				SoundEngine.PlaySound(SoundID.Item153, vector);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.ai[0] < 0) return false;
			Projectile.WhipPointsForCollision.Clear();
			NPC owner = Owner;
			Vector2 handPosition = owner.Center;
			if (owner.ModNPC is Lost_Diver lostDiver) {
				handPosition = lostDiver.GetHandPosition();
			}
			FillWhipControlPoints(Projectile, handPosition, Projectile.WhipPointsForCollision);
			for (int m = 0; m < Projectile.WhipPointsForCollision.Count; m++) {
				Point point = Projectile.WhipPointsForCollision[m].ToPoint();
				projHitbox.Location = new Point(point.X - projHitbox.Width / 2, point.Y - projHitbox.Height / 2);
				if (projHitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Flagellash_Buff_0.ID + (int)Projectile.ai[1], 240);
			Projectile.damage = (int)(Projectile.damage * 0.68);
		}

		// This method draws a line between all points of the whip, in case there's empty space between the sprites.
		private void DrawLine(List<Vector2> list) {
			//Texture2D texture = TextureAssets.FishingLine.Value;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new Vector2(texture.Width / 2, 3);
			int progress = -2;
			Vector2 pos = list[0];
			for (int i = 0; i < list.Count; i++) {
				Vector2 element = list[i];
				Vector2 diff;
				if (i == list.Count - 1) {
					diff = element - list[i - 1];
				} else {
					diff = list[i + 1] - element;
				}
				if (diff.HasNaNs()) {
					continue;
				}

				float dist = (diff.Length() + 2);
				if (progress + dist >= texture.Width - 2) {
					progress = 0;
				}
				if (i == list.Count - 1) {
					progress = texture.Width - (int)dist;
				}
				Rectangle frame = new Rectangle(0, progress + 2, 6, (int)dist);
				progress += (int)dist;
				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				Color color = Color.Lerp(Lighting.GetColor(element.ToTileCoordinates()), Color.White, 0.85f);
				Vector2 scale = Vector2.One;//new Vector2(1, dist / frame.Height);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation + MathHelper.Pi, origin, scale, SpriteEffects.None, 0);

				pos += diff;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.ai[0] < 0) return false;
			List<Vector2> list = [];
			FillWhipControlPoints(Projectile, Owner.Center, list);
			DrawLine(list);
			return false;
		}
		public static void FillWhipControlPoints(Projectile proj, Vector2 playerArmPosition, List<Vector2> controlPoints) {
			int timeToFlyOut = UseTime * proj.MaxUpdates;
			int segments = proj.WhipSettings.Segments;
			float rangeMultiplier = proj.WhipSettings.RangeMultiplier;
			float num = proj.ai[0] / timeToFlyOut;
			float num2 = 0.5f;
			float num3 = 1f + num2;
			float num4 = MathHelper.Pi * 10f * (1f - num * num3) * (-proj.spriteDirection) / segments;
			float num5 = num * num3;
			float num6 = 0f;
			if (num5 > 1f) {
				num6 = (num5 - 1f) / num2;
				num5 = MathHelper.Lerp(1f, 0f, num6);
			}
			float num7 = (UseTime * 2) * num;
			float num8 = proj.velocity.Length() * num7 * num5 * rangeMultiplier / segments;
			float num9 = 1f;
			Vector2 vector = playerArmPosition;
			float num10 = -MathHelper.PiOver2;
			Vector2 vector2 = vector;
			float num11 = MathHelper.PiOver2 + MathHelper.PiOver2 * proj.spriteDirection;
			Vector2 vector3 = vector;
			float num12 = MathHelper.PiOver2;
			controlPoints.Add(playerArmPosition);
			for (int i = 0; i < segments; i++) {
				float num13 = i / (float)segments;
				float num14 = num4 * num13 * num9;
				Vector2 vector4 = vector + num10.ToRotationVector2() * num8;
				Vector2 vector5 = vector3 + num12.ToRotationVector2() * (num8 * 2f);
				Vector2 vector6 = vector2 + num11.ToRotationVector2() * (num8 * 2f);
				float num15 = 1f - num5;
				float num16 = 1f - num15 * num15;
				Vector2 value = Vector2.Lerp(vector5, vector4, num16 * 0.9f + 0.1f);
				Vector2 vector7 = Vector2.Lerp(vector6, value, num16 * 0.7f + 0.3f);
				Vector2 spinningpoint = playerArmPosition + (vector7 - playerArmPosition) * new Vector2(1f, num3);
				float num17 = num6;
				num17 *= num17;
				Vector2 item = spinningpoint.RotatedBy(proj.rotation + 4.712389f * num17 * proj.spriteDirection, playerArmPosition);
				controlPoints.Add(item);
				num10 += num14;
				num12 += num14;
				num11 += num14;
				vector = vector4;
				vector3 = vector5;
				vector2 = vector6;
			}
		}
	}
}

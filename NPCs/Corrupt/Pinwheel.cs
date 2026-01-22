using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Corrupt {
	public class Pinwheel : ModNPC, IWikiNPC {
		public float KnockbackMult => DigState == state_air ? 1 : 0.5f;
		public Rectangle DrawRect => new(0, 0, 30, 30);
		public int AnimationFrames => 6;
		public int FrameDuration => 3;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			CorruptGlobalNPC.NPCTypes.Add(Type);
			AssimilationLoader.AddNPCAssimilation<Corrupt_Assimilation>(Type, 0.03f);
		}
		public override void SetDefaults() {
			NPC.lifeMax = 85;
			NPC.defense = 6;
			NPC.damage = 16;
			NPC.width = 38;
			NPC.height = 38;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = KnockbackMult;
			NPC.value = 61;
			NPC.behindTiles = true;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneSkyHeight) return 0;
			return 0.07f * (spawnInfo.Player.position.Y / 16 > Main.rockLayer ? 2 : 1);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption
			);
		}
		public float DigTime = 60 * 0.5f;
		public override void OnSpawn(IEntitySource source) {
			NPC.ai[0] = DigTime;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.RottenChunk, 3));
			npcLoot.Add(ItemDropRule.Common(ItemID.DemoniteOre, 25));
		}
		Vector2 lastAIVelocity = Vector2.Zero;
		public ref float DigState => ref NPC.ai[1];
		public ref float Spin => ref NPC.ai[2];
		public const float state_air = 0;
		public const float state_normal = 1;
		public const float state_front_wall = 2;
		public const float state_back_wall = 3;
		public const float state_ceiling = 4;
		public override void AI() {
			NPC.TargetClosest();

			float acceleration = 0.2f;
			float air_deceleration_flat = 0.02f;
			float air_deceleration_mult = 0.99f;
			float max_speed = 6f;
			#region Movement
			bool justKnocked = NPC.velocity != lastAIVelocity;
			if (justKnocked) {
				Spin = NPC.velocity.X;
				NPC.collideY = NPC.collideX = false;
			}
			if (DigState == state_ceiling || NPC.collideY || NPC.collideX) {
				Spin += acceleration * NPC.direction;
				if (Spin * NPC.direction > max_speed) Spin = max_speed * NPC.direction;
				if (DigState == state_ceiling || (NPC.collideY && NPC.collideX && NPC.oldVelocity.Y < 0)) {
					if (NPC.collideY) {
						DigState = state_ceiling;
						NPC.velocity.X = -Spin;
						Vector2 climbUp = Vector2.UnitY * NPC.direction * -Spin;
						NPC.velocity.Y = Collision.TileCollision(NPC.position, climbUp, NPC.width, NPC.height, true, true).Y;
						Min(ref NPC.velocity.Y, -2);
						NPC.GravityMultiplier *= -1;
					} else {
						DigState = state_air;
						Vector2 climbUp = Vector2.UnitX * NPC.direction * Spin;
						if (climbUp.TrySet(Collision.TileCollision(NPC.position, climbUp, NPC.width, NPC.height, true, true))) {
							if (DigState.TrySet(state_normal)) NPC.position += climbUp;
							NPC.velocity.X = Spin;
							NPC.velocity.Y = -1;
						}
					}
				} else if (NPC.collideX) {
					NPC.velocity.Y = Spin * -NPC.direction;
					if (Math.Abs(NPC.velocity.X) < 1) NPC.velocity.X = NPC.direction;
					DigState = state_normal;
				} else {
					NPC.velocity.X = Spin;
					DigState = state_normal;
				}
			} else {
				if (justKnocked) DigState = state_air;
				bool hasTarget = NPC.target >= 0;
				bool fall = hasTarget && NPC.targetRect.Bottom().Y - NPC.velocity.Y > NPC.Bottom.Y;
				Vector2 climbDown = Vector2.UnitX * NPC.direction * -Spin;
				if (DigState == state_air) {
					Spin *= air_deceleration_mult;
					MathUtils.LinearSmoothing(ref Spin, 0, air_deceleration_flat);
					DigState = state_air;
				} else if (NPC.velocity.Y < 0 && Math.Abs(NPC.velocity.X) == 1) {
					NPC.velocity.X = Spin;
					NPC.velocity.Y = 0;
					DigState = state_front_wall;
				} else if (climbDown.TrySet(Collision.TileCollision(NPC.position, climbDown, NPC.width, NPC.height, fall, fall))) {
					if (NPC.targetRect.Y > NPC.Bottom.Y) {
						if (DigState.TrySet(state_back_wall)) NPC.position += climbDown;
						NPC.velocity.X = 0;
						NPC.velocity.Y = Spin;
					} else {
						NPC.velocity.X = Spin;
						NPC.velocity.Y = -Spin;
						DigState = state_air;
					}
				} else {
					DigState = state_air;
				}
			}
			NPC.knockBackResist = KnockbackMult;
			/*if (NPC.collideY) {
				if (!wasCollideY || DigState < bladeOffsetMax) {
					NPC.netUpdate = true;
					if (MathUtils.LinearSmoothing(ref DigState, bladeOffsetMax, acceleration / 4)) { // the value after "acc / " is the speed multiplier for burrowing the blade
						wasCollideY = true;
					}
					if (DigState <= -bladeAllowedOut) {
						NPC.rotation += ((acceleration * 3) / NPC.width) * NPC.direction;
						if (!NPC.collideX) NPC.velocity.X *= 0.1f * NPC.direction;
						int width = 60 - 40;
						for (int i = -width; i <= width; i += width / 16) {
							if (i % 4 != 0) continue;
							Vector2 pos = NPC.Bottom;
							pos.X += i;
							Tile tile = Framing.GetTileSafely(pos.ToTileCoordinates());
							if (tile.HasSolidTile() && Main.rand.NextBool(30)) Collision.HitTiles(tile.GetTilePosition().ToWorldCoordinates(), new(0, -2.5f), 0, 0);
						}
					}
				}
				if (!NPC.collideX) {
					NPC.velocity.X += acceleration * NPC.direction;
					if (NPC.velocity.X * NPC.direction > max_speed) NPC.velocity.X = max_speed * NPC.direction;
				}
				NPC.knockBackResist = 0.2f;
			} else {
				if (DigState > -bladeOffsetMax) MathUtils.LinearSmoothing(ref DigState, -bladeOffsetMax, acceleration / 6); // the value after "acc / " is the speed multiplier for unburrowing the blade
				wasCollideY = DigState > -bladeAllowedOut;
				NPC.netUpdate = true;
			}*/
			NPC.rotation += Spin / NPC.width; // I love radians
			#endregion Movement

			#region Dig up Dust
			if (Math.Abs(NPC.velocity.X) > 0.07f && NPC.collideY) {
				Vector2 pos = NPC.velocity.X < 0 ? NPC.BottomRight : NPC.BottomLeft;
				Tile tile = Framing.GetTileSafely((pos + Vector2.UnitY).ToTileCoordinates());

				if (tile.HasSolidTile()) {
					NPC.netUpdate = true;
					int extra = 0;
					Vector2 vel = new(0.6f * -NPC.velocity.X, -2.5f);
					if (NPC.ai[0]-- <= 0) {
						extra = 2;
						int type = Lingering_Shadowflame.ShadeIDs[Main.rand.Next(Lingering_Shadowflame.ShadeIDs.Count)];
						int dmg = 10;
						if (Main.hardMode && Main.expertMode) {// enemies only scale with progress in expert mode and higher
							type = Lingering_Shadowflame.CursedIDs[Main.rand.Next(Lingering_Shadowflame.CursedIDs.Count)];
							dmg = 20;
						}
						NPC.SpawnProjectile(NPC.GetSource_FromAI(), pos, vel, type, dmg, 0);
						NPC.ai[0] = DigTime;
					}
					if (Main.rand.NextBool(10) || extra > 0) {
						vel.Y *= 5;
						for (int i = 0; i < 1 + extra; i++) {
							Collision.HitTiles(tile.GetTilePosition().ToWorldCoordinates(), vel, 0, 0);
							Tile tile1 = Framing.GetTileSafely((NPC.Bottom + Vector2.UnitY).ToTileCoordinates());
							if (tile1.HasSolidTile()) Collision.HitTiles(tile1.GetTilePosition().ToWorldCoordinates(), vel, 0, 0);
						}
					}
				}
			}
			#endregion Dig up Dust
		}
		public override void FindFrame(int frameHeight) {
			lastAIVelocity = NPC.velocity; // oddly, this is run on the server too, so it can safely be used to store the velocity after gravity & collisions
			NPC.collideX = NPC.velocity.X != NPC.oldVelocity.X;
			NPC.collideY = NPC.velocity.Y != NPC.oldVelocity.Y;
			NPC.DoFrames(Main.rand.Next(3, 6)); // meant to simulate random blinking times
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			if (NPC.IsABestiaryIconDummy) {
				drawColor = Color.White;
			}
			Main.EntitySpriteDraw(
				texture,
				NPC.Center - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				NPC.frame.Size() * 0.5f,
				NPC.scale,
				SpriteEffects.None
			);
			/*string debugText = ""; // debugging code
			if (NPC.IsABestiaryIconDummy) {
				debugText = $"Color: {drawColor}\n       Offset: {offset}\n            Rot: {NPC.rotation}";
			} else {
				debugText = $"AI: {NPC.aiStyle}, {AIType}, S/Dir: {NPC.spriteDirection}, {NPC.direction}\nOffset: {offset}\nVel: {NPC.velocity}, {Math.Abs(NPC.velocity.X)}, {Math.Abs(NPC.velocity.Y)}\nWasCollideY: {wasCollideY}\nTarget: {NPC.HasValidTarget}, {NPC.GetTargetData().Invalid}, {NPC.GetTargetData().Type}\nAI: {NPC.ai[0]}, {DigState}, {Spin}, {NPC.ai[3]}\nLAI: {NPC.ai[0]}, {DigState}, {Spin}, {NPC.ai[3]}";
			}
			OriginExtensions.DrawDebugTextAbove(spriteBatch, debugText, NPC.Top + offset - screenPos);*/
			//NPC.Hitbox.DrawDebugOutlineSprite(Color.White, -screenPos, false);
			return false;
		}
	}
	public abstract class Lingering_Shadowflame : ModProjectile {
		public abstract int BaseProj { get; }
		public override string Texture => $"Terraria/Images/Projectile_{BaseProj}";
		public virtual bool Hardmode => false;
		public static List<int> ShadeIDs = [];
		public static List<int> CursedIDs = [];
		public virtual int Flame => ModContent.BuffType<Shadefire_Debuff>();
		public virtual int FlameTime => 60;
		public override void SetStaticDefaults() {
			if (Hardmode) CursedIDs.Add(Type);
			else ShadeIDs.Add(Type);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(BaseProj);
			Projectile.timeLeft = 60 * 3;
		}
		public override void AI() {
			Color color = Hardmode ? Color.LimeGreen : Color.MediumPurple;
			Lighting.AddLight(Projectile.Center, color.ToVector3() * 1.2f);
			Dust.NewDustDirect(Projectile.TopLeft, Projectile.width, Projectile.height, ModContent.DustType<Flare_Dust>(), 0, -3, 0, color);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Flame, FlameTime);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(Flame, FlameTime);
		}
		public override bool PreDraw(ref Color lightColor) {
			lightColor = lightColor.MultiplyRGB(Hardmode ? Color.LimeGreen : Color.MediumPurple);
			return true;
		}
	}
	public class Lingering_Shadowflame1 : Lingering_Shadowflame {
		public override int BaseProj => ProjectileID.GreekFire1;
	}
	public class Lingering_Shadowflame2 : Lingering_Shadowflame {
		public override int BaseProj => ProjectileID.GreekFire2;
	}
	public class Lingering_Shadowflame3 : Lingering_Shadowflame {
		public override int BaseProj => ProjectileID.GreekFire3;
	}
	public class Lingering_Cursedflame1 : Lingering_Shadowflame {
		public override int BaseProj => ProjectileID.GreekFire1;
		public override bool Hardmode => true;
		public override int Flame => BuffID.CursedInferno;
	}
	public class Lingering_Cursedflame2 : Lingering_Cursedflame1 {
		public override int BaseProj => ProjectileID.GreekFire2;
	}
	public class Lingering_Cursedflame3 : Lingering_Cursedflame1 {
		public override int BaseProj => ProjectileID.GreekFire3;
	}
}

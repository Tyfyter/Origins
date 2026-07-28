//#define DRAWPLATFORM //uncomment this to see where the platform is
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.LootConditions;
using Origins.Projectiles;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	public class Scrapyard_Stryder : ModNPC, IWikiNPC, IAshenEnemy, IPlatformNPC, ISpecialTargetingNPC {
		public Rectangle DrawRect => new(0, 0, 34, 46);
		public int AnimationFrames => 3;
		public int FrameDuration => 3;
		public static int PowerUpTime => 18;
		public Vector2 PlatformOffset => new(NPC.spriteDirection * -28 - NPC.width * 0.5f, -22);
		public float PlatformWidth => 134;
		private Vector2 GunPos => NPC.Center + new Vector2(NPC.spriteDirection * 28, -20);
		Vector2 IPlatformNPC.OldPlatformPosition { get; set; }
		static AutoLoadingTexture glowTexture = typeof(Scrapyard_Stryder).GetDefaultTMLName("_Glow");
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with { Position = new(15, 45), PortraitPositionXOverride = -5, PortraitPositionYOverride = 0 };
			GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, Ashen_Biome.SpawnRates.ScrapyardStryder);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Unicorn;
			NPC.lifeMax = 475;
			NPC.defense = 24;
			NPC.damage = 48;
			NPC.width = 64;
			NPC.height = 64;
			NPC.value = 230;
			NPC.knockBackResist = 0.5f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			AIType = NPCID.Unicorn;
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public void TargetClosest(bool faceTarget = true, Vector2? checkPosition = null) {
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, SearchFilter);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (faceTarget && NPC.ShouldFaceTarget(ref searchResults)) NPC.FaceTarget();
			}
		}
		bool SearchFilter(Player player) => player?.OriginPlayer()?.standingOnPlatformNPC != NPC;
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override bool PreAI() {
			const float channel_max = 4;
			const float begin_shoot_dist = 16 * 16;
			const float cancel_shoot_dist = begin_shoot_dist * 2;
			NPCAimedTarget target = NPC.GetTargetData();
			Vector2 gunPos = GunPos;
			Vector2 targetPos = gunPos.Clamp(target.Hitbox);
			if (NPC.confused) targetPos = targetPos.RotatedBy(MathHelper.Pi, gunPos);
			switch ((int)NPC.ai[2]) {
				default:
				if (NPC.ai[2] == 0) {
					if ((targetPos.X - gunPos.X) * NPC.spriteDirection > 0 && targetPos.IsWithin(gunPos, begin_shoot_dist)) {
						NPC.ai[2] = Main.rand.NextBool().ToDirectionInt();
						if (NPC.ai[2] == 1) {
							NPC.ai[0] = 0;
							NPC.ai[1] = 0;
						}
					}
				} else if ((gunPos.X - targetPos.X) * NPC.spriteDirection > 0 || !targetPos.IsWithin(gunPos, cancel_shoot_dist)) NPC.ai[2] = 0;
				break;

				case 1:
				if (NPC.collideY) NPC.velocity.X *= 0.95f;
				if ((gunPos.X - targetPos.X) * NPC.spriteDirection > 0 || !targetPos.IsWithin(gunPos, cancel_shoot_dist)) {
					NPC.ai[2] = 0;
					NPC.ai[1] = 0;
					break;
				}
				Dust dust = Dust.NewDustDirect(gunPos - Vector2.One * 2, 0, 0, DustID.GoldFlame, 0, 0, 255, new Color(255, 150, 30));
				dust.noGravity = true;
				dust.velocity *= 3f;
				if (++NPC.ai[0] > 15) {
					NPC.ai[0] = 0;
					if (++NPC.ai[1] >= channel_max) {
						int projType;
						Vector2 velocity = Vector2.Zero;
						if (NPC.GetGlobalNPC<OriginGlobalNPC>().silencedDebuff) {
							NPC.ai[2] = 2;
							projType = ProjectileType<Abrasion_Blaster_Explosion_Hostile>();
						} else {
							NPC.ai[2] = 0;
							projType = ProjectileType<Abrasion_Blaster_Hostile>();
							velocity = (targetPos - gunPos).SafeNormalize(new Vector2(NPC.spriteDirection, 0)) * 12;
						}
						NPC.ai[0] = 0;
						Projectile.NewProjectile(
							NPC.GetSource_FromAI(),
							gunPos,
							velocity,
							projType,
							(int)(40 * ContentExtensions.DifficultyDamageMultiplier),
							12,
							ai1: NPC.whoAmI
						);
					}
				}
				return false;
				case 2:
				if (NPC.collideY) NPC.velocity.X *= 0.95f;
				if (++NPC.ai[0] > 30) {
					NPC.ai[2] = 0;
					NPC.ai[1] = 0;
				}
				return false;
			}
			return base.PreAI();
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.velocity.Y != 0) {
				NPC.frame.Y = NPC.frame.Height * 5;
				NPC.frameCounter = 0;
				return;
			}
			if (NPC.IsABestiaryIconDummy) NPC.DoFrames(16, 6);
			else if (NPC.ai[2] != 1 || Math.Abs(NPC.position.X - NPC.oldPosition.X) > 0.5f) NPC.DoFrames(16, (NPC.position.X - NPC.oldPosition.X) * NPC.direction);
			else {
				NPC.frame.Y = NPC.frame.Height * 5;
				NPC.frameCounter = 0;
			}
			NPC.spriteDirection = NPC.direction;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.MeatGrinder, 200));
			npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 5, amountDroppedMaximum: 11));
			npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ItemDropRule.Common(ItemType<Phoenum>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ItemType<The_Muffler>(), 80));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			base.HitEffect(hit);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Color glowColor = Color.White;
			NPCLoader.DrawEffects(NPC, ref glowColor);
			glowColor = NPC.GetNPCColorTintedByBuffs(glowColor);
			switch (NPC.ai[2]) {
				case -1:
				glowColor = Color.Lime;
				break;
				case 1:
				glowColor = Color.Blue;
				break;
			}
			spriteBatch.DrawGlowingNPCPart(
				TextureAssets.Npc[Type].Value,
				glowTexture,
				NPC.Bottom + Vector2.UnitY * 2 - screenPos,
				NPC.frame,
				NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor)),
				glowColor,
				NPC.rotation,
				NPC.frame.Size() * new Vector2(0.5f + (NPC.spriteDirection * 0.15f), 1),
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			if (NPC.ai[2] == 1) {
				Texture2D texture;
				Rectangle frame = default;
				int frameMax;
				if (NPC.ai[1] > 2) {
					texture = Abrasion_Blaster_Hostile.texture2;
					frame.Width = 18;
					frame.Height = 18;
					frameMax = 2;
				} else if (NPC.ai[1] > 1) {
					texture = Abrasion_Blaster_Hostile.texture1;
					frame.Width = 16;
					frame.Height = 16;
					frameMax = 3;
				} else {
					texture = Abrasion_Blaster_Hostile.texture0;
					frame.Width = 10;
					frame.Height = 10;
					frameMax = 4;
				}
				switch ((int)(++NPC.localAI[0] / frameMax % 4)) {
					case 1:
					frame.Y += frame.Height + 2;
					break;

					case 3:
					frame.Y += (frame.Height + 2) * 2;
					break;
				}
				Main.EntitySpriteDraw(
					texture,
					GunPos - Main.screenPosition,
					frame,
					new Color(1f, 1f, 1f, 0.8f),
					0,
					frame.Size() * 0.5f,
					1,
					0
				);
			}
			NPC.DrawConfused();
			return false;
		}
#if DRAWPLATFORM
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 platformStart = NPC.position + PlatformOffset - screenPos;
			OriginExtensions.DrawDebugLineSprite(platformStart, platformStart + PlatformWidth * Vector2.UnitX, Color.Red);
		}
		struct DebugFlag : IDebugFlag;
#endif

		public class Abrasion_Blaster_Hostile : ModProjectile {
			public static AutoLoadingTexture texture0 = typeof(Abrasion_Blaster).GetDefaultTMLName() + "_Charge1";
			public static AutoLoadingTexture texture1 = typeof(Abrasion_Blaster).GetDefaultTMLName() + "_Charge2";
			public static AutoLoadingTexture texture2 = typeof(Abrasion_Blaster).GetDefaultTMLName() + "_Charge3";
			public override string Texture => typeof(Abrasion_Blaster).GetDefaultTMLName();
			public override void SetDefaults() {
				Projectile.width = 18;
				Projectile.height = 18;
				Projectile.hostile = true;
				Projectile.tileCollide = true;
				Projectile.extraUpdates = 1;
				Projectile.appliesImmunityTimeOnSingleHits = true;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.localNPCHitCooldown = -1;
			}
			public override void AI() {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0, 0, 255, new Color(255, 150, 30));
				dust.position = Projectile.Center;
				dust.noGravity = true;
			}
			public override void OnHitPlayer(Player target, Player.HurtInfo info) {
				Projectile.Kill();
			}
			public override void OnKill(int timeLeft) {
				if (Projectile.IsLocallyOwned()) {
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						default,
						ProjectileType<Abrasion_Blaster_Explosion_Hostile>(),
						Projectile.damage,
						Projectile.knockBack,
						ai1: -1
					);
				}
			}
			public override bool PreDraw(ref Color lightColor) {
				Texture2D texture;
				Rectangle frame = default;
				int frameMax;
				if (Projectile.ai[0] > 5) {
					texture = texture2;
					frame.Width = 18;
					frame.Height = 18;
					frameMax = 2;
				} else if (Projectile.ai[0] > 2) {
					texture = texture1;
					frame.Width = 16;
					frame.Height = 16;
					frameMax = 3;
				} else {
					texture = texture0;
					frame.Width = 10;
					frame.Height = 10;
					frameMax = 4;
				}
				if (++Projectile.frameCounter > frameMax) {
					Projectile.frameCounter = 0;
					Projectile.frame = (Projectile.frame + 1) & 0b11;
				}
				switch (Projectile.frame) {
					case 1:
					frame.Y += frame.Height + 2;
					break;

					case 3:
					frame.Y += (frame.Height + 2) * 2;
					break;
				}
				Main.EntitySpriteDraw(
					texture,
					Projectile.Center - Main.screenPosition,
					frame,
					new Color(1f, 1f, 1f, 0.8f),
					Projectile.rotation,
					frame.Size() * 0.5f,
					Projectile.scale,
					0
				);
				return false;
			}
		}
		public class Abrasion_Blaster_Explosion_Hostile : ExplosionProjectile {
			public override DamageClass DamageType => DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			public override int Size => 72;
			public override bool Hostile => true;
			public override bool? CanHitNPC(NPC target) {
				if (target.whoAmI == Projectile.ai[1]) return true;
				return base.CanHitNPC(target);
			}
			public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
				if (target.whoAmI == Projectile.ai[1]) modifiers.DefenseEffectiveness *= 0;
			}
		}
	}
}

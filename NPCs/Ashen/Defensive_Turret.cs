using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	[ReinitializeDuringResizeArrays]
	public class Defensive_Turret : ModNPC, IAshenEnemy, Repairboy.IReparable {
		public static bool[] OohShiny = ProjectileID.Sets.Factory.CreateNamedSet($"{nameof(Defensive_Turret)}_{nameof(OohShiny)}").RegisterBoolSet(
			ProjectileID.Flare,
			ProjectileID.BlueFlare,
			ProjectileID.CursedFlare,
			ProjectileID.RainbowFlare,
			ProjectileID.ShimmerFlare,
			ProjectileID.SpelunkerFlare
		);
		public static bool[] TargetProjectilesLow = ProjectileID.Sets.Factory.CreateNamedSet($"{nameof(Defensive_Turret)}_{nameof(TargetProjectilesLow)}").RegisterBoolSet(
			ProjectileID.Grenade,
			ProjectileID.StickyGrenade,
			ProjectileID.BouncyGrenade,
			ProjectileID.Beenade,
			ProjectileID.MolotovCocktail
		);
		static AutoLoadingTexture glowTexture = typeof(Defensive_Turret).GetDefaultTMLName("_Glow");
		static AutoLoadingTexture armTexture = typeof(Defensive_Turret).GetDefaultTMLName("_Arm");
		static AutoLoadingTexture gunTexture = typeof(Defensive_Turret).GetDefaultTMLName("_Gun");
		static AutoLoadingTexture gunGlowTexture = typeof(Defensive_Turret).GetDefaultTMLName("_Gun_Glow");
		public static float MaxTargetDist => 16 * 75;
		public static float ShotRate => 67 - ContentExtensions.DifficultyDamageMultiplier * 7;
		public static int ShotDamage => (int)(35 * ContentExtensions.DifficultyDamageMultiplier);
		public static float ShotVelocity => 12;
		public bool IsDeactivated {
			get => NPC.ai[2] != 0;
			set => NPC.ai[2] = value.ToInt();
		}
		ProjectileTargetIndex ProjectileTarget {
			get => NPC.target - 500;
			set => NPC.target = value + 500;
		}
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with {
				Rotation = MathHelper.Pi,
				Position = new(10, 50),
				PortraitPositionXOverride = 0,
				PortraitPositionYOverride = 10
			};
			NPCID.Sets.UsesNewTargetting[Type] = true;
			for (int i = ProjectileID.GrenadeI; i <= ProjectileID.ProximityMineIV; i++) TargetProjectilesLow[i] = true;
			for (int i = ProjectileID.ClusterRocketI; i <= ProjectileID.MiniNukeMineII; i++) TargetProjectilesLow[i] = true;
			for (int i = ProjectileID.ClusterSnowmanRocketI; i <= ProjectileID.DrySnowmanRocket; i++) TargetProjectilesLow[i] = true;
			Projectile probe = new();
			for (int i = 0; i < ProjectileLoader.ProjectileCount; i++) {
				if (TargetProjectilesLow[i]) continue;
				probe.SetDefaults(i);
				if (probe.aiStyle is ProjAIStyleID.Explosive or ProjAIStyleID.GolfBall) TargetProjectilesLow[i] = true;
			}
		}
		Vector2 GunOrigin => NPC.Top - new Vector2(NPC.spriteDirection * 10, 4);
		public override void SetDefaults() {
			NPC.lifeMax = 160;
			NPC.defense = 22;
			NPC.damage = 10;// contact damage, not shot damage
			NPC.width = 68;
			NPC.height = 68;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.knockBackResist = float.Epsilon;// 0 completely disables knockback, which would prevent us from using it to spawn gores
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public override bool PreAI() {
			NPC.velocity = default;
			return base.PreAI();
		}
		public override void AI() {
			NPC.velocity = default;
			NPC.dontTakeDamage = IsDeactivated;
			NPC.damage = NPC.dontTakeDamage ? 0 : NPC.defDamage;
			if (NPC.dontTakeDamage) {
				NPC.frame.Y = 4;
				NPC.spriteDirection = NPC.direction;
				return;
			} else if (NPC.frame.Y == 4) {
				NPC.frame.Y = 0;
			}
			if (NPC.frame.Y == 1) {
				Lighting.AddLight(
					GunOrigin + NPC.rotation.ToRotationVector2() * 20,
					Color.Orange.ToVector3()
				);
			}
			UpdateTarget();

			Vector2 diff = NPC.targetRect.Center() - NPC.Top;
			float angle = diff.ToRotation();
			GeometryUtils.AngularSmoothing(ref NPC.rotation, angle, 0.05f);
			NPC.spriteDirection = diff.X < 0 ? -1 : 1;
			if (NPC.frame.Y != 0 && NPC.frameCounter.CycleUp(4, 1)) NPC.frame.Y.CycleUp(4);
			NPC.ai[1].Cooldown();
			if (NPC.HasValidTarget || ProjectileTarget.HasTarget) {
				NPC.ai[0]++;
				float shotRate = ShotRate;
				if (GeometryUtils.AngleDif(NPC.rotation, angle, out _) < 0.5f) {
					if (!CollisionExt.CanHitRay(GunOrigin, GunOrigin.Clamp(NPC.targetRect))) {
						Min(ref NPC.ai[0], shotRate * 0.75f);
						NPC.ai[1] += 2;
					}
					if (NPC.ai[0] > shotRate) {
						NPC.ai[0] = 0;
						SoundEngine.PlaySound(Origins.Sounds.HeavyCannon.WithPitch(-0.5f), NPC.Top);
						Vector2 direction = NPC.rotation.ToRotationVector2();
						NPC.SpawnProjectile(null,
							GunOrigin + direction * 20,
							direction * ShotVelocity,
							ModContent.ProjectileType<Defensive_Turret_P>(),
							ShotDamage,
							1,
							NPC.target
						);
						NPC.frame.Y++;
					}
				} else {
					Min(ref NPC.ai[0], shotRate * 0.5f);
				}
			} else {
				NPC.ai[0] = 0;
			}
		}
		public void DoTargeting() {
			foreach (Projectile target in Main.ActiveProjectiles) {
				if (OohShiny[target.type] && SetTargetRect(target)) {
					ProjectileTarget = target;
					return;
				}
			}

			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, target => {
				if (!NPC.Center.WithinRange(target.Center, MaxTargetDist)) return false;
				return CollisionExt.CanHitRay(NPC.Center, target.Center);
			});
			if (!searchResults.FoundTarget) searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, target => NPC.Center.WithinRange(target.Center, MaxTargetDist * 0.25f));
			
			if (!searchResults.FoundTarget) searchResults = SearchForTarget(NPC, TargetSearchFlag.NPCs, npcFilter: target => {
				if (target.ModNPC is IAshenEnemy || !NPC.Center.WithinRange(target.Center, MaxTargetDist)) return false;
				return CollisionExt.CanHitRay(NPC.Center, target.Center);
			});
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
			} else {
				foreach (Projectile target in Main.ActiveProjectiles) {
					if (TargetProjectilesLow[target.type] && SetTargetRect(target)) {
						ProjectileTarget = target;
						return;
					}
				}
				NPC.target = -1;
				NPC.targetRect = NPC.Hitbox.Recentered(NPC.Top + 128 * NPC.direction * Vector2.UnitX);
			}
		}
		public void UpdateTarget() {
			if (ProjectileTarget.HasTarget && SetTargetRect(ProjectileTarget.GetProjectile())) return;
			if (!NPC.HasValidTarget || NPC.ai[0] == 0 || NPC.ai[1] >= 45 || NPC.justHit || !NPC.Center.WithinRange(NPC.targetRect.Center(), MaxTargetDist)) {
				NPC.ai[1] = 0;
				DoTargeting();
				return;
			}
			NPC.targetRect = NPC.GetTargetData().Hitbox;
		}
		bool SetTargetRect(Projectile target) {
			if (CanTargetProjectile(target)) return false;
			NPC.targetRect = target.Hitbox;
			float updateCount = (target.ModProjectile?.ShouldUpdatePosition() ?? true) ? target.MaxUpdates : 0;
			Utils.ChaseResults chase = Utils.GetChaseResults(GunOrigin, ShotVelocity * 3, target.Center, target.velocity * updateCount);
			if (chase.InterceptionHappens) NPC.targetRect = NPC.targetRect.Recentered(chase.InterceptionPosition);
			return true;
		}
		bool CanTargetProjectile(Projectile target) {
			if (target is null || !target.active || (!OohShiny[target.type] && !TargetProjectilesLow[target.type])) return false;
			Vector2 center = NPC.Center;
			Vector2 targetPos = center.Clamp(target.Hitbox);
			return center.WithinRange(targetPos, MaxTargetDist) && CollisionExt.CanHitRay(center, targetPos);
		}
		public override bool CheckDead() {
			Max(ref NPC.life, 1);
			IsDeactivated = true;
			return false;
		}
		public override bool NeedSaving() => true;
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			modifiers.Knockback = StatModifier.Default;// purely visual
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
		}
		public override void HitEffect(NPC.HitInfo hit) {
			Vector2 velocity = hit.GetKnockbackFromHit();
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), velocity, "Gores/NPCs/Ashen_Gore1");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), velocity, "Gores/NPCs/Ashen_Gore2");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), velocity, "Gores/NPCs/Ashen_Gore3");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), velocity, "Gores/NPCs/Ashen_Gore4");
				for (int i = 0; i < 7; i++) {
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
				}
			} else if (Main.rand.NextBool(5)) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
			NPC.velocity = default;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects effects = SpriteEffects.FlipHorizontally;
			float rotation = NPC.rotation;
			if (MathHelper.WrapAngle(NPC.rotation) is < -MathHelper.PiOver2 or >= MathHelper.PiOver2) {
				effects = SpriteEffects.None;
				rotation += MathHelper.Pi;
			}
			spriteBatch.DrawGlowingNPCPart(
				gunTexture,
				gunGlowTexture,
				GunOrigin - screenPos,
				gunTexture.Frame(verticalFrames: 5, frameY: NPC.frame.Y),
				drawColor,
				Color.White,
				rotation,
				new Vector2(80, 23).Apply(effects, gunTexture.Value.Size()),
				NPC.scale,
				effects
			);
			spriteBatch.Draw(
				armTexture,
				NPC.Bottom - screenPos,
				null,
				drawColor,
				0,
				new Vector2(-2, 93).Apply(effects, armTexture.Value.Size()),
				NPC.scale,
				effects,
			0);
			Rectangle frame = TextureAssets.Npc[Type].Frame(verticalFrames: 2, frameY: IsDeactivated.ToInt());
			spriteBatch.DrawGlowingNPCPart(
				TextureAssets.Npc[Type].Value,
				glowTexture,
				NPC.Bottom - screenPos,
				frame,
				drawColor,
				Color.White,
				0,
				frame.Size() * new Vector2(0.5f, 1),
				NPC.scale,
				SpriteEffects.None
			);
			return false;
		}
		public override void SaveData(TagCompound tag) {
			tag[nameof(IsDeactivated)] = NPC.ai[2];
			tag[nameof(NPC.direction)] = NPC.direction;
		}
		public override void LoadData(TagCompound tag) {
			tag.TryGet(nameof(IsDeactivated), out NPC.ai[2]);
			tag.TryGet(nameof(NPC.direction), out NPC.direction);
			if (IsDeactivated) NPC.life = 1;
			NPC.rotation = MathHelper.PiOver2 * (1 - NPC.direction);
		}
		public bool? NeedsRepair(NPC repairboy, ref float cost, ref Rectangle hitbox) => IsDeactivated ? true : null;
		public bool Repair(int repairAmount) {
			if (!IsDeactivated) return false;
			NPC.life += Main.rand.RandomRound(repairAmount * 0.05f);
			if (NPC.life >= NPC.lifeMax) {
				NPC.life = NPC.lifeMax;
				IsDeactivated = false;
			}
			return true;
		}
		public class Defensive_Turret_P : ModProjectile {
			public override string Texture => "Origins/Items/Weapons/Ammo/Metal_Slug_P";
			public static int ID { get; private set; }
			public override void SetStaticDefaults() {
				ProjectileID.Sets.TrailingMode[Type] = 2;
				ProjectileID.Sets.TrailCacheLength[Type] = 45;
				ProjectileID.Sets.DrawScreenCheckFluff[Type] = ProjectileID.Sets.TrailCacheLength[Type] * 10 + 64;
				ID = Type;
			}
			public override void SetDefaults() {
				OriginsSets.Projectiles.HomingEffectivenessMultiplier[Type] = 0.0125f;
				Projectile.aiStyle = 0;
				Projectile.extraUpdates = 2;
				Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				Projectile.width = 28;
				Projectile.height = 28;
				Projectile.hostile = true;
				Projectile.penetrate = 1;
				Projectile.timeLeft = 900;
				Projectile.scale = 0.85f;
				Projectile.appliesImmunityTimeOnSingleHits = true;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.localNPCHitCooldown = 6;
			}
			ProjectileTargetIndex ProjectileTarget => ((int)Projectile.ai[0]) - 500;
			public override bool OnTileCollide(Vector2 oldVelocity) {
				Projectile.velocity = default;
				Projectile.timeLeft = 1;
				return true;
			}
			public override void AI() {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				if (Projectile.soundDelay == 0) {
					Projectile.soundDelay = -1;
					Dust.NewDustPerfect(
						Projectile.Center + Projectile.velocity.Normalized(out _) * 120,
						ModContent.DustType<Rocket_Launch>(),
						Projectile.velocity,
						newColor: Color.DimGray
					);
				}
				if (ProjectileTarget.HasTarget) {
					Projectile flare = ProjectileTarget.GetProjectile();
					if (!flare.active || (!OohShiny[flare.type] && !TargetProjectilesLow[flare.type])) {
						Projectile.ai[0] = -1;
					} else {
						Rectangle hitbox = Projectile.Hitbox;
						hitbox.Inflate(32, 32);
						if (hitbox.Intersects(flare.Hitbox)) {
							Projectile.Kill();
						}
					}
				}
			}
			public override void OnKill(int timeLeft) {
				ExplosiveGlobalProjectile.DoExplosion(Projectile, 80, sound: SoundID.Item62, hostile: true);
				foreach (Projectile flare in Main.ActiveProjectiles) {
					Rectangle hitbox = Projectile.Hitbox;
					hitbox.Inflate(16, 16);
					if (flare.active && (OohShiny[flare.type] || TargetProjectilesLow[flare.type]) && hitbox.Intersects(flare.Hitbox)) {
						flare.Kill();
					}
				}
				if (NetmodeActive.Server) return;
				if (Projectile.owner != Main.myPlayer) {
					if (!Projectile.hide) {
						Projectile.hide = true;
						try {
							Projectile.active = true;
							Projectile.timeLeft = timeLeft;
							Projectile.Update(Projectile.whoAmI);
						} finally {
							Projectile.active = false;
							Projectile.timeLeft = 0;
						}
					}
					return;
				}
				Vector2[] oldPos = [.. Projectile.oldPos];
				float[] oldRot = [.. Projectile.oldRot];
				Vector2 halfSize = new(14);
				for (int i = 0; i < oldPos.Length; i++) {
					if (oldPos[i] == default) {
						Array.Resize(ref oldPos, i);
						Array.Resize(ref oldRot, i);
						break;
					}
					oldPos[i] += halfSize;
					oldRot[i] += MathHelper.PiOver2;
				}
				Dust.NewDustPerfect(
					Main.LocalPlayer.Center,
					ModContent.DustType<Vertex_Trail_Dust>(),
					Vector2.Zero
				).customData = new Vertex_Trail_Dust.TrailData(oldPos, oldRot, StripColors(Color.Goldenrod), StripWidth, 4);
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				modifiers.ScalingArmorPenetration += 0.75f;
				modifiers.Knockback *= 1.8f;
			}
			public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
				modifiers.SourceDamage *= 2 - (ContentExtensions.DifficultyDamageMultiplier - 1) * 0.25f;
				modifiers.ScalingArmorPenetration += 0.75f;
				modifiers.Knockback *= 1.8f;
			}
			public override bool? CanHitNPC(NPC target) => target.ModNPC is not IAshenEnemy;
			public override void OnHitPlayer(Player target, Player.HurtInfo info) {
				Min(ref Projectile.timeLeft, 1);
			}
			public override bool PreDraw(ref Color lightColor) {
				SpriteEffects spriteEffects = SpriteEffects.None;
				if (Projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;

				MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
				Vector2[] oldPos = [.. Projectile.oldPos];
				float[] oldRot = [.. Projectile.oldRot];
				for (int i = 0; i < oldPos.Length; i++) {
					if (oldPos[i] == default) {
						Array.Resize(ref oldPos, i);
						Array.Resize(ref oldRot, i);
						break;
					}
					oldRot[i] += MathHelper.PiOver2;
				}
				miscShaderData.UseSaturation(-2.8f);
				miscShaderData.UseOpacity(4f);
				miscShaderData.Apply();
				_vertexStrip.PrepareStripWithProceduralPadding(oldPos, oldRot, StripColors(Color.Goldenrod), StripWidth, -Main.screenPosition + Projectile.Size / 2f);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				return false;
			}
			static VertexStrip.StripColorFunction StripColors(Color color) => progressOnStrip => {
				if (float.IsNaN(progressOnStrip)) return Color.Transparent;
				float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				return color * (1f - lerpValue * lerpValue);
			};
			static float StripWidth(float progressOnStrip) {
				float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				return MathHelper.Lerp(0, 8, 1f - lerpValue * lerpValue);
			}
			private static readonly VertexStrip _vertexStrip = new();
		}
		struct ProjectileTargetIndex {
			int index;
			public readonly bool HasTarget => index >= 0 && index <= ((Main.maxProjectiles << 9) | (Main.maxPlayers + 1));
			public static implicit operator int(ProjectileTargetIndex value) => value.index;
			public static implicit operator ProjectileTargetIndex(int value) => new() { index = value };
			public static implicit operator ProjectileTargetIndex(Projectile target) => (target.whoAmI << 9) | target.owner;
			public readonly Projectile GetProjectile() => OriginExtensions.GetProjectile(index & 0b111111111, index >> 9);
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Partybringer : ModItem {
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Blind_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Partybringer_P>(64, 40, 8f, 46, 28, true);
			Item.value = Item.buyPrice(gold: 7);
			Item.rare = ItemRarityID.Yellow;
			Item.ArmorPenetration += 5;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, -8f);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			switch (Main.rand.Next(player.ownedProjectileCounts[ModContent.ProjectileType<Partybringer_Turret>()] < Partybringer_Turret.MaxTurrets ? 6 : 5)) {
				default:
				type = Item.shoot;
				break;
				case 1:
				type = ModContent.ProjectileType<Partybringer_P1>();
				break;
				case 2 or 3:
				type = ModContent.ProjectileType<Partybringer_P2>();
				break;
				case 4 or 5:
				type = ModContent.ProjectileType<Partybringer_P3>();
				break;
			}
			//type = ModContent.ProjectileType<Partybringer_P3>();
			Vector2 offset = velocity.SafeNormalize(default);
			position += offset.RotatedBy(player.direction * -MathHelper.PiOver2) * 6 - offset * 8;
			position += offset * (CollisionExt.Raymarch(position, offset, 32) - 8);
			position -= velocity;
		}
	}
	public abstract class Partybringer_P_Base : ModProjectile {
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.tileCollide = true;
			Projectile.width = Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 300;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.02f * (MathF.Pow((++Projectile.ai[0]) / 30, 0.5f) + 1);
			Projectile.rotation += Projectile.direction * 0.15f;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 30; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Main.rand.Next(139, 143), Projectile.velocity.X, Projectile.velocity.Y, 0, default(Color), 1.2f);
				dust.velocity.X += Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.Y += Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.X *= 1f + Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.Y *= 1f + Main.rand.Next(-50, 51) * 0.01f;
				dust.velocity.X += Main.rand.Next(-50, 51) * 0.05f;
				dust.velocity.Y += Main.rand.Next(-50, 51) * 0.05f;
				dust.scale *= 1f + Main.rand.Next(-30, 31) * 0.01f;
			}

			for (int i = 0; i < 15; i++) {
				Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity, Main.rand.Next(276, 283));
				gore.velocity.X += Main.rand.Next(-50, 51) * 0.01f;
				gore.velocity.Y += Main.rand.Next(-50, 51) * 0.01f;
				gore.velocity.X *= 1f + Main.rand.Next(-50, 51) * 0.01f;
				gore.velocity.Y *= 1f + Main.rand.Next(-50, 51) * 0.01f;
				gore.scale *= 1f + Main.rand.Next(-20, 21) * 0.01f;
				gore.velocity.X += Main.rand.Next(-50, 51) * 0.05f;
				gore.velocity.Y += Main.rand.Next(-50, 51) * 0.05f;
			}
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
		}
	}
	public class Partybringer_P : Partybringer_P_Base, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Partybringer_P).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Partybringer_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void AI() {
			this.DoGravity(0.02f * (MathF.Pow((++Projectile.ai[0]) / 30, 0.5f) + 1));
			Projectile.rotation += Projectile.direction * 0.15f;
		}
	}
	public class Partybringer_P1 : Partybringer_P_Base {
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
			if (Projectile.owner != Main.myPlayer) return;
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Partybringer_Fog>(),
				Projectile.damage,
				1,
				Projectile.owner
			);
		}
	}
	public class Partybringer_Fog : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 96;
			Projectile.timeLeft = 200;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 100;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				for (int i = 0; i < 100; i++) {
					float scale = 4f + Main.rand.NextFloat() * 0.8f;
					Vector2 velocity = Main.rand.NextVector2Circular(0.25f, 0.25f) * 0.2f + Main.rand.NextVector2CircularEdge(0.4f, 0.4f) * 0.2f;
					velocity *= 4f;
					Gore.NewGorePerfect(Projectile.GetSource_FromThis(), Projectile.Center + velocity * 4, velocity, ModContent.GoreType<Partybringer_Fog_Gore>(), scale);
				}
				Projectile.ai[0] = 1;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Blind_Debuff.ID, 100);
		}
	}
	public class Partybringer_Fog_Gore : ModGore {
		public override string Texture => typeof(Partybringer_Fog).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			GoreID.Sets.SpecialAI[Type] = 5;
		}
		public override bool Update(Gore gore) {
			if (gore.timeLeft > 60) {
				if (gore.alpha > 240 && Main.rand.NextBool()) gore.alpha--;
			} else if (Main.rand.NextBool()) {
				gore.alpha++;
			}
			return true;
		}
	}
	public class Partybringer_P2 : Partybringer_P_Base {
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
			if (Projectile.owner != Main.myPlayer) return;
			if (Main.rand.NextBool()) {
				int i = Main.rand.Next(1, 2);
				bool forceStar = i == 1;
				for (; i-- > 0;) {
					int item = Item.NewItem(
						Projectile.GetSource_Death(),
						Projectile.Center,
						ItemID.Heart
					);
					if (Main.netMode == NetmodeID.MultiplayerClient) {
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
					}
				}
				for (i = Main.rand.Next(forceStar ? 1 : 0, 4); i-- > 0;) {
					int item = Item.NewItem(
						Projectile.GetSource_Death(),
						Projectile.Center,
						ItemID.Star
					);
					if (Main.netMode == NetmodeID.MultiplayerClient) {
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
					}
				}
			} else {
				int type = ModContent.ProjectileType<Bang_Snap_P>();
				int count = Main.rand.Next(4, 8);
				float rot = MathHelper.TwoPi / count;
				for (int i = count; i-- > 0;) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						GeometryUtils.Vec2FromPolar(8, rot * i + Main.rand.NextFloat(-0.1f, 0.1f)) + Main.rand.NextVector2Unit(),
						type,
						(Projectile.damage * 3) / 5,
						6,
						Projectile.owner
					);
				}
			}
		}
	}
	public class Partybringer_P3 : Partybringer_P_Base {
		public int CanisterID {
			get => (int)Projectile.localAI[2];
			set => Projectile.localAI[2] = value;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse_WithAmmo ammoSource) {
				CanisterID = CanisterGlobalItem.GetCanisterType(ammoSource.AmmoItemIdUsed);
				if (CanisterID == -1) {
					if (CanisterGlobalItem.CanisterDatas[CanisterID] is null) CanisterID = 0;
				}
			}
		}
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
			if (Projectile.owner != Main.myPlayer) return;
			if (Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<Partybringer_Turret>()] < Partybringer_Turret.MaxTurrets && Main.rand.NextBool()) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					-Vector2.UnitY,
					ModContent.ProjectileType<Partybringer_Turret>(),
					Projectile.damage,
					10
				);
			} else {
				const float speed = 8f;
				const float maxDist = 120 * 120;
				List<(float angle, float weight)> targets = [];
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.CanBeChasedBy(Projectile)) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Projectile.Center);
						Vector2 diff = currentPos - Projectile.Center;
						if (GeometryUtils.AngleToTarget(currentPos - Projectile.Center, speed, 0.16f) is not float angle) continue;
						float dist = diff.LengthSquared();
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (Projectile.damage / 2 > npc.life + npc.defense) {
							currentWeight = 0;
						}
						if (targets.Count >= 3) {
							for (int j = 0; j < 3; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (angle, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((angle, currentWeight));
						}
					}
				}
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						GeometryUtils.Vec2FromPolar(speed, targets[i].angle),
						ProjectileID.PartyGirlGrenade,
						Projectile.damage * 2,
						10
					);
				}
			}
		}
	}
	public class Partybringer_Turret : ModProjectile {
		static AutoLoadingAsset<Texture2D> podTexture = typeof(Partybringer_Turret).GetDefaultTMLName() + "_Pods";
		public static int MaxTurrets => 15;
		public int CanisterID {
			get => (int)Projectile.localAI[2];
			set => Projectile.localAI[2] = value;
		}
		int forceDeployTimer = 60;
		int assumeBrokenTimer = 600;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 13;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.width = 30;
			Projectile.height = 32;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 0;
			Projectile.timeLeft = 60 * 25;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parent) {
				CanisterID = (int)parent.localAI[2];
			}
		}
		public override void AI() {
			const int volleys = 5;
			const int shots_per_volley = 3;
			const int time_per_volley = 45;
			const int time_per_rocket = 9;
			if (Projectile.wet) {
				if (Collision.GetWaterLine(Projectile.Center.ToTileCoordinates(), out float waterLineHeight)) {
					float num = Projectile.Center.Y;
					if (Projectile.frame >= 3) {
						num -= 4;
					}
					float num2 = num + 8f;
					if (num2 + Projectile.velocity.Y >= waterLineHeight) {
						if (num > waterLineHeight) {
							Projectile.velocity.Y -= 0.4f;
							if (Projectile.velocity.Y < -6f) {
								Projectile.velocity.Y = -6f;
							}
						} else {
							Projectile.velocity.Y = waterLineHeight - num2;
							if (Projectile.velocity.Y < -3f) {
								Projectile.velocity.Y = -3f;
							}
							if (Projectile.velocity.Y == 0f) {
								Projectile.velocity.Y = float.Epsilon;
							}
						}
						if (Projectile.ai[0] < 10) Projectile.ai[0] += 2f;
					}
				} else {
					Projectile.velocity.Y -= 0.4f;
				}
				Projectile.frameCounter = 30;
			} else {
				Projectile.frameCounter--;
			}
			if (Projectile.ai[0] < 10) {
				if (Projectile.ai[0] > 0 && --forceDeployTimer > 0) {
					Projectile.ai[0]--;
					assumeBrokenTimer = 600;
				}
				if (--assumeBrokenTimer <= 0) Projectile.Kill();
			} else {
				Projectile.ai[0]++;
			}
			if (Projectile.frame != 12) Projectile.localAI[1] = Projectile.ai[0];
			Projectile.frame = Math.Min((int)(Projectile.ai[0] / 3), 12);

			if (Projectile.ai[1] < volleys * shots_per_volley || Projectile.localAI[0] > 0) Projectile.timeLeft = 2;
			if (Projectile.frame == 12) {
				float distanceFromTarget = 2000f;
				Vector2 targetCenter = default;
				int target = -1;
				bool hasPriorityTarget = false;
				void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
					bool isCurrentTarget = npc.whoAmI == Projectile.ai[2];
					if ((isCurrentTarget || isPriorityTarget || !hasPriorityTarget) && npc.CanBeChasedBy()) {
						Vector2 pos = Projectile.position;
						float between = Vector2.Distance(npc.Center, pos);
						between *= isCurrentTarget ? 0 : 1;
						bool closer = distanceFromTarget > between;
						bool lineOfSight = Collision.CanHitLine(pos, 8, 8, npc.position, npc.width, npc.height);
						if ((closer || !foundTarget) && lineOfSight) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							target = npc.whoAmI;
							foundTarget = true;
							hasPriorityTarget = isPriorityTarget;
						}
					}
				}
				bool foundTarget = Main.player[Projectile.owner].GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
				if (foundTarget) {
					Vector2 diff = targetCenter - Projectile.Center;
					if (OriginExtensions.AngularSmoothing(ref Projectile.rotation, diff.ToRotation(), 0.5f)) {
						Projectile.ai[2] = target;
						if ((Projectile.ai[0] - Projectile.localAI[1]) > ((int)(Projectile.ai[1] / shots_per_volley)) * time_per_volley && Projectile.localAI[0] <= 0) {
							if (Projectile.owner == Main.myPlayer) {
								Projectile.NewProjectile(
									Projectile.GetSource_FromAI(),
									Projectile.Center - Vector2.UnitY * 12,
									diff.SafeNormalize(default).RotatedByRandom(0.05f) * 12,
									//ModContent.ProjectileType<Partybringer_Turret_Rocket_Yellow>(),
									Main.rand.Next(Partybringer_Turret_Rocket.Projectiles),
									Projectile.damage / 2,
									Projectile.knockBack,
									Projectile.owner,
									target
								);
							}
							Projectile.ai[1] += 1;
							Projectile.localAI[0] = time_per_rocket;
						}
					}
				}
			}
			if (Projectile.localAI[0] > 0) Projectile.localAI[0]--;

			Vector2 dir = Vector2.Zero;
			if (Projectile.Hitbox.OverlapsAnyTiles(out List<Point> intersectingTiles)) {
				float mult = 1f / intersectingTiles.Count;
				for (int i = 0; i < intersectingTiles.Count; i++) {
					dir -= (intersectingTiles[i].ToWorldCoordinates() - Projectile.Center) * mult;
				}
			}
			if (dir == Vector2.Zero) {
				Projectile.velocity.Y += 0.16f;
			} else {
				Projectile.velocity = dir * 0.03f;
			}
			Projectile.velocity *= 0.995f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity *= 0.93f;
			if (Projectile.ai[0] < 10) Projectile.ai[0] += 2;
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false;
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 innerTubePosition = Projectile.Center;
			if (Projectile.frame >= 3) {
				innerTubePosition.Y -= 4;
			}
			if (Projectile.frameCounter > 0) {
				Main.EntitySpriteDraw(
					TextureAssets.Extra[ExtrasID.FloatingTube].Value,
					innerTubePosition - Main.screenPosition,
					new Rectangle(6, 34, 30, 16),
					lightColor,
					0,
					new Vector2(15, 0),
					1f,
					SpriteEffects.None
				);
			}
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Vector2.UnitY * 4 - Main.screenPosition,
				texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				lightColor,
				0,
				new(23),
				Projectile.scale,
				0
			);
			if (Projectile.frameCounter > 0) {
				Main.EntitySpriteDraw(
					TextureAssets.Extra[ExtrasID.FloatingTube].Value,
					innerTubePosition - Main.screenPosition,
					new Rectangle(6, 90, 30, 16),
					lightColor,
					0,
					new Vector2(15, 0),
					1f,
					SpriteEffects.None
				);
			}
			Vector2 extraOffset = Vector2.Zero;
			if (Projectile.frame < 9) {
				extraOffset.Y -= 32 * Math.Max((9 * 3) - Projectile.ai[0], 0);
			} else if (Projectile.frame == 9) {
				extraOffset = new(-4, -2);
			} else if (Projectile.frame == 10) {
				extraOffset = new(2, -4);
			}
			Vector2 podPos = (Projectile.Center + extraOffset - Vector2.UnitY * 8);
			bool facingLeft = Math.Cos(Projectile.rotation) < 0;
			Main.EntitySpriteDraw(
				podTexture,
				podPos - Main.screenPosition,
				null,
				Lighting.GetColor(podPos.ToTileCoordinates()),
				Projectile.rotation + (facingLeft ? MathHelper.Pi : 0),
				new(23, 13),
				Projectile.scale,
				facingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			return false;
		}
	}
	public abstract class Partybringer_Turret_Rocket : ModProjectile {
		public abstract Color Color { get; }
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Celeb2Rocket;
		public static List<int> Projectiles { get; private set; }
		public int Target {
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = ProjectileID.Sets.TrailingMode[ProjectileID.Celeb2Rocket];
			ProjectileID.Sets.TrailCacheLength[Type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.Celeb2Rocket];
			(Projectiles ??= []).Add(Type);
		}
		public override void Unload() {
			Projectiles = null;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 120;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			Color color = Color;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Dust.NewDustPerfect(
				Projectile.Center - Projectile.velocity.SafeNormalize(default),
				ModContent.DustType<Flare_Dust>(),
				-Projectile.velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.9f, 1f),
				newColor: color,
				Scale: 0.85f
			).noGravity = true;
			Lighting.AddLight(Projectile.Center, color.ToVector3());
		}
		public override bool PreDraw(ref Color lightColor) {
			DrawRocket(Color, lightColor);
			return false;
		}
		public void DrawRocket(Color color, Color lightColor) {
			Vector2 position = Projectile.position + Projectile.Size / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(3);
			Vector2 origin = frame.Size() / 2f;
			Vector2 origin2 = new(frame.Width / 2, 0f);
			Vector2 halfSize = Projectile.Size / 2f;
			Color bufferColor = color;
			bufferColor.A = 127;
			bufferColor *= 0.8f;
			Rectangle frame2 = frame;
			frame2.X += frame2.Width * 2;
			for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
				if (Projectile.oldPos[i] != default) {
					Vector2 oldPos1 = Projectile.oldPos[i] + halfSize;
					Vector2 oldPos2 = Projectile.oldPos[i - 1] + halfSize;
					Vector2 scale14 = new(Vector2.Distance(oldPos1, oldPos2) / frame.Width, 1f);
					Main.EntitySpriteDraw(
						texture,
						oldPos1 - Main.screenPosition,
						frame2,
						bufferColor * (1f - i / (float)Projectile.oldPos.Length),
						Projectile.oldRot[i],
						origin2,
						scale14 * 0.85f,
						SpriteEffects.None
					);
				}
			}
			Main.EntitySpriteDraw(texture, position, frame, lightColor, Projectile.rotation, origin, Projectile.scale * 0.85f, SpriteEffects.None);
			frame.X += frame.Width;
			bufferColor = color;
			bufferColor.A = 80;
			Main.EntitySpriteDraw(texture, position, frame, bufferColor, Projectile.rotation, origin, Projectile.scale * 0.85f, SpriteEffects.None);
		}
		public abstract override void OnKill(int timeLeft);
		public static Vector2[] Star(int spikes, float outerSize, float innerSize) {
			float portion = MathHelper.TwoPi / (spikes * 2);
			Vector2[] directions = new Vector2[spikes * 2];
			for (int i = 0; i < directions.Length; i++) {
				directions[i] = GeometryUtils.Vec2FromPolar(i % 2 == 0 ? outerSize : innerSize, i * portion - MathHelper.PiOver2);
			}
			return directions;
		}
		public void MakeShape(Color color, float scale, params Vector2[] vertices) => MakeShape(color, scale, Vector2.Zero, vertices);
		public void MakeShape(Color color, float scale, Vector2 offset, params Vector2[] vertices) {
			float spread = 0.25f / scale;
			for (int i = 0; i < vertices.Length; i++) {
				Vector2 a = vertices[i];
				Vector2 b = vertices[(i + 1) % vertices.Length];
				float speed = spread / a.Distance(b);
				for (float j = 0; j < 1; j += speed) {
					Vector2 direction = (Vector2.Lerp(a, b, j) + offset) * scale;
					Dust.NewDustPerfect(
						Projectile.Center,
						ModContent.DustType<Flare_Dust>(),
					direction,
						newColor: color,
						Scale: 0.85f
					).noGravity = true;
				}
			}
		}
	}
	public class Partybringer_Turret_Rocket_Red : Partybringer_Turret_Rocket {
		public override Color Color => Color.Red;
		public override void SetDefaults() {
			base.SetDefaults();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			float extraRot = Main.rand.NextFloat(MathHelper.TwoPi);
			MakeShape(Color, 6, Star(2, 1, 1).Scaled(new(1, 1.5f)).RotatedBy(extraRot));
			if (Main.rand.NextBool()) MakeShape(Color, 3, Star(2, 1, 1).Scaled(new(1, 1.5f)).RotatedBy(extraRot));
		}
	}
	public class Partybringer_Turret_Rocket_Yellow : Partybringer_Turret_Rocket {
		public override Color Color => Color.Gold;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.ArmorPenetration += 4;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			if (Main.rand.NextBool()) {
				MakeShape(Color, 6, Star(3, 1, 0.3f).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
			} else {
				Color color = Color;
				for (int i = 0; i < 30; i++) {
					Dust dust = Dust.NewDustDirect(
						Projectile.Center,
						0,
						0,
						ModContent.DustType<Sparkler_Dust>(),
						newColor: color
					);
					dust.noGravity = false;
					dust.velocity = Main.rand.NextVector2Circular(1, 1) * 8;
					dust = Dust.NewDustDirect(
						Projectile.Center,
						0,
						0,
						ModContent.DustType<Sparkler_Dust>(),
						newColor: color,
						Scale: 1.5f
					);
					dust.noGravity = true;
					dust.velocity = Main.rand.NextVector2Circular(1, 1) * 10;
				}
			}
		}
	}
	public class Partybringer_Turret_Rocket_Green : Partybringer_Turret_Rocket {
		public override Color Color => Color.Lime;
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = -oldVelocity.X;
			if (Projectile.velocity.Y != oldVelocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
			return false;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			if (Main.rand.NextBool()) {
				MakeShape(Color, 6, Star(4, 1, 0.3f).Scaled(new(1, 1.25f)).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
			} else {
				Color color = Color;
				for (int i = 0; i < 30; i++) {
					Dust dust = Dust.NewDustDirect(
						Projectile.Center,
						0,
						0,
						ModContent.DustType<Sparkler_Dust>(),
						newColor: color
					);
					dust.noGravity = false;
					dust.velocity = Main.rand.NextVector2Circular(1, 1) * 8;
					dust = Dust.NewDustDirect(
						Projectile.Center,
						0,
						0,
						ModContent.DustType<Sparkler_Dust>(),
						newColor: color,
						Scale: 1.5f
					);
					dust.noGravity = true;
					dust.velocity = Main.rand.NextVector2Circular(1, 1) * 10;
				}
			}
		}
	}
	public class Partybringer_Turret_Rocket_Blue : Partybringer_Turret_Rocket {
		public override Color Color => Color.DodgerBlue;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.extraUpdates++;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			if (Main.rand.NextBool()) {
				MakeShape(Color, 6, Star(5, 1, 0.4f).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
			} else {
				Vector2[] directions = [
					new Vector2(0f, -2f),
					new Vector2(1.25f, -1.85f),
					new Vector2(0.12f, 0.46f),
					new Vector2(-0.1f, 0.245f),
					new Vector2(-1.02f, 1.785f),
					new Vector2(-0.54f, -0.615f),
					new Vector2(-0.32f, -0.4f)
				];
				if (Main.rand.NextBool()) directions = directions.Scaled(new(-1, 1));
				MakeShape(Color, 4, directions.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)));
			}
		}
	}
	public class Partybringer_Turret_Rocket_Purple : Partybringer_Turret_Rocket {
		public override Color Color => Color.Magenta;
		public override void AI() {
			if (Target != -1) {
				NPC target = Main.npc[Target];
				if (target.CanBeChasedBy(Projectile)) {
					float scaleFactor = 16f * Origins.HomingEffectivenessMultiplier[Projectile.type];

					Vector2 targetVelocity = (target.Center - Projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.083333336f);
				} else {
					Target = -1;
				}
			}
			base.AI();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			Vector2[] vertices = Star(6, 1, 0.5f).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
			MakeShape(Color, 6, vertices);
			if (Main.rand.NextBool()) MakeShape(Color.White, 3, vertices.RotatedBy(MathHelper.Pi / 6));
		}
	}
	public class Partybringer_Turret_Rocket_Canister : Partybringer_Turret_Rocket, ICanisterProjectile {
		public override Color Color => Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().CanisterData.InnerColor;
		public AutoLoadingAsset<Texture2D> OuterTexture { get; }
		public AutoLoadingAsset<Texture2D> InnerTexture { get; }
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj && !parentProj.TryGetGlobalProjectile<CanisterGlobalProjectile>(out _)) {
				Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().CanisterID = (int)parentProj.localAI[2];
			}
		}
		public override void AI() {
			if (Target != -1) {
				NPC target = Main.npc[Target];
				if (target.CanBeChasedBy(Projectile)) {
					float scaleFactor = 16f * Origins.HomingEffectivenessMultiplier[Projectile.type];

					Vector2 targetVelocity = (target.Center - Projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.083333336f);
				} else {
					Target = -1;
				}
			}
			base.AI();
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			DrawRocket(canisterData.InnerColor, lightColor);
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			if (Main.rand.NextBool()) {
				MakeShape(Color, 6, Star(7, 1, 0.5f).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)));
			} else {
				Color color = Color;
				Vector2[] vertices = Star(5, 1, 0.5f).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
				MakeShape(color, 6, vertices);
				MakeShape(color, 6, vertices.RotatedBy(MathHelper.Pi));
			}
		}
	}
}

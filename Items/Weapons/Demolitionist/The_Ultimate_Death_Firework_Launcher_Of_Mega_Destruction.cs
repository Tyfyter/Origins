using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using Origins.UI;
using Terraria.GameInput;
using Terraria.Audio;

namespace Origins.Items.Weapons.Demolitionist {
	public class The_Ultimate_Death_Firework_Launcher_Of_Mega_Destruction : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<TUDFLOMD_Rocket_Canister>(80, 9, 12f, 46, 28, true);
			Item.useAnimation *= 4;
			Item.useLimitPerAnimation = 4;
			Item.knockBack = 3;
			Item.reuseDelay = Item.useTime;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Cyan;
			//Item.ArmorPenetration += 15;
		}
		public override Vector2? HoldoutOffset() => new(-5, -8);
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.LunarBar, 18)
			.AddIngredient<Partybringer>()
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(Item.UseSound, player.Center);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Main.rand.Next(TUDFLOMD_Rocket.Projectiles);
			Vector2 unit = velocity.Normalized(out _);
			Vector2 perp = unit.RotatedBy(player.direction * MathHelper.PiOver2);
			switch (player.ItemUsesThisAnimation) {
				case 1:
				position += unit * 84 + perp * 8;
				break;
				case 2:
				position += unit * 64 + perp * 8;
				break;
				case 3:
				position += unit * 84 + perp * -10;
				break;
				case 4:
				position += unit * 64 + perp * -10;
				break;
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			float distanceFromTarget = 16 * 17.5f;
			int target = -1;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.CanBeChasedBy()) {
					Vector2 pos = Main.MouseWorld;
					float between = Vector2.Distance(pos.Clamp(npc.Hitbox), pos);
					if (distanceFromTarget > between) {
						distanceFromTarget = between;
						target = npc.whoAmI;
					}
				}
			}
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				type,
				damage,
				knockback,
				ai0: target
			);
			return false;
		}
	}
	public class TUDFLOMD_Lock_On_HUD : SwitchableUIState {
		public override void AddToList() => OriginSystem.Instance.ItemUseHUD.AddState(this);
		public override bool IsActive() => Main.LocalPlayer.HeldItem.type == ModContent.ItemType<The_Ultimate_Death_Firework_Launcher_Of_Mega_Destruction>();
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float distanceFromTarget = 16 * 17.5f;
			NPC target = null;
			PlayerInput.SetZoom_MouseInWorld();
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.CanBeChasedBy()) {
					Vector2 pos = Main.MouseWorld;
					float between = Vector2.Distance(pos.Clamp(npc.Hitbox), pos);
					if (distanceFromTarget > between) {
						distanceFromTarget = between;
						target = npc;
					}
				}
			}
			if (target is null) return;
			Texture2D value = TextureAssets.LockOnCursor.Value;
			Rectangle frame1 = new(0, 0, value.Width, 12);
			Rectangle frame2 = new(0, 16, value.Width, 12);
			Color color1 = new(43, 185, 255, 220);
			float num = 0.94f + MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi) * 0.06f;
			color1 *= num;
			Color color2 = color1.MultiplyRGBA(new Color(0.75f, 0.75f, 0.75f, 1f));
			float gravDir = Main.player[Main.myPlayer].gravDir;
			float num2 = 1f;
			float num5 = 1f;
			int size = target.width;
			if (target.height > size) {
				size = target.height;
			}
			Main.UIScaleMatrix.Decompose(out Vector3 scale, out _, out _);
			float swoom = MathF.Pow((Main.GlobalTimeWrappedHourly * 2) % 2 - 1, 3);
			for (int j = 0; j < 5; j++) {
				float rotation = MathHelper.TwoPi / 5f * j + swoom * MathHelper.TwoPi / 5;
				Vector2 pos = target.Center + GeometryUtils.Vec2FromPolar(size / 2 + 8 * Math.Abs(swoom), rotation + MathHelper.PiOver2) - Main.screenPosition;
				pos.X = ((int)pos.X) / scale.X;
				pos.Y = ((int)pos.Y) / scale.Y;
				pos = Main.ReverseGravitySupport(pos);
				rotation = rotation * (gravDir == 1f ? 1 : -1) + MathHelper.Pi * (gravDir == 1f ? 1 : 0);
				spriteBatch.Draw(value, pos, frame1, color1, rotation, frame1.Size() / 2f, new Vector2(0.58f, 1f) * num2 * num5 * 1f / 2f, SpriteEffects.None, 0f);
				spriteBatch.Draw(value, pos, frame2, color2, rotation, frame2.Size() / 2f, new Vector2(0.58f, 1f) * num2 * num5 * 1f / 2f, SpriteEffects.None, 0f);
			}
		}
	}
	public abstract class TUDFLOMD_Rocket : ModProjectile {
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
			if (Projectile.soundDelay == 0) {
				Projectile.soundDelay = -1;
				Dust.NewDustPerfect(
					Projectile.Center + Projectile.velocity.Normalized(out _) * 64,
					ModContent.DustType<Rocket_Launch>(),
					Projectile.velocity
				);
			}
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
						scale14,
						SpriteEffects.None
					);
				}
			}
			Main.EntitySpriteDraw(texture, position, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
			frame.X += frame.Width;
			bufferColor = color;
			bufferColor.A = 80;
			Main.EntitySpriteDraw(texture, position, frame, bufferColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
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
			List<TUDFLOMD_Subdust> dusts = [];
			for (int i = 0; i < vertices.Length; i++) {
				Vector2 a = vertices[i];
				Vector2 b = vertices[(i + 1) % vertices.Length];
				float speed = spread / a.Distance(b);
				for (float j = 0; j < 1; j += speed) {
					Vector2 direction = (Vector2.Lerp(a, b, j) + offset) * scale;
					dusts.Add(new(Projectile.Center, direction, 0.85f, color));
				}
			}
			Dust.NewDustPerfect(
				Projectile.Center,
				ModContent.DustType<TUDFLOMDust>(),
				Vector2.Zero
			).customData = dusts.ToArray();
		}
	}
	public class TUDFLOMD_Rocket_Canister : TUDFLOMD_Rocket, ICanisterProjectile {
		public override Color Color => Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().CanisterData?.InnerColor ?? Color.Black;
		public AutoLoadingAsset<Texture2D> OuterTexture { get; }
		public AutoLoadingAsset<Texture2D> InnerTexture { get; }
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
			DrawRocket(canisterData?.InnerColor ?? Color.Black, lightColor);
		}
		public override void OnKill(int timeLeft) {
			MakeShape(Color, 8, Star(5, 1.5f, 0.5f).RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f)));
		}
	}
	public class TUDFLOMD_Rocket_Red : TUDFLOMD_Rocket {
		public override Color Color => Color.Red;
		HashSet<int> hitTargets = [];
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = 3;
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
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.owner == Main.myPlayer && Projectile.penetrate > 0) {
				hitTargets.Add(target.whoAmI);
				Target = -1;
				float dist = 16 * 15;
				dist *= dist;
				bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
					if (hitTargets.Contains(target.whoAmI)) return false;
					float newDist = Projectile.DistanceSQ(target.Center);
					if (newDist < dist) {
						dist = newDist;
						foundTarget = true;
						Target = target.whoAmI;
						return true;
					}
					return false;
				});
				if (Target == -1) {
					Projectile.Kill();
				} else {
					Projectile.netUpdate = true;
					int penetrate = Projectile.penetrate;
					int size = Projectile.width;
					try {
						Projectile.penetrate = -1;
						Projectile.Resize(size * 6, size * 6);
						ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, false, SoundID.Item14, 0, 15, 0);
						Projectile.Damage();
						Color color = Color;
						Vector2[] directions = [
							-Vector2.UnitY * 1.5f,
							Vector2.UnitX,
							Vector2.UnitY * 1.5f,
							-Vector2.UnitX
						];
						MakeShape(color, 8, directions);
						MakeShape(color, 16, directions);
					} finally {
						Projectile.Resize(size, size);
						Projectile.penetrate = penetrate;
					}
				}
			}
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			Color color = Color;
			Vector2[] directions = [
				-Vector2.UnitY * 1.5f,
				Vector2.UnitX,
				Vector2.UnitY * 1.5f,
				-Vector2.UnitX
			];
			MakeShape(color, 8, directions);
			MakeShape(color, 16, directions);
		}
	}
	public class TUDFLOMD_Rocket_Yellow : TUDFLOMD_Rocket {
		public override Color Color => Color.Gold;
		public override void AI() {
			if (Target != -1) {
				NPC target = Main.npc[Target];
				if (target.CanBeChasedBy(Projectile)) {
					float scaleFactor = 16f * Origins.HomingEffectivenessMultiplier[Projectile.type];

					Vector2 targetVelocity = (target.Center - Projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor * (1 - Projectile.ai[1] / 200);
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity.RotatedBy(Math.Sin(++Projectile.ai[1] * 0.25f) * 1), 0.083333336f);
				} else {
					Target = -1;
				}
			} else {
				Projectile.velocity = Projectile.velocity.RotatedBy(Math.Sin(++Projectile.ai[2] * 0.25f - MathHelper.PiOver2) * 0.1f) * (1 - (Projectile.ai[2] / 120) * 0.02f);
			}
			base.AI();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			if (Projectile.owner == Main.myPlayer) {
				List<Vector2> projectiles = [];
				int tries = 10;
				while (projectiles.Count < 6 || --tries < 0) {
					projectiles = OriginExtensions.FelisCatusSampling(Vector2.Zero, 8, 11, 4, 16);
				}
				for (int i = 0; i < projectiles.Count; i++) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						projectiles[i],
						ModContent.ProjectileType<TUDFLOMD_Rocket_Yellow_Explosion>(),
						Projectile.damage / 3,
						Projectile.knockBack * 0.5f,
						Projectile.owner,
						ai1: Main.rand.NextFloat(30, 60)
					);
				}
				for (int i = 0; i < 3; i++) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Main.rand.NextVector2Circular(6, 6),
						ModContent.ProjectileType<TUDFLOMD_Rocket_Yellow_Explosion>(),
						Projectile.damage / 3,
						Projectile.knockBack * 0.5f,
						Projectile.owner,
						ai1: Main.rand.NextFloat(30, 60)
					);
				}
			}
		}
	}
	public class TUDFLOMD_Rocket_Yellow_Explosion : TUDFLOMD_Rocket {
		public override Color Color => Color.Gold;
		public override void SetStaticDefaults() { }
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override void AI() {
			if (--Projectile.ai[1] <= 0) Projectile.Kill();
			Projectile.velocity *= 0.9f;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 32, false, SoundID.Item40.WithPitchRange(-0.5f, 0.5f).WithVolume(1), 0, 0, 0);
			Color color = Color;
			for (int i = 0; i < 8; i++) {
				Dust dust = Dust.NewDustDirect(
					Projectile.Center,
					0,
					0,
					ModContent.DustType<Sparkler_Dust>(),
					newColor: color
				);
				dust.noGravity = false;
				dust.velocity *= 5;
				dust = Dust.NewDustDirect(
					Projectile.Center,
					0,
					0,
					ModContent.DustType<Sparkler_Dust>(),
					newColor: color
				);
				dust.noGravity = true;
				dust.velocity *= 3;
			}
		}
	}
	public class Sparkler_Dust : ModDust {
		public override string Texture => typeof(Flare_Dust).GetDefaultTMLName();
		public override bool Update(Dust dust) {
			dust.fadeIn--;
			if (!dust.noLight && !dust.noLightEmittence) {
				float scale = dust.scale;
				if (scale > 1f) scale = 1f;
				Lighting.AddLight(dust.position, dust.color.ToVector3() * scale);
			}
			if (dust.noGravity) {
				dust.velocity *= 0.93f;
				if (dust.fadeIn == 0f) {
					dust.scale += 0.0025f;
				}
			} else {
				dust.velocity.X *= 0.95f;
				if (dust.velocity.Y < 0) dust.velocity.Y *= 0.93f;
				else dust.velocity.Y *= 0.96f;
				dust.scale -= 0.0005f;
			}
			if (WorldGen.SolidTile(Framing.GetTileSafely(dust.position)) && !dust.noGravity) {
				dust.scale *= 0.9f;
				dust.velocity *= 0.25f;
			}
			return true;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) {
			return dust.color.MultiplyRGB(lightColor) with { A = 25 };
		}
		public override bool PreDraw(Dust dust) {
			float trail = Math.Abs(dust.velocity.X) + Math.Abs(dust.velocity.Y);
			trail *= 0.3f;
			trail *= 10f;
			if (trail > 10f) trail = 10f;
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
	public class TUDFLOMD_Rocket_Green : TUDFLOMD_Rocket {
		public override Color Color => Color.Lime;
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.hide && Target != -1) return false;
			return null;
		}
		public override void AI() {
			if (++Projectile.ai[1] >= 80) {
				Projectile.hide = false;
				Projectile.tileCollide = true;
				if (Target != -1) {
					NPC target = Main.npc[Target];
					if (target.CanBeChasedBy(Projectile)) {
						Projectile.tileCollide = false;
						if (Projectile.ai[1] == 80) {
							Projectile.Center = target.Center + Projectile.velocity * 12 + Projectile.velocity.SafeNormalize(default) * Math.Max(target.width, target.height) * 0.5f;
							Projectile.velocity = -Projectile.velocity;
							for (int i = 0; i < Projectile.oldPos.Length; i++) {
								Projectile.oldPos[i] = default;
							}
						} else {
							float scaleFactor = 16f * Origins.HomingEffectivenessMultiplier[Projectile.type];

							Vector2 targetVelocity = (target.Center - Projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
							Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.083333336f);
						}
					} else {
						Target = -1;
					}
				} else {
					if (Projectile.ai[1] == 80) {
						for (int i = 0; i < Projectile.oldPos.Length; i++) {
							Projectile.oldPos[i] = default;
						}
					}
					Projectile.timeLeft -= 4;
				}
			} else if (Projectile.ai[1] >= 30) {
				Projectile.hide = true;
				Projectile.tileCollide = Target == -1;
			}
			if (!Projectile.hide) base.AI();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			Color color = Color;
			for (int i = 0; i < 60; i++) {
				Dust dust = Dust.NewDustDirect(
					Projectile.Center,
					0,
					0,
					ModContent.DustType<Sparkler_Dust>(),
					newColor: color
				);
				dust.noGravity = false;
				dust.velocity = Main.rand.NextVector2Circular(1, 1) * 16;
				dust = Dust.NewDustDirect(
					Projectile.Center,
					0,
					0,
					ModContent.DustType<Sparkler_Dust>(),
					newColor: color,
					Scale: 1.5f
				);
				dust.noGravity = true;
				dust.velocity = Main.rand.NextVector2Circular(1, 1) * 20;
			}
		}
	}
	public class TUDFLOMD_Rocket_Blue : TUDFLOMD_Rocket {
		public override Color Color => new(0, 172, 248);
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.timeLeft = 300;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Target != -1 && source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parent) {
				parent.ai[2] = -1;
			}
		}
		public override void AI() {
			if (++Projectile.ai[2] < 15) {
				if (Target != -1) {
					NPC target = Main.npc[Target];
					if (target.CanBeChasedBy(Projectile)) {
						float speed = 10f * Origins.HomingEffectivenessMultiplier[Projectile.type];
						if (GeometryUtils.AngleToTarget(target.Center - Projectile.Center, speed, 0.10f, true) is float angle) {

							Vector2 targetVelocity = GeometryUtils.Vec2FromPolar(speed, angle).SafeNormalize(default) * speed;
							Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.25f);
						} else {
							Projectile.ai[1] = 1;
							Projectile.extraUpdates++;
						}
					}
				}
			} else {
				if (Projectile.ai[1] == 0) {
					if (Projectile.velocity.Y > 2) {
						Projectile.ai[1] = 1;
						Projectile.extraUpdates++;
					}
				} else if (Target != -1) {
					NPC target = Main.npc[Target];
					if (target.CanBeChasedBy(Projectile)) {
						float scaleFactor = 16f * Origins.HomingEffectivenessMultiplier[Projectile.type];

						Vector2 targetVelocity = (target.Center - Projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
						Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.083333336f);
					} else {
						Target = -1;
					}
				} else {
					Projectile.timeLeft--;
				}
				Projectile.velocity.Y += 0.10f;
			}
			base.AI();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Item14, 0, 15, 0);
			Color color = Color;
			Vector2[] directions = [
				new Vector2(0f, -2f),
				new Vector2(1.5f, -1.85f),
				new Vector2(0.12f, 0.44f),
				new Vector2(-0.1f, 0.225f),
				new Vector2(-1.02f, 1.785f),
				new Vector2(-0.54f, -0.595f),
				new Vector2(-0.32f, -0.38f)
			];
			if (Main.rand.NextBool()) directions = directions.Scaled(new(-1, 1));
			MakeShape(color, 6, directions);
		}
	}
	public class TUDFLOMD_Rocket_Purple : TUDFLOMD_Rocket {
		public override Color Color => Color.Magenta;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.timeLeft = 5 * 15 + 14;
		}
		public override void AI() {
			if (Projectile.timeLeft > 0 && Projectile.timeLeft % 15 == 0) {
				if (Projectile.owner == Main.myPlayer) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Vector2.Zero,
						ModContent.ProjectileType<TUDFLOMD_Rocket_Purple_Explosion>(),
						Projectile.damage - Projectile.damage / 4,
						Projectile.knockBack,
						Projectile.owner
					);
				}
				if (Target != -1) {
					NPC target = Main.npc[Target];
					if (target.CanBeChasedBy(Projectile)) {
						Vector2 targetVelocity = (target.Center - Projectile.Center).SafeNormalize(-Vector2.UnitY).RotatedByRandom(((Projectile.timeLeft / 15) - 1) * 0.3f) * 12;
						Projectile.velocity = targetVelocity;
					} else {
						Target = -1;
					}
				}
			}
			base.AI();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 128, false, SoundID.Item14, 0, 15, 0);
			Color color = Color;
			float rot = Main.rand.NextFloat(-0.2f, 0.2f);
			Vector2 scale = new(0.75f, 1f);
			Vector2[] star = Star(6, 2, 0.75f).Scaled(scale).RotatedBy(rot);
			MakeShape(color, 8, star);
			for (int i = (timeLeft - 1) / 15; i-- > 0;) {
				MakeShape(new(191, 171, 143), 2, (GeometryUtils.Vec2FromPolar(10, (i + 1.15f) * -(MathHelper.TwoPi / 6)) * scale).RotatedBy(rot), star.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)));
			}
		}
	}
	public class TUDFLOMD_Rocket_Purple_Explosion : TUDFLOMD_Rocket {
		public override Color Color => new(191, 171, 143);
		public override void SetStaticDefaults() { }
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.timeLeft = 1;
		}
		public override void AI() { }
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 64, false, SoundID.Item14, 0, 15, 0);
			Color color = Color;
			MakeShape(color, 2, Star(6, 2, 0.75f).Scaled(new(0.75f, 1f)).RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)));
		}
	}
}

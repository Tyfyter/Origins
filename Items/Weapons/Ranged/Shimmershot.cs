using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Items.Materials;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Projectiles;
using PegasusLib;
using PegasusLib.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Shimmershot : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.gunProj[Type] = true;
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				if (proj.friendly) global.SetUpdateCountBoost(proj, proj.MaxUpdates);
			});
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Handgun);
			Item.damage = 54;
			Item.knockBack = 8;
			Item.crit = 15;
			Item.useTime = Item.useAnimation = 36;
			Item.shoot = ModContent.ProjectileType<Shimmershot_P>();
			Item.shootSpeed = 8;
			Item.width = 38;
			Item.height = 18;
			Item.autoReuse = true;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = null;
		}
		public static bool isShooting = false;
		public override bool AltFunctionUse(Player player) => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (source.Context?.Contains("gunProj") != true) {
				Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, ai1: 1, ai2: player.altFunctionUse == 2 ? -1 : 0);
				return false;
			}
			return true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Aetherite_Bar>(13)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Shimmershot_P : ModProjectile, IPreDrawSceneProjectile, ITriggerSCBackground {
		public override string Texture => typeof(Shimmershot).GetDefaultTMLName();
		public static int BarrelOffset => 2;
		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.aiStyle = ProjAIStyleID.HeldProjectile;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.ignoreWater = true;
		}
		SlotId chargeSound;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			OriginPlayer originPlayer = player.OriginPlayer();
			if (Projectile.ai[2] == 1) {
				SoundEngine.PlaySound(SoundID.Item67.WithPitch(-1f), Projectile.position);
				SoundEngine.PlaySound(SoundID.Item142, Projectile.position);
				SoundEngine.PlaySound(Origins.Sounds.HeavyCannon, Projectile.position);
				Projectile.ai[2] = 0;
			}
			if (Projectile.localAI[1] == 0) Projectile.localAI[1] = CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem);
			if (Main.myPlayer == Projectile.owner) { // charging
				if (Projectile.localAI[0].Warmup(Projectile.localAI[1])) {
					SoundEngine.PlaySound(SoundID.Item25.WithPitchOffset(1)); // full charge sound
					SoundEngine.PlaySound(SoundID.Zombie103.WithPitch(1f));
					Projectile.netUpdate = true;
				}
				if (player.channel) {
					if (SoundEngine.TryGetActiveSound(chargeSound, out ActiveSound sound)) {
						MathUtils.LinearSmoothing(ref sound.Volume, (Projectile.localAI[0] < 0 || !player.channel) ? 0f : 0.75f, 1f / 20);
					} else {
						int type = Type;
						chargeSound = SoundEngine.PlaySound(Origins.Sounds.ShimmershotCharging, null, soundInstance => {
							soundInstance.Pitch = Math.Max(Projectile.localAI[0] / Projectile.localAI[1], 0);
							return Projectile.active && Projectile.type == type;
						});
						SoundEngine.TryGetActiveSound(chargeSound, out sound);
						sound.Volume = 1 / 20f;
					}
				}
			}
			if (!player.noItems && !player.CCed) {
				Vector2 position = player.MountedCenter + (new Vector2(8, -6 * player.direction * player.gravDir).RotatedBy(Projectile.rotation - MathHelper.PiOver2)).Floor();
				Projectile.position = position;
				position += position.DirectionTo(Main.MouseWorld).RotatedBy(player.direction * MathHelper.PiOver2) * BarrelOffset;
				if (Main.myPlayer == Projectile.owner && (Main.mouseRight || (Projectile.ai[0] <= 1 && Projectile.ai[2] != -1 && Main.mouseLeft))) {
					Vector2 direction = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - position;
					if (player.gravDir == -1f) direction.Y = (Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - position.Y;

					Vector2 velocity = Vector2.Normalize(direction);
					if (velocity.HasNaNs()) velocity = -Vector2.UnitY;
					if (Projectile.velocity != velocity) {
						Projectile.velocity = velocity;
						Projectile.netUpdate = true;
					}
				}
				Projectile.rotation = Projectile.velocity.ToRotation();
				if (Main.myPlayer == Projectile.owner && --Projectile.ai[0] <= 0) {
					if (Projectile.ai[2] != -1) {
						originPlayer.luckyHatSetTime = 0;
						if (Main.mouseLeft && Projectile.ai[2] != -1) {
							ActuallyShoot();
						} else if (!player.channel) {
							Projectile.Kill();
						}
					} else {
						Projectile.ai[2] = 0;
					}
				}
			} else {
				Projectile.Kill();
			}
			if (Projectile.localAI[0] >= Projectile.localAI[1]) {
				Projectile.position += Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(0.3f, 0.6f);
				Projectile.rotation += Main.rand.NextFloat(0.02f) * Main.rand.NextBool().ToDirectionInt();
			}
		}
		public virtual bool ActuallyShoot() {
			Player player = Main.player[Projectile.owner];
			Vector2 position = Projectile.position;
			position += position.DirectionTo(Main.MouseWorld).RotatedBy(player.direction * -MathHelper.PiOver2) * BarrelOffset;
			bool fullCharge = Projectile.localAI[0] >= Projectile.localAI[1];
			if (player.PickAmmo(player.HeldItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemId)) {
				if (fullCharge) {
					projToShoot = ModContent.ProjectileType<Shimmershot_Bullet>();
					damage *= 2;
				}
				EntitySource_ItemUse_WithAmmo projectileSource = new(player, player.HeldItem, usedAmmoItemId, "gunProj");
				Vector2 velocity = Projectile.velocity;
				velocity *= speed;

				position += Projectile.velocity * 37 + new Vector2(-Projectile.velocity.Y, Projectile.velocity.X) * 6 * player.direction;
				CombinedHooks.ModifyShootStats(player, player.HeldItem, ref position, ref velocity, ref projToShoot, ref damage, ref knockBack);
				try {
					Shimmershot.isShooting = true;
					if (CombinedHooks.Shoot(player, player.HeldItem, projectileSource, position, velocity, projToShoot, damage, knockBack)) {
						Projectile.NewProjectile(projectileSource, position, velocity, projToShoot, damage, knockBack, Projectile.owner);
					}
				} finally {
					Shimmershot.isShooting = false;
				}
				Projectile.ai[2] = 1;
				Projectile.ai[0] = CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem);
				Projectile.localAI[0] = Projectile.ai[0] * -1f;
				Projectile.localAI[1] = Projectile.ai[0] * 2f;
				Projectile.netUpdate = true;
				return true;
			}
			return false;
		}
		public override bool ShouldUpdatePosition() => false;
		public DrawData GetDrawData(Color lightColor) {
			SpriteEffects dir = Main.player[Projectile.owner].direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (Main.player[Projectile.owner].gravDir == -1f) {
				dir ^= SpriteEffects.FlipVertically;
			}
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new Vector2(27, 12);
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			return new(
				texture,
				(Projectile.position - Main.screenPosition).Floor(),
				frame,
				lightColor,
				Projectile.rotation,
				origin.Apply(dir, frame.Size()),
				Projectile.scale,
				dir
			);
		}

		public void PreDrawScene() {
			bool charged = Projectile.localAI[0] >= Projectile.localAI[1];
			MathUtils.LinearSmoothing(ref Projectile.localAI[2], charged.ToInt(), 1f / (charged ? 6 : 4));
			if (Projectile.localAI[2] > 0 && !Main.player[Projectile.owner].OriginPlayer().weakShimmer) {
				SC_Phase_Three_Midlay.DrawDatas.AddRange(Outlineify(GetDrawData(Color.White * Projectile.localAI[2])));
			}
		}
		public IEnumerable<DrawData> Outlineify(DrawData data) {
			Vector2 basePos = data.position;
			for (int i = 0; i < 4; i++) {
				data.position = basePos + (data.rotation + MathHelper.PiOver2 * i).ToRotationVector2() * 2 * Projectile.scale;
				yield return data;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Main.player[Projectile.owner].OriginPlayer().weakShimmer) {
				SpriteBatchState state = Main.spriteBatch.GetState();
				Main.spriteBatch.Restart(state, SpriteSortMode.Immediate);
				foreach (DrawData data in Outlineify(GetDrawData(Color.White * Projectile.localAI[2]) with { shader = ContentSamples.CommonlyUsedContentSamples.ColorOnlyShaderIndex })) {
					GameShaders.Armor.Apply(ContentSamples.CommonlyUsedContentSamples.ColorOnlyShaderIndex, Projectile, data);
					data.Draw(Main.spriteBatch);
				}
				Main.spriteBatch.Restart(state);
			}
			Main.EntitySpriteDraw(GetDrawData(lightColor));
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
			writer.Write(Projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
			Projectile.localAI[1] = reader.ReadSingle();
		}
	}
	public class Shimmershot_Bullet : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VenomBullet;
		public virtual int AuraID => Shimmershot_Aura.ID;
		public virtual float AuraDamage => 0.5f;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BulletHighVelocity);
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.localAI[0] += 1f;
			if (Projectile.localAI[0] > 3f)
				Projectile.alpha = 0;

			if (Projectile.ai[0] >= 20f) {
				Projectile.ai[0] = 20f;
				Projectile.velocity.Y += 0.075f;
			}
			int auraProjIndex = (int)Projectile.ai[1] - 1;
			if (auraProjIndex < 0) {
				Projectile.ai[1] = (Projectile.SpawnProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.position,
					default,
					AuraID,
					(int)(Projectile.damage * AuraDamage),
					0,
					Projectile.identity
				)?.identity ?? -1) + 1;
			} else {
				Projectile auraProj = null;
				foreach (Projectile other in Main.ActiveProjectiles) {
					if (other.identity == auraProjIndex && other.owner == Projectile.owner) {
						auraProj = other;
						break;
					}
				}
				if ((auraProj?.active ?? false) && auraProj.type == AuraID) {
					auraProj.position = Projectile.Center;
					auraProj.rotation = Projectile.rotation;
				} else {
					Projectile.ai[1] = 0;
				}
			}
		}
		public override Color? GetAlpha(Color lightColor) {
			return Projectile.alpha == 0 ? new Color(255, 255, 255, 200) : Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.DefenseEffectiveness *= 1.5f; // to make it not effectively gain a ton of armor piercing on charged shots
			modifiers.CritDamage *= 1.5f;
		}
	}
	public class Shimmershot_Aura : ModProjectile, ITriggerSCBackground {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			ProjectileID.Sets.TrailingMode[Type] = -1;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 16 * 400;
			if (GetType().GetProperty("ID", BindingFlags.Static | BindingFlags.Public) is PropertyInfo id && id.PropertyType == typeof(int)) id.SetValue(null, Type);
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.hide = true;
			Projectile.width = Projectile.height = 0;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Projectile ownerProj = null;
			if (Projectile.ai[0] >= 0) {
				foreach (Projectile other in Main.ActiveProjectiles) {
					if (other.identity == Projectile.ai[0] && other.owner == Projectile.owner) {
						ownerProj = other;
						break;
					}
				}
			}
			if ((ownerProj?.active ?? false) && ownerProj.ModProjectile is Shimmershot_Bullet) {
				Projectile.scale = ownerProj.scale;
				Projectile.Center = ownerProj.Center;
				Projectile.rotation = ownerProj.rotation;
			} else {
				Projectile.ai[0] = -1;
			}
			for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
				Projectile.oldPos[i] = Projectile.oldPos[i - 1];
				Projectile.oldRot[i] = Projectile.oldRot[i - 1];
			}
			Projectile.oldPos[0] = Projectile.position;
			Projectile.oldRot[0] = Projectile.rotation;
			if (Projectile.oldPos[^1] == Projectile.position && Projectile.oldRot[^1] == Projectile.rotation) Projectile.Kill();
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return CollisionExtensions.PolygonIntersectsRect(GetPolygon().ToArray(), targetHitbox);
		}
		int lastCacheTime = -1;
		List<(Vector2 start, Vector2 end)> polygonCache;
		private List<(Vector2 start, Vector2 end)> GetPolygon() {
			if (Projectile.timeLeft == lastCacheTime && polygonCache is not null) return polygonCache;
			lastCacheTime = Projectile.timeLeft;
			Vector2 width = new(12, 0);
			Vector2 rot = width.RotatedBy(Projectile.rotation);
			Vector2 lastPos0 = Projectile.position - rot;
			Vector2 lastPos1 = Projectile.position + rot;
			Vector2 nextPos0 = default, nextPos1 = default;
			polygonCache = [(lastPos1, lastPos0)];
			for (int i = 0; i < Projectile.oldPos.Length; i++) {
				Vector2 nextPos = Projectile.oldPos[i];
				if (nextPos == default) break;
				rot = width.RotatedBy(Projectile.oldRot[i]);
				nextPos0 = nextPos - rot;
				nextPos1 = nextPos + rot;

				polygonCache.Add((lastPos0, nextPos0));
				polygonCache.Add((nextPos1, lastPos1));
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}
			polygonCache.Add((nextPos0, nextPos1));
			return polygonCache;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindProjectiles.Add(index);
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}
			Origins.shaderOroboros.Capture();
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:AnimatedTrail"];

			Vector2[] positions = new Vector2[Projectile.oldPos.Length - 1];
			float[] rotations = new float[Projectile.oldPos.Length - 1];
			int count = 0;
			for (int i = 1; i < Projectile.oldPos.Length; i++) {
				positions[i - 1] = Projectile.oldPos[i];
				rotations[i - 1] = Projectile.oldRot[i] + MathHelper.PiOver2;
				if (Projectile.oldPos[i] == default) break;
				count++;
			}
			float frameStart = 1 - count / (float)Projectile.oldPos.Length;
			miscShaderData.UseImage0(TextureAssets.Projectile[Type]);
			miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			//miscShaderData.UseImage0(TextureAssets.Extra[189]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
			miscShaderData.Shader.Parameters["uAlphaMatrix1"].SetValue(new Vector4(0.7f, 0, 0, 0));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(frameStart, 0, 1 - frameStart, 1));
			miscShaderData.Shader.Parameters["uSourceRect1"].SetValue(new Vector4(frameStart, MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.2f, 1 - frameStart, 1));
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (_) => Color.White, (_) => 24, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			miscShaderData.Shader.Parameters["uSourceRect1"].SetValue(new Vector4(frameStart, (float)Main.timeForVisualEffects * -0.1f, 1 - frameStart, 1));
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (_) => Color.White, (_) => 24, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = renderTarget.Size() * 0.5f;
			SC_Phase_Three_Midlay.DrawDatas.Add(new(
				renderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None
			));
			return false;
		}
		public override void OnKill(int timeLeft) {
			if (renderTarget is not null) {
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref renderTarget);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref renderTarget);
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
	// joke variation
	public class Shimmershotgun : Shimmershot {
		public override string Texture => typeof(Shimmershot).GetDefaultTMLName();
		public override void AddRecipes() { }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (base.Shoot(player, source, position, velocity, type, damage, knockback)) {
				for (int i = 0; i < 5; i++) {
					Projectile.NewProjectile(source, position, velocity.RotatedByRandom((i + 1) * 0.1f), type, damage, knockback);
				}
			}
			return false;
		}
	}
}

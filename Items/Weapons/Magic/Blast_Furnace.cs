using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static Origins.Items.Weapons.Melee.Personal_Laser_Blade_P;

namespace Origins.Items.Weapons.Magic {
	public class Blast_Furnace : ModItem, ICustomWikiStat {
		public const int max_charges = 5;
		public string[] Categories => [
			WikiCategories.SpellBook
		];
		public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.DamageType = DamageClass.Magic;
			Item.damage = 20;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 37;
			Item.useAnimation = 37;
			Item.shoot = ModContent.ProjectileType<Blast_Furnace_Charge>();
			Item.shootSpeed = 16f;
			Item.mana = 13;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item34;
			Item.autoReuse = false;
			Item.channel = true;
		}
		public override bool AltFunctionUse(Player player) => player.OriginPlayer().blastFurnaceCharges < max_charges;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse != 2) type = ModContent.ProjectileType<Blast_Furnace_P>();
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) => damage *= Utils.Remap(player.OriginPlayer().blastFurnaceCharges, 0, max_charges, 1, 2);
		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) => knockback *= Utils.Remap(player.OriginPlayer().blastFurnaceCharges, 0, max_charges, 1, 5);
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) return base.Shoot(player, source, position, velocity, type, damage, knockback);
			ref int blastFurnaceCharges = ref player.OriginPlayer().blastFurnaceCharges;
			for (int i = 3 + (blastFurnaceCharges + 1) / 3; i > 0; i--) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(1f),
					type,
					damage,
					knockback
				);
			}
			if (blastFurnaceCharges > 0) blastFurnaceCharges--;
			return false;
		}
	}
	public class Blast_Furnace_UI : SwitchableUIState {
		public override void AddToList() => OriginSystem.Instance.ItemUseHUD.AddState(this);
		public override bool IsActive() => Main.LocalPlayer.HeldItem.ModItem is Blast_Furnace;
		public override InterfaceScaleType ScaleType => InterfaceScaleType.Game;
		readonly AutoLoadingAsset<Texture2D> texture = typeof(Blast_Furnace_UI).GetDefaultTMLName();
		public override void Draw(SpriteBatch spriteBatch) {
			Rectangle frame = texture.Frame(verticalFrames: Blast_Furnace.max_charges + 1, frameY: Main.LocalPlayer.OriginPlayer().blastFurnaceCharges);
			spriteBatch.Draw(
				texture,
				Main.LocalPlayer.Top.Floor() - Vector2.UnitY * (12 - Main.LocalPlayer.gfxOffY) - frame.Size() * new Vector2(0.5f, 1f) - Main.screenPosition,
				frame,
				Color.White
			);
		}
	}
	public class Blast_Furnace_Charge : ModProjectile {
		public static float ChargeTimeMultiplier => 0.65f;
		public override string Texture => typeof(Blast_Furnace).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse { Player: Player player }) Projectile.ai[0] = player.itemTimeMax * ChargeTimeMultiplier;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (!Projectile.TryGetOwner(out Player player)) {
				Projectile.Kill();
				return;
			}
			Projectile.position = player.MountedCenter;
			if (!player.channel) {
				Projectile.Kill();
				return;
			}
			player.SetDummyItemTime(5);
			if (--Projectile.ai[0] <= 0) {
				if (player.HeldItem?.ModItem?.AltFunctionUse(player) != true) {
					Projectile.Kill();
					return;
				}
				player.OriginPlayer().blastFurnaceCharges++;
				Projectile.ai[0] += CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem) * ChargeTimeMultiplier;
			}
		}
	}
	public class Blast_Furnace_P : ModProjectile {
		public override string Texture => typeof(Blast_Furnace).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
		}
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.timeLeft = 60;
			Projectile.extraUpdates = 2;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool ShouldUpdatePosition() => false;
		record struct Wave(double Frequency, double Amplitude, double Phase) {
			public double Sample(double position) => Math.Sin(position * Frequency + Phase) * Amplitude;
		}
		Wave[] waves;
		public override void OnSpawn(IEntitySource source) {
			waves = new Wave[Main.rand.Next(13, 21)];
			for (int i = 0; i < waves.Length; i++) {
				waves[i] = new(Main.rand.NextFloat(0.1f, 0.5f), Main.rand.NextFloat(0.02f, 0.1f), Main.rand.NextFloat(MathHelper.TwoPi));
			}
		}
		Vector2 originalVelocity;
		public override void AI() {
			Projectile.Opacity = Projectile.timeLeft / 25f;
			if (Projectile.TryGetOwner(out Player player)) Projectile.position = player.MountedCenter;
			if (Projectile.ai[2] == 0) originalVelocity = Projectile.velocity;
			if (++Projectile.ai[2] >= Projectile.oldPos.Length) return;
			Projectile.ai[0] += Projectile.velocity.X;
			Projectile.ai[1] += Projectile.velocity.Y;
			ProcessTick();
			double value = 0;
			for (int i = 0; i < waves.Length; i++) value += waves[i].Sample(Projectile.ai[2]);
			Projectile.velocity = Projectile.velocity.RotatedBy(value);
			Projectile.velocity = Vector2.Lerp(
				originalVelocity,
				Projectile.velocity,
				float.Clamp(Vector2.Dot(originalVelocity.Normalized(out _), new Vector2(Projectile.ai[0], Projectile.ai[1]).Normalized(out _)) * 2, 0, 1)
			);
			SetHitboxCache();
			void ProcessTick() {
				Projectile.oldPos[(int)Projectile.ai[2]] = new Vector2(Projectile.ai[0], Projectile.ai[1]);
				Projectile.oldRot[(int)Projectile.ai[2]] = Projectile.velocity.ToRotation() + MathHelper.Pi;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (polygonCache is null) return false;
			targetHitbox.Offset((-Projectile.position).ToPoint());
			return CollisionExtensions.PolygonIntersectsRect(polygonCache, targetHitbox);
		}
		(Vector2 start, Vector2 end)[] polygonCache;
		private void SetHitboxCache() {
			Vector2 width = new(12, 0);
			Vector2 rot = width.RotatedBy(Projectile.rotation);
			Vector2 lastPos0 = -rot;
			Vector2 lastPos1 = rot;
			Vector2 nextPos0 = default, nextPos1 = default;
			int count = int.Min((int)Projectile.ai[2], Projectile.oldPos.Length);
			polygonCache = new (Vector2 start, Vector2 end)[count * 2 + 2];
			int lineIndex = 0;
			polygonCache[lineIndex++] = (lastPos1, lastPos0);
			for (int i = 0; i < count; i++) {
				Vector2 nextPos = Projectile.oldPos[i];
				rot = width.RotatedBy(Projectile.oldRot[i]);
				nextPos0 = nextPos - rot;
				nextPos1 = nextPos + rot;

				polygonCache[lineIndex++] = (lastPos0, nextPos0);
				polygonCache[lineIndex++] = (nextPos1, lastPos1);
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}
			polygonCache[lineIndex++] = (nextPos0, nextPos1);
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			Vector2[] oldPos = new Vector2[int.Min((int)Projectile.ai[2], Projectile.oldPos.Length)];
			for (int i = 0; i < oldPos.Length; i++) {
				oldPos[i] = Projectile.oldPos[i] + Projectile.position;
			}
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:LaserBlade"];
			miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.FlameLashTrailShape]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
			miscShaderData.UseSaturation(-1);
			miscShaderData.UseOpacity(2);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, Projectile.oldRot, BladeSecondaryColors, BladeWidth,  -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			miscShaderData.UseSaturation(0.5f);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, Projectile.oldRot, BladeColors, BladeWidth, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			return false;
		}

		private Color BladeColors(float progressOnStrip) => new Color(255, 69, 0, 32) * Projectile.Opacity;
		private Color BladeSecondaryColors(float progressOnStrip) => new Color(218, 165, 32, 32) * Projectile.Opacity;
		private static float BladeWidth(float progressOnStrip) {
			return 12;
		}
	}
}

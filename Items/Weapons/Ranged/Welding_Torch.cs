using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Welding_Torch : ModItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ElfMelter);
			Item.damage = 25;
			Item.useAnimation = 20;
			Item.useTime = 5;
			Item.mana = 5;
			Item.width = 36;
			Item.height = 16;
			Item.useAmmo = AmmoID.None;
			Item.shoot = ModContent.ProjectileType<Welding_Torch_P>();
			Item.shootSpeed = 4f;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration = 25;
			Item.UseSound = SoundID.Item34;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position += velocity.SafeNormalize(default) * 12;
		}
	}
	public class Welding_Torch_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_85";
		public static float Lifetime => 60f;
		public static float MinSize => 30f;
		public static float MaxSize => 6f;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 7;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 6;
			Projectile.penetrate = 4;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.extraUpdates = 3;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			if (Projectile.localAI[2] == 0) {
				Projectile.localAI[2] = 1 + Projectile.wet.ToInt();
			}
			Projectile.localAI[0] += 1f;
			if (Projectile.localAI[2] == 1) {
				Lighting.AddLight(Projectile.Center, 0f, 0.85f, 0.4f);
			}
			//Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostStaff);
			Projectile.ai[0]++;
			Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
			Projectile.alpha = (int)(200 * (1 - (Projectile.localAI[0] / Lifetime)));
			Projectile.rotation += 0.3f * Projectile.direction;
			Projectile.velocity *= 0.97f;
			if (Projectile.ai[0] > Lifetime) {
				Projectile.Kill();
			}
			Rectangle hitbox = Projectile.Hitbox;
			for (int i = 0; i < healCooldown.Length; i++) {
				if (healCooldown[i] > 0) healCooldown[i]--;
			}
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (healCooldown[other.whoAmI] > 0) continue;
				if (Projectile.Colliding(hitbox, other.Hitbox) && other.ModProjectile is IArtifactMinion artifactMinion && artifactMinion.Life < artifactMinion.MaxLife) {
					float oldHealth = artifactMinion.Life;
					artifactMinion.Life += Projectile.damage * 0.15f + 0.004f * artifactMinion.MaxLife;
					if (artifactMinion.Life > artifactMinion.MaxLife) artifactMinion.Life = artifactMinion.MaxLife;
					CombatText.NewText(other.Hitbox, CombatText.HealLife, (int)Math.Round(artifactMinion.Life - oldHealth), true, dot: true);
					healCooldown[other.whoAmI] = 20;
				}
			}
		}
		int[] healCooldown = new int[Main.maxProjectiles];
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int scale = (int)Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize - 6, MaxSize - 6);
			hitbox.Inflate(scale, scale);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, hit.Crit ? 600 : 360);
		}
		public override bool PreDraw(ref Color lightColor) {
			Color color1 = new(255, 160, 80, 200);
			Color color2 = new(255, 120, 30, 200);
			Color color3 = new(255, 120, 30, 93);
			Color color4 = new(30, 30, 30, 100);
			const float num = 60f;
			const float num2 = 12f;
			const float fromMax = num + num2;
			Texture2D value = TextureAssets.Projectile[Type].Value;
			float num3 = 0.35f;
			float num4 = 0.7f;
			float num5 = 0.85f;
			float num6 = ((Projectile.localAI[0] > num - 10f) ? 0.175f : 0.2f);
			int verticalFrames = 7;
			float num9 = Utils.Remap(Projectile.localAI[0], num, fromMax, 1f, 0f);
			float num10 = Math.Min(Projectile.localAI[0], 20f);
			float num11 = Utils.Remap(Projectile.localAI[0], 0f, fromMax, 0f, 1f);
			Rectangle rectangle = value.Frame(1, verticalFrames, 0, 3);
			if (num11 >= 1f) return false;
			for (int i = 0; i < 2; i++) {
				for (float num13 = 1f; num13 >= 0f; num13 -= num6) {
					Color obj = ((num11 < 0.1f) ? Color.Lerp(Color.Transparent, color1, Utils.GetLerpValue(0f, 0.1f, num11, clamped: true)) : ((num11 < 0.2f) ? Color.Lerp(color1, color2, Utils.GetLerpValue(0.1f, 0.2f, num11, clamped: true)) : ((num11 < num3) ? color2 : ((num11 < num4) ? Color.Lerp(color2, color3, Utils.GetLerpValue(num3, num4, num11, clamped: true)) : ((num11 < num5) ? Color.Lerp(color3, color4, Utils.GetLerpValue(num4, num5, num11, clamped: true)) : ((!(num11 < 1f)) ? Color.Transparent : Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(num5, 1f, num11, clamped: true))))))));
					float num14 = (1f - num13) * Utils.Remap(num11, 0f, 0.2f, 0f, 1f);
					Vector2 vector = Projectile.Center - Main.screenPosition + Projectile.velocity * (0f - num10) * num13;
					Color color5 = obj * num14;
					float num15 = 1f / num6 * (num13 + 1f);
					float num16 = Projectile.rotation + num13 * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly * num15 * 2f;
					float num17 = Projectile.rotation - num13 * MathHelper.PiOver2 - Main.GlobalTimeWrappedHourly * num15 * 2f;
					switch (i) {
						case 0:
						Main.EntitySpriteDraw(value, vector + Projectile.velocity * (0f - num10) * num6 * 0.5f, rectangle, color5 * num9 * 0.25f, num16 + (float)Math.PI / 4f, rectangle.Size() / 2f, Projectile.scale, SpriteEffects.None);
						Main.EntitySpriteDraw(value, vector, rectangle, color5 * num9, num17, rectangle.Size() / 2f, Projectile.scale, SpriteEffects.None);
						break;
						case 1:
						color5.A = 0;
						Main.EntitySpriteDraw(value, vector + Projectile.velocity * (0f - num10) * num6 * 0.2f, rectangle, color5 * num9 * 0.25f, num16 + (float)Math.PI / 2f, rectangle.Size() / 2f, 0.75f * Projectile.scale, SpriteEffects.None);
						Main.EntitySpriteDraw(value, vector, rectangle, color5 * num9, num17 + (float)Math.PI / 2f, rectangle.Size() / 2f, 0.75f * Projectile.scale, SpriteEffects.None);
						break;
					}
				}
			}
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}

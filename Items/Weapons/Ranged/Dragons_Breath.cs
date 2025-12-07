using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics.Primitives;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Dragons_Breath : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Gun"
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.OnFire3];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ElfMelter);
			Item.damage = 17;
			Item.useAnimation = 20;
			Item.useTime = 5;
			Item.width = 36;
			Item.height = 16;
			Item.useAmmo = ItemID.Fireblossom;
			Item.shoot = ModContent.ProjectileType<Dragons_Breath_P>();
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration = 20;
			Item.UseSound = SoundID.Item34;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-6, 0);
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) => Main.rand.NextBool(3, 5);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position += velocity.SafeNormalize(default) * 36;
		}
	}
	public class Dragons_Breath_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_85";
		public static float Lifetime => 50f;
		public static float MinSize => 16f;
		public static float MaxSize => 66f;
		private Vector2 startingVel = Vector2.Zero;
		private float[] rots = new float[21];
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
			Projectile.extraUpdates = 1;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.timeLeft = (int)Lifetime;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
				rots[i] = Main.rand.NextFloatDirection();
		}
		public override void AI() {
			Projectile.velocity *= 1.01f;
			if (Projectile.wet && !Projectile.lavaWet) {
				Projectile.localAI[2] += 1.9f;
				Projectile.localAI[0] -= 0.9f;
			}
			Projectile.localAI[0] += 1f;
			if (Projectile.ai[0] == 0) {
				float progress = (Projectile.localAI[0] + Projectile.localAI[2]) / Lifetime;
				if (Main.rand.NextFloat(1) < progress * 0.85f) {
					Projectile.ai[0] = Main.rand.NextFloat(0.02f, 0.07f) * Main.rand.NextBool().ToDirectionInt();
					Projectile.ai[1] = Main.rand.NextFloat(0.5f, 1f);
					Projectile.ai[2] = Main.rand.NextFloat(0f, 0.1f);
					Projectile.netUpdate = true;
				}
			} else {
				Projectile.localAI[1] += Projectile.ai[1] + Projectile.ai[2] * Projectile.localAI[1];
				Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] * (Projectile.localAI[1] - 10) * 0.3f);
				float abs = Math.Abs(Projectile.ai[0]) * 6;
				Projectile.localAI[2] += abs * 2;
				Projectile.localAI[0] -= abs;
			}
			Dust.NewDust(Projectile.Hitbox.Location.ToVector2(), Projectile.Hitbox.Width, Projectile.Hitbox.Height, DustID.OrangeTorch);
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int scale = (int)MathHelper.Lerp(MinSize, MaxSize, 1 - (Projectile.timeLeft / Lifetime));
			hitbox.Inflate(scale, scale);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, hit.Crit ? 600 : 360);
		}
		public override bool PreDraw(ref Color lightColor) {
			/*Projectile.DrawFlamethrower(
				new Color(255, 80, 20, 200),
				new Color(255, 80, 20, 200),
				new Color(255, 20, 20, 93),
				new Color(30, 30, 30, 100)
			);*/
			//Color color1 = new(255, 80, 20, 200);
			//Color color2 = new(255, 80, 20, 200);
			//Color color3 = new(255, 20, 20, 93);
			//Color color4 = new(30, 30, 30, 100);
			//const float num = 60f;
			//const float num2 = 12f;
			//const float fromMax = num + num2;
			//Texture2D value = TextureAssets.Projectile[Projectile.type].Value;
			//float num3 = 0.35f;
			//float num4 = 0.7f;
			//float num5 = 0.85f;
			//float num6 = ((Projectile.localAI[0] > num - 10f) ? 0.175f : 0.2f);
			//int verticalFrames = 7;
			//float progress = Utils.Remap(Projectile.localAI[0], 0f, fromMax, 0f, 1f);
			//float fade = Utils.Remap(Projectile.localAI[0] + Projectile.localAI[2], num, fromMax, 1f, 0f);
			//float num10 = Math.Min(Projectile.localAI[0] + Projectile.localAI[2], 20f);
			//float num11 = Utils.Remap(Projectile.localAI[0] + Projectile.localAI[2], 0f, fromMax, 0f, 1f);
			//float scale = Utils.Remap(progress, 0.2f, 0.5f, 0.25f, 1f);
			//Rectangle rectangle = value.Frame(1, verticalFrames, 0, (int)Utils.Remap(num11, 0.5f, 1f, 3f, 5f));
			//if (num11 >= 1f) return false;
			//for (int i = 0; i < 2; i++) {
			//	for (float num13 = 1f; num13 >= 0f; num13 -= num6) {
			//		Color obj = ((num11 < 0.1f) ? Color.Lerp(Color.Transparent, color1, Utils.GetLerpValue(0f, 0.1f, num11, clamped: true)) : ((num11 < 0.2f) ? Color.Lerp(color1, color2, Utils.GetLerpValue(0.1f, 0.2f, num11, clamped: true)) : ((num11 < num3) ? color2 : ((num11 < num4) ? Color.Lerp(color2, color3, Utils.GetLerpValue(num3, num4, num11, clamped: true)) : ((num11 < num5) ? Color.Lerp(color3, color4, Utils.GetLerpValue(num4, num5, num11, clamped: true)) : ((!(num11 < 1f)) ? Color.Transparent : Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(num5, 1f, num11, clamped: true))))))));
			//		float num14 = (1f - num13) * Utils.Remap(num11, 0f, 0.2f, 0f, 1f);
			//		Vector2 vector = Projectile.oldPos[(int)(num10 * num13)] - Main.screenPosition;
			//		Color color5 = obj * num14;
			//		float num15 = 1f / num6 * (num13 + 1f);
			//		float num16 = Projectile.rotation + num13 * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly * num15 * 2f;
			//		float num17 = Projectile.rotation - num13 * MathHelper.PiOver2 - Main.GlobalTimeWrappedHourly * num15 * 2f;
			//		switch (i) {
			//			case 0:
			//			Main.EntitySpriteDraw(value, vector + Projectile.velocity * (0f - num10) * num6 * 0.5f, rectangle, color5 * fade * 0.25f, num16 + (float)Math.PI / 4f, rectangle.Size() / 2f, scale, SpriteEffects.None);
			//			Main.EntitySpriteDraw(value, vector, rectangle, color5 * fade, num17, rectangle.Size() / 2f, scale, SpriteEffects.None);
			//			break;
			//			case 1:
			//			Main.EntitySpriteDraw(value, vector + Projectile.velocity * (0f - num10) * num6 * 0.2f, rectangle, color5 * fade * 0.25f, num16 + (float)Math.PI / 2f, rectangle.Size() / 2f, scale * 0.75f, SpriteEffects.None);
			//			Main.EntitySpriteDraw(value, vector, rectangle, color5 * fade, num17 + (float)Math.PI / 2f, rectangle.Size() / 2f, scale * 0.75f, SpriteEffects.None);
			//			break;
			//		}
			//	}
			//}


			default(DragonBreathFire).Draw(Projectile, Projectile.timeLeft / Lifetime, Color.Lerp(Color.Red, Color.Orange, Projectile.timeLeft / Lifetime), Color.Goldenrod, rots);

			return false;
		}

		public struct DragonBreathFire {

			private static VertexRectangle rect = new VertexRectangle();
			private static VertexStrip strip = new VertexStrip();
			public void Draw(Projectile projectile, float progress, Color color, Color smokeColor, float[] rots) {
				for (int i = 0; i < projectile.oldPos.Length; i++) {
					MiscShaderData shaderData = GameShaders.Misc["Origins:FireShader"];
				shaderData.UseColor(color);
				shaderData.UseSecondaryColor(smokeColor);
				shaderData.UseShaderSpecificData(new(progress, rots[i], 0, 0));
				shaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
				shaderData.Apply();

				rect.Draw(projectile.oldPos[i] - Main.screenPosition, Color.White, new(256), 0, projectile.oldPos[i]);
				}

			}

		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}

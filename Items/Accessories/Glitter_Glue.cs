using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories;
public class Glitter_Glue : ModItem {
	public override void SetStaticDefaults() {
		OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, _) => {
			if (!Main.dayTime) global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 1);
		});
	}
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = ItemRarityID.Yellow;
		Item.master = true;
		Item.damage = 40;
		Item.DamageType = DamageClass.Magic;
		Item.shoot = ModContent.ProjectileType<Glitter_Glue_P>();
		Item.knockBack = 1;
		Item.useTime = 60 * 2;// controls cooldown
		Item.useAnimation = Item.useTime;
		Item.useLimitPerAnimation = 12; // controls burst count
		Item.value = Item.sellPrice(gold: 5);
		Item.ArmorPenetration += 3;
	}
	public override void UpdateAccessory(Player player, bool hideVisual) => player.OriginPlayer().glitterGlue = Item;
	public override bool MagicPrefix() => true;
	public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
		if (Main.dayTime) damage *= 1.25f;
	}
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Magic, 1), (PrefixCategory.Accessory, 2));
	}
}
public class Glitter_Glue_P : ModProjectile {
	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FairyQueenMagicItemShot;
	bool isDay;
	public Color Color => isDay ? Main.OurFavoriteColor : Projectile.GetFairyQueenWeaponsColor();
	public override void SetStaticDefaults() {
		ProjectileID.Sets.TrailingMode[Type] = 2;
		ProjectileID.Sets.TrailCacheLength[Type] = 20;
		ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		ProjectileID.Sets.DrawScreenCheckFluff[Type] = 960;
	}
	public override void SetDefaults() {
		Projectile.DamageType = DamageClass.Magic;
		Projectile.friendly = true;
		Projectile.width = 30;
		Projectile.height = 30;
		Projectile.aiStyle = 0;
		Projectile.alpha = 255;
		Projectile.penetrate = 3;
		Projectile.timeLeft = 240;
		Projectile.tileCollide = true;
		Projectile.ignoreWater = true;
		Projectile.extraUpdates = 1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 60;
		isDay = Main.dayTime;
	}
	public override void AI() {
		bool startup = false;
		bool canHome = false;

		float speed = 20f;
		float value = 0.075f;
		float value2 = 0.125f;

		if (Projectile.timeLeft > 180f)
			startup = true;
		else if (Projectile.timeLeft > 20f)
			canHome = true;

		if (startup) {
			float num6 = (float)Math.Cos(Projectile.whoAmI % 6f / 6f + Projectile.position.X / 320f + Projectile.position.Y / 160f);
			Projectile.velocity *= 0.97f;
			Projectile.velocity = Projectile.velocity.RotatedBy(num6 * MathHelper.TwoPi * 0.125f * 1f / 30f);
		}

		if (Projectile.friendly) {
			int targetIndex = (int)Projectile.ai[0];
			if (Main.npc.IndexInRange(targetIndex) && !Main.npc[targetIndex].CanBeChasedBy(this)) {
				targetIndex = -1;
				Projectile.ai[0] = -1f;
				Projectile.netUpdate = true;
			}

			if (targetIndex == -1 && Projectile.ai[0].TrySet(Projectile.FindTargetWithLineOfSight())) {
				Projectile.netUpdate = true;
			}
		}

		if (canHome) {
			int targetIndex = (int)Projectile.ai[0];
			Vector2 direction = Projectile.velocity;

			if (Projectile.friendly) {
				if (Main.npc.IndexInRange(targetIndex)) {
					Max(ref Projectile.timeLeft, 10);

					direction = Projectile.DirectionTo(Main.npc[targetIndex].Center) * speed;
				} else {
					Projectile.timeLeft--;
				}
			}

			float amount = MathHelper.Lerp(value, value2, Utils.GetLerpValue(180, 30f, Projectile.timeLeft, clamped: true));
			Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, direction, amount);
			Projectile.velocity *= MathHelper.Lerp(0.85f, 1f, Utils.GetLerpValue(0f, 90f, Projectile.timeLeft, clamped: true));
		}

		Projectile.Opacity = Utils.GetLerpValue(240f, 220f, Projectile.timeLeft, clamped: true);
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
	}
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		Projectile.damage -= Projectile.damage / 5;
		if (Projectile.ai[0].TrySet(Projectile.FindTargetWithLineOfSight()))
			Projectile.netUpdate = true;
	}
	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
		width = 4;
		height = 4;
		return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
	}
	public override bool OnTileCollide(Vector2 oldVelocity) {
		if (Projectile.velocity.X != oldVelocity.X)
			Projectile.velocity.X = oldVelocity.X * -1f;

		if (Projectile.velocity.Y != oldVelocity.Y)
			Projectile.velocity.Y = oldVelocity.Y * -1f;

		return false;
	}
	public override Color? GetAlpha(Color lightColor) {
		Color color = Color * Projectile.Opacity;
		color.A /= 2;
		return color;
	}
	public override bool PreDraw(ref Color lightColor) {
		Vector2 center = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
		Color color = Projectile.GetAlpha(default);
		SpriteEffects dir = SpriteEffects.None;
		if (Projectile.spriteDirection == -1) dir = SpriteEffects.FlipHorizontally;
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() * 0.5f;
			for (int i = 1; i < Projectile.oldPos.Length; i += 2) {
				Vector2 oldPos = Projectile.oldPos[i];
				if (oldPos == Vector2.Zero) continue;

				Vector2 position = oldPos + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
				Main.EntitySpriteDraw(
					texture,
					position,
					null,
					color * ((Projectile.oldPos.Length - i) / (ProjectileID.Sets.TrailCacheLength[Type] * 1.5f)),
					Projectile.oldRot[i],
					origin,
					MathHelper.Lerp(Projectile.scale, 1, i / 15f),
					(Projectile.oldSpriteDirection[i] == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None
				);
			}
			{
				Color sparkleColor = Color;
				sparkleColor.A = 0;
				Main.EntitySpriteDraw(texture, center, null, sparkleColor, Projectile.rotation, origin, Projectile.scale * 0.9f, dir);

				Texture2D sparkleTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
				float sparkleBrightness = Utils.GetLerpValue(15f, 30f, Projectile.timeLeft, clamped: true)
					* Utils.GetLerpValue(240f, 200f, Projectile.timeLeft, clamped: true)
					* (1f + 0.2f * MathF.Cos(Main.GlobalTimeWrappedHourly % 30f / 0.5f * MathHelper.TwoPi * 3f))
					* 0.8f;
				Vector2 sparkleScale1 = new Vector2(0.5f, 5f) * sparkleBrightness * 0.4f;
				Vector2 sparkleScale2 = new Vector2(0.5f, 2f) * sparkleBrightness * 0.4f;
				Color sparkleColor1 = sparkleColor * sparkleBrightness;
				Color sparkleColor2 = sparkleColor * sparkleBrightness * 0.5f;

				DrawData data = new(sparkleTexture, center, null, sparkleColor1, Projectile.rotation + MathHelper.PiOver2, sparkleTexture.Size() * 0.5f, sparkleScale1, dir);
				Main.EntitySpriteDraw(data with { rotation = Projectile.rotation, scale = sparkleScale1 });
				Main.EntitySpriteDraw(data with { scale = sparkleScale2 });
				Main.EntitySpriteDraw(data with { color = sparkleColor2, rotation = Projectile.rotation, scale = sparkleScale1 * 0.6f});
				Main.EntitySpriteDraw(data with { color = sparkleColor2, scale = sparkleScale2 * 0.6f });
			}
			Main.EntitySpriteDraw(
				texture,
				center,
				null,
				color,
				Projectile.rotation,
				origin,
				Projectile.scale,
				dir
			);
		}
		return false;
	}
	public override void OnKill(int timeLeft) {
		Color color = Color;
		SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
		for (int i = 0; i < Projectile.oldPos.Length; i++) {
			Vector2 oldPos = Projectile.oldPos[i];
			if (oldPos == Vector2.Zero) break;

			int dustCount = Main.rand.Next(1, 3);
			float dustScale = MathHelper.Lerp(0.3f, 1f, Utils.GetLerpValue(Projectile.oldPos.Length, 0f, i, clamped: true));

			if (i >= Projectile.oldPos.Length * 0.3f) dustCount--;
			if (i >= Projectile.oldPos.Length * 0.75f) dustCount -= 2;

			for (float j = 0f; j < dustCount; j++) {
				int index = Dust.NewDust(oldPos, Projectile.width, Projectile.height, DustID.RainbowMk2, 0f, 0f, 0, color);
				if (index == 6000) continue;
				Dust dust = Main.dust[index];
				dust.velocity *= Main.rand.NextFloat() * 0.8f;
				dust.noGravity = true;
				dust.scale = 0.9f + Main.rand.NextFloat() * 1.2f;
				dust.fadeIn = Main.rand.NextFloat() * 1.2f * dustScale;
				dust.scale *= dustScale;

				dust = Dust.CloneDust(index);
				dust.scale /= 2f;
				dust.fadeIn *= 0.85f;
				dust.color = new Color(255, 255, 255, 255);
			}
		}
	}
}

using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other {
	public class Armor_Power_Up : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.IgnoresEncumberingStone[Type] = true;
			ItemID.Sets.IsAPickup[Type] = true;
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Heart);
			Item.width = 70;
			Item.height = 42;
			Item.rare = ItemRarityID.Green;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			gravity = 0.2f;
			maxFallSpeed = 2;
			float num = Main.rand.Next(90, 111) * 0.01f;
			num *= Main.essScale;
			Lighting.AddLight(Item.Center, 0, 0.6f * num, 0);
			Item.velocity *= 1.02f;
		}
		public override bool OnPickup(Player player) {
			SoundEngine.PlaySound(Origins.Sounds.PowerUp);
			player.Heal(350);
			Dust.NewDustPerfect(Item.Center, Armor_Power_Up_Dust.ID, Vector2.Zero);
			return false;
		}
		public override Color? GetAlpha(Color lightColor) => Color.White * (Main.essScale + 0.5f);
		public override bool ItemSpace(Player player) => true;
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float _scale, int whoAmI) {
			Vector2 center = Item.Center - Main.screenPosition;
			Main.instance.LoadItem(ProjectileID.RainbowRodBullet);
			Texture2D texture = TextureAssets.Projectile[ProjectileID.RainbowRodBullet].Value;
			Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;
			float scale = 3;
			Vector2 spinningpoint6 = new Vector2(2f * scale + (float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi) * 0.4f, 0f).RotatedBy(0 + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);
			for (float f = 0f; f < 1f; f += 1f / 6f) {
				Vector2 pos = center + spinningpoint6.RotatedBy(f * MathHelper.TwoPi);
				float num = Main.rand.Next(90, 111) * 0.01f;
				num *= Main.essScale;
				Main.EntitySpriteDraw(texture, pos, null, Color.Lime * num, 0, origin, scale * num, SpriteEffects.None);
			}
			return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}
	}
	public class Armor_Power_Up_Dust : ModDust {
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override bool Update(Dust dust) {
			dust.scale = dust.scale * 0.99f - 0.01f;
			if (dust.scale <= 0) dust.active = false;
			return false;
		}
		public override bool PreDraw(Dust dust) {
			Vector2 center = dust.position - Main.screenPosition;
			Texture2D texture = Texture2D.Value;
			Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;
			float scale = 3 * dust.scale;
			Vector2 spinningpoint6 = new Vector2(2f * scale + (float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi) * 0.4f, 0f).RotatedBy(0 + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);
			for (float f = 0f; f < 1f; f += 1f / 6f) {
				Vector2 pos = center + spinningpoint6.RotatedBy(f * MathHelper.TwoPi);
				float num = Main.rand.Next(90, 111) * 0.01f;
				num *= Main.essScale * dust.scale;
				Main.EntitySpriteDraw(texture, pos, null, Color.Lime * num, 0, origin, scale * num, SpriteEffects.None);
			}
			return false;
		}
	}
}

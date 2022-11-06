using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;

namespace Origins.Items.Tools {
	public class Amoeba_Hook : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amoeba Hook");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 18f;
			Item.shoot = ProjectileType<Amoeba_Hook_Projectile>();
		}
	}
	public class Amoeba_Hook_Projectile : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.Hook;
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.netImportant = true;
		}

		// Amethyst Hook is 300, Static Hook is 600
		public override float GrappleRange() {
			return 450f;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks) {
			numHooks = 1;
		}

		// default is 11, Lunar is 24
		public override void GrappleRetreatSpeed(Player player, ref float speed) {
			speed = 11f;
		}

		public override void GrapplePullSpeed(Player player, ref float speed) {
			speed = 0f;
		}

		public override bool PreDraw(ref Color lightColor) {
			Player owner = Main.player[Projectile.owner];
			Vector2 playerCenter = owner.MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 distToProj = playerCenter - Projectile.Center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();
			distToProj.Normalize();
			distToProj *= 8f;
			DrawData data;
			while (distance > 8f && !float.IsNaN(distance)) {
				center += distToProj;
				distance = (playerCenter - center).Length();
				Color drawColor = lightColor;

				data = new DrawData(TextureAssets.Chain30.Value, center - Main.screenPosition,
					new Rectangle(0, 0, TextureAssets.Chain30.Value.Width, TextureAssets.Chain30.Value.Height), drawColor, projRotation,
					new Vector2(TextureAssets.Chain30.Value.Width * 0.5f, TextureAssets.Chain30.Value.Height * 0.5f), new Vector2(0.75f,1), SpriteEffects.None, 0);
				data.shader = owner.cGrapple;
				Main.EntitySpriteDraw(data);
			}
			return true;
		}
		public override void PostAI() {
			if(Projectile.ai[0] != 2f)return;
			Player player = Main.player[Projectile.owner];
			player.grappling[--player.grapCount] = -1;
			if (player.whoAmI == Main.myPlayer) {
				if (Terraria.GameInput.PlayerInput.Triggers.JustPressed.Jump || (Projectile.ai[1] == 26 && player.controlJump)) {
					Projectile.Kill();
				}
			}
			player.GetModPlayer<OriginPlayer>().hookTarget = Projectile.whoAmI;
			if (player.Hitbox.Intersects(Projectile.Hitbox)) {
				if(Projectile.ai[1] > 0) Projectile.ai[1] = 26;
			} else {
				Projectile.ai[1]++;
			}
		}
	}
}

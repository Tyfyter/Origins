using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Origins.Items.Weapons.Summoner;

namespace Origins.Items.Tools {
	public class Amoeba_Hook : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 18f;
			Item.shoot = ProjectileType<Amoeba_Hook_P>();
			Item.value = Item.sellPrice(silver: 8);
		}
	}
	public class Amoeba_Hook_P : ModProjectile {
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
		public override bool PreDrawExtras() {
			Player owner = Main.player[Projectile.owner];
			Texture2D texture = TextureAssets.Projectile[ProjectileType<Flagellash_P>()].Value;
			Vector2 playerCenter = owner.MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 distToProj = playerCenter - Projectile.Center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();
			distToProj.Normalize();
			distToProj *= 8f;
			DrawData data;
			int progress = -2;
			while (distance > 8f && !float.IsNaN(distance)) {
				center += distToProj;
				distance = (playerCenter - center).Length();
				Color drawColor = Lighting.GetColor(center.ToTileCoordinates());

				float dist = (8 + 2);
				if (progress + dist >= texture.Width - 2) {
					progress = 0;
				}
				Rectangle frame = new(0, progress + 2, 6, (int)dist);


				data = new DrawData(texture,
					center - Main.screenPosition,
					frame,
					GetAlpha(drawColor) ?? drawColor,
					projRotation,
					frame.Size() * 0.5f,
					1,
					SpriteEffects.None
				) {
					shader = owner.cGrapple
				};
				Main.EntitySpriteDraw(data);
				progress += 8;
			}
			return false;
		}
		public override Color? GetAlpha(Color lightColor) => new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f);
		public override void PostAI() {
			if (Projectile.ai[0] != 2f) return;
			Player player = Main.player[Projectile.owner];
			player.grappling[--player.grapCount] = -1;
			if (player.whoAmI == Main.myPlayer) {
				if (Terraria.GameInput.PlayerInput.Triggers.JustPressed.Jump || (Projectile.ai[1] == 26 && player.controlJump)) {
					Projectile.Kill();
				}
			}
			player.GetModPlayer<OriginPlayer>().hookTarget = Projectile.whoAmI;
			if (player.Hitbox.Intersects(Projectile.Hitbox)) {
				if (Projectile.ai[1] > 0) Projectile.ai[1] = 26;
			} else {
				Projectile.ai[1]++;
			}
		}
	}
}

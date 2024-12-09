using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Origins.Items.Weapons.Summoner;
using Terraria.Utilities;
using System.Collections.Generic;

namespace Origins.Items.Tools {
	public class Chunky_Hook : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 16f;
			Item.shoot = ProjectileType<Chunky_Hook_P>();
			Item.value = Item.sellPrice(silver: 8);
		}
	}
	public class Chunky_Hook_P : ModProjectile {
		public static int ID { get; private set; }
		AutoLoadingAsset<Texture2D> chain = typeof(Chunky_Hook_P).GetDefaultTMLName() + "_Chain";
		public override void SetStaticDefaults() {
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.netImportant = true;
		}
		public override float GrappleRange() => 30 * 16;
		public override void NumGrappleHooks(Player player, ref int numHooks) => numHooks = 3;
		public override void GrappleRetreatSpeed(Player player, ref float speed) => speed = 18f;
		public override void GrapplePullSpeed(Player player, ref float speed) => speed = 11f;
		public override bool PreDrawExtras() {
			if (Projectile.localAI[0] == 0) {
				Projectile.localAI[0] = Main.rand.NextFloat(float.Epsilon, ushort.MaxValue);
			}
			Player owner = Main.player[Projectile.owner];
			Vector2 playerCenter = owner.MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 distToProj = playerCenter - Projectile.Center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();
			distToProj.Normalize();
			distToProj *= 12f;

			Texture2D lineTexture = TextureAssets.FishingLine.Value;
			Vector2 lineOrigin = new(lineTexture.Width / 2, 2);
			Color lineColor = new(44, 39, 58);
			Vector2 lineScale = new(1, (12 + 2) / lineTexture.Height);
			float lineRotation = distToProj.ToRotation() + MathHelper.PiOver2;

			List<(Vector2 pos, Color light)> points = new((int)(distance / 16f));
			while (distance > 16f && !float.IsNaN(distance)) {
				center += distToProj;
				distance = (playerCenter - center).Length();
				Color lightColor = Lighting.GetColor(center.ToTileCoordinates());
				Main.EntitySpriteDraw(new DrawData(
					lineTexture,
					center - Main.screenPosition,
					null,
					lineColor.MultiplyRGB(lightColor),
					lineRotation,
					lineOrigin,
					lineScale,
					SpriteEffects.None
					) {
					shader = owner.cGrapple
				});
				points.Add((center, lightColor));
			}

			Rectangle frame = new(0, 0, chain.Value.Width, chain.Value.Height / 2);
			Vector2 origin = frame.Size() * 0.5f;
			FastRandom fastRandom = new((int)Projectile.localAI[0]);
			for (int i = 0; i < points.Count; i++) {
				frame.Y = fastRandom.Next(2) * frame.Height;
				fastRandom.NextSeed();
				Main.EntitySpriteDraw(new DrawData(
					chain,
					points[i].pos - Main.screenPosition,
					frame,
					points[i].light,
					projRotation,
					origin,
					new Vector2(1f, 1),
					SpriteEffects.None
					) {
					shader = owner.cGrapple
				});
			}
			return false;
		}
	}
}

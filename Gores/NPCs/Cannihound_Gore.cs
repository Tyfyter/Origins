using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Gores.NPCs {
	public abstract class GoreProjectile : ModProjectile {
		protected abstract Vector2 Size { get; }

		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.width = (int)(Math.Min(Size.X, Size.Y) * 0.9f);
			Projectile.height = Projectile.width;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = Gore.goreTime;
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool PreAI() {
			Projectile.rotation += Projectile.velocity.X * 0.1f;
			Projectile.velocity.Y += 0.4f;
			Vector4 slopeCollision = Collision.SlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			Projectile.position = slopeCollision.XY();
			Projectile.velocity = slopeCollision.ZW();
			Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			if (Projectile.velocity.Y == 0f) {
				Projectile.velocity.X *= 0.97f;
				if (Projectile.velocity.X > -0.01 && Projectile.velocity.X < 0.01) {
					Projectile.velocity.X = 0f;
				}
			}
			if (Projectile.ai[0] > 0) {
				Projectile.ai[0] -= 1;
			} else {
				Projectile.alpha += 1;
			}
			Projectile.position += Projectile.velocity;
			if (Projectile.alpha >= 255) {
				Projectile.active = false;
			}
			return false;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int size = (int)(Math.Max(Size.X, Size.Y) * 0.9f);
			hitbox = new Rectangle(0, 0, size, size).Recentered(hitbox.Center());
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Point lightPos = Projectile.position.ToTileCoordinates();
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.spriteBatch.Draw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				Lighting.GetColor(lightPos) * ((255f - Projectile.alpha) / 255f),
				Projectile.rotation,
				texture.Size() * 0.5f,
				Projectile.scale,
				SpriteEffects.None,
			0f);
			return false;
		}
	}
	public class Cannihound_Gore1 : GoreProjectile {
		public override string Texture => "Origins/Gores/Meat_Dog_Gore1";
		protected override Vector2 Size => new(30, 20);
	}
	public class Cannihound_Gore2 : GoreProjectile {
		public override string Texture => "Origins/Gores/Meat_Dog_Gore2";
		protected override Vector2 Size => new(26, 20);
	}
	public class Cannihound_Gore3 : GoreProjectile {
		public override string Texture => "Origins/Gores/Meat_Dog_Gore3";
		protected override Vector2 Size => new(8, 18);
	}
}
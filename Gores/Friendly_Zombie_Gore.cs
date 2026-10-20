using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoMod.Cil;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using DelegateMethods = PegasusLib.Reflection.DelegateMethods;

namespace Origins.Gores {
	[Obsolete("Removed", true)]
	public class DustsBehindTiles : ILoadable {
		public void Load(Mod mod) { }
		public void Unload() { }
		public static void Add(int type) { }
	}
	public abstract class GoreDust : ModDust {
		protected abstract Rectangle Frame { get; }
		public override void SetStaticDefaults() {
			ChildSafety.SafeDust[Type] = false;
		}
		public override void OnSpawn(Dust dust) {
			dust.frame = Frame;
			dust.fadeIn = Gore.goreTime;
		}
		public override bool Update(Dust dust) {
			dust.rotation += dust.velocity.X * 0.1f;
			dust.velocity.Y += 0.4f;
			int size = (int)(Math.Min(dust.frame.Width, dust.frame.Height) * 0.9f * dust.scale);
			Vector2 halfSize = new(size * 0.5f);
			Vector4 slopeCollision = Collision.SlopeCollision(dust.position - halfSize, dust.velocity, size, size);
			dust.position = slopeCollision.XY() + halfSize;
			dust.velocity = slopeCollision.ZW();
			dust.velocity = Collision.TileCollision(dust.position - halfSize, dust.velocity, size, size);
			if (dust.velocity.Y == 0f) {
				dust.velocity.X *= 0.97f;
				if (dust.velocity.X > -0.01 && dust.velocity.X < 0.01) {
					dust.velocity.X = 0f;
				}
			}
			if (dust.fadeIn > 0) {
				dust.fadeIn -= 1;
			} else {
				dust.alpha += 1;
			}
			dust.position += dust.velocity;
			if (dust.alpha >= 255) {
				dust.active = false;
			}
			return false;
		}
		public override bool PreDraw(Dust dust) {
			Point lightPos = dust.position.ToTileCoordinates();
			Main.spriteBatch.Draw(
				Texture2D.Value,
				dust.position - Main.screenPosition,
				dust.frame,
				Lighting.GetColor(lightPos) * ((255f - dust.alpha) / 255f),
				dust.rotation,
				dust.frame.Size() * 0.5f,
				dust.scale,
				SpriteEffects.None,
			0f);
			return false;
		}
	}
	public class Friendly_Zombie_Gore1 : GoreDust {
		protected override Rectangle Frame => new(0, 0, 20, 24);
	}
	public class Friendly_Zombie_Gore2 : GoreDust {
		protected override Rectangle Frame => new(0, 0, 16, 10);
	}
	public class Friendly_Zombie_Gore3 : GoreDust {
		protected override Rectangle Frame => new(0, 0, 16, 12);
	}
}
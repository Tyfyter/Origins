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
using Origins.Graphics;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace Origins.Items.Tools {
	[LegacyName("Chunky_Hook")]
	public class Tangela_Tether : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 16f;
			Item.shoot = ProjectileType<Tangela_Tether_P>();
			Item.value = Item.sellPrice(silver: 8);
		}
	}
	[LegacyName("Chunky_Hook_P")]
	public class Tangela_Tether_P : ModProjectile, ITangelaHaver {
		public static int ID { get; private set; }
		AutoLoadingAsset<Texture2D> chain = typeof(Tangela_Tether_P).GetDefaultTMLName() + "_Chain";
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
			if (!TangelaSeed.HasValue) return false;
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

			int i = 0;
			Rectangle frame = new(0, 0, chain.Value.Width, chain.Value.Height / 2);
			Vector2 origin = frame.Size() * 0.5f;
			FastRandom fastRandom = new((int)Projectile.localAI[0]);
			while (distance > 16f && !float.IsNaN(distance)) {
				center += distToProj;
				distance = (playerCenter - center).Length();
				frame.Y = fastRandom.Next(2) * frame.Height;
				TangelaVisual.DrawTangela(
					chain,
					center - Main.screenPosition,
					frame,
					projRotation + (fastRandom.Next(2) == 0).ToDirectionInt() * MathHelper.PiOver2,
					origin,
					Vector2.One,
					SpriteEffects.None,
					TangelaSeed.Value + ++i
				);
			}
			return false;
		}
		public int? TangelaSeed { get; set; }
		public override bool PreDraw(ref Color lightColor) {
			TangelaVisual.DrawTangela(
				this,
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				Projectile.rotation,
				TextureAssets.Projectile[Type].Size() * 0.5f,
				Vector2.One,
				SpriteEffects.None
			);
			return false;
		}
	}
}

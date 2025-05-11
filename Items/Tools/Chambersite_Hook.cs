using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Origins.Tiles.Other;

namespace Origins.Items.Tools {
	public class Chambersite_Hook : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 20f;
			Item.shoot = ProjectileType<Chambersite_Hook_P>();
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<Chambersite_Item>(15)
				.AddTile(TileID.MythrilAnvil);
		}
	}
	public class Chambersite_Hook_P : ModProjectile {
		public static int ID { get; private set; }
		AutoLoadingAsset<Texture2D> chain => typeof(Chambersite_Hook_P).GetDefaultTMLName() + "_Chain";
		public override void SetStaticDefaults() {
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.netImportant = true;
		}

		// Amethyst Hook is 300, Static Hook is 600
		public override float GrappleRange() => 480f;

		public override void NumGrappleHooks(Player player, ref int numHooks) => numHooks = 1;

		// default is 11, Lunar is 24
		public override void GrappleRetreatSpeed(Player player, ref float speed) => speed = 16f;

		public override void GrapplePullSpeed(Player player, ref float speed) => speed = 11f;
		public override bool PreDrawExtras() {
			Player owner = Main.player[Projectile.owner];
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
				if (progress + dist >= chain.Value.Width - 2) {
					progress = 0;
				}
				Rectangle frame = new(0, progress + 2, 6, (int)dist);


				data = new DrawData(chain,
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
		public override Color? GetAlpha(Color lightColor) => lightColor;
	}
}

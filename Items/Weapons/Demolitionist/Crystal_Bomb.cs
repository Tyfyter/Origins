using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Microsoft.Xna.Framework.MathHelper;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Demolitionist {
    public class Crystal_Bomb : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Bomb");
			Tooltip.SetDefault("Explodes into several crystal shards");
			SacrificeTotal = 99;

		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 75;
			Item.value *= 14;
			Item.shoot = ModContent.ProjectileType<Crystal_Bomb_P>();
			Item.shootSpeed *= 1.5f;
			Item.knockBack = 5f;
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.CrystalShard);
			recipe.AddIngredient(ItemID.Grenade, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		#region blend mode testing
		/*public override bool AltFunctionUse(Player player) {
            return true;
        }*/
		/*public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
                if(player.controlUp) {
                    Crystal_Grenade_P.blendState.ColorSourceBlend++;
                    Crystal_Grenade_P.blendState.ColorSourceBlend = (Blend)((int)Crystal_Grenade_P.blendState.ColorSourceBlend%13);
                    Main.NewText(Crystal_Grenade_P.blendState.ColorSourceBlend);
                }
                if(player.controlLeft) {
                    Crystal_Grenade_P.blendState.AlphaSourceBlend++;
                    Crystal_Grenade_P.blendState.AlphaSourceBlend = (Blend)((int)Crystal_Grenade_P.blendState.AlphaSourceBlend%13);
                    Main.NewText(Crystal_Grenade_P.blendState.AlphaSourceBlend);
                }
                if(player.controlDown) {
                    Crystal_Grenade_P.blendState.ColorDestinationBlend++;
                    Crystal_Grenade_P.blendState.ColorDestinationBlend = (Blend)((int)Crystal_Grenade_P.blendState.ColorDestinationBlend%13);
                    Main.NewText(Crystal_Grenade_P.blendState.ColorDestinationBlend);
                }
                if(player.controlRight) {
                    Crystal_Grenade_P.blendState.AlphaDestinationBlend++;
                    Crystal_Grenade_P.blendState.AlphaDestinationBlend = (Blend)((int)Crystal_Grenade_P.blendState.AlphaDestinationBlend%13);
                    Main.NewText(Crystal_Grenade_P.blendState.AlphaDestinationBlend);
                }
                item.shoot = ProjectileID.None;
                item.consumable = false;
                return true;
            }
            item.shoot = ModContent.ProjectileType<Crystal_Grenade_P>();
            item.consumable = true;
            return base.CanUseItem(player);
        }*/
		#endregion
	}
	public class Crystal_Bomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Crystal_Bomb";
		public static BlendState blendState => new BlendState() {
			ColorSourceBlend = Blend.SourceAlpha,
			AlphaSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.InverseSourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha,
		};
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Bomb");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.timeLeft = 135;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Bomb;
			return true;
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			int t = ModContent.ProjectileType<Crystal_Grenade_Shard>();
			int count = 14 - Main.rand.Next(3);
			float rot = TwoPi / count;
			for (int i = count; i > 0; i--) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vec2FromPolar(rot * i, 6) + Main.rand.NextVector2Unit()) + (Projectile.velocity / 12), t, Projectile.damage / 4, 6, Projectile.owner);
			}
		}
		public override void PostDraw(Color lightColor) {
			Texture2D lightMap = new Texture2D(Main.spriteBatch.GraphicsDevice, 10, 10);
			Color[] lightData = new Color[100];
			Vector2 pos = Projectile.position;
			Vector3 col;
			for (int x = 0; x < 10; x++) {
				pos.X += 2;
				for (int y = 0; y < 10; y++) {
					pos.Y += 2;
					col = Lighting.GetSubLight(pos);
					lightData[(y * 10) + x] = new Color(((col.X + col.Y + col.Z) / 1.5f - 0.66f) * Min(Projectile.timeLeft / 85f, 1), 0, 0);
				}
				pos.Y -= 20;
			}
		}
	}
}

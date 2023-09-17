using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Outreach : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Ripper Lance");
			// Tooltip.SetDefault("'Very pointy'");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 26;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 46;
			Item.height = 46;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Outreach_P>();
			Item.shootSpeed = 3.75f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, position, velocity.RotatedBy(-0.5f * player.direction), type, damage, knockback, player.whoAmI);
			return false;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 9);
			recipe.AddIngredient(ModContent.ItemType<NE8>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Outreach_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Outreach_P";
		public override void SetStaticDefaults() {
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Spear);
            Projectile.timeLeft = 3600;
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = 0;
        }
        public float movementFactor {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void AI() {
            Player projOwner = Main.player[Projectile.owner];
            Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
            Projectile.direction = projOwner.direction;
            projOwner.heldProj = Projectile.whoAmI;
            projOwner.itemTime = projOwner.itemAnimation;
            Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
            Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
            if (!projOwner.frozen) {
                if (movementFactor == 0f) {
                    movementFactor = 2.5f;
                    Projectile.netUpdate = true;
                }
                if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2 - 1) {
                    movementFactor -= 2.5f;
                } else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1) {
                    movementFactor += 2.7f;
                }
            }
            Projectile.position += Projectile.velocity * movementFactor;
            if (projOwner.itemAnimation == 0) {
                Projectile.Kill();
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            if (Projectile.spriteDirection == 1) {
                Projectile.rotation -= MathHelper.Pi / 2f;
            }
        }
        public override bool PreDraw(ref Color lightColor) {
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, (Projectile.Center) - Main.screenPosition, new Rectangle(0, 0, 72, 72), lightColor, Projectile.rotation, new Vector2(62, 8), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
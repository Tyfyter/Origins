using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Projectiles;
namespace Origins.Items.Weapons.Melee {
	public class Outreach : ModItem, ICustomWikiStat {
		static short glowmask;
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			ItemID.Sets.Spears[Type] = true;
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
			//Projectile.NewProjectile(source, position, velocity.RotatedBy(-0.5f * player.direction), type, damage, knockback, player.whoAmI);
			return true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<NE8>(), 5)
            .AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool MeleePrefix() => true;
	}
	public class Outreach_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Outreach_P";
		static new AutoCastingAsset<Texture2D> GlowTexture;
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(base.GlowTexture);
			}
		}
		public override void Unload() {
			GlowTexture = null;
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Spear);
            Projectile.timeLeft = 3600;
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = 0;
			Projectile.scale = 1f;
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
                if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2 - 1) {
                    movementFactor -= 1.7f;
                } else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1) {
                    movementFactor += 1.7f;
                }
            }
            Projectile.position += Projectile.velocity * movementFactor * Projectile.scale;
            if (projOwner.ItemAnimationEndingOrEnded) {
                Projectile.Kill();
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            if (Projectile.spriteDirection == 1) {
                Projectile.rotation -= MathHelper.Pi / 2f;
            }
        }
        public override bool PreDraw(ref Color lightColor) {
            Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				(Projectile.Center) - Main.screenPosition,
				new Rectangle(0, 0, 72, 72),
				lightColor,
				Projectile.rotation,
				new Vector2(62, 8),
				Projectile.scale,
				SpriteEffects.None
			);
            Main.EntitySpriteDraw(
				GlowTexture,
				(Projectile.Center) - Main.screenPosition,
				new Rectangle(0, 0, 72, 72),
				Color.White,
				Projectile.rotation,
				new Vector2(62, 8),
				Projectile.scale,
				SpriteEffects.None
			);
            return false;
        }
    }
}
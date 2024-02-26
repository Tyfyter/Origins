using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
    public class Amenonuhoko : ModItem {
        public string[] Categories => new string[] {
            "Spear"
        };
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ItemID.Sets.Spears[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Gungnir);
			Item.damage = 64;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 66;
			Item.height = 68;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<Amenonuhoko_P>();
			Item.shootSpeed = 5f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Gungnir);
			recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>(), 5);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 15);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Amenonuhoko_P : ModProjectile {
		public override string Texture => base.Texture;
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.timeLeft = 3600;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.aiStyle = 0;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			projOwner.heldProj = Projectile.whoAmI;
			Projectile.direction = projOwner.direction;
			Projectile.spriteDirection = projOwner.direction;
			projOwner.itemTime = projOwner.itemAnimation;
			Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 1f) {
					movementFactor = 1.5f;
					Projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2) {
					movementFactor -= 1.1f;
				} else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1) {
					movementFactor += 1.3f;
				}
				if (projOwner.itemAnimation == 14 || Projectile.timeLeft == 0) {

				}
			}
			Projectile.position += Projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0) {
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.PiOver2;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition + Projectile.velocity * 5,
				null,
				lightColor,
				Projectile.rotation,
				new Vector2(39 + 34 * Projectile.spriteDirection, 7),
				Projectile.scale,
				Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
			0);
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
		}
	}
}

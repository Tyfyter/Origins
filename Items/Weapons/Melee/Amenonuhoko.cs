using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Projectiles;
namespace Origins.Items.Weapons.Melee {
    public class Amenonuhoko : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Spear",
			"ToxicSource"
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.Spears[Type] = true;
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
			Recipe.Create(Type)
			.AddIngredient(ItemID.Gungnir)
			.AddIngredient(ModContent.ItemType<Bottled_Brine>(), 5)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Amenonuhoko_P : ModProjectile {
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.timeLeft = 3600;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.aiStyle = 0;
			Projectile.scale = 1f;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			projOwner.heldProj = Projectile.whoAmI;
			Projectile.direction = projOwner.direction;
			Projectile.spriteDirection = projOwner.direction;
			projOwner.itemTime = projOwner.itemAnimation;
			Projectile.Center = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			if (!projOwner.frozen) {
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2) {
					movementFactor -= 1.1f;
					if (Projectile.ai[2] == 0) {
						Projectile.ai[2] = 1;
						Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							Projectile.Center + Projectile.velocity * movementFactor * Projectile.scale,
							Projectile.velocity,
							ModContent.ProjectileType<Amenonuhoko_Cloud>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner
						);
					}
				} else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1) {
					movementFactor += 1.2f;
				}
			}
			Projectile.position += Projectile.velocity * movementFactor * Projectile.scale;
			if (projOwner.ItemAnimationEndingOrEnded) {
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
	public class Amenonuhoko_Cloud : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SporeCloud;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Main.projFrames[ProjectileID.SporeCloud];
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SporeCloud);
			AIType = ProjectileID.SporeCloud;
		}
		public override Color? GetAlpha(Color lightColor) {
			lightColor.G = (byte)(lightColor.G * 0.9f);
			lightColor.B = (byte)(MathHelper.Min(lightColor.B * 1.1f, 255));
			return lightColor;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
		}
	}
}

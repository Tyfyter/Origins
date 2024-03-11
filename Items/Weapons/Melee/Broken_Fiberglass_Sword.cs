using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Melee {
	public class Broken_Fiberglass_Sword : ModItem, IElementalItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Sword"
        };
        public ushort Element => Elements.Fiberglass;
		public override void SetDefaults() {
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 24;
			Item.height = 26;
			Item.useTime = 14;
			Item.useAnimation = 14;
			Item.useStyle = ItemUseStyleID.Rapier;
			Item.knockBack = 6;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.shootSpeed = 3;
			Item.shoot = ModContent.ProjectileType<Broken_Fiberglass_Sword_Stab>();
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}
	}
	public class Broken_Fiberglass_Sword_Stab : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Broken_Fiberglass_Sword";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.timeLeft = 3600;
			Projectile.width = 20;
			Projectile.height = 20;
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
			Projectile.position.X = ownerMountedCenter.X - (float)(Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (float)(Projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 0f) {
					movementFactor = 4.7f;
					Projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 7) {
					movementFactor -= 1.7f;
				} else if (projOwner.itemAnimation > projOwner.itemAnimationMax * 6f / 7) {
					movementFactor += 1.3f;
				}
			}
			Projectile.position += Projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0) {
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
			Projectile.rotation -= MathHelper.Pi / 2f;
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(Mod.Assets.Request<Texture2D>("Items/Weapons/Melee/Broken_Fiberglass_Sword").Value, (Projectile.Center - Projectile.velocity * 2) - Main.screenPosition, new Rectangle(0, 0, 24, 26), lightColor, Projectile.rotation, new Vector2(12, 13), 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}

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
using Origins.Items.Weapons.Magic;
using Origins.Projectiles.Weapons;
using Terraria.Graphics.Shaders;
namespace Origins.Items.Weapons.Melee {
    public class Amenonuhoko : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Spear,
			WikiCategories.ToxicSource
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.Spears[Type] = true;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Toxic_Shock_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Gungnir);
			Item.damage = 64;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 66;
			Item.height = 68;
			Item.useTime = 22;
			Item.useAnimation = 22;
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
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 12)
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
					movementFactor -= 0.8f;
					if (Projectile.ai[2] == 0) {
						Projectile.ai[2] = 1;
						Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							Projectile.Center + Projectile.velocity * movementFactor * Projectile.scale,
							Projectile.velocity,
							ModContent.ProjectileType<Amenonuhoko_Laser>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner
						);
					}
				} else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1) {
					movementFactor += 2.2f;
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
			SpriteEffects effects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition + Projectile.velocity * 5,
				null,
				lightColor,
				Projectile.rotation,
				new Vector2(105, 7).Apply(effects, TextureAssets.Projectile[Type].Size()),
				Projectile.scale,
				effects,
			0);
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
		}
	}
	public class Amenonuhoko_Laser : ModProjectile {
		public override string Texture => typeof(Chemical_Laser).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ShadowBeamFriendly);
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.hide = true;
			Projectile.timeLeft = 120;
		}
		public override void AI() {
			ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
			for (int i = 0; i < 2; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, 1, 1, DustID.Electric);
				dust.position = Projectile.position - Projectile.velocity * (i * 0.5f);
				dust.position.X += Projectile.width / 2;
				dust.position.Y += Projectile.height / 2;
				dust.scale = Main.rand.NextFloat(0.65f, 0.65f);
				dust.velocity = dust.velocity * 0.2f + Projectile.velocity * 0.1f;
				dust.shader = shader;
				dust.noGravity = false;
				dust.noLight = true;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 80);
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.owner != Main.myPlayer && Projectile.hide) {
				Projectile.hide = false;
				try {
					Projectile.active = true;
					Projectile.timeLeft = timeLeft;
					Projectile.Update(Projectile.whoAmI);
				} finally {
					Projectile.active = false;
					Projectile.timeLeft = 0;
				}
			}
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

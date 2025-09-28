using Origins.Projectiles;
using Origins.Tiles.Other;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Chambersite_Phasesaber : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 48;
			Item.DamageType = DamageClass.Melee;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 3;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item15;
			Item.shoot = ModContent.ProjectileType<Chambersite_Phasesaber_P>();
			Item.shootSpeed = 11;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void UseItemFrame(Player player) {
			if (player.altFunctionUse != 0) player.itemLocation = default;
		}
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) => noHitbox = player.altFunctionUse != 0;
		public override bool CanShoot(Player player) => player.altFunctionUse != 0;
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
		public override void AddRecipes() => CreateRecipe()
			.AddRecipeGroup("Origins:Gem Phaseblades")
			.AddIngredient<Chambersite_Item>(4)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
	public class Chambersite_Phasesaber_P : ModProjectile {
		public override string Texture => typeof(Chambersite_Phasesaber).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.aiStyle = ProjAIStyleID.Boomerang;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.scale = 1f;
			DrawOriginOffsetX = 0;
			DrawOriginOffsetY = 0;
			Projectile.manualDirectionChange = true;
		}
		public override bool ShouldUpdatePosition() => true;
		public override bool PreAI() {
			if (Projectile.ai[0] != 1 && Projectile.TryGetOwner(out Player player) && player.HeldItem.type != Chambersite_Phasesaber.ID) {
				Projectile.aiStyle = -1;
				Projectile.velocity *= 0.9f;
				SpinToTargetRotation(MathHelper.PiOver4 * Projectile.direction * 3);
				Projectile.ai[2] = 1;
				return false;
			}
			const int HalfSpriteWidth = 48 / 2;
			const int HalfSpriteHeight = 48 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			if (Projectile.ai[0] == 0 && Projectile.ai[2] == 0) {
				Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
			}
			Projectile.ai[2] = 0;
			Projectile.aiStyle = ProjAIStyleID.Boomerang;
			return true;
		}
		bool SpinToTargetRotation(float targetRot) {
			float diff = GeometryUtils.AngleDif(Projectile.rotation, targetRot, out int dir);
			if (diff < 0.01f) {
				Projectile.rotation = targetRot;
				return true;
			} else if (diff > 0.21 || dir == Projectile.direction) {
				Projectile.rotation += 0.2f * Projectile.direction;
				Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation);
				return false;
			} else {
				Projectile.rotation = targetRot;
				return true;
			}
		}
		public override void PostAI() {
			if (Projectile.ai[2] == 1) return;
			Projectile.rotation -= 0.2f * Projectile.direction;
			if (Projectile.ai[0] == 1) {
				if (Projectile.localAI[1] == 0) {
					Projectile.localAI[1] = 1;
				} else {
					float targetRot = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * Projectile.direction * 3 + MathHelper.PiOver2;
					if (Projectile.localAI[0] == 0) {
						if (SpinToTargetRotation(targetRot)) Projectile.localAI[0] = 1;
					} else {
						Projectile.rotation = targetRot;
					}
				}
			}
			Projectile.spriteDirection = Projectile.direction;
			if (Projectile.numUpdates == 0) {
				Projectile.soundDelay++;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			Projectile.aiStyle = -1;
			return base.CanHitNPC(target);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.velocity = Projectile.oldVelocity;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width -= width / 6;
			height -= height / 6;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) => false;
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			if (Projectile.ai[2] == 1) {
				hitbox.Inflate((hitbox.Width * -2) / 5, 0);
			}
			/*float scale = (Projectile.scale - 1) * 0.5f;
			hitbox.Inflate((int)(hitbox.Width * scale), (int)(hitbox.Height * scale));*/
		}
	}
}

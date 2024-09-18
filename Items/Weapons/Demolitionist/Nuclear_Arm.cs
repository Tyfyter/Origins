using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Tiles.Other;
namespace Origins.Items.Weapons.Demolitionist {
	[AutoloadEquip(EquipType.HandsOn)]
    public class Nuclear_Arm : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "Launcher"
        ];
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.TerraBlade);
			Item.damage = 120;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 37;
			Item.useAnimation = 37;
			Item.knockBack = 4f;
			Item.shoot = ModContent.ProjectileType<Nuclear_Arm_P>();
			Item.shootSpeed = 5;
			Item.UseSound = SoundID.Item45;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
			Item.glowMask = glowmask;
            Item.ArmorPenetration += 3;
        }
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			//player.handon = Item.handOnSlot;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			player.handon = Item.handOnSlot;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ExplosivePowder, 10)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 20)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
	public class Nuclear_Arm_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Nuclear_Arm_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Main.projFrames[Type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (++Projectile.frameCounter > 4) {
				Projectile.frame = (Projectile.frame + 1) % 4;
				Projectile.frameCounter = 0;
			}
			if (Main.rand.NextBool(3)) Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Torch);
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketI;
			Projectile.penetrate = -1;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			if (Projectile.owner == Main.myPlayer) {
				Player player = Main.LocalPlayer;
				if (player.active && !player.dead && !player.immune) {
					Rectangle projHitbox = Projectile.Hitbox;
					ProjectileLoader.ModifyDamageHitbox(Projectile, ref projHitbox);
					Rectangle playerHitbox = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
					if (projHitbox.Intersects(playerHitbox)) {
						player.Hurt(
							Terraria.DataStructures.PlayerDeathReason.ByProjectile(Main.myPlayer, Projectile.whoAmI),
							Main.DamageVar(Projectile.damage, -player.luck),
							Math.Sign(player.Center.X - Projectile.Center.X),
							true
						);
					}
				}
			}
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			return true;
		}
	}
}

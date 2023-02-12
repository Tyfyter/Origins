using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	[AutoloadEquip(EquipType.HandsOn)]
    public class Nuclear_Arm : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nuclear Arm");
			Tooltip.SetDefault("'Like nuclear arms? Well now you can have nuclear arms on your... arms'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.TerraBlade);
			Item.noUseGraphic = true;
			Item.damage = 44;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.knockBack = 4f;
			Item.shoot = ModContent.ProjectileType<Nuclear_Arm_P>();
			Item.shootSpeed = 5;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.buyPrice(gold: 2);
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			//player.handon = Item.handOnSlot;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			player.handon = Item.handOnSlot;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ExplosivePowder, 10);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 20);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>(), 2);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
	}
	public class Nuclear_Arm_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Nuclear_Arm_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nuclear Arm");
			Main.projFrames[Type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
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

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Holy_Hand_Grenade : ModItem { 
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 100;
			Item.shootSpeed = 9;
			Item.value = Item.sellPrice(gold: 5);
			Item.shoot = ModContent.ProjectileType<Holy_Hand_Grenade_P>();
			Item.rare = ItemRarityID.Pink;
			Item.consumable = false;
			Item.maxStack = 1;
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient(ItemID.HallowedBar, 10)
			.AddIngredient(ItemID.SoulofFright, 5)
			.AddIngredient(ItemID.SoulofMight, 5)
			.AddIngredient(ItemID.SoulofSight, 5)
			.AddIngredient(ItemID.Bomb, 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
	public class Holy_Hand_Grenade_P : ModProjectile {
		public override string Texture => typeof(Holy_Hand_Grenade).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;
			Projectile.timeLeft = 60 * 20;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse s &&  s.Item.ModItem is not Holy_Hand_Grenade) Projectile.ai[2] = 1;
		}
		public override void AI() {
			if (OriginsModIntegrations.CheckAprilFools()) {
				Projectile.hide = true;
				Projectile.velocity = default;
				Player player = Main.player[Projectile.owner];
				player.heldProj = Projectile.whoAmI;
				if (player.compositeFrontArm.enabled) {
					Projectile.Center = player.GetCompositeArmPosition(false);
				} else {
					Vector2 offset = (player.bodyFrame.Y / 56) switch {
						0 => new(-7, 11),
						1 => new(-9, -7),
						2 => new(5, -7),
						3 => new(8, 7),
						4 => new(5, 11),
						5 => new(-9, -7),
						6 => new(-5, 7),
						7 => new(-7, 5),
						8 => new(-7, 5),
						9 => new(-7, 5),
						10 => new(-7, 7),
						11 => new(-5, 7),
						12 => new(-5, 7),
						13 => new(-5, 7),
						14 => new(-3, 5),
						15 => new(-1, 5),
						16 => new(-1, 5),
						17 => new(-3, 7),
						18 => new(-5, 7),
						19 => new(-5, 7),
						_ => default
					};
					Projectile.Center = player.MountedCenter + offset * new Vector2(player.direction, player.gravDir);
				}
			} else {
				Projectile.hide = false;
			}
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.friendly = true;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 192;
			Projectile.height = 192;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			if (Projectile.ai[2] == 0) SoundEngine.PlaySound(Origins.Sounds.HolyHandGrenade, Projectile.Center);
		}
	}
}

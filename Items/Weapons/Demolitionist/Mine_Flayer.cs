#define ANIMATED
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using Origins.Tiles.Other;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Mine_Flayer : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher",
			"CanistahUser"
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TerraBlade);
			Item.shootsEveryUse = false;
			Item.damage = 48;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 4;
			Item.useAnimation = 40;
			Item.knockBack = 4f;
			Item.useAmmo = ModContent.ItemType<Resizable_Mine_Wood>();
			Item.shoot = ModContent.ProjectileType<Mine_Flayer_P>();
			Item.shootSpeed = 9f;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
			Item.reuseDelay = 60;
			Item.autoReuse = false;
			Item.UseSound = null;
			Item.ArmorPenetration += 2;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.reuseDelay == 0) {
				Item.useStyle = ItemUseStyleID.RaiseLamp;
			} else {
				Item.useStyle = ItemUseStyleID.Swing;
			}
		}
		public override void UseItemFrame(Player player) {
			if (player.HeldItem.type != Type) return;
			if (player.itemAnimation == player.itemTime) {
				switch ((player.itemAnimation / 4) % 3) {
					case 0:
					player.bodyFrame.Y = player.bodyFrame.Height * 3;
					break;

					case 1:
					player.bodyFrame.Y = player.bodyFrame.Height * 2;
					break;

					case 2:
					player.bodyFrame.Y = player.bodyFrame.Height;
					break;
				}
				return;
			}
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return !Main.rand.NextBool(3, 5);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 30)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rotor>(), 8)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public override bool? UseItem(Player player) {
			Vector2 position = player.itemLocation;
			SoundEngine.PlaySound(SoundID.Item61.WithPitch(0.25f), position);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = (Vector2.UnitY * -player.gravDir)
				.RotatedBy(player.direction * (1 - player.itemAnimation / (float)player.itemAnimationMax - 0.15f) * 3.5f)
				* velocity.Length();
			type = Item.shoot;
		}
	}
	public class Mine_Flayer_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Projectile.timeLeft = 420;
			Projectile.scale = 0.5f;
			Projectile.penetrate = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 5;
		}
		public override void AI() {
			Projectile.velocity.Y -= 0.2f;
			this.DoGravity(0.2f);
		}
	}
}

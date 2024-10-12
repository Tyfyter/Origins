using Origins.Dev;
using Origins.Items.Weapons.Ammo;
using Origins.Layers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Slag_Bucket : ModItem, ICustomWikiStat {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Loader.Instance.shieldGlowMasks.Add(Item.shieldSlot, Texture + "_Shield_Glow");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(48, 36);
			Item.rare = CursedRarity.ID;
			Item.value = Item.sellPrice(gold: 2);
			Item.shoot = ModContent.ProjectileType<Slag_Bucket_Projectile>();
			Item.defense = 3;
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			player.noKnockback = true;
			player.fireWalk = true;
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.slagBucketCursed = true;
			originPlayer.slagBucket = true;
			originPlayer.retributionShield = true;
			originPlayer.retributionShieldItem = Item;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Scrap_Barrier>())
			.AddIngredient(ModContent.ItemType<Shield_of_Retribution>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Slag_Bucket_Uncursed : Uncursed_Cursed_Item<Slag_Bucket>, ICustomWikiStat {
		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = ItemRarityID.Green;
			Item.defense = 6;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			player.noKnockback = true;
			player.fireWalk = true;
			player.endurance += (1 - player.endurance) * 0.05f;
			player.lifeRegen += 4;
			originPlayer.slagBucket = true;
			originPlayer.retributionShield = true;
			originPlayer.retributionShieldItem = Item;
		}
		public override void AddRecipes() {
			base.AddRecipes();
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Scrap_Barrier_Uncursed>())
			.AddIngredient(ModContent.ItemType<Shield_of_Retribution>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Slag_Bucket_Projectile : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ExplosiveBullet;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BulletHighVelocity);
			Projectile.DamageType = DamageClass.Generic;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(240, 360));
		}
	}
}

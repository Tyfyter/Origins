using Origins.Buffs;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Venom_Shield : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.EoCShield);
			Item.damage = 57;
			Item.defense = 3;
			Item.knockBack = 0f; // damage reduction percent
			Item.shoot = ModContent.ProjectileType<Venom_Shield_P>();
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Pink;
			Item.expert = false;
		}
		public override void UpdateEquip(Player player) {
			player.noKnockback = true;
			player.fireWalk = true;
			player.dashType = 2;
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.retributionShield = true;
			originPlayer.retributionShieldItem = Item;
			originPlayer.dashBaseDamage = Item.damage;
			originPlayer.dashHitDebuffs.Add((Toxic_Shock_Debuff.ID, 180..280));
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Shield_of_Retribution>()
			.AddIngredient<Venom_Fang>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Venom_Shield_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CursedBullet;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BulletHighVelocity);
			Projectile.DamageType = DamageClass.Generic;
			Projectile.penetrate = 1;
			Projectile.alpha = 0;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 60);
		}
	}
}

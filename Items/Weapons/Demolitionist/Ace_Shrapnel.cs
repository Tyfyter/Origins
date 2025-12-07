using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Journal;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Ace_Shrapnel : ModItem, IJournalEntrySource<Ace_Shrapnel.Ace_Shrapnel_Entry>, ICustomWikiStat {
		public class Ace_Shrapnel_Entry : JournalEntry {
			public override string TextKey => "Ace_Shrapnel";
			public override JournalSortIndex SortIndex => new("The_Ashen", 3);
		}
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Impeding_Shrapnel_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.DefaultToLauncher(12, 20, 54, 26);
			Item.useTime = 1;
			Item.shoot = ModContent.ProjectileType<Impeding_Shrapnel_Shard>();
			Item.shootSpeed = 2;
			Item.useAmmo = ModContent.ItemType<Scrap>();
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.consumeAmmoOnFirstShotOnly = true;
			Item.ArmorPenetration += 6;
			Item.reuseDelay = 0;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<NE8>(), 10)
			.AddIngredient(ModContent.ItemType<Scrap>(), 15)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-18, -7);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (velocity != default) {
				Vector2 direction = Vector2.Normalize(velocity);
				position += direction * 32 + direction.RotatedBy(MathHelper.PiOver2 * player.direction) * -10;
			}
			velocity = velocity.RotatedByRandom(0.27f);
			type = Item.shoot;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				type,
				damage,
				knockback,
				player.whoAmI,
				ai2: 0.5f
			);
			return false;
		}
	}
}

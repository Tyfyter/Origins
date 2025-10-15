using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Felnum_Longbow : ModItem, ICustomWikiStat {
		public const int baseDamage = 19;
        public string[] Categories => [
            WikiCategories.Bow
        ];
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoldBow);
			Item.damage = baseDamage;
			Item.width = 18;
			Item.height = 58;
			Item.useTime = Item.useAnimation = 32;
			Item.shootSpeed *= 2.5f;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(silver: 65);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0);
		}
	}
}

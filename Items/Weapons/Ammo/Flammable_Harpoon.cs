using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
namespace Origins.Items.Weapons.Ammo {
	public class Flammable_Harpoon : ModItem, ICustomWikiStat {
		public static int ID { get; private set; }
        public string[] Categories => [
            WikiCategories.Harpoon
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Flammable_Harpoon_P.ID;
			Item.ammo = Harpoon.ID;
			Item.value = Item.sellPrice(silver: 3);
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
            .AddIngredient(ItemID.Gel)
            .AddRecipeGroup(RecipeGroupID.IronBar, 8)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 8)
            .AddIngredient(ItemID.Gel)
            .AddIngredient(ModContent.ItemType<Harpoon>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Flammable_Harpoon_P : Harpoon_P {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(360, 480));
		}
	}
}

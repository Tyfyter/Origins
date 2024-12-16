using Origins.Items.Materials;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ammo {
	public class Alkaline_Harpoon : ModItem, ICustomWikiStat {
		public static int ID { get; private set; }
        public string[] Categories => [
            "Harpoon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 16;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Alkaline_Harpoon_P.ID;
			Item.ammo = Harpoon.ID;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 2)
			.AddRecipeGroup(RecipeGroupID.IronBar, 2)
			.AddIngredient(ModContent.ItemType<Alkaliphiliac_Tissue>())
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 2)
			.AddIngredient(ModContent.ItemType<Harpoon>(), 2)
			.AddIngredient(ModContent.ItemType<Alkaliphiliac_Tissue>())
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Alkaline_Harpoon_P : Harpoon_P {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Venom, Main.rand.Next(270, 360));
		}
	}
}

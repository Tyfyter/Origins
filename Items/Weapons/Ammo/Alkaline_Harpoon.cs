﻿using Origins.Items.Materials;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ammo {
	public class Alkaline_Harpoon : ModItem, ICustomWikiStat {
		public static int ID { get; private set; } = -1;
        public string[] Categories => new string[] {
            "Harpoon"
        };
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
			Recipe recipe = Recipe.Create(Type, 8);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 8);
			recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ModContent.ItemType<Harpoon>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Alkaline_Harpoon_P : Harpoon_P {
		public static new int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Venom, Main.rand.Next(270, 360));
		}
	}
}

using Origins.Items.Other.Consumables;
using Origins.Journal;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Dusk;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
    public class Adhesive_Wrap : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Adhesive Wrap");
			Item.ResearchUnlockCount = 25;

		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(copper: 18);
			Item.maxStack = 999;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 5);
			recipe.AddIngredient(ModContent.ItemType<Silicon>());
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = Recipe.Create(ItemID.AdhesiveBandage);
			recipe.AddIngredient(ItemID.GlowingMushroom, 3);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Alkahest : ModItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Alkahest_Mat_Entry).Name;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Alkahest");
			// Tooltip.SetDefault("'Don't touch it'");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.sellPrice(silver: 9);
			Item.rare = ItemRarityID.Orange;
		}
		public class Alkahest_Mat_Entry : JournalEntry {
			public override string TextKey => "Alkahest";
			public override ArmorShaderData TextShader => null;
		}
	}
	public class Bark : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.rare = ItemRarityID.Gray;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Rubber>());
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.GlassKiln);
			recipe.Register();
		}
	}
	public class Bat_Hide : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bat Hide");
			Item.ResearchUnlockCount = 25;

		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.rare = ItemRarityID.Gray;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.Leather);
			recipe.AddIngredient(this, 4);
			recipe.AddTile(TileID.HeavyWorkBench);
			recipe.Register();
		}
	}
	public class Black_Bile : ModItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Black_Bile_Entry).Name;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Black Bile");
			// Tooltip.SetDefault("'So depressing it makes the party girl cry'");
			Item.ResearchUnlockCount = 25;

		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Orange;
		}
		public class Black_Bile_Entry : JournalEntry {
			public override string TextKey => "Black_Bile";
			public override ArmorShaderData TextShader => null;
		}
	}
	public class Bleeding_Obsidian_Shard : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bleeding Obsidian Shard");
			// Tooltip.SetDefault("'Weakens those who touch it'");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 48;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.rare = ItemRarityID.LightRed;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Bleeding_Obsidian_Item>());
			recipe.AddIngredient(this, 8);
			recipe.Register();

			recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>());
			recipe.Register();
		}
	}
	public class Bottled_Brine : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bottled Brine");
			Item.ResearchUnlockCount = 30;
		}
		public override void SetDefaults() {
			Item.maxStack = 1;
			Item.value = Item.sellPrice(copper: 40);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Bottle);
			recipe.AddIngredient(ItemID.Stinger, 2);
			recipe.AddIngredient(ModContent.ItemType<Magic_Brine_Dropper>());
			recipe.AddTile(TileID.AlchemyTable);
			recipe.Register();
		}
	}
	public class Brineglow : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Brineglow");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.sellPrice(copper: 30);
			Item.rare = ItemRarityID.White;
			Item.glowMask = glowmask;
		}
	}
	public class Bud_Barnacle : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Bud Barnacle");
			Item.ResearchUnlockCount = 30;
		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.sellPrice(copper: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.BattlePotion);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Wrycoral_Item>());
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();

			recipe = Recipe.Create(ItemID.CoffinMinecart);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 10);
			recipe.AddIngredient(Type, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.MechanicalWorm);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
			recipe.AddIngredient(ItemID.SoulofNight, 6);
			recipe.AddIngredient(Type, 6);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(ItemID.MonsterLasagna);
			recipe.AddIngredient(Type, 8);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();

			recipe = Recipe.Create(ItemID.UnholyArrow, 5);
			recipe.AddRecipeGroup(ItemID.WoodenArrow, 5);
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Busted_Servo : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Busted Servo");
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.buyPrice(silver: 2);
			Item.rare = ItemRarityID.Pink;
		}
	}
	public class Chambersite : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Chambersite");
			// Tooltip.SetDefault("'Loses all of its color when exposed to light'");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.buyPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
	}
	public class Chromtain_Bar : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Chromtain Bar");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.FragmentSolar, 4);
			recipe.AddIngredient(ItemID.SoulofMight, 10);
			recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 4);
			recipe.AddTile(TileID.Anvils); //Omni-Printer also not implemented
			recipe.Register();
		}
	}
	public class Defiled_Bar : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Bar");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Magiluminescence);
			recipe.AddIngredient(Type, 12);
			recipe.AddIngredient(ItemID.Topaz, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Dawn_Key : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Dawn Key");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 14;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class Defiled_Key : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Key");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 14;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class Dusk_Key : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Dusk Key");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 14;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class Eitrite_Bar : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Eitrite Bar");
			// Tooltip.SetDefault("'So alkaline-concentrated it could be used as a power source'");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(silver: 81);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 999;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Ore_Item>(), 4);
			recipe.AddTile(TileID.AdamantiteForge);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.TitaniumBar);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 2);
			recipe.AddIngredient(ModContent.ItemType<Magic_Brine_Dropper>());
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.AdamantiteBar);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 2);
			recipe.AddIngredient(ModContent.ItemType<Magic_Brine_Dropper>());
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Element36_Bundle : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Element-36 Bundle");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.FragmentNebula, 2);
			recipe.AddIngredient(ItemID.FragmentStardust, 2);
			recipe.AddIngredient(ModContent.ItemType<Fibron_Plating>(), 4);
			recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 4);
			recipe.AddTile(TileID.Anvils); //Omni-Printer also not implemented
			recipe.Register();
		}
	}
	[LegacyName("Infested_Bar")]
	public class Encrusted_Bar : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Encrusted Bar");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Ore_Item>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Magiluminescence);
			recipe.AddIngredient(Type, 12);
			recipe.AddIngredient(ItemID.Topaz, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Eyndum_Bar : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Eyndum Bar");
			// Tooltip.SetDefault("'\"Half-life\" means nothing when used in the same sentence'");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = CrimsonRarity.ID;
		}
	}
	public class Felnum_Bar : ModItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Felnum_Mat_Entry).Name;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Felnum Bar");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Felnum_Ore_Item>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();
		}
	}
	public class Fibron_Plating : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Fibron Plating");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.buyPrice(gold: 3, silver: 40);
			Item.rare = CrimsonRarity.ID;
		}
	}
	public class Formium_Bar : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Formium Bar");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.buyPrice(gold: 3, silver: 40);
			Item.rare = ButterscotchRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Formium_Scrap>(), 6);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
	public class Formium_Scrap : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Formium Scrap");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.buyPrice(silver: 56);
			Item.rare = ItemRarityID.Purple;
		}
	}
	public class Hell_Key : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Hell Key");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 14;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class Illegal_Explosive_Parts : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Illegal Explosive Parts");
			// Tooltip.SetDefault("'All explosive parts are illegal...'");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.buyPrice(gold: 20);
			Item.rare = ItemRarityID.LightRed;
		}
	}
	public class Lunar_Token : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Lunar Token");
			// Tooltip.SetDefault("'Valuable to the demented'");
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 4));
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DefenderMedal);
			Item.maxStack = 999;
			Item.rare = ItemRarityID.Cyan;
			Item.glowMask = glowmask;
		}
	}
	public class Magic_Hair_Spray : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Magic Hair Treatment Gel");
			// Tooltip.SetDefault("'Keeps your hair in perfect form!'");
			Item.ResearchUnlockCount = 1;

		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.buyPrice(copper: 80);
			Item.rare = ItemRarityID.Quest;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.BottledWater, 5);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddIngredient(ItemID.Gel, 5);
			recipe.AddIngredient(ModContent.ItemType<Silicon>(), 2);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
	public class Mushroom_Key : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Mushroom Key");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 14;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class FragmentNova : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Nova Fragment");
			// Tooltip.SetDefault("'The essence of a dying star in its final moments...'");
			glowmask = Origins.AddGlowMask(this);
			ItemID.Sets.ItemNoGravity[Type] = true;
			ItemID.Sets.ItemIconPulse[Type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FragmentSolar);
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.FragmentSolar);
			recipe.AddIngredient(ItemID.FragmentVortex);
			recipe.AddIngredient(ItemID.FragmentNebula);
			recipe.AddIngredient(ItemID.FragmentStardust);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
	public class Ocean_Key : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Ocean Key");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 14;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	[LegacyName("Peat_Moss_Item")]
	public class Peat_Moss : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Peat Moss");
			// Tooltip.SetDefault("The Demolitionist might find this interesting...");
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Peat_Moss_Tile>());
			Item.maxStack = 999;
			Item.value = Item.buyPrice(silver: 3);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.ExplosivePowder);
			recipe.AddIngredient(this, 3);
			recipe.AddTile(TileID.GlassKiln);
			recipe.Register();
		}
	}
	public class Power_Core : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Power Core");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Pink;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.HallowedBar, 2);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 4);
			recipe.AddTile(TileID.Anvils); //Fabricator not implemented yet
			recipe.Register();
		}
	}
	public class Qube : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Qube");
			// Tooltip.SetDefault("'Physical information, like everything else in the world'");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(gold: 1, silver: 60);
			Item.rare = ButterscotchRarity.ID;
			Item.glowMask = glowmask;
		}
	}
	public class Riven_Key : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Riven} Key");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 30;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Yellow;
			Item.glowMask = glowmask;
		}
	}
	public class Riven_Carapace : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Riven} Carapace");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.sellPrice(gold: 1, copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.ObsidianHelm);
			recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddIngredient(this, 5);
			recipe.AddTile(TileID.Hellforge);
			recipe.Register();
			recipe = Recipe.Create(ItemID.ObsidianPants);
			recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddIngredient(this, 5);
			recipe.AddTile(TileID.Hellforge);
			recipe.Register();
			recipe = Recipe.Create(ItemID.ObsidianShirt);
			recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddIngredient(this, 10);
			recipe.AddTile(TileID.Hellforge);
			recipe.Register();

			recipe = Recipe.Create(ItemID.VoidVault);
			recipe.AddIngredient(ItemID.Bone, 15);
			recipe.AddIngredient(ItemID.JungleSpores, 8);
			recipe.AddIngredient(this, 15);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
			recipe = Recipe.Create(ItemID.VoidLens);
			recipe.AddIngredient(ItemID.Bone, 30);
			recipe.AddIngredient(ItemID.JungleSpores, 15);
			recipe.AddIngredient(this, 30);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
	public class Rotor : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(copper: 40);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ItemID.HallowedBar);
			recipe.AddIngredient(ModContent.ItemType<Silicon>());
			recipe.AddTile(TileID.Anvils); //Fabricator not implemented yet
			recipe.Register();
		}
	}
	public class Rubber : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.buyPrice(copper: 6);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.Flipper);
			recipe.AddIngredient(Type, 15);
			recipe.AddIngredient(ModContent.ItemType<Silicon>(), 8);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = Recipe.Create(ItemID.FloatingTube);
			recipe.AddIngredient(Type, 20);
			recipe.AddIngredient(ModContent.ItemType<Silicon>(), 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Silicon : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Silicon");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(copper: 44);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.SandBlock, 3);
			recipe.AddTile(TileID.GlassKiln);
			recipe.Register();
		}
	}
	public class Strange_String : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Strange String");
			// Tooltip.SetDefault("'Involuntary neurectomy'");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.sellPrice(copper: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.BattlePotion);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Wilting_Rose_Item>());
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();

			recipe = Recipe.Create(ItemID.CoffinMinecart);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 10);
			recipe.AddIngredient(Type, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.MechanicalWorm);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
			recipe.AddIngredient(ItemID.SoulofNight, 6);
			recipe.AddIngredient(Type, 6);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(ItemID.MonsterLasagna);
			recipe.AddIngredient(Type, 8);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();

			recipe = Recipe.Create(ItemID.UnholyArrow, 5);
			recipe.AddRecipeGroup(ItemID.WoodenArrow, 5);
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Tree_Sap : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Tree Sap");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(copper: 2);
			Item.rare = ItemRarityID.Gray;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Rubber>());
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.GlassKiln);
			recipe.Register();
		}
	}
	public class Undead_Chunk : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Undead Chunk");
			//Tooltip.SetDefault("'I'm still alive ya'know'");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.ObsidianHelm);
			recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddIngredient(this, 5);
			recipe.AddTile(TileID.Hellforge);
			recipe.Register();
			recipe = Recipe.Create(ItemID.ObsidianPants);
			recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddIngredient(this, 5);
			recipe.AddTile(TileID.Hellforge);
			recipe.Register();
			recipe = Recipe.Create(ItemID.ObsidianShirt);
			recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddIngredient(this, 10);
			recipe.AddTile(TileID.Hellforge);
			recipe.Register();

			recipe = Recipe.Create(ItemID.VoidVault);
			recipe.AddIngredient(ItemID.Bone, 15);
			recipe.AddIngredient(ItemID.JungleSpores, 8);
			recipe.AddIngredient(this, 15);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
			recipe = Recipe.Create(ItemID.VoidLens);
			recipe.AddIngredient(ItemID.Bone, 30);
			recipe.AddIngredient(ItemID.JungleSpores, 15);
			recipe.AddIngredient(this, 30);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
	public class Unpowered_Eyndum_Core : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Unpowered Eyndum Core");
			// Tooltip.SetDefault("'Limitless potential'");
			Item.ResearchUnlockCount = 2;

		}
		public override void SetDefaults() {
			Item.maxStack = 8;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ButterscotchRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			//recipe.AddIngredient(ModContent.ItemType<Superconductor>(), 10);
			recipe.AddIngredient(ModContent.ItemType<Eyndum_Bar>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 4);
			recipe.AddTile(TileID.Anvils); //No Omni-Printer
			recipe.Register();
		}
	}
	public class Valkyrum_Bar : ModItem {
		//Alloy of Felnum and Angelium, might have to replace it with Chlorophye :o
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Valkyrum Bar");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Ectoplasm);
			recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>());
			//recipe.AddIngredient(ModContent.ItemType<_Bar>(), 1);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();
		}
	}
	public class Wilting_Rose_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Wilting Rose");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(copper: 20);
		}
	}
	public class Wrycoral_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Wrycoral");
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.value = Item.sellPrice(copper: 20);
		}
	}
}
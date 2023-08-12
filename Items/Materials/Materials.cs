using Origins.Items.Other.Consumables;
using Origins.Journal;
using Origins.Tiles.Ashen;
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
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(copper: 18);
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 25;
			ItemID.Sets.ShimmerTransformToItem[ItemID.CursedFlame] = ItemID.Ichor;
			ItemID.Sets.ShimmerTransformToItem[ItemID.Ichor] = ModContent.ItemType<Black_Bile>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Black_Bile>()] = ModContent.ItemType<Alkahest>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Alkahest>()] = ModContent.ItemType<Respyrite>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Respyrite>()] = ItemID.CursedFlame;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(silver: 9);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = Item.CommonMaxStack;
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
			Item.rare = ItemRarityID.Gray;
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 25;

		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Gray;
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.Leather);
			recipe.AddIngredient(this, 4);
			recipe.AddTile(TileID.HeavyWorkBench);
			recipe.Register();
		}
	}
	public class Biocomponent10 : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 30;
			ItemID.Sets.ShimmerTransformToItem[ItemID.RottenChunk] = ItemID.Vertebrae;
			ItemID.Sets.ShimmerTransformToItem[ItemID.Vertebrae] = ModContent.ItemType<Strange_String>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Strange_String>()] = ModContent.ItemType<Bud_Barnacle>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bud_Barnacle>()] = ModContent.ItemType<Biocomponent10>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Biocomponent10>()] = ItemID.RottenChunk;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(copper: 2);
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.BattlePotion);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Surveysprout>());
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();

			recipe = Recipe.Create(ItemID.UnholyArrow, 5);
			recipe.AddIngredient(ItemID.WoodenArrow, 5);
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Black_Bile : ModItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Black_Bile_Entry).Name;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(silver: 10);
			Item.maxStack = Item.CommonMaxStack;
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
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 48;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 30;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 40);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Bottle);
			recipe.AddIngredient(ItemID.Stinger, 2);
			recipe.AddIngredient(ModContent.ItemType<Magic_Brine_Dropper>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
	public class Brineglow : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 30);
			Item.glowMask = glowmask;
		}
	}
	public class Bud_Barnacle : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 30;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.BattlePotion);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Wrycoral_Item>());
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();

			recipe = Recipe.Create(ItemID.MonsterLasagna);
			recipe.AddIngredient(Type, 8);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();

			recipe = Recipe.Create(ItemID.UnholyArrow, 5);
			recipe.AddIngredient(ItemID.WoodenArrow, 5);
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Busted_Servo : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Pink;
		}
	}
	public class Chambersite : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 8);
			Item.rare = ItemRarityID.Blue;
		}
	}
	public class Chromtain_Bar : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class Defiled_Key : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class Dusk_Key : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class Eitrite_Bar : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 81);
			Item.rare = ItemRarityID.Orange;
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
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = CrimsonRarity.ID;
		}
	}
	public class Felnum_Bar : ModItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Felnum_Mat_Entry).Name;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 68);
			Item.rare = CrimsonRarity.ID;
		}
	}
	public class Formium_Bar : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 68);
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
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Purple;
		}
	}
	public class Hell_Key : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class Illegal_Explosive_Parts : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ItemID.Sets.ShimmerTransformToItem[ItemID.IllegalGunParts] = ModContent.ItemType<Illegal_Explosive_Parts>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Illegal_Explosive_Parts>()] = ItemID.IllegalGunParts;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
		}
	}
	public class Lunar_Token : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 4));
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DefenderMedal);
			Item.rare = ItemRarityID.Cyan;
			Item.glowMask = glowmask;
		}
	}
	public class Magic_Hair_Spray : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;

		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 40);
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
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	public class NE8 : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
			ItemID.Sets.ShimmerTransformToItem[ItemID.ShadowScale] = ItemID.TissueSample;
			ItemID.Sets.ShimmerTransformToItem[ItemID.TissueSample] = ModContent.ItemType<Undead_Chunk>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Undead_Chunk>()] = ModContent.ItemType<Riven_Carapace>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Riven_Carapace>()] = ModContent.ItemType<NE8>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<NE8>()] = ItemID.ShadowScale;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.rare = ItemRarityID.Blue;
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
	public class FragmentNova : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
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
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Yellow;
		}
	}
	[LegacyName("Peat_Moss_Item")]
	public class Peat_Moss : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Peat_Moss_Tile>());
			Item.value = Item.sellPrice(copper: 60);
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
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 20);
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
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(gold: 1, silver: 60);
			Item.rare = ButterscotchRarity.ID;
			Item.glowMask = glowmask;
		}
	}
	public class Respyrite : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 9);
			Item.rare = ItemRarityID.Orange;
		}
	}
	public class Riven_Key : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Yellow;
			Item.glowMask = glowmask;
		}
	}
	public class Riven_Carapace : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.maxStack = Item.CommonMaxStack;
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
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 6);
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
	public class Sanguinite_Bar : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Magiluminescence);
			recipe.AddIngredient(Type, 12);
			recipe.AddIngredient(ItemID.Topaz, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Silicon : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.BattlePotion);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Wilting_Rose_Item>());
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Bottles);
			recipe.Register();

			recipe = Recipe.Create(ItemID.UnholyArrow, 5);
			recipe.AddIngredient(ItemID.WoodenArrow, 5);
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Surveysprout : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
			ItemID.Sets.ShimmerTransformToItem[ItemID.Deathweed] = ModContent.ItemType<Wilting_Rose_Item>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Wilting_Rose_Item>()] = ModContent.ItemType<Wrycoral_Item>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Wrycoral_Item>()] = ModContent.ItemType<Surveysprout>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Surveysprout>()] = ItemID.Deathweed;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 20);
		}
	}
	public class Tree_Sap : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
			Item.ResearchUnlockCount = 2;

		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
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
		//Alloy of Felnum and a Dawn material. I can imagine a pearl-like color now
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Ectoplasm);
			recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>());
			//recipe.AddIngredient(ModContent.ItemType<_Bar>(), 1);
			recipe.AddTile(TileID.AdamantiteForge);
			recipe.Register();
		}
	}
	public class Waste_Pump : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
			ItemID.Sets.ShimmerTransformToItem[ItemID.VileMushroom] = ItemID.ViciousMushroom;
			ItemID.Sets.ShimmerTransformToItem[ItemID.ViciousMushroom] = ModContent.ItemType<Soulspore_Item>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Soulspore_Item>()] = ModContent.ItemType<Acetabularia_Item>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Acetabularia_Item>()] = ModContent.ItemType<Waste_Pump>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Waste_Pump>()] = ItemID.VileMushroom;
		}
        public override void SetDefaults() {
			Item.width = 14;
			Item.height = 18;
			Item.maxStack = Item.CommonMaxStack;
		}
    }
	public class Wilting_Rose_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 20);
		}
	}
	public class Wrycoral_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 20);
		}
	}
}
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
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
    public abstract class MaterialItem : ModItem {
		public virtual bool HasTooltip => false;
		public virtual bool HasGlowmask => false;
		public virtual string GlowTexture => Texture + "_Glow";
		public override LocalizedText Tooltip => HasTooltip ? base.Tooltip : LocalizedText.Empty;
		public virtual int ResearchUnlockCount => 25;
		public virtual int Rare => ItemRarityID.White;
		public virtual int Value => 0;
		protected short glowmask = -1;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = ResearchUnlockCount;
			if (HasGlowmask) glowmask = Origins.AddGlowMask(GlowTexture);
		}
		public override void SetDefaults() {
			Item.rare = Rare;
			Item.value = Value;
			Item.maxStack = Item.CommonMaxStack;
			Item.glowMask = glowmask;
		}
	}
	public class Adhesive_Wrap : MaterialItem {
		public override int ResearchUnlockCount => 25;
		public override int Value => Item.sellPrice(copper: 18);
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 3);
			recipe.AddIngredient(ModContent.ItemType<Silicon_Item>());
            recipe.AddIngredient(ModContent.ItemType<Tree_Sap>());
            recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = Recipe.Create(ItemID.AdhesiveBandage);
			recipe.AddIngredient(ItemID.GlowingMushroom, 3);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Alkahest : MaterialItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Alkahest_Mat_Entry).Name;
		public override int ResearchUnlockCount => 25;
		public override int Rare => ItemRarityID.Orange;
		public override int Value => Item.sellPrice(silver: 9);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ShimmerTransformToItem[ItemID.CursedFlame] = ItemID.Ichor;
			ItemID.Sets.ShimmerTransformToItem[ItemID.Ichor] = ModContent.ItemType<Black_Bile>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Black_Bile>()] = ModContent.ItemType<Alkahest>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Alkahest>()] = ModContent.ItemType<Respyrite>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Respyrite>()] = ItemID.CursedFlame;
		}
		public class Alkahest_Mat_Entry : JournalEntry {
			public override string TextKey => "Alkahest";
			public override ArmorShaderData TextShader => null;
		}
	}
	public class Bark : MaterialItem {
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Rubber>());
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.GlassKiln);
			recipe.Register();
		}
	}
	public class Bat_Hide : MaterialItem {
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.Leather);
			recipe.AddIngredient(this, 4);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Biocomponent10 : MaterialItem {
		public override int ResearchUnlockCount => 30;
		public override int Value => Item.sellPrice(copper: 2);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ShimmerTransformToItem[ItemID.RottenChunk] = ItemID.Vertebrae;
			ItemID.Sets.ShimmerTransformToItem[ItemID.Vertebrae] = ModContent.ItemType<Strange_String>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Strange_String>()] = ModContent.ItemType<Bud_Barnacle>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bud_Barnacle>()] = ModContent.ItemType<Biocomponent10>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Biocomponent10>()] = ItemID.RottenChunk;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.UnholyArrow, 5);
			recipe.AddIngredient(ItemID.WoodenArrow, 5);
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Black_Bile : MaterialItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Black_Bile_Entry).Name;
		public override int Rare => ItemRarityID.Orange;
		public override int Value => Item.sellPrice(silver: 10);
		public class Black_Bile_Entry : JournalEntry {
			public override string TextKey => "Black_Bile";
			public override ArmorShaderData TextShader => null;
		}
	}
	public class Bleeding_Obsidian_Shard : MaterialItem {
		public override bool HasGlowmask => true;
		public override int ResearchUnlockCount => 48;
		public override int Rare => ItemRarityID.LightRed;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Bleeding_Obsidian_Item>());
			recipe.AddIngredient(this, 8);
			recipe.Register();

			recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>());
			recipe.Register();
		}
	}
	public class Bottled_Brine : MaterialItem {
		public override int ResearchUnlockCount => 30;
		public override int Value => Item.sellPrice(copper: 40);
        public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Bottle);
            recipe.AddIngredient(ItemID.Stinger, 2);
            recipe.AddIngredient(ModContent.ItemType<Magic_Brine_Dropper>());
            recipe.AddTile(TileID.Bottles);
            recipe.Register();
        }
    }
	public class Bud_Barnacle : MaterialItem {
		public override int ResearchUnlockCount => 30;
		public override int Value => Item.sellPrice(copper: 2);
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.UnholyArrow, 5);
			recipe.AddIngredient(ItemID.WoodenArrow, 5);
			recipe.AddIngredient(Type);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Busted_Servo : MaterialItem {
		public override int ResearchUnlockCount => 99;
		public override int Value => Item.sellPrice(silver: 2);
		public override int Rare => ItemRarityID.Pink;
	}
	public class Chromtain_Bar : MaterialItem {
		public override int Value => Item.sellPrice(gold: 1);
		public override int Rare => CrimsonRarity.ID;
	}
	public class Defiled_Bar : MaterialItem {
		public override int Value => Item.sellPrice(silver: 30);
		public override int Rare => ItemRarityID.Blue;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Magiluminescence);
            recipe.AddIngredient(ItemID.Topaz, 5);
            recipe.AddIngredient(Type, 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Eitrite_Bar : MaterialItem {
        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            ItemID.Sets.ShimmerTransformToItem[ItemID.HallowedBar] = ModContent.ItemType<Eitrite_Bar>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Eitrite_Bar>()] = ItemID.HallowedBar;
        }
        public override int Value => Item.sellPrice(silver: 81);
		public override int Rare => ItemRarityID.Orange;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Ore_Item>(), 4);
			recipe.AddTile(TileID.AdamantiteForge);
			recipe.Register();

            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.AdamantiteBar);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 2);
            recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();

            recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.TitaniumBar);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 2);
			recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Element36_Bundle : MaterialItem {
		public override int Value => Item.sellPrice(gold: 1);
		public override int Rare => CrimsonRarity.ID;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.FragmentNebula);
			recipe.AddIngredient(ModContent.ItemType<Fibron_Plating>(), 4);
			recipe.AddIngredient(ModContent.ItemType<Formium_Bar>());
			recipe.AddTile(TileID.Anvils); //Interstellar Sampler also not implemented
			recipe.Register();
		}
	}
	[LegacyName("Infested_Bar")]
	public class Encrusted_Bar : MaterialItem {
		public override int Value => Item.sellPrice(silver: 30);
		public override int Rare => ItemRarityID.Blue;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Ore_Item>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Magiluminescence);
            recipe.AddIngredient(ItemID.Topaz, 5);
            recipe.AddIngredient(Type, 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Felnum_Bar : MaterialItem, IJournalEntryItem {
		public override int Value => Item.sellPrice(silver: 40);
		public override int Rare => ItemRarityID.Green;
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Felnum_Mat_Entry).Name;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Felnum_Ore_Item>(), 3);
			recipe.AddTile(TileID.Hellforge);
			recipe.Register();
		}
	}
	public class Fibron_Plating : MaterialItem {
		public override int Value => Item.sellPrice(silver: 68);
		public override int Rare => CrimsonRarity.ID;
	}
	public class Formium_Bar : MaterialItem {
		public override int Value => Item.sellPrice(silver: 68);
		public override int Rare => ButterscotchRarity.ID;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Formium_Scrap>(), 6);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
	public class Formium_Scrap : MaterialItem {
		public override int Value => Item.sellPrice(silver: 10);
		public override int Rare => ItemRarityID.Purple;
	}
	public class Illegal_Explosive_Parts : MaterialItem {
		public override int ResearchUnlockCount => 1;
		public override int Value => Item.sellPrice(gold: 4);
		public override int Rare => ItemRarityID.LightRed;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ShimmerTransformToItem[ItemID.IllegalGunParts] = ModContent.ItemType<Illegal_Explosive_Parts>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Illegal_Explosive_Parts>()] = ItemID.IllegalGunParts;
		}
	}
	public class Lunar_Token : MaterialItem {
		public override bool HasGlowmask => true;
		public override string GlowTexture => Texture;
		public override int ResearchUnlockCount => 100;
		public override int Value => -1;
		public override int Rare => ItemRarityID.Cyan;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 4));
		}
	}
	public class Magic_Hair_Spray : MaterialItem {
		public override int ResearchUnlockCount => 1;
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Quest;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.BottledWater, 5);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddIngredient(ModContent.ItemType<Silicon_Item>());
			recipe.AddTile(TileID.Bottles);
			recipe.Register();
		}
	}
	public class NE8 : MaterialItem {
		public override int Rare => ItemRarityID.Blue;
		public override int Value => Item.sellPrice(silver: 1, copper: 50);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ShimmerTransformToItem[ItemID.ShadowScale] = ItemID.TissueSample;
			ItemID.Sets.ShimmerTransformToItem[ItemID.TissueSample] = ModContent.ItemType<Undead_Chunk>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Undead_Chunk>()] = ModContent.ItemType<Riven_Carapace>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Riven_Carapace>()] = ModContent.ItemType<NE8>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<NE8>()] = ItemID.ShadowScale;
		}
	}
	public class Nova_Fragment : MaterialItem {
		public override string GlowTexture => Texture;
		public override int Value => Item.sellPrice(silver: 20);
		public override int Rare => ItemRarityID.Cyan;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ItemNoGravity[Type] = true;
			ItemID.Sets.ItemIconPulse[Type] = true;
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
	public class Power_Core : MaterialItem {
		public override bool HasGlowmask => true;
		public override int ResearchUnlockCount => 20;
		public override int Value => Item.sellPrice(silver: 20);
		public override int Rare => ItemRarityID.Pink;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.HallowedBar, 2);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 4);
			recipe.AddTile(TileID.Anvils); //Fabricator not implemented yet
			recipe.Register();
		}
	}
	public class Respyrite : MaterialItem {
		public override int Value => Item.sellPrice(silver: 9);
		public override int Rare => ItemRarityID.Orange;
	}
	public class Riven_Carapace : MaterialItem {
		public override bool HasGlowmask => true;
		public override int Rare => ItemRarityID.Blue;
		public override int Value => Item.sellPrice(silver: 1, copper: 50);
	}
	public class Rotor : MaterialItem {
		public override int ResearchUnlockCount => 99;
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Pink;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ItemID.HallowedBar);
			recipe.AddIngredient(ModContent.ItemType<Silicon_Item>());
			recipe.AddTile(TileID.Anvils); //Fabricator not implemented yet
			recipe.Register();
		}
	}
	public class Rubber : MaterialItem {
		public override int Value => Item.sellPrice(copper: 6);
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.Flipper);
			recipe.AddIngredient(Type, 15);
			recipe.AddIngredient(ModContent.ItemType<Silicon_Item>(), 8);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = Recipe.Create(ItemID.FloatingTube);
			recipe.AddIngredient(Type, 20);
			recipe.AddIngredient(ModContent.ItemType<Silicon_Item>(), 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Sanguinite_Bar : MaterialItem {
		public override int Value => Item.sellPrice(silver: 30);
		public override int Rare => ItemRarityID.Blue;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Magiluminescence);
            recipe.AddIngredient(ItemID.Topaz, 5);
            recipe.AddIngredient(Type, 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Strange_String : MaterialItem {
		public override int Value => Item.sellPrice(copper: 2);
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(ItemID.UnholyArrow, 5);
			recipe.AddIngredient(ItemID.WoodenArrow, 5);
            recipe.AddIngredient(Type);
            recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Tree_Sap : MaterialItem {
		public override int Value => Item.sellPrice(copper: 2);
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Rubber>());
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.GlassKiln);
			recipe.Register();

            recipe = Recipe.Create(ItemID.LesserHealingPotion, 2);
            recipe.AddIngredient(ItemID.Bottle, 2);
            recipe.AddIngredient(ModContent.ItemType<Tree_Sap>());
            recipe.AddTile(TileID.Bottles);
            recipe.Register();
        }
	}
	public class Undead_Chunk : MaterialItem {
		public override bool HasGlowmask => true;
		public override int Rare => ItemRarityID.Blue;
		public override int Value => Item.sellPrice(silver: 1, copper: 50);
	}
	public class Valkyrum_Bar : MaterialItem {
		//Alloy of Felnum and a Dawn material. I can imagine a pearl-like color now
		public override int Value => Item.sellPrice(gold: 1);
		public override int Rare => ItemRarityID.Yellow;
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Ectoplasm);
			recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>());
			//recipe.AddIngredient(ModContent.ItemType<_Bar>(), 1);
			recipe.AddTile(TileID.AdamantiteForge);
			recipe.Register();
		}
	}

	#region biome keys
	public class Dawn_Key : MaterialItem {
		public override int ResearchUnlockCount => 1;
		public override int Value => 0;
		public override int Rare => ItemRarityID.Yellow;
	}
	public class Defiled_Key : Dawn_Key { }
	public class Dusk_Key : Dawn_Key { }
	public class Hell_Key : Dawn_Key { }
	public class Mushroom_Key : Dawn_Key { }
	public class Ocean_Key : Dawn_Key { }
	public class Riven_Key : Dawn_Key {
		public override bool HasGlowmask => true;
	}
	#endregion
}
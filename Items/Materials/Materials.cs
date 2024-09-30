using Origins.Dev;
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
    public abstract class MaterialItem : ModItem, ICustomWikiStat {
		protected override bool CloneNewInstances => true;
		public virtual bool HasTooltip => false;
		public virtual bool HasGlowmask => false;
		public virtual string GlowTexture => Texture + "_Glow";
		public override LocalizedText Tooltip => HasTooltip ? base.Tooltip : LocalizedText.Empty;
		public virtual int ResearchUnlockCount => 25;
		public virtual int Rare => ItemRarityID.White;
		public virtual int Value => 0;
		bool? ICustomWikiStat.Hardmode => Hardmode;
		public abstract bool Hardmode { get; }
		protected short glowmask = -1;
		protected int tileID = -1;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = ResearchUnlockCount;
			if (HasGlowmask) glowmask = Origins.AddGlowMask(GlowTexture);
		}
		public override void SetDefaults() {
			if (tileID != -1) Item.DefaultToPlaceableTile(tileID);
			Item.rare = Rare;
			Item.value = Value;
			Item.maxStack = Item.CommonMaxStack;
			Item.glowMask = glowmask;
		}
	}
	public class Adhesive_Wrap : MaterialItem {
		public override int ResearchUnlockCount => 25;
		public override int Value => Item.sellPrice(copper: 18);
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(Type, 3)
			.AddIngredient(ModContent.ItemType<Silicon_Item>())
            .AddIngredient(ModContent.ItemType<Tree_Sap>())
            .AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemID.AdhesiveBandage)
			.AddIngredient(ItemID.GlowingMushroom, 3)
			.AddIngredient(this)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	public class Alkahest : MaterialItem, IJournalEntryItem {
        public string[] Categories => [
            "LoreItem"
        ];
        public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Alkahest_Mat_Entry).Name;
		public override int ResearchUnlockCount => 25;
		public override int Rare => ItemRarityID.Orange;
		public override int Value => Item.sellPrice(silver: 9);
		public override bool Hardmode => true;
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
		}
	}
	public class Bark : MaterialItem {
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Rubber>())
			.AddIngredient(this)
			.AddTile(TileID.GlassKiln)
			.Register();
		}
	}
	public class Bat_Hide : MaterialItem {
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(ItemID.Leather)
			.AddIngredient(this, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	public class Biocomponent10 : MaterialItem {
		public override int ResearchUnlockCount => 30;
		public override int Value => Item.sellPrice(copper: 2);
		public override bool Hardmode => false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ShimmerTransformToItem[ItemID.RottenChunk] = ItemID.Vertebrae;
			ItemID.Sets.ShimmerTransformToItem[ItemID.Vertebrae] = ModContent.ItemType<Strange_String>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Strange_String>()] = ModContent.ItemType<Bud_Barnacle>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bud_Barnacle>()] = ModContent.ItemType<Biocomponent10>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Biocomponent10>()] = ItemID.RottenChunk;
		}
		public override void AddRecipes() {
			Recipe.Create(ItemID.UnholyArrow, 5)
			.AddIngredient(ItemID.WoodenArrow, 5)
			.AddIngredient(Type)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Black_Bile : MaterialItem, IJournalEntryItem {
        public string[] Categories => [
            "LoreItem"
        ];
        public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Black_Bile_Entry).Name;
		public override int Rare => ItemRarityID.Orange;
		public override int Value => Item.sellPrice(silver: 10);
		public override bool Hardmode => true;
		public class Black_Bile_Entry : JournalEntry {
			public override string TextKey => "Black_Bile";
		}
	}
	public class Bleeding_Obsidian_Shard : MaterialItem {
		public override bool HasGlowmask => true;
		public override int ResearchUnlockCount => 48;
		public override int Rare => ItemRarityID.LightRed;
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Bleeding_Obsidian_Item>())
			.AddIngredient(this, 8)
			.Register();

			Recipe.Create(Type, 8)
			.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>())
			.Register();
		}
	}
	public class Bottled_Brine : MaterialItem {
		public override int ResearchUnlockCount => 30;
		public override int Value => Item.sellPrice(copper: 40);
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.Bottle)
            .AddIngredient(ItemID.Stinger, 2)
            .AddIngredient(ModContent.ItemType<Magic_Brine_Dropper>())
            .AddTile(TileID.Bottles)
            .Register();
        }
    }
	public class Bud_Barnacle : MaterialItem {
		public override int ResearchUnlockCount => 30;
		public override int Value => Item.sellPrice(copper: 2);
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(ItemID.UnholyArrow, 5)
			.AddIngredient(ItemID.WoodenArrow, 5)
			.AddIngredient(Type)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Busted_Servo : MaterialItem {
		public override int ResearchUnlockCount => 99;
		public override int Value => Item.sellPrice(silver: 2);
		public override int Rare => ItemRarityID.Pink;
		public override bool Hardmode => true;
	}
	public class Chromtain_Bar : MaterialItem {
		public override int Value => Item.sellPrice(gold: 1);
		public override int Rare => CrimsonRarity.ID;
		public override bool Hardmode => true;
		public override void Load() {
			base.Load();
			tileID = Bar_Tile.AddBarTile(this);
		}
	}
	public class Defiled_Bar : MaterialItem {
        public string[] Categories => [
            "LoreItem"
        ];
        public override int Value => Item.sellPrice(silver: 30);
		public override int Rare => ItemRarityID.Blue;
		public override bool Hardmode => false;
		public override void Load() {
			base.Load();
			tileID = Bar_Tile.AddBarTile(this);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 3)
			.AddTile(TileID.Furnaces)
			.Register();
		}
	}
	public class Eitrite_Bar : MaterialItem {
		public override bool Hardmode => true;
		public override void Load() {
			base.Load();
			tileID = Bar_Tile.AddBarTile(this);
		}

		public override void SetStaticDefaults() {
            base.SetStaticDefaults();
			ItemID.Sets.ShimmerTransformToItem[ItemID.HallowedBar] = ModContent.ItemType<Eitrite_Bar>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Eitrite_Bar>()] = ItemID.HallowedBar;
        }
        public override int Value => Item.sellPrice(silver: 81);
		public override int Rare => ItemRarityID.Orange;
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Ore_Item>(), 3)
			.AddTile(TileID.AdamantiteForge)
			.Register();

            Recipe.Create(Type)
            .AddRecipeGroup("AdamantiteBars")
            .AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 2)
            .AddIngredient(ModContent.ItemType<Bottled_Brine>())
            .AddTile(TileID.MythrilAnvil)
            .Register();
		}
	}
	public class Element36_Bundle : MaterialItem {
		public override int Value => Item.sellPrice(gold: 1);
		public override int Rare => CrimsonRarity.ID;
		public override bool Hardmode => true;
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ModContent.ItemType<Fibron_Plating>(), 4)
			.AddIngredient(ModContent.ItemType<Formium_Bar>())
			.AddTile(TileID.Anvils) //Interstellar Sampler also not implemented
			.Register();
		}
	}
	public class Encrusted_Bar : MaterialItem {
		public override int Value => Item.sellPrice(silver: 30);
		public override int Rare => ItemRarityID.Blue;
		public override bool Hardmode => false;
		public override void Load() {
			base.Load();
			tileID = Bar_Tile.AddBarTile(this);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Encrusted_Ore_Item>(), 3)
			.AddTile(TileID.Furnaces)
			.Register();
		}
	}
	public class Felnum_Bar : MaterialItem, IJournalEntryItem {
        public string[] Categories => [
            "LoreItem"
        ];
        public override int Value => Item.sellPrice(silver: 40);
		public override int Rare => ItemRarityID.Green;
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Felnum_Mat_Entry).Name;
		public override bool Hardmode => false;
		public override void Load() {
			base.Load();
			tileID = Bar_Tile.AddBarTile(this);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Ore_Item>(), 3)
			.AddTile(TileID.Hellforge)
			.Register();
		}
	}
	public class Fibron_Plating : MaterialItem {
		public override int Value => Item.sellPrice(silver: 68);
		public override int Rare => CrimsonRarity.ID;
		public override bool Hardmode => true;
	}
	public class Formium_Bar : MaterialItem {
		public override int Value => Item.sellPrice(silver: 68);
		public override int Rare => ButterscotchRarity.ID;
		public override bool Hardmode => true;
		public override void Load() {
			base.Load();
			tileID = Bar_Tile.AddBarTile(this);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Formium_Scrap>(), 6)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
	public class Formium_Scrap : MaterialItem {
		public override int Value => Item.sellPrice(silver: 10);
		public override int Rare => ItemRarityID.Purple;
		public override bool Hardmode => true;
	}
	public class Lunar_Token : MaterialItem {
		public override bool HasGlowmask => true;
		public override string GlowTexture => Texture;
		public override int ResearchUnlockCount => 100;
		public override int Value => -1;
		public override int Rare => ItemRarityID.Cyan;
		public override bool Hardmode => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 4));
		}
	}
	public class Magic_Hair_Spray : MaterialItem {
		public override int ResearchUnlockCount => 1;
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Quest;
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddIngredient(ItemID.BottledWater, 5)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ModContent.ItemType<Silicon_Item>())
			.AddTile(TileID.Bottles)
			.Register();
		}
	}
	public class NE8 : MaterialItem {
		public override int Rare => ItemRarityID.Blue;
		public override int Value => Item.sellPrice(silver: 1, copper: 50);
		public override bool Hardmode => false;
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
		public override bool HasGlowmask => true;
		public override string GlowTexture => Texture;
		public override int Value => Item.sellPrice(silver: 20);
		public override int Rare => ItemRarityID.Cyan;
		public override bool Hardmode => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ItemNoGravity[Type] = true;
			ItemID.Sets.ItemIconPulse[Type] = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentStardust)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
	public class Power_Core : MaterialItem {
		public override bool HasGlowmask => true;
		public override int ResearchUnlockCount => 20;
		public override int Value => Item.sellPrice(silver: 20);
		public override int Rare => ItemRarityID.Pink;
		public override bool Hardmode => true;
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.HallowedBar, 2)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 3)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
	public class Respyrite : MaterialItem {
		public override int Value => Item.sellPrice(silver: 9);
		public override int Rare => ItemRarityID.Orange;
		public override bool Hardmode => true;
	}
	public class Riven_Carapace : MaterialItem {
		public override bool HasGlowmask => true;
		public override int Rare => ItemRarityID.Blue;
		public override int Value => Item.sellPrice(silver: 1, copper: 50);
		public override bool Hardmode => false;
	}
	public class Rotor : MaterialItem {
		public override int ResearchUnlockCount => 99;
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Pink;
		public override bool Hardmode => true;
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemID.HallowedBar)
			.AddIngredient(ModContent.ItemType<Silicon_Item>())
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
	public class Rubber : MaterialItem {
		public override int Value => Item.sellPrice(copper: 6);
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(ItemID.Flipper)
			.AddIngredient(Type, 15)
			.AddIngredient(ModContent.ItemType<Silicon_Item>(), 8)
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemID.FloatingTube)
			.AddIngredient(Type, 20)
			.AddIngredient(ModContent.ItemType<Silicon_Item>(), 10)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	public class Sanguinite_Bar : MaterialItem {
		public override int Value => Item.sellPrice(silver: 30);
		public override int Rare => ItemRarityID.Blue;
		public override bool Hardmode => false;
		public override void Load() {
			base.Load();
			tileID = Bar_Tile.AddBarTile(this);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>(), 3)
			.AddTile(TileID.Furnaces)
			.Register();
		}
	}
	public class Strange_String : MaterialItem {
		public override int Value => Item.sellPrice(copper: 2);
		public override bool Hardmode => false;
		public override void AddRecipes() {
            Recipe.Create(ItemID.UnholyArrow, 5)
			.AddIngredient(ItemID.WoodenArrow, 5)
            .AddIngredient(Type)
            .AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Tree_Sap : MaterialItem {
		public override int Value => Item.sellPrice(copper: 2);
		public override bool Hardmode => false;
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Rubber>())
			.AddIngredient(this)
			.AddTile(TileID.GlassKiln)
			.Register();

            Recipe.Create(ItemID.LesserHealingPotion, 2)
            .AddIngredient(ItemID.Bottle, 2)
            .AddIngredient(ModContent.ItemType<Tree_Sap>())
            .AddTile(TileID.Bottles)
            .Register();
        }
	}
	public class Undead_Chunk : MaterialItem {
		public override bool HasGlowmask => true;
		public override int Rare => ItemRarityID.Blue;
		public override int Value => Item.sellPrice(silver: 1, copper: 50);
		public override bool Hardmode => false;
	}
	public class Valkyrum_Bar : MaterialItem {
		//Alloy of Felnum and a Dawn material. I can imagine a pearl-like color now
		public override int Value => Item.sellPrice(gold: 1);
		public override int Rare => ItemRarityID.Yellow;
		public override bool Hardmode => true;
		public override void Load() {
			base.Load();
			//tileID = Bar_Tile.AddBarTile(this);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Ectoplasm)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>())
			//.AddIngredient(ModContent.ItemType<_Bar>(), 1)
			.AddTile(TileID.AdamantiteForge)
			.Register();
		}
	}

	#region biome keys
	public class Dawn_Key : MaterialItem {
		public override int ResearchUnlockCount => 1;
		public override int Value => 0;
		public override int Rare => ItemRarityID.Yellow;
		public override bool Hardmode => true;
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
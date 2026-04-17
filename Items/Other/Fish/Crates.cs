using AltLibrary.Common.AltOres;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Pets;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Tiles;
using Origins.Tiles.Brine;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.AdvancedMiscShaderData.Parameter;
using static Origins.Items.Other.Fish.Fishing_Crate_Item;
using static Terraria.ID.ItemID;
using static Terraria.ModLoader.ModContent;

namespace Origins.Items.Other.Fish {
	#region chunky crate
	public class Chunky_Crate : Fishing_Crate_Item {
		public override Color MapColor => new(200, 200, 200);
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] defiled = [
				new OneFromRulesRule(1,
					ItemDropRule.NotScalingWithLuck(ItemType<Kruncher>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Dim_Starlight>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Monolith_Rod>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Krakram>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Suspicious_Looking_Pebble>())
				),
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores),
					new OneFromRulesRule(3, 2, Bars)),
				new OneFromRulesRule(3, Potions)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(defiled));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	#endregion
	#region bilious crate
	public class Bilious_Crate : Fishing_Crate_Item {
		public override Color MapColor => new(100, 100, 100);
		public override bool Hardmode => true;
		public override int ShimmerResult => ItemType<Chunky_Crate>();
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] defiled = [
				new OneFromRulesRule(1,
					ItemDropRule.NotScalingWithLuck(ItemType<Kruncher>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Dim_Starlight>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Monolith_Rod>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Krakram>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Suspicious_Looking_Pebble>())
				),
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, [..Ores, ..HardmodeOres]),
					new OneFromRulesRule(3, 2, [..Bars, ..HardmodeBars])),
				new OneFromRulesRule(3, Potions),
				BiomeCrate_SoulOfNight,
				ItemDropRule.NotScalingWithLuck(ItemType<Black_Bile>(), 2, 2, 5)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(defiled));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	#endregion
	#region crusty crate
	public class Crusty_Crate : Fishing_Crate_Item {
		public override Color MapColor => new(0, 125, 165);
		public override Color TileGlowColor => new(0.394f, 0.879f, 0.912f);
		static short glowmask;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.glowMask = glowmask;
		}
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] riven = [
				new OneFromRulesRule(1,
					ItemDropRule.NotScalingWithLuck(ItemType<Riven_Splitter>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Amebolize_Incantation>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Splitsplash>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Riverang>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Amoeba_Toy>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Primordial_Soup>())
				),
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores),
					new OneFromRulesRule(3, 2, Bars)),
				new OneFromRulesRule(3, Potions)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	#endregion
	#region festering crate
	public class Festering_Crate : Fishing_Crate_Item {
		public override Color MapColor => new(100, 100, 100);
		public override Color TileGlowColor => new(0.394f, 0.879f, 0.912f);
		public override int ShimmerResult => ItemType<Crusty_Crate>();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Origins.AddGlowMask(this);
		}
		public override bool Hardmode => true;
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] riven = [
				new OneFromRulesRule(1,
					ItemDropRule.NotScalingWithLuck(ItemType<Riven_Splitter>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Amebolize_Incantation>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Splitsplash>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Riverang>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Amoeba_Toy>()),
					ItemDropRule.NotScalingWithLuck(ItemType<Primordial_Soup>())
				),
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, [..Ores, ..HardmodeOres]),
					new OneFromRulesRule(3, 2, [..Bars, ..HardmodeBars])),
				new OneFromRulesRule(3, Potions),
				BiomeCrate_SoulOfNight,
				ItemDropRule.NotScalingWithLuck(ItemType<Alkahest>(), 2, 2, 5)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(riven));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	#endregion
	public class Ashen_Crates : Fishing_Crate_Set<HardmodeVariant> {
		public override Color GetMapColor(HardmodeVariant option) => option switch {
			HardmodeVariant.Normal => new Color(70, 0, 19),
			HardmodeVariant.Hardmode => new Color(206, 149, 0),
			_ => throw new NotImplementedException(),
		};

		public override string GetName(HardmodeVariant option) => option switch {
			HardmodeVariant.Normal => "Waste_Crate",
			HardmodeVariant.Hardmode => "Hazardous_Waste_Crate",
			_ => throw new NotImplementedException(),
		};

		public override IEnumerable<IItemDropRule> GetRules(HardmodeVariant option) {
			yield return ItemDropRule.AlwaysAtleastOneSuccess([
				Ashen_Biome.OrbDropRule,
				BiomeChest_GoldCoin,
				option.OresAndBars(),
				new OneFromRulesRule(3, Potions),
				..option.SoulsOfNight(),
				..option.OnlyHardmode(ItemDropRule.NotScalingWithLuck(ItemType<Alkahest>(), 2, 2, 5))
			]);
			yield return new OneFromRulesRule(2, BiomeCrate_ExtraPotions);
			yield return ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait);
		}
	}
	#region residual crate
	public class Residual_Crate : Fishing_Crate_Item {
		public override Color MapColor => new(0, 100, 102);
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] brine = [
				new OneFromRulesRule(1,
					ItemDropRule.NotScalingWithLuck(ItemType<Brineglow_Item>(), 1, 5, 16),
					ItemDropRule.NotScalingWithLuck(ItemType<Peat_Moss_Item>(), 1, 5, 16)
				),
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores),
					new OneFromRulesRule(3, 2, Bars)),
				new OneFromRulesRule(3, Potions)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(brine));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(new OneFromRulesRule(2,
				ItemDropRule.NotScalingWithLuck(ItemType<Focus_Potion>(), 1, 1, 3),
				ItemDropRule.NotScalingWithLuck(ItemType<Antisolve_Potion>(), 1, 1, 3)
			));
			itemLoot.Add(ItemDropRule.OneFromOptions(4,
				ItemType<Sour_Apple>(),
				ItemType<Caeser_Salad>()
			));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	#endregion
	#region basic crate
	public class Basic_Crate : Fishing_Crate_Item {
		public override Color MapColor => new(0, 62, 64);
		public override bool Hardmode => true;
		public override int ShimmerResult => ItemType<Residual_Crate>();
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule[] brine = [
				new OneFromRulesRule(1,
					ItemDropRule.NotScalingWithLuck(ItemType<Brineglow_Item>(), 1, 5, 16),
					ItemDropRule.NotScalingWithLuck(ItemType<Peat_Moss_Item>(), 1, 5, 16),
					ItemDropRule.NotScalingWithLuck(ItemType<Geothermal_Sludge>(), 1, 5, 16)
				),
				BiomeChest_GoldCoin,
				ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, [.. Ores, .. HardmodeOres]),
					new OneFromRulesRule(3, 2, [.. Bars, .. HardmodeBars])),
				new OneFromRulesRule(3, Potions)
			];
			itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(brine));
			itemLoot.Add(new OneFromRulesRule(2, BiomeCrate_ExtraPotions));
			itemLoot.Add(new OneFromRulesRule(2,
				ItemDropRule.NotScalingWithLuck(ItemType<Focus_Potion>(), 1, 1, 3),
				ItemDropRule.NotScalingWithLuck(ItemType<Antisolve_Potion>(), 1, 1, 3)
			));
			itemLoot.Add(ItemDropRule.OneFromOptions(4,
				ItemType<Sour_Apple>(),
				ItemType<Caeser_Salad>()
			));
			itemLoot.Add(ItemDropRule.SequentialRulesNotScalingWithLuck(2, BiomeCrate_ExtraBait));
		}
	}
	#endregion
	public abstract class Fishing_Crate_Item : ModItem, ICustomWikiStat {
		string[] ICustomWikiStat.Categories {
			get {
				return [
					WikiCategories.GrabBag,
					..(Hardmode ? [WikiCategories.Hardmode] : Array.Empty<string>()),
					..Categories
				];
			}
		}
		public virtual IEnumerable<string> Categories => [];
		public virtual Color TileGlowColor => Color.Black;
		public virtual float TileGlowLightAmount => 0.01f;
		public virtual float TileGlowFancyLightAmount => 0.2f;
		public virtual int ShimmerResult { get; set; }
		public abstract Color MapColor { get; }
		public static IItemDropRule BiomeChest_GoldCoin => ItemDropRule.Common(GoldCoin, 4, 5, 13);//normally NotScalingWithLuck
		public static IItemDropRule BiomeCrate_SoulOfNight => ItemDropRule.NotScalingWithLuck(SoulofNight, 2, 2, 5);
		public static IItemDropRule[] Ores => field ??= (from o in AltLib.GetAltOres()
			orderby o.OreSlot
			where o.OreSlot <= GetInstance<GoldOreSlot>()
			select ItemDropRule.NotScalingWithLuck(o.oreItem, 1, 20, 35)
			).ToArray();
		public static IItemDropRule[] Bars => field ??= (from o in AltLib.GetAltOres()
			orderby o.OreSlot
			where o.OreSlot <= GetInstance<GoldOreSlot>()
			select ItemDropRule.NotScalingWithLuck(o.bar, 1, 6, 16)
			).ToArray();
		public static IItemDropRule[] HardmodeOres => field ??= (from o in AltLib.GetAltOres()
			orderby o.OreSlot
			where o.OreSlot <= GetInstance<AdamantiteOreSlot>()
			select ItemDropRule.NotScalingWithLuck(o.oreItem, 1, 20, 35)
			).ToArray();
		public static IItemDropRule[] HardmodeBars => field ??= (from o in AltLib.GetAltOres()
			orderby o.OreSlot
			where o.OreSlot <= GetInstance<AdamantiteOreSlot>()
			select ItemDropRule.NotScalingWithLuck(o.bar, 1, o.OreSlot <= GetInstance<GoldOreSlot>() ? 6 : 5, 16)
			).ToArray();
		public static IItemDropRule[] Potions => [
			ItemDropRule.NotScalingWithLuck(ObsidianSkinPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(SpelunkerPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(HunterPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(GravitationPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(MiningPotion, 1, 2, 4),
			ItemDropRule.NotScalingWithLuck(HeartreachPotion, 1, 2, 4)
		];
		public static IItemDropRule[] BiomeCrate_ExtraPotions => [
			ItemDropRule.NotScalingWithLuck(188, 1, 5, 17),
			ItemDropRule.NotScalingWithLuck(189, 1, 5, 17)
		];
		public static IItemDropRule[] BiomeCrate_ExtraBait => [
			ItemDropRule.NotScalingWithLuck(2676, 3, 2, 6),
			ItemDropRule.NotScalingWithLuck(2675, 1, 2, 6)
		];
		public virtual bool Hardmode => false;
		protected override bool CloneNewInstances => true;
		[CloneByReference]
		Fishing_Crate_Tile tile;
		public override void Load() {
			Mod.AddContent(tile = new Fishing_Crate_Tile(this));
		}
		public override void SetStaticDefaults() {
			Sets.IsFishingCrate[Type] = true;
			Sets.IsFishingCrateHardmode[Type] = Hardmode;
			if (ShimmerResult > 0) Sets.ShimmerTransformToItem[Type] = ShimmerResult;
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(CrimsonFishingCrate);
			Item.createTile = tile.Type;
			Item.placeStyle = 0;
			Item.rare = ItemRarityID.Green;
		}
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
		}
		public override bool CanRightClick() {
			return true;
		}
	}
	[Autoload(false)]
	public class Fishing_Crate_Tile(Fishing_Crate_Item item) : ModTile, IGlowingModTile {
		public override string Texture => item.Texture;
		public override string Name => item.Name + "_Tile";
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public bool Glows { get; private set; } = false;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (!Glows) return;
			color.DoFancyGlow(item.TileGlowColor.ToVector3() * item.TileGlowFancyLightAmount, tile.TileColor);
		}
		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileTable[Type] = true;

			if (Glows) {
				Main.tileLighted[Type] = true;
				if (!Main.dedServ) GlowTexture = Request<Texture2D>(Texture + "_Glow");
			}

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.newTile.StyleHorizontal = true; // Optional, if you add more placeStyles for the item
			TileObjectData.newTile.CoordinatePadding = 0;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.addTile(Type);

			AddMapEntry(item.MapColor, item.DisplayName);
		}
		public override bool CreateDust(int i, int j, ref int type) => false;
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (!Glows) return;
			drawData.glowColor = GlowColor;
			drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY, 18, 18);
			drawData.glowTexture = GlowTexture;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (!Glows) return;
			r = (item.TileGlowColor.R / 255f) * item.TileGlowLightAmount;
			g = (item.TileGlowColor.G / 255f) * item.TileGlowLightAmount;
			b = (item.TileGlowColor.B / 255f) * item.TileGlowLightAmount;
		}
		public override void Load() {
			if (item.TileGlowColor != Color.Black) {
				Glows = true;
				this.SetupGlowKeys();
			}
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	[AttributeUsage(AttributeTargets.Enum)]
	abstract class FishingCrateOptionsAttribute<TOptions> : Attribute where TOptions : struct, Enum {
		public abstract void SetupShimmer(Dictionary<TOptions, Fishing_Crate_Set<TOptions>.Set_Crate> items);
		public abstract bool IsHardmode(TOptions option);
	}
	public abstract class Fishing_Crate_Set<TOptions> : ILoadable where TOptions : struct, Enum {
		public abstract string GetName(TOptions option);
		public abstract Color GetMapColor(TOptions option);
		public virtual Color GetGlowColor(TOptions option) => Color.Black;
		public virtual LocalizedText GetTooltip(TOptions option) => LocalizedText.Empty;
		public abstract IEnumerable<IItemDropRule> GetRules(TOptions option);
		FishingCrateOptionsAttribute<TOptions> handler;
		readonly Dictionary<TOptions, Set_Crate> items = [];
		Set_Crate first;
		void ILoadable.Load(Mod mod) {
			if (typeof(TOptions).GetAttribute<FishingCrateOptionsAttribute<TOptions>>() is not FishingCrateOptionsAttribute<TOptions> attribute) throw new NotSupportedException($"{nameof(TOptions)} does not have a FishingCrateOptions attribute");
			handler = attribute;
			foreach (TOptions option in Enum.GetValues<TOptions>()) {
				Set_Crate crate = new(this, option);
				items[option] = crate;
				mod.AddContent(crate);
				first ??= crate;
			}
		}
		void ILoadable.Unload() { }
		public class Set_Crate(Fishing_Crate_Set<TOptions> set, TOptions option) : Fishing_Crate_Item {
			[field: CloneByReference]
			public Fishing_Crate_Set<TOptions> Set { get; } = set;
			bool First => this == Set.first;
			public override Color MapColor => Set.GetMapColor(option);
			public override Color TileGlowColor => Set.GetGlowColor(option);
			public override string Name => Set.GetName(option);
			protected override bool CloneNewInstances => true;
			public override bool Hardmode => Set.handler.IsHardmode(option);
			public override LocalizedText Tooltip => Set.GetTooltip(option);
			public override void SetStaticDefaults() {
				base.SetStaticDefaults();
				Origins.AddGlowMask(this);
				if (First) Set.handler.SetupShimmer(Set.items);
			}
			public override void ModifyItemLoot(ItemLoot itemLoot) {
				foreach (IItemDropRule rule in Set.GetRules(option)) itemLoot.Add(rule);
			}
		}
	}
	static class CrateExtensions {
		public static IItemDropRule OresAndBars(this HardmodeVariant variant) {
			switch (variant) {
				case HardmodeVariant.Hardmode:
				return ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, HardmodeOres),
					new OneFromRulesRule(3, 2, HardmodeBars)
				);
				default:
				return ItemDropRule.SequentialRulesNotScalingWithLuck(1,
					new OneFromRulesRule(5, Ores),
					new OneFromRulesRule(3, 2, Bars)
				);
			}
		}
		public static IEnumerable<IItemDropRule> SoulsOfNight(this HardmodeVariant variant) => variant.OnlyHardmode(BiomeCrate_SoulOfNight);
		public static IEnumerable<IItemDropRule> OnlyHardmode(this HardmodeVariant variant, IItemDropRule rule) {
			switch (variant) {
				case HardmodeVariant.Hardmode:
				yield return rule;
				break;
			}
		}
	}
	sealed class Impl_HardmodeVariant : FishingCrateOptionsAttribute<HardmodeVariant> {
		public override bool IsHardmode(HardmodeVariant option) => option == HardmodeVariant.Hardmode;
		public override void SetupShimmer(Dictionary<HardmodeVariant, Fishing_Crate_Set<HardmodeVariant>.Set_Crate> items) {
			Sets.ShimmerTransformToItem[items[HardmodeVariant.Hardmode].Type] = items[HardmodeVariant.Normal].Type;
		}
	}
	[Impl_HardmodeVariant]
	public enum HardmodeVariant {
		Normal,
		Hardmode
	}
}

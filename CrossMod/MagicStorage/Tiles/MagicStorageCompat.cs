using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Origins.Items.Materials;
using MagicStorageItems = MagicStorage.Items;
using MagicStorage.CrossMod.Storage;
using MagicStorage.Items;
using MagicStorage.Components;
using StorageUnit = MagicStorage.Components.StorageUnit;

namespace Origins.CrossMod.MagicStorage.Tiles {
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Defiled_Storage_Tier() : OriginsStorageTier<Defiled_Bar, UpgradeHellstone>(1) {
		public override StorageUnitTier UpgradesFrom => Basic;
		public override StorageUnitTier UpgradesTo => Hellstone;
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Encrusted_Storage_Tier() : OriginsStorageTier<Encrusted_Bar, UpgradeHellstone>(1) {
		public override StorageUnitTier UpgradesFrom => Basic;
		public override StorageUnitTier UpgradesTo => Hellstone;
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Sanguinite_Storage_Tier() : OriginsStorageTier<Sanguinite_Bar, UpgradeHellstone>(1) {
		public override StorageUnitTier UpgradesFrom => Basic;
		public override StorageUnitTier UpgradesTo => Hellstone;
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public abstract class OriginsStorageTier<TMaterial, TNextUpgradeItem>(int tier) : OriginsStorageTier(tier) where TMaterial : ModItem where TNextUpgradeItem : ModItem {
		public override int MaterialItem => ModContent.ItemType<TMaterial>();
		public override int NextUpgradeItem => ModContent.ItemType<TNextUpgradeItem>();
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class MagicStorageValidityCheckerFixer : ILoadable {
		public void Load(Mod mod) {
			MonoModHooks.Add(typeof(TEStorageUnit).GetMethod(nameof(TEStorageUnit.ValidTile)), (orig_ValidTile orig, TEStorageUnit self, in Tile tile) => {
				if (tile.TileFrameX % 36 == 0 && tile.TileFrameY % 36 == 0) return TileLoader.GetTile(tile.TileType) is StorageUnit || orig(self, tile);
				return false;
			});
		}
		delegate bool orig_ValidTile(TEStorageUnit self, in Tile tile);
		delegate bool hook_ValidTile(orig_ValidTile orig, TEStorageUnit self, in Tile tile);
		public void Unload() {}
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public abstract class OriginsStorageTier(int tier) : StorageUnitTier {
		internal int upgradeItemType;
		internal int coreItemType;
		internal int storageUnitItemType;
		internal int storageUnitTileType;
		public abstract int MaterialItem { get; }
		public abstract int NextUpgradeItem { get; }
		public abstract StorageUnitTier UpgradesFrom { get; }
		public abstract StorageUnitTier UpgradesTo { get; }
		public virtual int ItemRarity => ItemRarityID.Blue;
		public override int UpgradeItemType => upgradeItemType;
		public override int CoreItemType => coreItemType;
		public override int Capacity => 40 * (tier + 1);
		public override int StorageUnitItemType => storageUnitItemType;
		public override int StorageUnitTileType => storageUnitTileType;
		public string BaseName => Name[..^"_Tier".Length];
		public override void Load() {
			base.Load();
			Mod.AddContent(new OriginsStorageUnit(this));
			Mod.AddContent(new OriginsStorageUpgrade(this));
			Mod.AddContent(new OriginsStorageCore(this));
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			if (UpgradesFrom != null) {
				SetUpgradeableFrom(UpgradesFrom);
			}
			if (UpgradesTo != null) {
				SetUpgradeableTo(UpgradesTo);
			}
		}
		public override void Frame(StorageUnitFullness fullness, bool active, out int frameX, out int frameY) {
			frameX = (int)fullness * 36;
			if (!active) {
				frameX += 108;
			}
			frameY = 0;
		}
		public override void GetState(int frameX, int frameY, out StorageUnitFullness fullness, out bool active) {
			fullness = (StorageUnitFullness)(frameX / 36 % 3);
			active = frameX < 108;
		}
		public override bool IsValidTile(int frameX, int frameY) => true;
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class OriginsStorageUpgrade(OriginsStorageTier tier) : BaseStorageUpgradeItem {
		public override StorageUnitTier Tier => tier;
		public override string Name => tier.BaseName + "_Upgrade";
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			tier.upgradeItemType = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = tier.ItemRarity;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(tier.MaterialItem, 10)
			.AddIngredient(ItemID.Amethyst)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class OriginsStorageUnit(OriginsStorageTier tier) : StorageUnit {
		public override string Name => Tier.BaseName + "_Unit";
		public override void Load() {
			base.Load();
			Mod.AddContent(new OriginsStorageUnitItem(this));
		}
		public OriginsStorageTier Tier { get; } = tier;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Tier.storageUnitTileType = Type;
		}
		public override int ItemType(int frameX, int frameY) => Tier.storageUnitItemType;
	}

	[ExtendsFromMod(nameof(MagicStorage))]
	public class OriginsStorageUnitItem(OriginsStorageUnit unit) : BaseStorageUnitItem {
		public override string Name => Unit.Name + "_Item";
		public OriginsStorageUnit Unit { get; } = unit;
		public override StorageUnitTier Tier => Unit.Tier;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Unit.Tier.storageUnitItemType = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = Unit.Tier.ItemRarity;
		}
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class OriginsStorageCore(OriginsStorageTier tier) : BaseStorageCore {
		public override string Name => tier.BaseName + "_Core";
		public override StorageUnitTier Tier => tier;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			tier.coreItemType = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = tier.ItemRarity;
		}
	}
}

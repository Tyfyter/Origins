using MonoMod.Utils;
using Origins.Items.Materials;
using PegasusLib.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Tools.Wiring; 
public class Screwdriver : AshenWireTool {
	protected override Upgrades Parts => Upgrades.Screwdriver;
	public override int Rarity => ItemRarityID.Blue;
}
public class Screwdriver_White : AshenWireTool {
	protected override Upgrades Parts => Upgrades.Screwdriver | Upgrades.White;
	public override int Rarity => ItemRarityID.Yellow;
}
public class Screwdriver_Logic : AshenWireTool {
	protected override Upgrades Parts => Upgrades.Screwdriver | Upgrades.Logic;
	public override int Rarity => ItemRarityID.Yellow;
}
public class Screwdriver_Logic_White : AshenWireTool {
	protected override Upgrades Parts => Upgrades.Screwdriver | Upgrades.White | Upgrades.Logic;
	public override int Rarity => ItemRarityID.Yellow;
}
public class Ashen_Grand_Design : AshenWireTool {
	protected override Upgrades Parts => Upgrades.GrandDesign | Upgrades.Screwdriver;
	public override int Rarity => ItemRarityID.Yellow;
	public override LocalizedText DisplayName => Mod.GetLocalization($"Items.{nameof(Ashen_Grand_Design)}.DisplayName");
	public override LocalizedText BaseTooltip => Language.GetOrRegister($"Mods.Origins.Items.{nameof(Ashen_Grand_Design)}.Tooltip");
	public override void SetStaticDefaults() {
		base.SetStaticDefaults();
		Item.staff[Type] = false;
	}
}
[LegacyName("Ashen_Grand_Design_White")]
public class Ashen_Grand_Design_White_Logic : Ashen_Grand_Design {
	protected override Upgrades Parts => Upgrades.GrandDesign | Upgrades.Screwdriver | Upgrades.White | Upgrades.Logic;
	public override int Rarity => ItemRarityID.Yellow;
	public override void AddRecipes() => AddAllRecipes();
}
public abstract class AshenWireTool : WireTool {
	protected abstract Upgrades Parts { get; }
	[Flags]
	protected enum Upgrades {
		GrandDesign = 1 << 0,
		Screwdriver = 1 << 1,
		White = 1 << 2,
		Logic = 1 << 3
	}
	public override IEnumerable<WireMode> Modes => [
		..GetIf(Upgrades.GrandDesign, WireMode.Sets.NormalWires),
		..GetIf(Upgrades.Screwdriver, OriginsSets.WireModes.AshenWires),
		..GetIf(Upgrades.White, OriginsSets.WireModes.GreaterAshenWires),
		..GetIf(Upgrades.Logic, OriginsSets.WireModes.LogicUpgrade)
	];
	protected IEnumerable<WireMode> GetIf(Upgrades condition, BitArray set) => Parts.HasFlag(condition) ? WireModeLoader.GetSorted(set) : [];
	protected IEnumerable<WireMode> GetIf(Upgrades condition, bool[] set) => Parts.HasFlag(condition) ? WireModeLoader.GetSorted(set) : [];
	protected IEnumerable<T> GetIf<T>(Upgrades condition, T value) => Parts.HasFlag(condition) ? [value] : [];
	public override LocalizedText DisplayName => Mod.GetLocalization($"Items.{nameof(Screwdriver)}.DisplayName");
	public virtual LocalizedText BaseTooltip => Language.GetOrRegister($"Mods.Origins.Items.{nameof(Screwdriver)}.Tooltip");
	public override LocalizedText Tooltip => OriginExtensions.CombineTooltips([
			BaseTooltip,
			..GetIf(Upgrades.White, Language.GetOrRegister($"Mods.Origins.Items.{nameof(Screwdriver_Upgrade_White)}.UpgradeTooltip")),
			..GetIf(Upgrades.Logic, Language.GetOrRegister($"Mods.Origins.Items.{nameof(Screwdriver_Upgrade_Logic)}.UpgradeTooltip"))
		]);
	public override void SetStaticDefaults() {
		Item.staff[Type] = true;
	}
	public override void UpdateInventory(Player player) {
		OriginPlayer originPlayer = player.OriginPlayer();
		if (Parts.HasFlag(Upgrades.GrandDesign)) {
			player.InfoAccMechShowWires = true;
			player.rulerLine = true;
			player.rulerGrid = true;
		}
		originPlayer.InfoAccMechShowAshenWires = true;
		if (Parts.HasFlag(Upgrades.Logic)) originPlayer.InfoAccMechModifyComponents = true;
	}
	protected static void AddAllRecipes() {
		Dictionary<Upgrades, int> items = new() {
			[Upgrades.GrandDesign] = ItemID.WireKite,
			[Upgrades.White] = ModContent.ItemType<Screwdriver_Upgrade_White>(),
			[Upgrades.Logic] = ModContent.ItemType<Screwdriver_Upgrade_Logic>()
		};
		foreach (AshenWireTool tool in ModContent.GetContent<AshenWireTool>()) items.Add(tool.Parts, tool.Type);
		ContentExtensions.CreateCombinationRecipes(items);
	}
}
public abstract class WireTool : ModItem, IWireTool {
	public abstract IEnumerable<WireMode> Modes { get; }
	public abstract int Rarity { get; }
	public override void SetDefaults() {
		Item.CloneDefaults(ItemID.WireKite);
		Item.shoot = ModContent.ProjectileType<ModWireChannel>();
		Item.channel = true;
		Item.useTurn = true;
		Item.rare = Rarity;
	}
	public override void UpdateInventory(Player player) {
		player.OriginPlayer().InfoAccMechShowAshenWires = true;
	}
	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		if (player.ownedProjectileCounts[type] > 0) return false;
		Projectile.NewProjectile(
			source,
			player.Center,
			default,
			type,
			0,
			0,
			-1,
			Player.tileTargetX,
			Player.tileTargetY
		);
		return false;
	}
}

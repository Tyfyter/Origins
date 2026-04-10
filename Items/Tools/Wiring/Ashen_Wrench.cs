using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Reflection;
using PegasusLib;
using PegasusLib.Content;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Tools.Wiring {
	public class Screwdriver : WireTool {
		public override IEnumerable<WireMode> Modes => WireModeLoader.GetSorted(OriginsSets.WireModes.AshenWires);
		public override int Rarity => ItemRarityID.Blue;
		public override void SetStaticDefaults() {
			Item.staff[Type] = true;
		}
	}
	public class Screwdriver_White : Screwdriver {
		public override IEnumerable<WireMode> Modes => base.Modes.Concat(WireModeLoader.GetSorted(OriginsSets.WireModes.GreaterAshenWires));
		public override int Rarity => ItemRarityID.Yellow;
		public override LocalizedText DisplayName => Mod.GetLocalization($"Items.{nameof(Screwdriver)}.DisplayName");
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Screwdriver)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Screwdriver_Upgrade_White)}.UpgradeTooltip")
			);
		public override void SetStaticDefaults() {
			Item.staff[Type] = true;
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Screwdriver>()
			.AddIngredient<Screwdriver_Upgrade_White>()
			.Register();
	}
	public class Ashen_Grand_Design : WireTool {
		public override IEnumerable<WireMode> Modes => [
			..WireModeLoader.GetSorted(WireMode.Sets.NormalWires),
			..WireModeLoader.GetSorted(OriginsSets.WireModes.AshenWires)
		];
		public override int Rarity => ItemRarityID.Orange;
		public override void UpdateInventory(Player player) {
			player.InfoAccMechShowWires = true;
			player.rulerLine = true;
			player.rulerGrid = true;
			player.OriginPlayer().InfoAccMechShowAshenWires = true;
			player.OriginPlayer().InfoAccMechModifyComponents = true;
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient(ItemID.WireKite)
			.AddIngredient<Screwdriver>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
	}
	public class Ashen_Grand_Design_White : Ashen_Grand_Design {
		public override IEnumerable<WireMode> Modes => [
			..WireModeLoader.GetSorted(WireMode.Sets.NormalWires),
			..WireModeLoader.GetSorted(OriginsSets.WireModes.AshenWires),
			..WireModeLoader.GetSorted(OriginsSets.WireModes.GreaterAshenWires),
			..WireModeLoader.GetSorted(OriginsSets.WireModes.LogicUpgrade)
		];
		public override int Rarity => ItemRarityID.Yellow;
		public override LocalizedText DisplayName => Mod.GetLocalization($"Items.{nameof(Ashen_Grand_Design)}.DisplayName");
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Ashen_Grand_Design)}.Tooltip"),
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Screwdriver_Upgrade_White)}.UpgradeTooltip")
			);
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Ashen_Grand_Design>()
			.AddIngredient<Screwdriver_Upgrade_White>()
			.Register();
			CreateRecipe()
			.AddIngredient(ItemID.WireKite)
			.AddIngredient<Screwdriver_White>()
			.Register();
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
}

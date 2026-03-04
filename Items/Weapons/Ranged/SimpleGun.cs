using Origins.Items.Weapons.Ammo;
using Origins.Tiles.Ashen;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged;
public class Grease_Gun : SimpleGun {
	public override int ScrapAmount => 25;
	public override int BarrelYOffset => 4;
	public override void OnSetDefaults() {
		Item.damage = 11;
		Item.knockBack = 1;
		Item.useTime = Item.useAnimation = 16;
		spread = 3;
		Item.autoReuse = true;
	}
	public override Vector2? HoldoutOffset() => new Vector2(-8, -2);
}
public class Tactical_SMG : SimpleGun {
	public override int ScrapAmount => 20;
	public override int BarrelYOffset => 5;
	public override void OnSetDefaults() {
		Item.damage = 8;
		Item.knockBack = 1;
		Item.shootSpeed = 5;
		Item.useTime = Item.useAnimation = 12;
		spread = 1;
		Item.autoReuse = true;
	}
	public override Vector2? HoldoutOffset() => new Vector2(-8, -2);
}
public class DMR : SimpleGun {
	public override int ScrapAmount => 35;
	public override int BarrelYOffset => 8;
	public override void OnSetDefaults() {
		Item.damage = 50;
		Item.knockBack = 4;
		Item.crit = 4;
		Item.shootSpeed = 10;
		Item.useTime = Item.useAnimation = 60;
		spread = 0.5f;
	}
	public override Vector2? HoldoutOffset() => new Vector2(-16, -4);
	public override void HoldItem(Player player) => player.scope = true;
}
[ReinitializeDuringResizeArrays]
public abstract class SimpleGun : ModItem {
	public static float[] SpreadMultipliers { get; }
	static SimpleGun() {
		SpreadMultipliers = PrefixID.Sets.Factory.CreateNamedSet(nameof(SpreadMultipliers)).RegisterFloatSet(1,
			PrefixID.Staunch, 0.75f,
			PrefixID.Unreal, 0.75f,
			PrefixID.Sighted, 0.5f,
			PrefixID.Hasty, 1.15f,
			PrefixID.Awkward, 1.15f,
			PrefixID.Frenzying, 1.25f,
			PrefixID.Powerful, 1.25f,
			PrefixID.Awful, 1.5f
		);
		if (ModLoader.TryGetMod("CritRework", out Mod critRework) && critRework.TryFind("Quality", out ModPrefix quality)) {
			SpreadMultipliers[quality.Type] = 0.6f;
		}
	}
	public abstract int ScrapAmount { get; }
	public virtual int BarrelYOffset { get; }
	public virtual void OnSetStaticDefaults() { }
	public virtual void OnSetDefaults() { }
	public virtual void OnApplyPrefix(int pre) { }
	public virtual void OnModifyTooltips(List<TooltipLine> tooltips) { }
	public virtual void OnModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }
	public virtual void OnAddRecipes() { }
	public float spread;
	public float defaultSpread;
	public sealed override void SetStaticDefaults() {
		ItemID.Sets.ExtractinatorMode[Type] = Type;
		ItemID.Sets.SkipsInitialUseSound[Type] = true;
		ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		OnSetStaticDefaults();
	}
	public sealed override void SetDefaults() {
		Item.DefaultToRangedWeapon(ProjectileID.Bullet, AmmoID.Bullet, 55, 7f);
		Item.UseSound = SoundID.Item11;
		OnSetDefaults();
		defaultSpread = spread;
	}
	public sealed override void ApplyPrefix(int pre) {
		spread *= SpreadMultipliers[pre];
		OnApplyPrefix(pre);
	}
	public sealed override void ModifyTooltips(List<TooltipLine> tooltips) {
		if (defaultSpread != 0) {
			float spreadDiff = (SpreadMultipliers[Item.prefix] - 1) * 100;
			if (spreadDiff != 0) {
				for (int i = tooltips.Count - 1; i >= 0; i--) {
					if (tooltips[i].Name.StartsWith("Prefix")) {
						tooltips.Insert(i + 1, new(Mod, "PrefixSpread", Language.GetTextValue("Mods.Origins.Tooltips.Modifiers.SpreadPercent", spreadDiff)) {
							IsModifier = true,
							IsModifierBad = spreadDiff > 0
						});
						break;
					}
				}
			}
		}
		OnModifyTooltips(tooltips);
	}
	public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		position += velocity.Normalized(out _).Perpendicular(player.direction) * BarrelYOffset;
		velocity += Main.rand.NextVector2Circular(spread, spread);
		OnModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
	}
	public sealed override void AddRecipes() {
		CreateRecipe()
		.AddIngredient<Scrap>(ScrapAmount)
		.AddIngredient(ItemID.IllegalGunParts)
		.AddTile<Metal_Presser>()
		.Register();
		OnAddRecipes();
	}
	public override bool AltFunctionUse(Player player) {
		if (OriginPlayer.LocalOriginPlayer?.isUsingScope ?? false) return false;
		Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
		return tile.TileType == TileID.Extractinator || tile.TileType == TileID.ChlorophyteExtractinator;
	}
	public override bool? UseItem(Player player) {
		if (player.altFunctionUse == 2) return true;
		SoundEngine.PlaySound(Item.UseSound, player.MountedCenter);
		return base.UseItem(player);
	}
	public override bool CanShoot(Player player) {
		if (player.altFunctionUse == 2) {
			Item.useStyle = ItemUseStyleID.Swing;
			player.altFunctionUse = 2;
			player.controlUseItem = true;
			int useTime = Item.useTime;
			int useAnimation = Item.useAnimation;
			try {
				Item.useTime = 10;
				Item.useAnimation = 15;
				Player.ItemCheckContext context = default;
				player.PlaceThing(ref context);
			} finally {
				Item.useTime = useTime;
				Item.useAnimation = useAnimation;
			}
			if (ItemLoader.ConsumeItem(Item, player) && --Item.stack <= 0) Item.TurnToAir();
			player.itemAnimation = player.itemTime;
			player.itemAnimationMax = player.itemTimeMax;
			return false;
		}
		Item.useStyle = ItemUseStyleID.Shoot;
		return base.CanShoot(player);
	}
	public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) {
		if (Main.rand.NextBool(10)) {
			resultType = ItemID.IllegalGunParts;
			resultStack = 1;
		} else {
			resultType = ModContent.ItemType<Scrap>();
			resultStack = Main.rand.Next(ScrapAmount - (ScrapAmount * 3) / 20, ScrapAmount + 1);
		}
	}
	public override void HoldItem(Player player) {
		if (Item.prefix == PrefixID.Sighted) player.scope = true;
	}
}

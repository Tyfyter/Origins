using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ShootAction = System.Action<Terraria.Player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float>;

namespace Origins.Items.Weapons.Demolitionist {
	[ReinitializeDuringResizeArrays]
	public class Hand_Grenade_Launcher : ModItem {
		public static ShootAction[] AltFireAction = ProjectileID.Sets.Factory.CreateNamedSet($"{nameof(Hand_Grenade_Launcher)}_{nameof(AltFireAction)}")
		.RegisterCustomSet<ShootAction>(null);
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToLauncher(16, 50, 44, 18);
			Item.shoot = ProjectileID.Grenade;
			Item.useAmmo = ItemID.Grenade;
			Item.shootSpeed = 5f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool? CanChooseAmmo(Item ammo, Player player) {
			if (player.altFunctionUse == 2 && AltFireAction[ammo.shoot] is null) return false;
			return base.CanChooseAmmo(ammo, player);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2 && AltFireAction[type] is ShootAction shootAction) {
				shootAction(player, source, position, velocity, type, damage, knockback);
				return false;
			}
			return true;
		}
		class Hand_Grenade_Launcher_Tooltip : GlobalItem {
			public override bool AppliesToEntity(Item entity, bool lateInstantiation) => lateInstantiation && entity.ammo == ItemID.Grenade;
			public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
				if (Main.LocalPlayer?.HeldItem?.ModItem is not Hand_Grenade_Launcher || AltFireAction[item.shoot] is null) return;
				void InsertTooltip(ref int i) {
					tooltips.Insert(i + 1, new(Mod, "CanAltFireTooltip", Language.GetTextValue("Mods.Origins.Items.Hand_Grenade_Launcher.CanAltFireTooltip")));
					i = 0;
				}
				for (int i = tooltips.Count - 1; i >= 0; i--) {
					switch (tooltips[i].Name) {
						case "Ammo":
						case "Consumable":
						case "Material":
						InsertTooltip(ref i);
						break;
						default:
						if (tooltips[i].Name.StartsWith("Tooltip")) {
							InsertTooltip(ref i);
							break;
						}
						if (i < tooltips.Count - 1 && !tooltips[i].Name.StartsWith("Prefix") && tooltips[i + 1].Name.StartsWith("Prefix")) {
							i--;
							InsertTooltip(ref i);
							break;
						}
						break;
					}
				}
			}
		}
	}
}

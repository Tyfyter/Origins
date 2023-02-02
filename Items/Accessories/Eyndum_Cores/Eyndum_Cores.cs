using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories.Eyndum_Cores {
	public abstract class Eyndum_Core : ModItem {
		public abstract Color CoreGlowColor { get; }
		public override bool CanRightClick() {
			if (Terraria.GameInput.PlayerInput.Triggers.Old.MouseRight) {
				return false;
			}
			Item equippedItem = Main.LocalPlayer.GetModPlayer<OriginPlayer>().eyndumCore.Value;
			if (equippedItem.type != Item.type) {
				int e = equippedItem.type;
				int t = Item.type;
				equippedItem.SetDefaults(t);
				Item.SetDefaults(e);
				SoundEngine.PlaySound(SoundID.Grab);
			}
			return false;
		}
	}
	public class Agility_Core : Eyndum_Core {
		public override Color CoreGlowColor => new Color(255, 220, 0, 160);
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Agility Core");
			Tooltip.SetDefault("Grants massive boosts in maneuverability");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.buyPrice(platinum: 5);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.wingTimeMax *= 2;
			player.moveSpeed *= 4f;
			player.runAcceleration *= 3f;
			player.maxRunSpeed *= 3f;
			player.jumpSpeedBoost += 5;
		}
	}
	public class Combat_Core : Eyndum_Core {
		public override Color CoreGlowColor => new Color(160, 0, 255, 160);
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Combat Core");
			Tooltip.SetDefault("Grants impressive boosts in combat capability");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.buyPrice(platinum: 5);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) *= 1.24f;
			player.GetCritChance(DamageClass.Generic) += 1.1f;
			player.GetKnockback(DamageClass.Generic) += 2;
		}
	}
	public class Construction_Core : Eyndum_Core {
		public override Color CoreGlowColor => new Color(255, 160, 0, 160);
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Construction Core");
			Tooltip.SetDefault("Allows incredible construction abilities");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.buyPrice(platinum: 5);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.tileSpeed *= 2f;
			player.wallSpeed *= 2.5f;
			player.pickSpeed *= 3f;
			Player.tileRangeX += 30;
			Player.tileRangeY += 30;
			player.blockRange += 40;
		}
	}
	public class Fishing_Core : Eyndum_Core {
		public override Color CoreGlowColor => new Color(255, 0, 160, 75);
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fishing Core");
			Tooltip.SetDefault("Facilitates flawless fishing");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.buyPrice(platinum: 5);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.fishingSkill += 100;
			player.accFishingLine = true;
			player.accLavaFishing = true;
		}
	}
	public class Lifeforce_Core : Eyndum_Core {
		public override Color CoreGlowColor => new Color(255, 0, 75, 160);
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Vitality Core");
			Tooltip.SetDefault("Provides extreme boosts in health\n'Get a life'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.buyPrice(platinum: 5);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.statLifeMax2 += player.statLifeMax2 / 2;
			player.lifeRegenCount += player.statLifeMax2 / 22;
		}
	}
	public class Magic_Core : Eyndum_Core {
		public override Color CoreGlowColor => new Color(255, 100, 0, 160);
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Magic Core");
			Tooltip.SetDefault("Graces one with true magic power");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.buyPrice(platinum: 5);
			Item.rare = CrimsonRarity.ID;
		}
		public override void UpdateEquip(Player player) {
			player.manaCost *= 0.8f;
			player.manaFlower = true;
			player.manaSickReduction *= 0.5f;//this is the damage reduction from mana sickness, it doesn't reduce it
		}
	}
}

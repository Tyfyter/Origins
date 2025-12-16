using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public class CursedRarity : ModRarity {
		public static string RarityName => "Cursed";
		public static int ID { get; private set; }
		public static Color Color => new Color(0.65f, 0f, 0.65f, Main.mouseTextColor / 255f).MultiplyRGBA(Main.MouseTextColorReal);
		public override Color RarityColor => Color;//new Color(136, 22, 156);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			return ID;
		}
	}
	public class AltCyanRarity : ModRarity {
		public static string RarityName => "Not Quite Cyan";
		public static int ID { get; private set; }
		public override Color RarityColor => new(43, 145, 255);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			if (offset == 0) {
				if (valueMult >= 1.2) {
					offset += 2;
				} else if (valueMult >= 1.05) {
					offset++;
				} else if (valueMult <= 0.8) {
					offset -= 2;
				} else if (valueMult <= 0.95) {
					offset--;
				}
			}
			return offset == 0 ? Type : ItemRarityID.Cyan + offset;
		}
	}
	//interesting ship
	public class ButterscotchRarity : ModRarity {
		public static string RarityName => "Butterscotch";
		public static int ID { get; private set; }
		public override Color RarityColor => new(226, 182, 65);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			switch (offset) {
				case 0:
				return ID;
				case 1:
				return CrimsonRarity.ID;
				case 2:
				return RoyalBlueRarity.ID;
				default:
				return ItemRarityID.Count + offset;
			}
		}
	}
	public class CrimsonRarity : ModRarity {
		public static string RarityName => "Crimson";
		public static int ID { get; private set; }
		public override Color RarityColor => new(164, 29, 7);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			switch (offset) {
				case -1:
				return ButterscotchRarity.ID;
				case 0:
				return ID;
				case 1:
				return RoyalBlueRarity.ID;
				case 2:
				return RoyalBlueRarity.ID;
				default:
				return ItemRarityID.Count + offset;
			}
		}
	}
	public class RoyalBlueRarity : ModRarity {
		public static string RarityName => "Royal Blue";
		public static int ID { get; private set; }
		public override Color RarityColor => new(12, 42, 165);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			switch (offset) {
				case -2:
				return ButterscotchRarity.ID;
				case -1:
				return CrimsonRarity.ID;
				case 0:
				return ID;
				default:
				return ItemRarityID.Count + offset;
			}
		}
	}
}

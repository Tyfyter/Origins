using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public class Cursed : ModRarity {
		public static int ID { get; private set; }
		public override Color RarityColor => new Color(136, 22, 156);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			switch (offset) {
				case 0:
					return ID;
				default:
					return ItemRarityID.Count + offset;
			}
		}
	}
		public class AltCyanRarity : ModRarity {
		public static int ID { get; private set; }
		public override Color RarityColor => new Color(43, 145, 255);
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
	public class Butterscotch : ModRarity {
		public static int ID { get; private set; }
		public override Color RarityColor => new Color(226, 182, 65);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			switch (offset) {
				case 0:
					return ID;
				case 1:
					return Crimson.ID;
				case 2:
					return RoyalBlue.ID;
				default:
					return ItemRarityID.Count + offset;
			}
		}
	}
	public class Crimson : ModRarity {
		public static int ID { get; private set; }
		public override Color RarityColor => new Color(164, 29, 7);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			switch (offset) {
				case 0:
					return ID;
				case 1:
					return RoyalBlue.ID;
				case 2:
					return RoyalBlue.ID;
				default:
					return ItemRarityID.Count + offset;
			}
		}
	}
	public class RoyalBlue : ModRarity {
		public static int ID { get; private set; }
		public override Color RarityColor => new Color(12, 42, 165);
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override int GetPrefixedRarity(int offset, float valueMult) {
			switch (offset) {
				case 0:
					return ID;
				default:
					return ItemRarityID.Count + offset;
			}
		}
	}
}

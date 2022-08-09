using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
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
}

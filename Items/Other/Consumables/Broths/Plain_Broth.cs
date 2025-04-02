using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using PegasusLib;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Plain_Broth : BrothBase {
		public static float Size => 64;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new(152, 155, 47),
				new(152, 155, 47),
				new(152, 155, 47)
			];
		}
	}
}

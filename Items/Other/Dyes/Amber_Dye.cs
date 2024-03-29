﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Items.Other.Dyes {
	public class Amber_Dye : Dye_Item {
		public static int ID { get; private set; }
		public static int ShaderID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(Main.PixelShaderRef, "ArmorStardust"))
			.UseImage("Images/Misc/noise")
			.UseColor(1.5f, 0.8f, 0.4f)
			.UseSecondaryColor(2.0f, 1.2f, 0.4f)
			.UseSaturation(1f);
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
			Item.ResearchUnlockCount = 3;
		}
	}
}

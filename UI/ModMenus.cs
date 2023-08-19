using Microsoft.Xna.Framework.Graphics;
using Origins.Backgrounds;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.UI {
	public class Riven_Hive_Mod_Menu : ModMenu {
		public override int Music => Origins.Music.Riven;
		public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<Riven_Surface_Background>();
		public override string DisplayName => "Riven Hive";//Language.GetOrRegister(Mod.GetLocalizationKey("ModMenu.Riven_Hive")).Value;
	}
}

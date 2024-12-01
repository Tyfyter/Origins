using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles {
	public class PetWikiProvider : ItemWikiProvider {
		public bool Condition(object obj, out WikiProvider provider, ref bool replaceGenericClassProvider) {
			if (obj is ModItem modItem && modItem.Item.buffType > -1 && (Main.vanityPet[modItem.Item.buffType] || Main.lightPet[modItem.Item.buffType]) && !(obj is ICustomWikiStat customWiki && customWiki.NeedsCustomSprite)) {
				provider = this;
				replaceGenericClassProvider = true;
				return true;
			}
			provider = null;
			return false;
		}
		protected override void Register() {
			ModTypeLookup<WikiProvider>.Register(this);
			WikiPageExporter.ConditionalDataProviders.Add(Condition);
		}
		public override IEnumerable<(string, JObject)> GetStats(ModItem modItem) {
			return base.GetStats(modItem).Select(s => {
				s.Item2.Add("Pet", new JObject {
					["Name"] = Lang.GetProjectileName(modItem.Item.shoot).Value,
					["Image"] = PageName(modItem) + "_Pet",
					["LightPet"] = Main.lightPet[modItem.Item.buffType]
				});
				return s;
			});
		}
		public override IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites(ModItem value) {
			return (value as ICustomPetFrames)?.GetAnimatedSprites?.Select(s => ("Pets/" + PageName(value) + "_Pet" + s.Item1, s.Item2)) ?? [];
		}
	}
	public interface ICustomPetFrames {
		public IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites { get; }
	}
}

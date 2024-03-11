using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Tiles {
	public class TileWikiProvider : ItemWikiProvider {
		public override string PageName(ModItem modItem) => base.PageName(modItem);
		public bool Condition(object obj, out WikiProvider provider, ref bool replaceGenericClassProvider) {
			if (obj is ModItem modItem && modItem.Item.createTile > -1 && !TileID.Sets.Torch[modItem.Item.createTile] && !(obj is ICustomWikiStat customWiki && customWiki.NeedsCustomSprite)) {
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
				s.Item2.Add("Images", new JArray() {
					s.Item2["Image"],
					"Tiles/" + PageName(modItem)
				});
				s.Item2.Remove("Image");
				return s;
			});
		}
		public override IEnumerable<(string, Texture2D)> GetSprites(ModItem modItem) {
			yield return (Path.Combine("Tiles", PageName(modItem)), SpriteGenerator.GenerateTileSprite(modItem.Item.createTile, modItem.Item.placeStyle));
		}
	}
}

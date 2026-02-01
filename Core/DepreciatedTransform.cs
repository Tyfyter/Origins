using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Core {
	//TODO: remove, moved to PegasusLib
	internal class DepreciatedTransform : ILoadable {
		public static Dictionary<string, string> conversions = new() {
			["Origins/Dusk_Stone_Item"] = nameof(ItemID.AshBlock)
		};
		public void Load(Mod mod) {
			try {
				MonoModHooks.Add(((orig_Load)ItemIO.Load).Method, LoadItem);
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(DepreciatedTransform), e)) throw;
			}
		}
		static void LoadItem(orig_Load orig, Item item, TagCompound tag) {
			if (tag.TryGet("mod", out string mod) && tag.TryGet("name", out string name) && conversions.TryGetValue($"{mod}/{name}", out string conversion)) {
				string[] parts = conversion.Split('/');
				switch (parts.Length) {
					case 1:
					tag["mod"] = "Terraria";
					tag["id"] = int.TryParse(conversion, out int id) ? id : ItemID.Search.GetId(conversion);
					break;
					case 2:
					tag["mod"] = parts[0];
					tag["name"] = parts[1];
					break;
				}
			}
			orig(item, tag);
		}
		delegate void hook_Load(orig_Load orig, Item item, TagCompound tag);
		delegate void orig_Load(Item item, TagCompound tag);
		public void Unload() {}
	}
}

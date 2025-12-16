using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Core {
	public interface IPreDrawAnything {
		public void PreDrawAnything();
	}
	public class AprilFoolsAssetSwitcher<T> : IPreDrawAnything where T : class {
		static readonly List<(RefGet<Asset<T>> slot, Asset<T> afOn, Asset<T> afOff)> options = [];
		static bool lastAF = false;
		public static void Add(RefGet<Asset<T>> slot, Asset<T> afOn, Asset<T> afOff = null) => options.Add((slot, afOn, afOff ?? slot()));
		static AprilFoolsAssetSwitcher() => OriginSystem.assetSwitchers.Add(new AprilFoolsAssetSwitcher<T>());
		void IPreDrawAnything.PreDrawAnything() {
			if (!lastAF.TrySet(OriginsModIntegrations.CheckAprilFools())) return;
			for (int i = 0; i < options.Count; i++) {
				(RefGet<Asset<T>> slot, Asset<T> afOn, Asset<T> afOff) = options[i];
				slot() = lastAF ? afOn : afOff;
			}
		}
	}
	/// <summary>
	/// Helper methods
	/// </summary>
	public static class AprilFoolsTextures {
		public static void AddItem(int type, Asset<Texture2D> afOn) => AprilFoolsAssetSwitcher<Texture2D>.Add(() => ref TextureAssets.Item[type], afOn, TextureAssets.Item[type]);
		public static void AddItem(ModItem item) => AddItem(item.Type, ModContent.Request<Texture2D>(item.Texture + "_AF"));
		public static void AddNPC(int type, Asset<Texture2D> afOn) => AprilFoolsAssetSwitcher<Texture2D>.Add(() => ref TextureAssets.Npc[type], afOn, TextureAssets.Npc[type]);
		public static void AddNPC(ModNPC npc) => AddNPC(npc.Type, ModContent.Request<Texture2D>(npc.Texture + "_AF"));
	}
}

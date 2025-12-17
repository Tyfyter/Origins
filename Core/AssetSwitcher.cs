using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Core {
	public interface IPreDrawAnything {
		public void PreDrawAnything();
	}
	public class AprilFoolsAssetSwitcher<TAsset> : IPreDrawAnything {
		static readonly List<(RefGet<TAsset> slot, TAsset afOn, TAsset afOff)> options = [];
		static bool lastAF = false;
		public static void Add(RefGet<TAsset> slot, TAsset afOn) {
			if (Main.dedServ) return;
			options.Add((slot, afOn, slot()));
		}
		static AprilFoolsAssetSwitcher() => OriginSystem.assetSwitchers.Add(new AprilFoolsAssetSwitcher<TAsset>());
		void IPreDrawAnything.PreDrawAnything() {
			if (!lastAF.TrySet(OriginsModIntegrations.CheckAprilFools())) return;
			for (int i = 0; i < options.Count; i++) {
				(RefGet<TAsset> slot, TAsset afOn, TAsset afOff) = options[i];
				slot() = lastAF ? afOn : afOff;
			}
		}
	}
	/// <summary>
	/// Helper methods
	/// </summary>
	public static class AprilFoolsTextures {
		public static void AddItem(int type, Asset<Texture2D> afOn) {
			if (Main.dedServ) return;
			AprilFoolsAssetSwitcher<Asset<Texture2D>>.Add(() => ref TextureAssets.Item[type], afOn);
		}
		public static void AddItem(ModItem item) {
			if (Main.dedServ) return;
			AddItem(item.Type, ModContent.Request<Texture2D>(item.Texture + "_AF"));
		}
		public static void AddNPC(int type, Asset<Texture2D> afOn) {
			if (Main.dedServ) return;
			AprilFoolsAssetSwitcher<Asset<Texture2D>>.Add(() => ref TextureAssets.Npc[type], afOn);
		}
		public static void AddNPC(ModNPC npc) {
			if (Main.dedServ) return;
			AddNPC(npc.Type, ModContent.Request<Texture2D>(npc.Texture + "_AF"));
		}
		public static void AddProjectile(int type, Asset<Texture2D> afOn) {
			if (Main.dedServ) return;
			AprilFoolsAssetSwitcher<Asset<Texture2D>>.Add(() => ref TextureAssets.Projectile[type], afOn);
		}
		public static void AddProjectile(ModProjectile projectile) {
			if (Main.dedServ) return;
			AddProjectile(projectile.Type, ModContent.Request<Texture2D>(projectile.Texture + "_AF"));
		}
		public static void AddGore(int type, Asset<Texture2D> afOn) {
			if (Main.dedServ) return;
			AprilFoolsAssetSwitcher<Asset<Texture2D>>.Add(() => ref TextureAssets.Gore[type], afOn);
		}
		public static void AddGore(ModGore gore) {
			if (Main.dedServ) return;
			AddGore(gore.Type, ModContent.Request<Texture2D>(gore.Texture + "_AF"));
		}
		public static void AddTile(int type, Asset<Texture2D> afOn) {
			if (Main.dedServ) return;
			AprilFoolsAssetSwitcher<Asset<Texture2D>>.Add(() => ref TextureAssets.Tile[type], afOn);
		}
		public static void AddTile(ModTile tile) {
			if (Main.dedServ) return;
			AddTile(tile.Type, ModContent.Request<Texture2D>(tile.Texture + "_AF"));
		}
		public static void Create(RefGet<Asset<Texture2D>> get, string path, string afPath = null) {
			if (Main.dedServ) return;
			afPath ??= path + "_AF";
			get() = ModContent.Request<Texture2D>(path);
			AprilFoolsAssetSwitcher<Asset<Texture2D>>.Add(get, ModContent.Request<Texture2D>(afPath));
		}
		public static void CreateAFTexture(this ModTexturedType texturedType, RefGet<Asset<Texture2D>> get) {
			Create(get, texturedType.Texture);
		}
	}
}

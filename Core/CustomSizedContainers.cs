using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Core {
	//TODO: remove, moved to PegasusLib
	internal class CustomSizedContainers : ILoadable {
		public void Load(Mod mod) {
			try {
				IL_Player.IsInInteractionRangeToMultiTileHitbox += il => {
					int tile = -1;
					int hitbox = -1;
					new ILCursor(il)
					.GotoNext(MoveType.AfterLabel,
						i => i.MatchLdsfld(typeof(TileID.Sets), nameof(TileID.Sets.BasicDresser)),
						i => i.MatchLdloca(out tile),
						i => i.MatchCall<Tile>("get_type"),
						i => i.MatchLdindU2(),
						i => i.MatchLdelemU1(),
						i => i.MatchBrfalse(out _),

						i => i.MatchLdloca(out hitbox)
					)
					.EmitLdarg1()
					.EmitLdarg2()
					.EmitLdloca(tile)
					.EmitLdloca(hitbox)
					.EmitDelegate((int chestPointX, int chestPointY, ref Tile tile, ref Rectangle hitbox) => {
						if (TileLoader.GetTile(tile.TileType) is ICustomSizeContainer container) {
							hitbox = new(chestPointX * 16, chestPointY * 16, container.Width * 16, container.Height * 16);
						}
					});
				};
			} catch (Exception e) {
				if (Origins.LogLoadingILError($"{nameof(CustomSizedContainers)}_FixInteractionRange", e)) throw;
			}
			try {
				IL_ChestUI.DrawName += il => new ILCursor(il)
				.GotoNext(i => i.MatchCall(typeof(TileLoader), nameof(TileLoader.DefaultContainerName)))
				.GotoPrev(i => i.MatchLdsfld(typeof(TileID.Sets), nameof(TileID.Sets.BasicChest)))
				.GotoNext(MoveType.After, 
					i => i.MatchCall<Tile>("get_type"),
					i => i.MatchLdindU2()
				)
				.EmitDelegate<Func<int, int>>(tileType => TileLoader.GetTile(tileType) is ICustomSizeContainer ? TileID.Containers : tileType);
			} catch (Exception e) {
				if (Origins.LogLoadingILError($"{nameof(CustomSizedContainers)}_FixDefaultName", e)) throw;
			}
		}
		public void Unload() { }
	}
	public interface ICustomSizeContainer {
		int Width { get; }
		int Height { get; }
	}
}

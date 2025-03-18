using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria;
using MonoMod.Cil;
using PegasusLib.Reflection;
using Terraria.ID;
using ReLogic.Content;

namespace Origins.Questing {
	public class Quest_Interaction_Hud : GlobalNPC, ITicker {
		static bool[] hasPopup = [];
		public static bool[] NPCTypeHasPopup {
			get {
				if (hasPopup.Length != NPCLoader.NPCCount) Array.Resize(ref hasPopup, NPCLoader.NPCCount);
				return hasPopup;
			}
		}
		public override void Load() {
			Origins.tickers.Add(this);
			On_Main.DrawNPCHeadFriendly += On_Main_DrawNPCHeadFriendly;
			IL_Main.DrawNPCHousesInUI += IL_Main_DrawNPCHousesInUI;
		}
		public override void Unload() {
			hasPopup = null;
		}
		void ITicker.Tick() {
			if (OriginClientConfig.Instance.QuestNotificationPosition == QuestNotificationPositions.None) return;
			_ = NPCTypeHasPopup;
			Array.Clear(hasPopup);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (hasPopup[npc.type]) continue;
				for (int i = 0; i < Quest_Registry.Quests.Count; i++) {
					Quest quest = Quest_Registry.Quests[i];
					if (quest.CanStart(npc) || (!quest.Completed && quest.CanComplete(npc))) {
						hasPopup[npc.type] = true;
						break;
					}
				}
			}
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (!OriginClientConfig.Instance.QuestNotificationPosition.HasFlag(QuestNotificationPositions.World)) return;
			if (!npc.IsABestiaryIconDummy && NPCTypeHasPopup[npc.type]) {
				Main.spriteBatch.Draw(
					TextureAssets.QuicksIcon.Value,
					npc.Top - (screenPos + Vector2.UnitY * 4),
					null,
					Main.MouseTextColorReal,
					0,
					TextureAssets.QuicksIcon.Size() * new Vector2(0.5f, 1f),
					(Main.mouseTextColor / 255f) * Main.UIScale,
					SpriteEffects.None,
				0);
			}
		}

		static void On_Main_DrawNPCHeadFriendly(On_Main.orig_DrawNPCHeadFriendly orig, Entity theNPC, byte alpha, float headScale, SpriteEffects dir, int townHeadId, float x, float y) {
			orig(theNPC, alpha, headScale, dir, townHeadId, x, y);
			if (!OriginClientConfig.Instance.QuestNotificationPosition.HasFlag(QuestNotificationPositions.Map)) return;
			if (theNPC is NPC npc && NPCTypeHasPopup[npc.type]) {
				Main.spriteBatch.Draw(
					TextureAssets.QuicksIcon.Value,
					new Vector2(x, y - ((TextureAssets.NpcHead[townHeadId].Height() * 0.5f + 6) * headScale)),
					null,
					Main.MouseTextColorReal,
					0,
					TextureAssets.QuicksIcon.Size() * new Vector2(0.5f, 1f),
					headScale * 1.15f * Main.mouseTextColor / 255f,
					SpriteEffects.None,
				0);
			}
		}
		static void IL_Main_DrawNPCHousesInUI(ILContext il) {
			ILCursor c = new(il);
			while (c.TryGotoNext(MoveType.After, i => i.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.Draw)))) {
				ILCursor check = new(c);
				check.Index--;
				while (!check.Prev.MatchLdsfld<Main>(nameof(Main.spriteBatch))) {
					MonoModMethods.SkipPrevArgument(check);
				}
				if (check.Next.MatchLdsfld(typeof(TextureAssets), nameof(TextureAssets.NpcHead))) {
					int targetIndex = c.Index - 1;
					if (!check.Next.Next.MatchLdloc(out int loc)) continue;

					while (!check.Prev.MatchCallvirt<Asset<Texture2D>>("get_" + nameof(Asset<>.Value))) check.Index++;

					c.EmitLdloc(loc);
					for (; check.Index < targetIndex; check.Index++) {
						c.Emit(check.Next.OpCode, check.Next.Operand);
					}
					static void DrawIcon(int headType, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
						if (!OriginClientConfig.Instance.QuestNotificationPosition.HasFlag(QuestNotificationPositions.Housing)) return;
						foreach (NPC npc in Main.ActiveNPCs) {
							if ((npc.ModNPC?.TownNPCStayingHomeless ?? false) || TownNPCProfiles.GetHeadIndexSafe(npc) != headType) continue;
							if (NPCTypeHasPopup[npc.type]) break;
							return;
						}
						Main.spriteBatch.Draw(
							TextureAssets.QuicksIcon.Value,
							position - Vector2.UnitY * (TextureAssets.NpcHead[headType].Height() * 0.5f + 2) * scale,
							null,
							Main.MouseTextColorReal,
							0,
							TextureAssets.QuicksIcon.Size() * new Vector2(0.5f, 1f),
							Main.mouseTextColor / 255f,
							SpriteEffects.None,
						0);
					}
					c.EmitDelegate(DrawIcon);
				}
			}
		}
	}
	[Flags]
	public enum QuestNotificationPositions {
		None = 0,
		World = 1 << 0,
		Map = 1 << 1,
		Housing = 1 << 2
	}
}

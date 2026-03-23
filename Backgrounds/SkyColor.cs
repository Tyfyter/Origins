using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Reflection;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ThoriumMod.Items.DD;

namespace Origins.Backgrounds {
	internal class SkyColor : ILoadable {
		static readonly LinkedList<SkyLayer> layers = [];
		void ILoadable.Load(Mod mod) {
			try {
				IL_Main.DoDraw += context => {
					ILCursor c = new(context);
					//IL_1031: ldc.i4.6
					//IL_1032: call void Terraria.TimeLogger::DetailedDrawTime(int32)
					c.GotoNext(MoveType.After, 
						i => i.MatchLdcI4(6),
						i => i.MatchCall(typeof(TimeLogger), nameof(TimeLogger.DetailedDrawTime))
					);
					c.EmitCall(((Delegate)Draw).Method);
				};
			} catch (Exception e) {
				if (Origins.LogLoadingILError(GetType().Name, e)) throw;
			}
		}
		public static int bgTopY;
		public static void Draw() {
			if (MainReflection.bgTopY is null) return;
			bgTopY = MainReflection.Instance_bgTopY;
			if (layers.Count <= 0) return;
			Rectangle destinationRectangle = new(MainReflection.Instance_bgStartX, bgTopY, MainReflection.Instance_bgLoops * 48, Math.Max(Main.screenHeight, 1400));
			if (destinationRectangle.Bottom < 1400) {
				destinationRectangle.Height += 1400 - destinationRectangle.Bottom;
			}
			LinkedListNode<SkyLayer> current;
			LinkedListNode<SkyLayer> next = layers.First;
			while (next is not null) {
				current = next;
				current.ValueRef.Draw(destinationRectangle);
				next = current.Next;
				if (current.Value.opacity == 0) layers.Remove(current);
			}
		}
		void ILoadable.Unload() { }
		public static void Activate(int background) {
			if (layers.Count > 0) {
				LinkedListNode<SkyLayer> current;
				LinkedListNode<SkyLayer> next = layers.Last;
				while (next is not null) {
					current = next;
					if (current.Value.Background == background) {
						current.ValueRef.Active = true;
						layers.Remove(current);
						layers.AddLast(current);
						return;
					}
					next = current.Previous;
				}
			}
			layers.AddLast(new SkyLayer(background));
		}
		record struct SkyLayer(int Background, bool Active = true) {
			public float opacity;
			public void Draw(Rectangle destinationRectangle) {
				Main.spriteBatch.Draw(TextureAssets.Background[Background].Value, destinationRectangle, Main.ColorOfTheSkies * opacity);
				MathUtils.LinearSmoothing(ref opacity, Active.ToInt(), 1f / 60f);
				Active = false;
			}
		}
	}
}

using MonoMod.Cil;
using Origins.Core;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Backgrounds {
	internal class SkyColor : ILoadable {
		static readonly LinkedList<SkyLayer> layers = [];
		static readonly Dictionary<int, IFancySkyColors> fancyColorRegistry = [];
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
			if (ModLoader.TryGetMod("FancyLighting", out Mod fancy) && fancy.Version >= new Version(1, 1)) {
				Origins.TryHookEvent("FancyLighting", "FancyLighting.FancySkyRendering", "PreDrawSky", PreDrawSky);
			}
		}
		static bool skipActuallyDrawing = false;
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
			skipActuallyDrawing = false;
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
			readonly IFancySkyColors fancyColors = fancyColorRegistry.TryGetValue(Background, out IFancySkyColors _fancyColors) ? _fancyColors : default;
			public void Draw(Rectangle destinationRectangle) {
				if (!skipActuallyDrawing) Main.spriteBatch.Draw(TextureAssets.Background[Background].Value, destinationRectangle, Main.ColorOfTheSkies * opacity);
				MathUtils.LinearSmoothing(ref opacity, Active.ToInt(), 1f / 60f);
				Active = false;
			}
			public readonly void ApplyFancyColors(ref Vector3 highSkyColor, ref Vector3 lowSkyColor, ref Vector3 skyColorMult) => fancyColors?.ApplyFancyColors(opacity, ref highSkyColor, ref lowSkyColor, ref skyColorMult);
		}
		public static void AddFancyColors(int background, IFancySkyColors colors) => fancyColorRegistry[background] = colors;
		public delegate void SkyColorModifier(ref Vector3 highSkyColor, ref Vector3 lowSkyColor, ref Vector3 skyColorMult);
		public static void PreDrawSky(ref Vector3 highSkyColor, ref Vector3 lowSkyColor, ref Vector3 skyColorMult) {
			LinkedListNode<SkyLayer> current;
			LinkedListNode<SkyLayer> next = layers.First;
			while (next is not null) {
				current = next;
				current.ValueRef.ApplyFancyColors(ref highSkyColor, ref lowSkyColor, ref skyColorMult);
				next = current.Next;
				if (current.Value.opacity == 0) layers.Remove(current);
			}
			skipActuallyDrawing = true;
		}
		public interface IFancySkyColors {
			public void ApplyFancyColors(float opacity, ref Vector3 highSkyColor, ref Vector3 lowSkyColor, ref Vector3 skyColorMult);
		}
		public readonly struct PlainFancySkyColors(Vector3 highColor, Vector3 lowColor) : IFancySkyColors {
			readonly void IFancySkyColors.ApplyFancyColors(float opacity, ref Vector3 highSkyColor, ref Vector3 lowSkyColor, ref Vector3 skyColorMult) {
				highSkyColor = Vector3.Lerp(highSkyColor, highColor, opacity);
				lowSkyColor = Vector3.Lerp(lowSkyColor, lowColor, opacity);
			}
		}
		public readonly struct InterpolatedFancySkyColors(HSLCurve highColors, HSLCurve lowColors) : IFancySkyColors {
			static float CurrentTime => Utils.GetDayTimeAs24FloatStartingFromMidnight();
			readonly void IFancySkyColors.ApplyFancyColors(float opacity, ref Vector3 highSkyColor, ref Vector3 lowSkyColor, ref Vector3 skyColorMult) {
				highSkyColor = Vector3.Lerp(highSkyColor, highColors[CurrentTime].RGBV, opacity);
				lowSkyColor = Vector3.Lerp(lowSkyColor, lowColors[CurrentTime].RGBV, opacity);
			}
		}
	}
}

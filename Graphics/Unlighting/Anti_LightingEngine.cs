using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Misc;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Origins.Graphics.Unlighting {
	[ReinitializeDuringResizeArrays]
	public class Anti_LightingEngine : ILoadable {
		readonly LightingEngine unlightingEngine = new();
		public void Load(Mod mod) {
			unlightingEngine.Rebuild();
			On_LightingEngine.ProcessScan += On_LightingEngine_ProcessScan;
			On_LightingEngine.ProcessBlur += On_LightingEngine_ProcessBlur;
			On_LightingEngine.Present += On_LightingEngine_Present;
			On_LightingEngine.GetColor += On_LightingEngine_GetColor;
			On_TileLightScanner.ApplyHellLight += (On_TileLightScanner.orig_ApplyHellLight orig, TileLightScanner self, Tile tile, int x, int y, ref Vector3 lightColor) => {
				if (self != tileScanner.Value) orig(self, tile, x, y, ref lightColor);
			};
			On_TileLightScanner.ApplySurfaceLight += (On_TileLightScanner.orig_ApplySurfaceLight orig, TileLightScanner self, Tile tile, int x, int y, ref Vector3 lightColor) => {
				if (self != tileScanner.Value) orig(self, tile, x, y, ref lightColor);
			};
			On_TileLightScanner.ApplyLiquidLight += (On_TileLightScanner.orig_ApplyLiquidLight orig, TileLightScanner self, Tile tile, ref Vector3 lightColor) => {
				if (self != tileScanner.Value) orig(self, tile, ref lightColor);
			};
			IL_TileLightScanner.ApplyTileLight += ApplyTileWallLight;
			IL_TileLightScanner.ApplyWallLight += ApplyTileWallLight;
			IL_LightingEngine.ApplyPerFrameLights += IL_LightingEngine_ApplyPerFrameLights;
			lightingEngine = new(() => _activeEngine.Value);
			tileScanner = new(() => _tileScanner.GetValue(unlightingEngine));

			string methodName = "CopyPerFrameUnlights";
			DynamicMethod getterMethod = new(methodName, typeof(void), [typeof(LightingEngine), typeof(LightingEngine)], true);
			ILGenerator gen = getterMethod.GetILGenerator();
			FieldInfo anyPerFrameUnglows = GetType().GetField(nameof(Anti_LightingEngine.anyPerFrameUnglows), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo _perFrameLights = typeof(LightingEngine).GetField("_perFrameLights", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Type PerFrameLight = typeof(LightingEngine).GetNestedType("PerFrameLight", BindingFlags.NonPublic);
			Type List = typeof(List<>).MakeGenericType(PerFrameLight);
			Type Span = typeof(Span<>).MakeGenericType(PerFrameLight);
			FieldInfo Color = PerFrameLight.GetField("Color", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			LocalBuilder i = gen.DeclareLocal(typeof(int));
			LocalBuilder current = gen.DeclareLocal(PerFrameLight);

			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Stsfld, anyPerFrameUnglows); // anyPerFrameUnglows = false;

			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldfld, _perFrameLights);
			gen.Emit(OpCodes.Callvirt, List.GetMethod(nameof(List<object>.Clear)));

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, _perFrameLights);
			gen.Emit(OpCodes.Call, List.GetProperty(nameof(List<object>.Count)).GetGetMethod());
			gen.Emit(OpCodes.Stloc, i); // int i = lights.Count;

			Label end = gen.DefineLabel();
			Label loop = gen.DefineLabel(); // loop:;
			gen.MarkLabel(loop);
			gen.Emit(OpCodes.Ldloc, i);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Sub);
			gen.Emit(OpCodes.Stloc, i); // i--;

			gen.Emit(OpCodes.Ldloc, i);
			gen.Emit(OpCodes.Ldc_I4_0);
			gen.Emit(OpCodes.Ble, end); // if (i < 0) break;

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, _perFrameLights);
			gen.Emit(OpCodes.Ldloc, i);
			gen.Emit(OpCodes.Callvirt, List.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
			gen.Emit(OpCodes.Stloc, current); // current = lights[i];

			Label foundNeg = gen.DefineLabel(); // if
			
			gen.Emit(OpCodes.Ldloc, current);
			gen.Emit(OpCodes.Ldfld, Color);
			gen.Emit(OpCodes.Ldfld, typeof(Vector3).GetField("X"));
			gen.Emit(OpCodes.Ldc_R4, 0.0f);
			gen.Emit(OpCodes.Blt_S, foundNeg); // current.Color.X < 0

			gen.Emit(OpCodes.Ldloc, current);
			gen.Emit(OpCodes.Ldfld, Color);
			gen.Emit(OpCodes.Ldfld, typeof(Vector3).GetField("Y"));
			gen.Emit(OpCodes.Ldc_R4, 0.0f);
			gen.Emit(OpCodes.Blt_S, foundNeg); // current.Color.Y < 0

			gen.Emit(OpCodes.Ldloc, current);
			gen.Emit(OpCodes.Ldfld, Color);
			gen.Emit(OpCodes.Ldfld, typeof(Vector3).GetField("Z"));
			gen.Emit(OpCodes.Ldc_R4, 0.0f);
			gen.Emit(OpCodes.Bge_Un, loop); // current.Color.Z < 0

			gen.MarkLabel(foundNeg);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldfld, _perFrameLights);
			gen.Emit(OpCodes.Ldloc, current);
			gen.Emit(OpCodes.Callvirt, List.GetMethod(nameof(List<object>.Add))); //arg1.Add(current);
			gen.Emit(OpCodes.Ldc_I4_1);
			gen.Emit(OpCodes.Stsfld, anyPerFrameUnglows); // anyPerFrameUnglows = true;

			gen.Emit(OpCodes.Br, loop); // goto loop;

			gen.MarkLabel(end);
			gen.Emit(OpCodes.Ret);

			CopyPerFrameUnlights = getterMethod.CreateDelegate<Action<LightingEngine, LightingEngine>>();
		}
#pragma warning disable IDE0044 // Add readonly modifier
		static bool anyUnglowingBlocks = false;
		static bool anyPerFrameUnglows = false;
#pragma warning restore IDE0044 // Add readonly modifier
		Action<LightingEngine, LightingEngine> CopyPerFrameUnlights;
		private void IL_LightingEngine_ApplyPerFrameLights(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After, i => i.MatchLdfld(typeof(LightingEngine).GetNestedType("PerFrameLight", BindingFlags.NonPublic), "Color"));
			c.EmitDelegate((Vector3 value) => value * unlightingFactor);
		}

		private void On_LightingEngine_Present(On_LightingEngine.orig_Present orig, LightingEngine self) {
			orig(self);
			if (self == lightingEngine.Value) orig(unlightingEngine);
		}

		private void ApplyTileWallLight(ILContext il) {
			ILCursor c = new(il);
			int loc = -1;
			Func<Instruction, bool>[] predicates = [
				i => i.MatchLdarg(5),//IL_3532: ldarg.s lightColor
				i => i.MatchLdfld(out _),//IL_3534: ldfld float32[FNA]Microsoft.Xna.Framework.Vector3::X
				i => i.MatchLdloc(out loc),//IL_3539: ldloc.0
				i => i.MatchBgeUn(out _)//IL_353a: bge.un.s IL_3544
			];
			while (c.TryGotoNext(MoveType.AfterLabel, predicates)) {
				/*int cursorIndex = c.Index;
				while (c.TryGotoNext(MoveType.After, i => i.MatchLdloc(loc))) {
					c.EmitDelegate((float value) => {
						anyUnglowingBlocks |= value < 0;
						return value * unlightingFactor;
					});
				}
				c.Index = cursorIndex;*/
				c.EmitLdloca(loc);
				c.EmitDelegate((ref float value) => {
					anyUnglowingBlocks |= value < 0;
					value *= unlightingFactor;
				});
				c.Index += predicates.Length - 1;
			}
		}

		private Vector3 On_LightingEngine_GetColor(On_LightingEngine.orig_GetColor orig, LightingEngine self, int x, int y) {
			Vector3 value = orig(self, x, y);
			if ((anyUnglowingBlocks || anyPerFrameUnglows) && self == lightingEngine.Value) {
				Vector3 unlight = orig(unlightingEngine, x, y);
				int num = (Main.tileColor.R + Main.tileColor.G + Main.tileColor.B) / 3;
				float minLight = (float)(num * 0.4) / 255f;
				if (Lighting.Mode == LightMode.Retro) {
					minLight = (Main.tileColor.R - 55) / 255f;
					if (minLight < 0f) {
						minLight = 0f;
					}
				} else if (Lighting.Mode == LightMode.Trippy) {
					minLight = (num - 55) / 255f;
					if (minLight < 0f) {
						minLight = 0f;
					}
				}
				value = Vector3.Max(value - unlight, Vector3.Min(new(minLight), value));
			}
			return value;
		}

		private void On_LightingEngine_ProcessBlur(On_LightingEngine.orig_ProcessBlur orig, LightingEngine self) {
			if (self == lightingEngine.Value) {
				try {
					unlightingFactor = -1;
					CopyPerFrameUnlights(self, unlightingEngine);
					orig(unlightingEngine);
				} finally {
					unlightingFactor = 1;
				}
			}
			orig(self);
		}
		static float unlightingFactor = 1;
		private void On_LightingEngine_ProcessScan(On_LightingEngine.orig_ProcessScan orig, LightingEngine self, Rectangle area) {
			anyUnglowingBlocks = false;
			orig(self, area);
			if (self == lightingEngine.Value) {
				try {
					unlightingFactor = -1;
					orig(unlightingEngine, area);
				} finally {
					unlightingFactor = 1;
				}
			}
		}

		readonly FastStaticFieldInfo<Lighting, ILightingEngine> _activeEngine = "_activeEngine";
		readonly FastFieldInfo<LightingEngine, TileLightScanner> _tileScanner = "_tileScanner";

		FrameCachedValue<ILightingEngine> lightingEngine;
		FrameCachedValue<TileLightScanner> tileScanner;
		public void Unload() { }
	}
}

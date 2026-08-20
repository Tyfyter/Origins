using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Reflection;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Origins.Graphics.Unlighting {
	[ReinitializeDuringResizeArrays]
	public class Anti_LightingEngine : ModSystem {
		static readonly BlendState subtractiveBlending = new() {
			ColorBlendFunction = BlendFunction.ReverseSubtract,
			AlphaBlendFunction = BlendFunction.Add,
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.Zero,
			AlphaDestinationBlend = Blend.One
		};
		static readonly BlendState fixNegativeResult = new() {
			ColorBlendFunction = BlendFunction.Max,
			AlphaBlendFunction = BlendFunction.Max,
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.Zero,
			AlphaDestinationBlend = Blend.One
		};
		static readonly HalfVector4 halfV4FullBright = new(1, 1, 1, 1);
		static uint[] rgba1010102FullBright = [];
		static HalfVector4[] halfVector4FullBright = [];
		internal static void ApplyFancyLightingHookFirst(Action orig) {
			orig();
			if (ModLoader.TryGetMod("FancyLighting", out Mod fancy)) fancy.Call("AddHook", "PostUpdateLightMap", (Delegate)PostUpdateLightMap);
		}
		static Texture2D unlightMapTexture;
		static void ResizeAndFill<T>(ref T[] array, int size, T fillValue) {
			int oldSize = array.Length;
			Array.Resize(ref array, size);
			if (size > oldSize) Array.Fill(array, fillValue, oldSize, size - oldSize);
		}
		static void PostUpdateLightMap(Texture2D lightMapTexture, Matrix samplingTransformation, Rectangle lightMapArea, bool cameraMode) {
			/*if (Weak_Shimmer_Debuff.isDrawingShimmeryThing) {
				switch (lightMapTexture.Format) {
					case SurfaceFormat.Rgba1010102:
					ResizeAndFill(ref rgba1010102FullBright, lightMapTexture.Width * lightMapTexture.Height, uint.MaxValue);
					lightMapTexture.SetData(rgba1010102FullBright);
					break;
					case SurfaceFormat.HalfVector4:
					ResizeAndFill(ref halfVector4FullBright, lightMapTexture.Width * lightMapTexture.Height, halfV4FullBright);
					lightMapTexture.SetData(halfVector4FullBright);
					break;
				}
				return;
			}*/
			if (!anyPerFrameUnglows) return;
			LightMap unlightMap = LightingMethods._activeLightMap.GetValue(unlightingEngine);
			if (lightMapTexture is RenderTarget2D renderTarget) {
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				RenderTargetUsage oldUsage = renderTarget.RenderTargetUsage;
				PegasusLib.Graphics.GraphicsMethods.SetRenderTargetUsage(renderTarget, RenderTargetUsage.PreserveContents);
				Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
				Main.spriteBatch.Begin(
					SpriteSortMode.Immediate,
					subtractiveBlending,
					SamplerState.LinearClamp,
					DepthStencilState.None,
					RasterizerState.CullNone
				);
				if (unlightMapTexture is null || unlightMapTexture.Width != unlightMap.Height || unlightMapTexture.Height != unlightMap.Width) {
					unlightMapTexture = new(Main.graphics.GraphicsDevice, unlightMap.Height, unlightMap.Width, false, SurfaceFormat.Vector4);
				}
				Vector3[] _colors = LightingMethods._colors.GetValue(unlightMap);
				Vector4[] colors = new Vector4[_colors.Length];
				for (int i = 0; i < _colors.Length; i++) colors[i] = new(_colors[i], 0);
				unlightMapTexture.SetData(colors);
				Main.spriteBatch.Draw(
					unlightMapTexture,
					lightMapTexture.Bounds,
					Color.White
				);
				Main.spriteBatch.End();
				PegasusLib.Graphics.GraphicsMethods.SetRenderTargetUsage(renderTarget, oldUsage);
				Main.graphics.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);
			} else {
				Rectangle activeProcessedArea = LightingMethods._activeProcessedArea.GetValue(unlightingEngine);
				Rectangle modifyingArea = activeProcessedArea;
				modifyingArea.X -= lightMapArea.X;
				modifyingArea.Y -= lightMapArea.Y;
				modifyingArea.Width -= modifyingArea.Right - lightMapArea.Width;
				modifyingArea.Height -= modifyingArea.Bottom - lightMapArea.Height;
				(modifyingArea.Y, modifyingArea.X, modifyingArea.Height, modifyingArea.Width) = (modifyingArea.X, modifyingArea.Y, modifyingArea.Width, modifyingArea.Height);
				uint[] colors = new uint[modifyingArea.Width * modifyingArea.Height];
				lightMapTexture.GetData(0, modifyingArea, colors, 0, colors.Length);
				for (int i = 0; i < colors.Length; i++) {
					int y = i % modifyingArea.Width;
					int x = i / modifyingArea.Width;
					uint r = Subtract((colors[i] >> 00) & 0b1111111111, unlightMap[x, y].X);
					uint g = Subtract((colors[i] >> 10) & 0b1111111111, unlightMap[x, y].Y);
					uint b = Subtract((colors[i] >> 20) & 0b1111111111, unlightMap[x, y].Z);
					uint a = (colors[i] >> 30) & 0b11;

					colors[i] = (a << 30) | (r << 0) | (g << 10) | (b << 20);
					static uint Subtract(uint a, float b) {
						uint _b = (uint)(b * 0b1111111111);
						if (a <= _b) return 0;
						return a - _b;
					}
				}
				lightMapTexture.SetData(0, modifyingArea, colors, 0, colors.Length);
			}
		}
		readonly static The_Engine unlightingEngine = new();
		public override void PostAddRecipes() {
			unlightingEngine.Rebuild();
			On_LightingEngine.ProcessScan += On_LightingEngine_ProcessScan;
			On_LightingEngine.ProcessBlur += On_LightingEngine_ProcessBlur;
			On_LightingEngine.Present += On_LightingEngine_Present;
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

			#region CopyPerFrameUnlights
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
			#endregion
		}
#pragma warning disable IDE0044 // Add readonly modifier
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
			LightMap _activeLightMap = LightingMethods._activeLightMap.GetValue(self);
			if (Weak_Shimmer_Debuff.isDrawingShimmeryThing) {
				Array.Fill(LightingMethods._colors.GetValue(_activeLightMap), Vector3.One);
				return;
			}
			if (self == lightingEngine.Value) {
				orig(unlightingEngine);
				if (anyPerFrameUnglows) {
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
					Vector3[] light = LightingMethods._colors.GetValue(_activeLightMap);
					Vector3[] unlight = LightingMethods._colors.GetValue(LightingMethods._activeLightMap.GetValue(unlightingEngine));
					for (int i = 0; i < light.Length; i++) light[i] = Vector3.Max(light[i] - unlight[i], Vector3.Min(new(minLight), light[i]));
				}
			}
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
					value *= unlightingFactor;
				});
				c.Index += predicates.Length - 1;
			}
		}

		private Vector3 On_LightingEngine_GetColor(On_LightingEngine.orig_GetColor orig, LightingEngine self, int x, int y) {
			if (Weak_Shimmer_Debuff.isDrawingShimmeryThing) return Vector3.One;
			Vector3 value = orig(self, x, y);
			if (anyPerFrameUnglows && self == lightingEngine.Value) {
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
		public class The_Engine : LightingEngine { }
	}
}

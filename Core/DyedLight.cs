using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Dyes;
using Origins.Reflection;
using Origins.Walls;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Core {
	//TODO: remove, moved to PegasusLib
	public class DyedLight {
		readonly Vector3 color;
		readonly Rectangle dest = new(0, 0, 1, 1);
		readonly Color tint = Color.White;
		Texture2D texture;
		RenderTarget2D renderTarget;
		public DyedLight(Color color) {
			this.color = color.ToVector3();
			if (Main.dedServ) return;
			Main.QueueMainThreadAction(() => {
				texture = new(Main.graphics.GraphicsDevice, 1, 1);
				texture.SetData([color]);
				renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);
			});
		}
		Color Get(ArmorShaderData shader, Entity entity) {
			Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			shader.Apply(entity);
			Main.spriteBatch.Draw(texture, dest, tint);
			Main.spriteBatch.End();
			Main.graphics.GraphicsDevice.SetRenderTarget(null);

			Color[] data = new Color[1];
			renderTarget.GetData(data);
			return data[0];
		}
		public void GetColorWithRateLimit(ref Vector3 color, ref ulong timer, Func<int?> armorShaderID, Player player, Entity entity) {
			if (OriginClientConfig.Instance.ProceduralLightSourceDyeRate != 0) {
				unchecked {
					if (PegasusLib.PegasusLib.gameFrameCount - (ulong)OriginClientConfig.Instance.ProceduralLightSourceDyeRate < timer) return;
				}
			}
			timer = PegasusLib.PegasusLib.gameFrameCount;
			color = GetColor(armorShaderID, player, entity);
		}
		public Vector3 GetColor(Func<int?> armorShaderID, Player player, Entity entity) {
			if (Main.dedServ) return color;
			if (!OriginClientConfig.Instance.DyeLightSources) return color;
			if (armorShaderID() is not int dye) return color;
			ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(dye, player);
			if (shader is null) return color;
			if (OriginClientConfig.Instance.ProceduralLightSourceDyeRate == 0 || dyeFunctionIsBetter.Contains(shader)) {
				if (dyeFunctions.TryGetValue(shader, out DyeFunction dyeFunction)) return dyeFunction(color, entity, this);
				return color;
			}
			return Get(shader, entity).ToVector3();
		}
		static Vector3 ReHue(Vector3 baseColor, Vector3 newHue) {
			Vector3 hsl = RGBToHSV(baseColor);
			Vector3 newHSL = RGBToHSV(newHue);
			return HSVToRGB(newHSL.X, hsl.Y * newHSL.Y, hsl.Z * newHSL.Z);
		}
		static DyeFunction ReHue(Vector3 newHue) => (color, _, _) => ReHue(color, newHue);
		static DyeFunction CacheFirstColor(ArmorShaderData shader) {
			Dictionary<DyedLight, Vector3> cache = [];
			return (color, entity, lightSource) => {
				if (!cache.TryGetValue(lightSource, out Vector3 value)) {
					value = color;
					if (shader is not null) lightSource.Get(shader, entity).ToVector3();
					cache[lightSource] = value;
				}
				return value;
			};
		}
		static Vector3 HSVToRGB(float hue, float saturation, float value) {
			float C = value * saturation;
			float X = C * (1 - Math.Abs((hue % 2) - 1));
			float m = value - C;
			Vector3 rgbOff = default;
			switch ((int)Math.Floor(hue)) {
				case 0:
				rgbOff.X = C;
				rgbOff.Y = X;
				break;
				case 1:
				rgbOff.Y = C;
				rgbOff.X = X;
				break;
				case 2:
				rgbOff.Y = C;
				rgbOff.Z = X;
				break;
				case 3:
				rgbOff.Z = C;
				rgbOff.Y = X;
				break;
				case 4:
				rgbOff.Z = C;
				rgbOff.X = X;
				break;
				default:
				rgbOff.X = C;
				rgbOff.Z = X;
				break;
			}
			return new(rgbOff.X + m, rgbOff.Y + m, rgbOff.Z + m);
		}
		static Vector3 RGBToHSV(Vector3 rgb) {
			float r = rgb.X;
			float g = rgb.Y;
			float b = rgb.Z;
			float Cmax = Math.Max(Math.Max(r, g), b);
			float Cmin = Math.Min(Math.Min(r, g), b);
			float delta = Cmax - Cmin;
			Vector3 hsv = new(0, 0, Cmax);
			if (delta <= 0) {
				hsv.X = 0;
			} else if (Cmax <= r) {
				hsv.X = ((g - b) / delta) % 6;
				if (hsv.X < 0) hsv.X += 6;
			} else if (Cmax <= g) {
				hsv.X = ((b - r) / delta + 2);
			} else {
				hsv.X = ((r - g) / delta + 4);
			}
			if (Cmax <= 0) {
				hsv.Y = 0;
			} else {
				hsv.Y = delta / Cmax;
			}
			return hsv;
		}
		static HashSet<ArmorShaderData> dyeFunctionIsBetter = [];
		static Dictionary<ArmorShaderData, DyeFunction> dyeFunctions = [];
		internal static void Initialize() {
			if (Main.dedServ) return;
			FastFieldInfo<ArmorShaderDataSet, List<ArmorShaderData>> _shaderData = "_shaderData";
			List<ArmorShaderData> shaderData = _shaderData.GetValue(GameShaders.Armor);
			FastFieldInfo<ArmorShaderData, Vector3> uColor = "_uColor";
			FastFieldInfo<ArmorShaderData, Vector3> uSecondaryColor = "_uSecondaryColor";
			const string distinct_pass_name = "ArmorMidnightRainbow";
			static int FindDistinctPass(EffectPassCollection passes) {
				int i = 0;
				foreach (EffectPass pass in passes) {
					if (pass.Name == distinct_pass_name) return i;
					i++;
				}
				return -1;
			}
			int distinctPassIndex = FindDistinctPass(Main.pixelShader.Techniques.First().Passes);
			for (int i = 0; i < shaderData.Count; i++) {
				ArmorShaderData shader = shaderData[i];
				if (FindDistinctPass(shader.Shader.Techniques.First().Passes) == distinctPassIndex) {
					if (OriginsSets.Misc.BasicColorDyeShaders[i]) {
						Vector3 color = uColor.GetValue(shader);
						dyeFunctions[shader] = ReHue(color);
						continue;
					}
					switch (ShaderDataMethods._passName.GetValue(shader)) {
						case "ArmorStardust": {
							Vector3 shaderColor = uColor.GetValue(shader);
							dyeFunctions[shader] = (color, entity, _) => {
								const float saturation_mult = 1.25f;
								Vector3 hsl = RGBToHSV(color);
								Vector3 sparkleColor = RGBToHSV(shaderColor);
								float[,] odds = Baryte_Wall.GetPerlin();
								sparkleColor.Y *= saturation_mult - float.Pow(odds[(int)(Main.timeForVisualEffects) % odds.GetLength(1), (entity.whoAmI * 7 * 5) % odds.GetLength(1)], 2) * saturation_mult * 0.85f;
								return HSVToRGB(sparkleColor.X, hsl.Y * sparkleColor.Y, hsl.Z * sparkleColor.Z);
							};
							dyeFunctionIsBetter.Add(shader);
							break;
						}

						case "ArmorMartian":
						case "ArmorWisp":
						case "ArmorMushroom":
						case "ArmorHighContrastGlow":
						case "ArmorHades":
						case "ArmorVortex":
						case "ArmorReflectiveColor": {
							Vector3 color = uColor.GetValue(shader);
							dyeFunctions[shader] = ReHue(color);
							break;
						}
						case "ArmorShiftingSands":
						case "ArmorShiftingPearlsands": {
							Vector3 color = uColor.GetValue(shader) / 1.1f;
							dyeFunctions[shader] = ReHue(color);
							break;
						}
						case "ArmorSolar":
						case "ArmorBrightnessGradient":
						case "ArmorColoredAndBlackGradient":
						case "ArmorColoredAndSilverTrimGradient":
						case "ArmorColoredGradient": {
							Vector3 color = Vector3.Lerp(uColor.GetValue(shader), uSecondaryColor.GetValue(shader), 0.5f);
							dyeFunctions[shader] = ReHue(color);
							break;
						}
						case "ArmorInvert":
						dyeFunctions[shader] = (color, _, _) => Vector3.One - color;
						break;
						case "ColorOnly": {
							Vector3 color = uColor.GetValue(shader);
							dyeFunctions[shader] = (_, _, _) => color;
							break;
						}
						case "ArmorPolarized": {
							Vector3 color = Vector3.One;
							dyeFunctions[shader] = ReHue(color);
							break;
						}
					}
				}
			}
			Cache<Rasterized_Dye>();
			Cache<Anti_Gray_Dye>();
			PreferDyeFunction<Shimmer_Dye>((_, entity, _) => {
				Vector3 color = default;
				LiquidRenderer.GetShimmerBaseColor(entity.Center.X / 16, entity.Center.Y / 16).Deconstruct(out color.X, out color.Y, out color.Z, out _);
				return color;
			});
		}
		static void Cache<TItem>() where TItem : ModItem {
			ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(ModContent.ItemType<TItem>());
			dyeFunctions[shader] = CacheFirstColor(shader);
			dyeFunctionIsBetter.Add(shader);
		}
		static void PreferDyeFunction<TItem>(DyeFunction function) where TItem : ModItem {
			ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(ModContent.ItemType<TItem>());
			dyeFunctions[shader] = function;
			dyeFunctionIsBetter.Add(shader);
		}
		public delegate Vector3 DyeFunction(Vector3 color, Entity entity, DyedLight lightSource);
	}
}

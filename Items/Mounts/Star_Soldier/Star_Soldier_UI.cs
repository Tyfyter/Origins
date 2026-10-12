#define EditAnimation
using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;
using Origins.Core;
using Origins.Core.Shaders;
using Origins.Graphics.Primitives;
using Origins.Misc;
using Origins.Projectiles;
using Origins.UI;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.Items.Mounts.Star_Soldier;

public class Star_Soldier_UI : SwitchableUIState {
	public static readonly Polygon.PrimitiveBatch primitiveBatch = new();
	[ReinitializeDuringResizeArrays]
	public static class Sets {
		public static Func<int, Vector2, bool>[] CustomBuffIndicator = BuffID.Sets.Factory.CreateNamedSet($"{nameof(Star_Soldier)}_{nameof(CustomBuffIndicator)}")
		.Description("Replaces the normal buff icon drawing when the player is using the Star Soldier mount")
		.RegisterCustomSet<Func<int, Vector2, bool>>(null);
		public static Action<SpriteBatch>[] InfoDisplayOverride = new Action<SpriteBatch>[InfoDisplayLoader.InfoDisplayCount];
		public static bool?[] OverrideDontDakeDamageTargeting = NPCID.Sets.Factory.CreateNamedSet($"{nameof(Star_Soldier)}_{nameof(OverrideDontDakeDamageTargeting)}")
		.Description("Lets the Star Soldier mount's targeting interface display information about this type of NPC even if NPC.dontTakeDamage is true, or prevents it even if NPC.dontTakeDamage is false")
		.RegisterCustomSet<bool?>(null,
			NPCID.BrainofCthulhu, false,
			NPCID.MoonLordFreeEye, false,
			NPCID.DD2LanePortal, false,
			NPCID.Deerclops, false,
			NPCID.EmpressButterfly, false,
			NPCID.FairyCritterBlue, false,
			NPCID.FairyCritterGreen, false,
			NPCID.FairyCritterPink, false,
			NPCID.Probe, false,
			NPCID.GraniteGolem, false,
			NPCID.GraniteFlyer, false
		);
		static Sets() {
			InfoDisplayOverride[InfoDisplay.DepthMeter.Type] = InfoAccessoryHUD.DrawDepthMeter;
			CustomBuffIndicator[BuffID.OnFire] = HPHUD.DrawFire(Color.Orange);
			CustomBuffIndicator[BuffID.CursedInferno] = HPHUD.DrawFire(Color.Chartreuse, false);
			CustomBuffIndicator[BuffID.OnFire3] = HPHUD.DrawFire(Color.OrangeRed);
		}
	}
	static readonly AdvancedMiscShaderData healthBarShader = new(ModContent.Request<Effect>("Origins/Effects/HUD"), "HealthBar");
	public override InterfaceScaleType ScaleType => InterfaceScaleType.None;
	public override void AddToList() {
		OriginSystem.Instance.MountHUD.AddState(this);
		healthBarShader.LoadThen(() => {
			uColor.Bind(healthBarShader);
			uSecondaryColor.Bind(healthBarShader);
			uSaturation.Bind(healthBarShader);
		});
	}
	static readonly HashSet<string> hideLayers = [
		"Vanilla: Resource Bars",
		"Vanilla: Info Accessories Bar",
		"Vanilla: Hotbar",
		"ThoriumMod: Combat Icon"
	];
	static readonly HashSet<string> hideLayersInventory = [
		"Vanilla: Resource Bars",
		"Vanilla: Hotbar",
		"ThoriumMod: Combat Icon"
	];
	public static Parameter uColor = Parameter.uColor with { Value = Color.OrangeRed.ToVector3() };
	public static Parameter uSecondaryColor = Parameter.uSecondaryColor with { Value = Vector3.Zero };
	public static Parameter uSaturation = Parameter.uSaturation;
	public static class Anchors {
		public static Vector2 topCenter;
		public static Vector2 leftCenter;
		public static Vector2 rightCenter;
		public static Vector2 bottomLeft;
		public static void Reset() {
			topCenter = new(Main.screenWidth * 0.5f, 8);
			leftCenter = new(8, Main.screenHeight * 0.5f);
			rightCenter = new(Main.screenWidth - 8, Main.screenHeight * 0.5f);
			bottomLeft = new(8, Main.screenHeight - 8);
		}
	}
	public override bool IsActive() {
		if (Main.LocalPlayer.dead || !Main.LocalPlayer.mount.IsMount<Star_Soldier>()) return false;
		OriginSystem.hideInterfaceLayers = Main.playerInventory ? hideLayersInventory : hideLayers;
		return true;
	}
	// We could go full NieR and let the player interact with the UI components as items
	IUISegment[] uiSegments = [
		new WeaponHUD(),
		new HPHUD(),
		new O2HUD(),
		new LavaHUD(),
		new TargetingHUD(),
		new InfoAccessoryHUD(),
		new FuelHUD(),

		new AlarmHUD()
	];
	public Star_Soldier_UI() : base() {
		OverrideSamplerState = SamplerState.PointClamp;
	}
	protected override void DrawSelf(SpriteBatch spriteBatch) {
		Player player = Main.LocalPlayer;
		if (Star_Soldier.GetHandler(player) is not Star_Soldier.MountHandler handler) return;
		Main.UIScaleMatrix.Decompose(out Vector3 _uiScale, out _, out _);
		Vector2 uiScale = _uiScale.XY();

		Anchors.Reset();
		for (int i = 0; i < uiSegments.Length; i++) uiSegments[i].Draw(spriteBatch, handler, uiScale);
		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
		Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
		Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		primitiveBatch.Flush();
	}
	interface IUISegment {
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale);
	}
	public static Vector2 DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, Vector2 scale) {
		if (OriginAccessibilityConfig.ItemSpecificOptions.StarSoldier_HUDOutlines) {
			return ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				Star_Soldier.Font,
				text,
				position,
				color,
				color.MultiplyRGB(new(100, 100, 100)),
				0,
				default,
				scale,
				spread: 1
			) - position;
		} else {
			return ChatManager.DrawColorCodedString(
				spriteBatch,
				Star_Soldier.Font,
				text,
				position,
				color,
				0,
				default,
				scale
			) - position;
		}
	}
	public class WeaponHUD : IUISegment {
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			Player player = Main.LocalPlayer;
			Vector2 scale = Main.GameViewMatrix.Zoom * uiScale;

			Vector2 pos = player.Center - Main.screenPosition;
			pos.Y += player.height * 0.5f + 8;
			pos = pos.Floor();

			//pos = pos.Transform(Main.UIScaleMatrix);
			(handler.chosenItem.item?.ModItem as Star_Soldier_Weapon)?.DrawHud(spriteBatch, ref pos, scale, in handler.chosenItem);
			(handler.altItem.item?.ModItem as Star_Soldier_Weapon)?.DrawHud(spriteBatch, ref pos, scale, in handler.altItem);
		}
	}
	public class HPHUD : IUISegment {
		public static Vector2 customBuffIndicatorPos;
		static readonly Polygon healthBarPoly = Polygon.Import(
			400,
			Vector2.UnitX * 0.5f,
			new(0, 0),
			new(0.05f, 0),
			new(0.95f, 0),
			new(1f, 0),
			new(0.95f, 0.04f),
			new(0.05f, 0.04f)
		);
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			bool doOutlines = OriginAccessibilityConfig.ItemSpecificOptions.StarSoldier_HUDOutlines;
			Player player = Main.LocalPlayer;
			Vector2 hpSize = new Vector2(400, 16) * uiScale;
			{ //HP bar
				Vector2 position = new((Main.screenWidth - hpSize.X) * 0.5f, 8);
				customBuffIndicatorPos = position;
				if (doOutlines) customBuffIndicatorPos.X -= 8;
				Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
				Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
				Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
				healthBarPoly.ResetPositions().Scale(uiScale).Translate(new(Main.screenWidth * 0.5f, 8));
				if (doOutlines) {
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					using Polygon.VertexCache _ = healthBarPoly.ModificationContext();
					for (int i = 0; i < healthBarPoly.vertices.Length; i++) {
						if (healthBarPoly.vertices[i].TextureCoordinate.Y == 0) healthBarPoly.vertices[i].Position.Y -= 4;
						else healthBarPoly.vertices[i].Position.Y += 2;
						switch (healthBarPoly.vertices[i].TextureCoordinate.X) {
							case 0:
							healthBarPoly.vertices[i].Position.X -= 8;
							break;
							case 0.05f:
							healthBarPoly.vertices[i].Position.X -= 2;
							break;
							case 0.95f:
							healthBarPoly.vertices[i].Position.X += 2;
							break;
							case 1:
							healthBarPoly.vertices[i].Position.X += 8;
							break;
						}
						healthBarPoly.vertices[i].Color = Color.OrangeRed.MultiplyRGB(new(100, 100, 100));
					}
					healthBarPoly.Draw();
				}
				healthBarShader.Apply(null,
					uColor,
					uSecondaryColor,
					uSaturation with { Value = handler.life / (float)Star_Soldier.MountHandler.MaxLife }
				);
				healthBarPoly.Draw();
				if (!Main.mouseText && healthBarPoly.Contains(Main.MouseScreen)) {
					player.cursorItemIconEnabled = false;
					string text = handler.life + "/" + Star_Soldier.MountHandler.MaxLife;
					Main.instance.MouseTextHackZoom(text);
					Main.mouseText = true;
				}
			}
			{ //buff icons
				Point position = new((int)((Main.screenWidth - hpSize.X) * 0.5f + hpSize.X * 0.075f), 28);
				Point currentPosition = position;
				int hovered = -1;
				AlarmHUD.playAlarm = false;
				if (player.burned) AlarmHUD.playAlarm = true;
				for (int i = 0; i < Player.MaxBuffs; i++) {
					if (player.buffType[i] > 0) {
						if (Sets.CustomBuffIndicator[player.buffType[i]] is Func<int, Vector2, bool> customIcon) {
							if (customIcon(i, uiScale)) {
								hovered = i;
								player.mouseInterface = true;
							}
							continue;
						}
						hovered = Main.DrawBuffIcon(hovered, i, currentPosition.X, currentPosition.Y);
						currentPosition.X += 38;
						if (currentPosition.X >= position.X + 9 * 38) {
							currentPosition.X = position.X;
							currentPosition.Y += 50;
						}
					} else {
						Main.buffAlpha[i] = 0.4f;
					}
				}
				if (PlayerInput.UsingGamepad && !Main.playerInventory)
					hovered = -1;
				Main.bannerMouseOver = false;
				if (hovered >= 0) {
					int buffType = player.buffType[hovered];
					string buffName = Lang.GetBuffName(buffType);
					string buffTooltip = Main.GetBuffTooltip(player, buffType);

					if (player.buffType[hovered] == BuffID.MonsterBanner) Main.bannerMouseOver = true;
					int rare = Main.meleeBuff[buffType] ? -10 : 0;

					BuffLoader.ModifyBuffText(buffType, ref buffName, ref buffTooltip, ref rare);
					Main.instance.MouseTextHackZoom(buffName, rare, 0, buffTooltip);
				}
			}
		}
		static readonly Polygon firePolygon = Polygon.Import(48, Polygon.Alignment.TopRight,
			new(139.39f, 158.13f),
			new(115.05f, 275.74f),
			new(94.78f, 93.25f),
			new(24.10f, 259.51f),
			new(11.36f, 338.30f),
			new(42.06f, 450.11f),
			new(179.36f, 528.90f),
			new(132.13f, 471.97f),
			new(116.79f, 393.34f),
			new(146.92f, 411.30f),
			new(141.12f, 374.80f),
			new(185.15f, 308.18f),
			new(199.64f, 360.90f),
			new(221.65f, 381.17f),
			new(236.13f, 400.87f),
			new(250.04f, 356.84f),
			new(262.20f, 378.86f),
			new(258.73f, 404.93f),
			new(260.47f, 423.46f),
			new(233.82f, 530.64f),
			new(345.63f, 468.07f),
			new(383.86f, 366.69f),
			new(365.90f, 247.35f),
			new(329.41f, 121.63f),
			new(305.86f, 180.67f),
			new(304.50f, 255.46f),
			new(272.63f, 178.41f),
			new(241.93f, 62.54f),
			new(181.68f, 12.14f)
		);
		public static Func<int, Vector2, bool> DrawFire(Color color, bool needsOil = true) => (buffIndex, uiScale) => {
			Player player = Main.LocalPlayer;
			Color _color = color * (float.Sin((float)Main.timeForVisualEffects / 10f + buffIndex) * 0.25f + 0.75f);
			for (int i = 0; i < firePolygon.vertices.Length; i++) firePolygon.vertices[i].Color = _color;
			firePolygon
			.ResetPositions()
			.Translate(customBuffIndicatorPos)
			.Draw(primitiveBatch);
			customBuffIndicatorPos.X -= firePolygon.TransformedSize.X;

			int buffTime = player.buffTime[buffIndex];
			if (needsOil) {
				int oiledIndex = player.FindBuffIndex(BuffID.Oiled);
				if (oiledIndex >= 0) Min(ref buffTime, player.buffTime[oiledIndex]);
			}
			string text = $"{buffTime / 60f:0.0}";
			DrawText(
				Main.spriteBatch,
				text,
				customBuffIndicatorPos + new Vector2((firePolygon.TransformedSize.X - Star_Soldier.Font.MeasureString(text).X) * 0.5f, firePolygon.TransformedSize.Y + 4),
				_color,
				uiScale * 0.85f
			);
			customBuffIndicatorPos.X -= 8;
			AlarmHUD.playAlarm = true;
			return firePolygon.Contains(Main.MouseScreen);
		};
	}
	public class O2HUD : IUISegment {
		static readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4] {
			new(default, Color.White, new(0, 0)),
			new(default, Color.White, new(1, 0)),
			new(default, Color.White, new(0, 1)),
			new(default, Color.White, new(1, 1))
		};
		static readonly short[] dices = [
			0, 1, 2,
			1, 3, 2
		];
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			bool doOutlines = OriginAccessibilityConfig.ItemSpecificOptions.StarSoldier_HUDOutlines;
			Player player = Main.LocalPlayer;
			if (player.breath <= 0) AlarmHUD.playAlarm = true;
			if (player.breath != player.breathMax) { //O2 bar
				Vector2 position = Anchors.bottomLeft;
				Vector2 width = new(8, 0);
				Vector2 height = new(0, -128);
				Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
				Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
				Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
				if (doOutlines) {
					for (int i = 0; i < vertices.Length; i++) {
						Vector2 pos = position;
						if (i % 2 != 0) pos += height - Vector2.UnitY * 2;
						else pos.Y += 2;
						if (i >= 2) pos += width + Vector2.UnitX * 2;
						else pos.X -= 2;
						vertices[i].Position = new(pos, 0);
						vertices[i].Color = Color.OrangeRed.MultiplyRGB(new(100, 100, 100));
					}

					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, 4);
					Anchors.bottomLeft.X += 2;
				}

				for (int i = 0; i < vertices.Length; i++) {
					Vector2 pos = position;
					if (i % 2 != 0) pos += height;
					if (i >= 2) pos += width;
					vertices[i].Position = new(pos, 0);
					vertices[i].Color = Color.White;
				}

				healthBarShader.Apply(null,
					uColor,
					uSecondaryColor,
					uSaturation with { Value = player.breath / (float)player.breathMax }
				);
				Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, 4);
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				Anchors.bottomLeft.X += 8;
				Anchors.bottomLeft.X += DrawText(spriteBatch,
					"O2",
					position + width + height + Vector2.UnitX * 4,
					Color.OrangeRed,
					uiScale * 0.85f
				).X;
				Anchors.bottomLeft.X += 8;
			}
		}
	}
	public class LavaHUD : IUISegment {
		static readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4] {
			new(default, Color.White, new(0, 0)),
			new(default, Color.White, new(1, 0)),
			new(default, Color.White, new(0, 1)),
			new(default, Color.White, new(1, 1))
		};
		static readonly short[] dices = [
			0, 1, 2,
			1, 3, 2
		];
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			bool doOutlines = OriginAccessibilityConfig.ItemSpecificOptions.StarSoldier_HUDOutlines;
			Player player = Main.LocalPlayer;
			if (player.lavaImmune) return;
			if (player.lavaRose && player.ashWoodBonus) return;
			float progress = player.lavaMax > 0 ? 1 - player.lavaTime / (float)player.lavaMax : player.lavaWet.ToInt();
			if (progress > 0) { //Lava Charm bar
				if (player.lavaWet && progress >= 0.9f) AlarmHUD.playAlarm = true;
				Vector2 position = Anchors.bottomLeft;
				Vector2 width = new(8, 0);
				Vector2 height = new(0, -128);
				Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
				Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
				Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
				if (doOutlines) {
					for (int i = 0; i < vertices.Length; i++) {
						Vector2 pos = position;
						if (i % 2 != 0) pos += height - Vector2.UnitY * 2;
						else pos.Y += 2;
						if (i >= 2) pos += width + Vector2.UnitX * 2;
						else pos.X -= 2;
						vertices[i].Position = new(pos, 0);
						vertices[i].Color = Color.OrangeRed.MultiplyRGB(new(100, 100, 100));
					}

					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, 4);
					Anchors.bottomLeft.X += 2;
				}

				for (int i = 0; i < vertices.Length; i++) {
					Vector2 pos = position;
					if (i % 2 != 0) pos += height;
					if (i >= 2) pos += width;
					vertices[i].Position = new(pos, 0);
					vertices[i].Color = Color.White;
				}

				healthBarShader.Apply(null,
					uColor,
					uSecondaryColor,
					uSaturation with { Value = progress }
				);
				Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, 4);
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				Anchors.bottomLeft.X += 8;
				Anchors.bottomLeft.X += DrawText(spriteBatch,
					"°A",
					position + width + height + Vector2.UnitX * 4,
					Color.OrangeRed,
					uiScale * 0.85f
				).X;
				Anchors.bottomLeft.X += 8;
			}
		}
	}
	public class TargetingHUD : IUISegment {
		readonly (Vector2 pos, Vector4 dimensions)[] lines = [
			(Vector2.Zero, new(1, 0, 0, 2)),
			(Vector2.Zero, new(0, 1, 2, 0)),
			(Vector2.UnitX, new(1, 0, 0, 2)),
			(Vector2.UnitX, new(0, 1, 2, 0)),
			(Vector2.UnitY, new(1, 0, 0, 2)),
			(Vector2.UnitY, new(0, 1, 2, 0)),
			(Vector2.One, new(1, 0, 0, 2)),
			(Vector2.One, new(0, 1, 2, 0)),
			(Vector2.One * 0.5f, new(0, 0, 4, 4)),
		];
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			const int offset_size = 8;
			Player player = Main.LocalPlayer;
			float nearestDist = float.PositiveInfinity;
			Entity nearestEntity = default;
			Rectangle rect = new(0, 0, 1, 1);
			foreach (NPC npc in Main.ActiveNPCs) {
				Rectangle hitbox = npc.Hitbox;
				if (hitbox.Width == 0 || hitbox.Height == 0) continue;
				Color color = Color.Lime;
				if (!npc.friendly && !NPCID.Sets.CountsAsCritter[npc.type]) {
					if (Sets.OverrideDontDakeDamageTargeting[npc.type] ?? npc.dontTakeDamage) continue;
					color = Color.OrangeRed;
				}

				if (npc == handler.LockOnTarget) color = Color.Red;
				if (npc.dontTakeDamage) color = Color.DimGray;

				GetDrawPositions(hitbox, out Vector4 actualSize, out Vector4 paddedSize);
				if (!OriginExtensions.Intersects(actualSize.XY(), actualSize.ZW(), Vector2.Zero, Main.ScreenSize.ToVector2())) continue;
				float dist = Main.MouseScreen.Clamp(actualSize.XY(), actualSize.XY() + actualSize.ZW()).DistanceSQ(Main.MouseScreen);

				for (int i = 0; i < lines.Length; i++) {
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						paddedSize.XY() + lines[i].pos * paddedSize.ZW(),
						rect,
						color,
						0,
						lines[i].pos,
						paddedSize.ZW() * lines[i].dimensions.XY() / 3 + uiScale * lines[i].dimensions.ZW(),
						SpriteEffects.None,
					0);
				}
				if (NPCID.Sets.ProjectileNPC[npc.type]) continue;
				if (npc.realLife != -1 && npc.realLife != npc.whoAmI) continue;
				if (Minimize(ref nearestDist, dist)) nearestEntity = npc;
			}
			foreach (Player playerTarget in Main.ActivePlayers) {
				if (playerTarget == player) continue;
				Rectangle hitbox = playerTarget.Hitbox;
				if (hitbox.Width == 0 || hitbox.Height == 0) continue;

				Color color = playerTarget.InOpposingTeam(player) ? Color.OrangeRed : Color.Lime;
				if (playerTarget == handler.LockOnTarget) color = Color.Red;

				GetDrawPositions(hitbox, out Vector4 actualSize, out Vector4 paddedSize);
				if (!OriginExtensions.Intersects(actualSize.XY(), actualSize.ZW(), Vector2.Zero, Main.ScreenSize.ToVector2())) continue;
				float dist = Main.MouseScreen.Clamp(actualSize.XY(), actualSize.XY() + actualSize.ZW()).DistanceSQ(Main.MouseScreen);

				for (int i = 0; i < lines.Length; i++) {
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						paddedSize.XY() + lines[i].pos * paddedSize.ZW(),
						rect,
						color,
						0,
						lines[i].pos,
						paddedSize.ZW() * lines[i].dimensions.XY() / 3 + uiScale * lines[i].dimensions.ZW(),
						SpriteEffects.None,
					0);
				}
				if (Minimize(ref nearestDist, dist)) nearestEntity = playerTarget;
			}
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.ModProjectile is not IArtifactMinion) continue;
				Rectangle hitbox = projectile.Hitbox;
				if (hitbox.Width == 0 || hitbox.Height == 0) continue;

				Color color = (projectile.hostile || Main.player[projectile.owner].InOpposingTeam(player)) ? Color.OrangeRed : Color.Lime;
				if (projectile == handler.LockOnTarget) color = Color.Red;

				GetDrawPositions(hitbox, out Vector4 actualSize, out Vector4 paddedSize);
				if (!OriginExtensions.Intersects(actualSize.XY(), actualSize.ZW(), Vector2.Zero, Main.ScreenSize.ToVector2())) continue;
				float dist = Main.MouseScreen.Clamp(actualSize.XY(), actualSize.XY() + actualSize.ZW()).DistanceSQ(Main.MouseScreen);

				for (int i = 0; i < lines.Length; i++) {
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						paddedSize.XY() + lines[i].pos * paddedSize.ZW(),
						rect,
						color,
						0,
						lines[i].pos,
						paddedSize.ZW() * lines[i].dimensions.XY() / 3 + uiScale * lines[i].dimensions.ZW(),
						SpriteEffects.None,
					0);
				}
				if (Minimize(ref nearestDist, dist)) nearestEntity = projectile;
			}
			Entity displayEntity = handler.LockOnTarget ?? nearestEntity;
			if (displayEntity is not null) {
				if (Keybindings.StarSoldierLockOn.JustPressed) {
					if (nearestEntity is null || nearestEntity == handler.LockOnTarget || nearestDist > 16 * 16 * 15 * 15) {
						handler.LockOnTarget = null;
					} else {
						handler.LockOnTarget = nearestEntity;
						SoundEngine.PlaySound(Origins.Sounds.ScannerPing.WithPitchRange(0.7f, 0.8f).WithVolume(0.3f), player.Center);
						//SoundEngine.PlaySound(SoundID.Chat.WithPitch(1.8f).WithVolume(0.6f), player.Center);
						//SoundEngine.PlaySound(SoundID.Item91.WithPitch(1.6f).WithVolume(0.6f), player.Center);
					}
				}

				StringBuilder builder = new();
				Color color = Color.OrangeRed;
				switch (displayEntity) {
					case NPC npc:
					builder.AppendLine(npc.GivenOrTypeName);
					if (!npc.dontTakeDamage) builder.Append($"{npc.GetLifePercent():P0}");
					if (npc.friendly || NPCID.Sets.CountsAsCritter[npc.type]) color = Color.Lime;
					break;

					case Player playerTarget:
					builder.AppendLine(playerTarget.name);
					builder.Append($"{playerTarget.statLife / (float)playerTarget.statLifeMax2:P0}");
					if (!player.InOpposingTeam(playerTarget)) color = Color.Lime;
					break;

					case Projectile projectile:
					if (projectile.ModProjectile is not IArtifactMinion artifact) break;
					builder.AppendLine(projectile.Name);
					builder.Append($"{artifact.Life / artifact.MaxLife:P0}");
					if (!projectile.hostile && !Main.player[projectile.owner].InOpposingTeam(player)) color = Color.Lime;
					break;
				}
				if (handler.LockOnTarget is not null) color = Color.Red;
				string text = builder.ToString().Trim();
				GetDrawPositions(displayEntity.Hitbox, out _, out Vector4 paddedSize);
				DrawText(spriteBatch,
					text,
					paddedSize.XY() + new Vector2(paddedSize.Z, 0),
					color,
					uiScale
				);
			}
			void GetDrawPositions(Rectangle hitbox, out Vector4 actualSize, out Vector4 paddedSize) {
				hitbox.X -= (int)Main.screenPosition.X;
				hitbox.Y -= (int)Main.screenPosition.Y;
				Vector2 hitboxPos = hitbox.TopLeft().Transform(Main.GameViewMatrix.TransformationMatrix);
				Vector2 hitboxSize = hitbox.BottomRight().Transform(Main.GameViewMatrix.TransformationMatrix) - hitboxPos;
				actualSize = new(hitboxPos, hitboxSize.X, hitboxSize.Y);
				paddedSize = actualSize + new Vector4(uiScale * -offset_size, uiScale.X * offset_size * 2, uiScale.Y * offset_size * 2);
			}
		}
	}
	public class InfoAccessoryHUD : IUISegment {
		static readonly FastStaticFieldInfo<List<InfoDisplay>> InfoDisplays = new(typeof(InfoDisplayLoader), nameof(InfoDisplays));
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			if (Main.playerInventory) return;
			/*DrawText(spriteBatch,
				"""
				ABCDEFGHIJKLMNOPQRSTUVWXYZ
				abcdefghijklmnopqrstuvwxyz
				 !$#"%&'()*+,-./0123456789
				:;<=>?@[\]^_`{|}~
				""",
				new Vector2(8, 8),
				Color.OrangeRed,
				Vector2.One
			);*/
			Player player = Main.LocalPlayer;

			int y = 8;
			int lineSpacing = 22;
			if (Main.screenHeight < 650) lineSpacing = 20;
			if (Main.mapStyle == 1 && Main.mapEnabled) {
				y = Main.screenHeight - 8 - 29;
				lineSpacing *= -1;
			}

			for (int i = 0; i < InfoDisplayLoader.InfoDisplayCount; i++) {
				InfoDisplay display = InfoDisplays.Value[i];
				if (!InfoDisplayLoader.Active(display) || player.hideInfo[i]) {
					continue;
				}
				if (Sets.InfoDisplayOverride[i] is Action<SpriteBatch> customDisplay) {
					customDisplay(spriteBatch);
					continue;
				}
				Color infoTextColor = Main.MouseTextColorReal.MultiplyRGBA(Color.OrangeRed);
				Color infoTextShadowColor = Color.OrangeRed.MultiplyRGB(new(100, 100, 100));
				string text = display.DisplayValue(ref infoTextColor, ref infoTextShadowColor);
				string name = "";
				switch (display) {
					case WatchesInfoDisplay: {
						double time = Main.time;
						if (!Main.dayTime) {
							// if it's night add this number
							time += 54000.0;
						}

						// Divide by seconds in a day * 24
						time = (time / 86400.0) * 24.0;
						// Dunno why we're taking 19.5. Something about hour formatting
						time = time - 7.5 - 12.0;
						// Format in readable time
						if (time < 0.0) {
							time += 24.0;
						}

						int intTime = (int)time;
						// Get the decimal points of time.
						double deltaTime = time - intTime;
						int seconds = ((int)(deltaTime * 60.0 * 60.0) % 60);
						// multiply them by 60. Minutes, probably
						deltaTime = (int)(deltaTime * 60.0);
						text = $"{intTime:0#}:{(int)deltaTime:0#}:{seconds:0#}";
						break;
					}
					case MetalDetectorInfoDisplay: {
						if (Main.SceneMetrics.bestOre <= 0) {
							continue;
						} else {
							int baseOption = 0;
							int tileType = Main.SceneMetrics.bestOre;
							if (Main.SceneMetrics.ClosestOrePosition.HasValue) {
								Point pos = Main.SceneMetrics.ClosestOrePosition.Value;
								Tile tile = Framing.GetTileSafely(pos);
								if (tile.HasTile) {
									MapHelper.GetTileBaseOption(pos.X, pos.Y, tile.TileType, tile, ref baseOption);
									tileType = tile.TileType;
									if (TileID.Sets.BasicChest[tileType] || TileID.Sets.BasicChestFake[tileType]) {
										baseOption = 0;
									}
								}
							}
							text = Language.GetTextValue("GameUI.OreDetected", Lang.GetMapObjectName(MapHelper.TileToLookup(tileType, baseOption)));
						}
						break;
					}
					case LifeformAnalyzerInfoDisplay: {
						const float maxDist = 1300;
						int bestRarity = 0;
						int index = -1;
						if (player.accCritterGuideCounter <= 0) {
							player.accCritterGuideCounter = 15;
							foreach (NPC npc in Main.ActiveNPCs) {
								if (npc.rarity > bestRarity && npc.Center.WithinRange(player.Center, maxDist)) {
									index = npc.whoAmI;
									bestRarity = npc.rarity;
								}
							}
							player.accCritterGuideNumber = (byte)index;
						} else {
							player.accCritterGuideCounter--;
							index = player.accCritterGuideNumber;
						}
						NPC rarestNPC = Main.npc.GetIfInRange(index);
						if ((rarestNPC?.active ?? false) && rarestNPC.rarity > 0) text = Main.npc[index].GivenOrTypeName;
						break;
					}
					case RadarInfoDisplay: {
						const float maxDist = 2000;
						if (player.accThirdEyeCounter <= 0) {
							player.accThirdEyeNumber = 0;
							player.accThirdEyeCounter = 15;
							foreach (NPC npc in Main.ActiveNPCs) {
								if (!npc.friendly && npc.damage > 0 && npc.lifeMax > 5 && !npc.dontCountMe && npc.Center.WithinRange(player.Center, maxDist)) {
									player.accThirdEyeNumber++;
								}
							}
						} else {
							player.accThirdEyeCounter--;
						}
						if (player.accThirdEyeNumber != 0) {
							text = player.accThirdEyeNumber != 1 ? Language.GetTextValue("GameUI.EnemiesNearby", player.accThirdEyeNumber) : Language.GetTextValue("GameUI.OneEnemyNearby");
						}
						break;
					}
					case TallyCounterInfoDisplay: {
						int lastCreatureHit = player.lastCreatureHit;
						if (lastCreatureHit > 0) {
							text = Lang.GetNPCNameValue(Item.BannerToNPC(lastCreatureHit)) + ": " + NPC.killCount[lastCreatureHit];
						}
						break;
					}
					case DPSMeterInfoDisplay: {
						player.checkDPSTime();
						if (player.getDPS() != 0) {
							text = Language.GetTextValue("GameUI.DPS", player.getDPS());
						}
						break;
					}
					case StopwatchInfoDisplay: {
						float currentSpeed = (player.velocity + player.instantMovementAccumulatedThisFrame).Length();
						int speedSlices = (int)(1f + currentSpeed * 6f);
						Min(ref speedSlices, player.speedSlice.Length);

						float averageSpeed = 0f;
						for (int j = speedSlices - 1; j > 0; j--) player.speedSlice[j] = player.speedSlice[j - 1];
						player.speedSlice[0] = currentSpeed;
						for (int m = 0; m < player.speedSlice.Length; m++) {
							if (m < speedSlices) {
								averageSpeed += player.speedSlice[m];
							} else {
								player.speedSlice[m] = averageSpeed / speedSlices;
							}
						}
						averageSpeed /= speedSlices;
						int pixelsPerMile = 42240;
						int ticksPerHour = 216000;
						float num20 = averageSpeed * ticksPerHour / pixelsPerMile;
						if (!player.merman && !player.ignoreWater) {
							float multiplier = 1f;
							bool[] moddedWet = player.GetModdedWetArray();
							for (int j = 0; j < moddedWet.Length; j++) {
								if (moddedWet[j]) {
									LiquidLoader.StopWatchMPHMultiplier(i, ref multiplier);
									goto skipVanillaWet;
								}
							}
							if (player.shimmerWet) multiplier = 0.375f;
							else if (player.honeyWet) multiplier = 0.25f;
							else if (player.wet) multiplier = 0.5f;
							skipVanillaWet:;
						}
						text = Language.GetTextValue("GameUI.Speed", Math.Round(num20));
						break;
					}
					case CompassInfoDisplay: {
						int xPos = (int)((player.position.X + (float)(player.width / 2)) * 2f / 16f - (float)Main.maxTilesX);
						text = (xPos > 0) ? Language.GetTextValue("GameUI.CompassEast", xPos) : ((xPos >= 0) ? Language.GetTextValue("GameUI.CompassCenter") : Language.GetTextValue("GameUI.CompassWest", -xPos));
						break;
					}
				}
#pragma warning disable CS0618 // Type or member is obsolete
				InfoDisplayLoader.ModifyDisplayValue(display, ref text);
				InfoDisplayLoader.ModifyDisplayName(display, ref name);
				InfoDisplayLoader.ModifyDisplayColor(display, ref infoTextColor, ref infoTextShadowColor);
#pragma warning restore CS0618 // Type or member is obsolete
				InfoDisplayLoader.ModifyDisplayParameters(display, ref text, ref name, ref infoTextColor, ref infoTextShadowColor);
				if (string.IsNullOrEmpty(text)) continue;

				Vector2 size = Star_Soldier.Font.MeasureString(text);
				infoTextColor = infoTextShadowColor * 2.55f;

				DrawText(spriteBatch,
					text,
					new Vector2(Main.screenWidth - size.X - 8, y),
					infoTextColor,
					Vector2.One
				);
				y += lineSpacing;
			}
		}
		public static void DrawDepthMeter(SpriteBatch spriteBatch) {
			Player player = Main.LocalPlayer;
			int depth = -(int)(((player.position.Y + player.height) * 2f / 16f) - Main.worldSurface * 2.0);

			Vector2 size = Star_Soldier.Font.MeasureString(depth.ToString());

			DrawText(spriteBatch,
				$"{depth}'",
				Anchors.rightCenter - new Vector2(size.X + 8 + 52, 0),
				Color.OrangeRed,
				Vector2.One
			);

			Vector2 position = Anchors.rightCenter + new Vector2(4, 12);
			DrawData smallTick = new(TextureAssets.MagicPixel.Value, position, new Rectangle(0, 0, 24, 2), Color.OrangeRed) {
				origin = new(24, 1)
			};
			DrawData largeTick = smallTick with { scale = new Vector2(2, 1) };
			for (int i = depth - 200; i < depth + 200; i++) {
				if (i % 20 != 0) continue;
				DrawData data = i % 100 == 0 ? largeTick : smallTick;
				data.color *= 1 - float.Pow((i - depth) / 200f, 4);
				data.position.Y -= i - depth;
				data.Draw(spriteBatch);
			}
			Anchors.rightCenter.X -= 52 + 8 + size.X;
		}
	}
	public class FuelHUD : IUISegment {
		static readonly Polygon fuelBarPoly = Polygon.Import(
			300,
			Vector2.UnitX * 0.5f,
			new(0, 0),
			new(0.05f, 0),
			new(0.95f, 0),
			new(1f, 0),
			new(0.95f, 0.04f),
			new(0.05f, 0.04f)
		);
		float fuelBarPos;
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			bool doOutlines = OriginAccessibilityConfig.ItemSpecificOptions.StarSoldier_HUDOutlines;
			Player player = Main.LocalPlayer;
			MathUtils.LinearSmoothing(ref fuelBarPos, (player.mount._flyTime == player.mount._data.flightTimeMax).Mul(fuelBarPoly.baseSize.Y + doOutlines.Mul(4)), 4f);
			Vector2 pos = new(-fuelBarPos, Main.screenHeight * 0.5f);
			fuelBarPoly.ResetPositions().Rotate(-MathHelper.PiOver2).Scale(uiScale).Translate(pos);
			{ //Fuel bar
				Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
				Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
				Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
				if (doOutlines) {
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					using Polygon.VertexCache _ = fuelBarPoly.ModificationContext();
					for (int i = 0; i < fuelBarPoly.vertices.Length; i++) {
						if (fuelBarPoly.vertices[i].TextureCoordinate.Y == 0) fuelBarPoly.vertices[i].Position.X -= 2;
						else fuelBarPoly.vertices[i].Position.X += 4;
						switch (fuelBarPoly.vertices[i].TextureCoordinate.X) {
							case 0:
							fuelBarPoly.vertices[i].Position.Y += 8;
							break;
							case 0.05f:
							fuelBarPoly.vertices[i].Position.Y += 2;
							break;
							case 0.95f:
							fuelBarPoly.vertices[i].Position.Y -= 2;
							break;
							case 1:
							fuelBarPoly.vertices[i].Position.Y -= 8;
							break;
						}
						fuelBarPoly.vertices[i].Color = Color.OrangeRed.MultiplyRGB(new(100, 100, 100));
					}
					fuelBarPoly.Draw();
					pos.X += 4;
				}
				healthBarShader.Apply(null,
					uColor,
					uSecondaryColor,
					uSaturation with { Value = player.mount._flyTime / (float)player.mount._data.flightTimeMax }
				);
				fuelBarPoly.Draw();
				pos.X += fuelBarPoly.TransformedSize.X;
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}
			if (handler.maxDashes > 0) {
				int height = 275;
				int segment = (int)((height * 0.75f) / handler.maxDashes);
				pos.X += 2;
				if (doOutlines) pos.X += 2;
				int visualDashCount = handler.availableDashes;
				if (handler.dashTime > 0) visualDashCount--;
				for (int i = 0; i < handler.maxDashes; i++) {
					Vector2 origin = new(0, segment * 0.5f + (height * ((i + 1f) / (handler.maxDashes + 1f) - 0.5f)) * 1.5f);
					if (doOutlines) {
						spriteBatch.Draw(
							TextureAssets.MagicPixel.Value,
							pos,
							new Rectangle(0, 0, 12, segment + 4),
							Color.OrangeRed.MultiplyRGB(new(100, 100, 100)),
							0,
							origin + new Vector2(2, 2),
							1,
							SpriteEffects.None,
						0);
					}
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						pos,
						new Rectangle(0, 0, 8, segment),
						i >= visualDashCount ? Color.Black : Color.OrangeRed,
						0,
						origin,
						1,
						SpriteEffects.None,
					0);
					if (i == handler.availableDashes && handler.dashTime <= 0) {
						int cooldown = (int)(segment * (1 - handler.dashCooldown / (float)handler.dashCooldownMax));
						spriteBatch.Draw(
							TextureAssets.MagicPixel.Value,
							pos + Vector2.UnitY * (segment - cooldown),
							new Rectangle(0, 0, 8, cooldown),
							Color.White,
							0,
							origin,
							1,
							SpriteEffects.None,
						0);
					}
				}
				pos.X += 8;
			}
		}
	}
	public class AlarmHUD : IUISegment {
		public static bool playAlarm;
		static float alarmVolumeMult;
		static SlotId alarmSoundSlot;

		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			if (playAlarm) {
				alarmSoundSlot.PlaySoundIfInactive(Origins.Sounds.Alarm1, updateCallback: sound => {
					if (!Main.LocalPlayer.mount.IsMount<Star_Soldier>()) {
						playAlarm = false;
						alarmVolumeMult = 0;
						return false;
					}
					MathUtils.LinearSmoothing(ref alarmVolumeMult, playAlarm.ToInt(), 1f / 15);
					sound.Volume = alarmVolumeMult;
					sound.Pitch = 1.5f;
					return alarmVolumeMult > 0;
				});
			}
		}
	}
}

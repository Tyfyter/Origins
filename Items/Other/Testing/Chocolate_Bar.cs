using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Tyfyter.Utils.KinematicUtils;

namespace Origins.Items.Other.Testing {
	public class Chocolate_Bar : TestingItem {
		const float upperLegAngle = 0.1171116f;
		const float lowerLegAngle = -0.1504474f;
		const float upperLegLength = 34.2f;
		const float lowerLegLength = 33.4f;
		Arm arm;
		Arm arm2;
		Vector2 target = Vector2.Zero;
		Vector2 target2 = Vector2.Zero;
		int mode;
		const int modeCount = 10;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			//item.name = "jfdjfrbh";
			Item.width = 16;
			Item.height = 26;
			Item.value = Item.sellPrice(platinum: 9001);
			Item.rare = ItemRarityID.Green;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 10;
			Item.useTime = 10;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool? UseItem(Player player)/* Suggestion: Return null instead of false */ {
			if (Main.myPlayer == player.whoAmI) {
				if (player.altFunctionUse == 2) {
					if (player.controlSmart) {
						mode = (mode + modeCount - 1) % modeCount;
					} else {
						mode = (mode + 1) % modeCount;
					}
					arm = null;
					switch (mode) {
						case 0:
						arm = new Arm() {
							bone0 = new PolarVec2(upperLegLength, upperLegAngle),
							bone1 = new PolarVec2(lowerLegLength, lowerLegAngle)
						};
						arm2 = new Arm() {
							bone0 = new PolarVec2(upperLegLength, upperLegAngle),
							bone1 = new PolarVec2(lowerLegLength, lowerLegAngle)
						};
						break;
					}
				} else {
					switch (mode) {
						case 1:
						case 0:
						if (player.controlSmart) {
							target2 = Main.MouseWorld;
						} else {
							target = Main.MouseWorld;
						}
						break;
					}
				}
				return true;
			}
			return false;
		}
		public override void HoldItem(Player player) {
			switch (mode) {
				case 2: {
					Point topLeft = Main.screenPosition.ToTileCoordinates();
					Point bottomRight = (Main.screenPosition + Main.ScreenSize.ToVector2()).ToTileCoordinates();
					bool[,] grid = CollisionExtensions.GeneratePathfindingGrid(topLeft, bottomRight, 1, 1);
					Point[] path = CollisionExtensions.GridBasedPathfinding(
						grid,
						(Main.LocalPlayer.MountedCenter - Main.screenPosition).ToTileCoordinates(),
						Main.MouseScreen.ToTileCoordinates()
					);
					for (int i = 0; i < path.Length; i++) {
						Dust.NewDustPerfect(path[i].ToWorldCoordinates() + Main.screenPosition - new Vector2(Main.screenPosition.X % 16, Main.screenPosition.Y % 16), 6, Vector2.Zero).noGravity = true;
					}
				}
				break;
			}
		}
		public void DrawAnimations(ref PlayerDrawSet drawInfo) {
			AutoCastingAsset<Texture2D> upperLegTexture = Mod.Assets.Request<Texture2D>("NPCs/Fiberglass/Fiberglass_Threader_Leg_Upper");
			AutoCastingAsset<Texture2D> lowerLegTexture = Mod.Assets.Request<Texture2D>("NPCs/Fiberglass/Fiberglass_Threader_Leg_Lower");
			AutoCastingAsset<Texture2D> pixelTexture = Mod.Assets.Request<Texture2D>("Projectiles/Pixel");
			switch (mode) {
				case 0: {
					Player player = Main.LocalPlayer;
					Vector2 start = player.Right;
					arm ??= new Arm() {
						bone0 = new PolarVec2(upperLegLength, upperLegAngle),
						bone1 = new PolarVec2(lowerLegLength, lowerLegAngle)
					};
					arm2 ??= new Arm() {
						bone0 = new PolarVec2(upperLegLength, upperLegAngle),
						bone1 = new PolarVec2(lowerLegLength, lowerLegAngle)
					};
					arm.start = start;
					float[] targets = arm.GetTargetAngles(target);
					OriginExtensions.AngularSmoothing(ref arm.bone0.Theta, targets[0], 0.2f);
					OriginExtensions.AngularSmoothing(ref arm.bone1.Theta, targets[1], 0.2f);

					Vector2 screenStart = arm.start - Main.screenPosition;
					drawInfo.DrawDataCache.Add(new DrawData(upperLegTexture, screenStart, null, Color.White, arm.bone0.Theta, new Vector2(3, 9), 1f, SpriteEffects.None, 0));
					drawInfo.DrawDataCache.Add(new DrawData(lowerLegTexture, screenStart + (Vector2)arm.bone0, null, Color.White, arm.bone0.Theta + arm.bone1.Theta, new Vector2(4, 8), 1f, SpriteEffects.None, 0));

					Vector2 start2 = player.Left;

					arm2.start = start2;
					float[] targets2 = arm2.GetTargetAngles(target2, true);
					OriginExtensions.AngularSmoothing(ref arm2.bone0.Theta, targets2[0], 0.2f);
					OriginExtensions.AngularSmoothing(ref arm2.bone1.Theta, targets2[1], 0.2f);

					Vector2 screenStart2 = arm2.start - Main.screenPosition;
					drawInfo.DrawDataCache.Add(new DrawData(upperLegTexture, screenStart2, null, Color.White, arm2.bone0.Theta, new Vector2(3, 3), 1f, SpriteEffects.FlipVertically, 0));
					drawInfo.DrawDataCache.Add(new DrawData(lowerLegTexture, screenStart2 + (Vector2)arm2.bone0, null, Color.White, arm2.bone0.Theta + arm2.bone1.Theta, new Vector2(4, 0), 1f, SpriteEffects.FlipVertically, 0));

					/**Vector2 diff = (target - start);
					float dist = diff.Length() / (upperLegLength + lowerLegLength);
					float minLength = 0.7f;
					float maxLength = 0.9f;
					if (player.controlUp) {
						minLength = 1f;
						maxLength = 1f;
					}else if (player.controlDown) {
						minLength = 0.3f;
						maxLength = 0.7f;
					}
					if (dist != 0) {
						if (dist < minLength) {
							player.velocity -= diff.SafeNormalize(Vector2.Zero) / (dist * 3);
						} else if (dist < 1f && dist > maxLength) {
							player.velocity += diff.SafeNormalize(Vector2.Zero) * (dist * 8 - 4);
						}
					}
					Vector2 diff2 = (target2 - start2);
					float dist2 = diff2.Length() / (upperLegLength + lowerLegLength);
					if (dist2 != 0) {
						if (dist2 < minLength) {
							player.velocity -= diff.SafeNormalize(Vector2.Zero) / (dist2 * 3);
						} else if (dist2 < 1f && dist2 > maxLength) {
							player.velocity += diff.SafeNormalize(Vector2.Zero) * (dist2 * 8 - 4);
						}
					}*/
				}
				break;
				case 1: {
					//0.65-1 rgb, 0.325-0.5 a
					float hp = 0.65f;//Main.LocalPlayer.statLife / (float)Main.LocalPlayer.statLifeMax2;
					drawInfo.DrawDataCache.Add(new DrawData(Mod.Assets.Request<Texture2D>("NPCs/ICARUS/ICARUS_Shield").Value, Main.MouseScreen, null, new Color(hp, hp, hp, hp / 2), 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0));
				}
				break;
			}
		}
	}
}

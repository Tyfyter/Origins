using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Strange_Computer : ModItem, ICustomWikiStat {
		internal static bool drawingStrangeLine = false;
		internal static List<int> projectiles = [];
		public string[] Categories => [
			"Info",
			"LoreItem",
			"Hidden"
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		static short glowmask;
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 20);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = CursedRarity.ID;
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.strangeComputer = true;
			originPlayer.strangeComputerColor = Color.Blue;
		}
		internal static void DrawStrangeLine() {
			Player player = Main.LocalPlayer;
			if (!player.GetModPlayer<OriginPlayer>().strangeComputer) return;
			drawingStrangeLine = true;
			List<List<Vector2>> positionss = [];
			List<List<float>> rotationss = [];
			int netMode = Main.netMode;
			Main.netMode = NetmodeID.SinglePlayer;// disable syncing
			projectiles.Clear();
			try {
				Item sItem = player.HeldItem;
				if (true) {
					bool canShoot = false;
					int projToShoot = sItem.shoot, usedAmmoItemId = 0;
					float speed = sItem.shootSpeed;
					if (sItem.useAmmo > 0) {
						canShoot = player.PickAmmo(sItem, out projToShoot, out speed, out _, out _, out usedAmmoItemId, false);
					} else {
						canShoot = true;
					}
					if (canShoot) {
						EntitySource_ItemUse_WithAmmo source = new(player, sItem, usedAmmoItemId);
						int discardi = 0;
						float discardf = 0;
						Vector2 pointPoisition = player.RotatedRelativePoint(player.MountedCenter);
						Vector2 velocity = (Main.MouseWorld - pointPoisition).SafeNormalize(default) * speed;
						CombinedHooks.ModifyShootStats(player, sItem, ref pointPoisition, ref velocity, ref projToShoot, ref discardi, ref discardf);
						if (CombinedHooks.Shoot(player, sItem, source, pointPoisition, velocity, projToShoot, 0, 0)) {
							Projectile.NewProjectile(source, pointPoisition, velocity, projToShoot, 0, 0);
						}
						for (int i = 0; i < projectiles.Count; i++) {
							if (!projectiles.IndexInRange(i)) continue;
							Projectile projectile = Main.projectile[projectiles[i]];
							List<Vector2> positions = [];
							List<float> rotations = [];
							int updates = 1000;
							while (projectile.active && --updates > 0) {
								positions.Add(projectile.Center);
								projectile.Update(i);
								rotations.Add((projectile.Center - positions[^1]).ToRotation());
								if (!WorldGen.InWorld((int)projectile.position.X / 16, (int)projectile.position.Y / 16)) break;
							}
							if (positions.Count > 0) {
								positions.Add(positions[^1] + projectile.oldVelocity);
								rotations.Add(rotations[^1]);
								positionss.Add(positions);
								rotationss.Add(rotations);
							}
						}
					}
				}
				Strange_Line_Drawer drawer = default;
				drawer.color = player.GetModPlayer<OriginPlayer>().strangeComputerColor;
				for (int i = 0; i < positionss.Count; i++) {
					drawer.Draw(positionss[i].ToArray(), rotationss[i].ToArray());
				}
			} finally {}
			Main.netMode = netMode;
			drawingStrangeLine = false;
		}
		public struct Strange_Line_Drawer {
			private static readonly VertexStrip _vertexStrip = new();
			public Color color;
			public readonly void Draw(Vector2[] pos, float[] rot) {
				if (pos.Length <= 1) return;
				MiscShaderData miscShaderData = GameShaders.Misc["Origins:Beam"];
				miscShaderData.UseImage0(TextureAssets.MagicPixel);
				miscShaderData.Apply();
				_vertexStrip.PrepareStrip(pos, rot, StripColors, StripWidth, -Main.screenPosition, pos.Length, includeBacksides: true);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}

			private readonly Color StripColors(float progressOnStrip) => color;

			private readonly float StripWidth(float progressOnStrip) => 1;
		}
	}
}

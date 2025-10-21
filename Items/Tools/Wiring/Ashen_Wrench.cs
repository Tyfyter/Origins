using CalamityMod.Items.Potions.Alcohol;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Melee;
using Origins.UI;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;

namespace Origins.Items.Tools.Wiring {
	public class Ashen_Wrench : ModItem, IWireTool {
		public override string Texture => "Terraria/Images/Item_" + ItemID.YellowWrench;
		public IEnumerable<WireMode> Modes => WireModeLoader.GetSorted(WireMode.Sets.AshenWires);
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WireKite);
			Item.color = Color.Chocolate;
			Item.shoot = ModContent.ProjectileType<Mod_Wire_Channel>();
			Item.channel = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.ownedProjectileCounts[type] > 0) return false;
			Projectile.NewProjectile(
				source,
				player.Center,
				default,
				type,
				0,
				0,
				-1,
				Player.tileTargetX,
				Player.tileTargetY
			);
			return false;
		}
	}
	public class Ashen_Grand_Design : ModItem, IWireTool {
		public override string Texture => "Terraria/Images/Item_" + ItemID.WireKite;
		public IEnumerable<WireMode> Modes => WireModeLoader.GetSorted(WireMode.Sets.NormalWires).Concat(WireModeLoader.GetSorted(WireMode.Sets.AshenWires));
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WireKite);
			Item.color = Color.Chocolate;
			Item.shoot = ModContent.ProjectileType<Mod_Wire_Channel>();
			Item.channel = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.ownedProjectileCounts[type] > 0) return false;
			Projectile.NewProjectile(
				source,
				player.Center,
				default,
				type,
				0,
				0,
				-1,
				Player.tileTargetX,
				Player.tileTargetY
			);
			return false;
		}
	}
	public class Mod_Wire_Channel : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WireKite;
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (!Projectile.IsLocallyOwned()) {
				Projectile.timeLeft = 5;
				return;
			}
			Player player = Main.player[Projectile.owner];
			if (player.HeldItem?.ModItem is not IWireTool wireTool) {
				Projectile.Kill();
				return;
			}
			if (player.channel) {
				Projectile.timeLeft = 5;
			} else {
				DoWireAction(
					player, 
					new((int)Projectile.ai[0], (int)Projectile.ai[1]),
					new(Player.tileTargetX, Player.tileTargetY),
					Wire_Mode_Kite.Cutter,
					wireTool.Modes.Where(i => Wire_Mode_Kite.EnabledWires[i.Type]).ToArray()
				);
				Projectile.Kill();
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);
		public override bool PreDraw(ref Color lightColor) {
			if (!Projectile.IsLocallyOwned()) return false;
			Player player = Main.player[Projectile.owner];
			if (player.HeldItem?.ModItem is not IWireTool wireTool) {
				Projectile.Kill();
				return false;
			}
			Rectangle screen = new(-16 * 5, -16 * 5, Main.screenWidth + 16 * 10, Main.screenHeight + 16 * 10);
			bool hasDrawn = false;
			Point[] positions = GetWirePositions(player, new((int)Projectile.ai[0], (int)Projectile.ai[1]), new(Player.tileTargetX, Player.tileTargetY));
			Color color = new(127, 127, 127, 0);
			Color outerColor = Wire_Mode_Kite.Cutter ? new(50, 50, 50, 255) : new(255, 255, 255, 0);
			int colorsCount = 0;
			bool hasExtra = false;
			foreach (WireMode mode in wireTool.Modes) {
				if (!Wire_Mode_Kite.EnabledWires[mode.Type]) continue;
				if (mode.WireKiteColor is Color innerColor) color = Color.Lerp(color, innerColor, 1f / ++colorsCount);
				hasExtra |= mode.IsExtra;
			}
			color *= 2;
			color.A = (byte)((255 - Math.Min(color.R + color.G + color.B, 255)) / 2);
			for (int i = 0; i < positions.Length; i++) {
				Vector2 screenPos = positions[i].ToWorldCoordinates(0, 0) - Main.screenPosition;
				if (hasDrawn.TrySet(screen.Contains(screenPos)) && !hasDrawn) {
					break;
				}
				if (!hasDrawn) continue;
				Rectangle wireFrame = new(0, 0, 16, 16);
				void ModifyFrame(int index) {
					if (!positions.IndexInRange(index)) return;
					wireFrame.X += (positions[index] - positions[i]) switch {
						(0, -1) => 18,
						(1, 0) => 36,
						(0, 1) => 72,
						(-1, 0) => 144,
						_ => 0
					};
				}
				ModifyFrame(i - 1);
				ModifyFrame(i + 1);
				if (hasExtra)
					Main.EntitySpriteDraw(TextureAssets.WireUi[11].Value, screenPos, null, outerColor, 0f, Vector2.Zero, 1f, SpriteEffects.None);
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					screenPos,
					wireFrame,
					color,
					0,
					Vector2.Zero,
					1,
					SpriteEffects.None
				);
				wireFrame.Y += 18;
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					screenPos,
					wireFrame,
					outerColor,
					0,
					Vector2.Zero,
					1,
					SpriteEffects.None
				);
			}
			return false;
		}
		static bool HasResearched(int type, Player player) => CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.TryGetValue(type, out int needed) && player.creativeTracker.ItemSacrifices.GetSacrificeCount(type) >= needed;
		public static void DoWireAction(Player player, Point start, Point end, bool cut, WireMode[] modes) {
			Dictionary<int, int> costs = [];
			for (int i = 0; i < modes.Length; i++) {
				if (HasResearched(modes[i].ItemType, player)) continue;
				costs[modes[i].ItemType] = 0;
			}
			foreach (Point pos in GetWirePositions(player, start, end)) {
				bool didAny = false;
				Vector2 worldPos = pos.ToWorldCoordinates(0, 0);
				for (int i = 0; i < modes.Length; i++) {
					if (!cut) {
						if (costs.ContainsKey(modes[i].ItemType) && !player.ConsumeItem(modes[i].ItemType)) continue;
					}
					if (modes[i].SetWire(pos.X, pos.Y, !cut)) {
						didAny |= true;
						SoundEngine.PlaySound(SoundID.Dig, worldPos);
						if (costs.TryGetValue(modes[i].ItemType, out int cost)) costs[modes[i].ItemType] = cost + 1;
					}
				}
				if (didAny && cut) {
					for (int k = 0; k < 5; k++) {
						Dust.NewDust(worldPos, 16, 16, DustID.Adamantite);
					}
				}
			}
			IEntitySource source = player.GetSource_Misc("GrandDesignOrMultiColorWrench");
			foreach (KeyValuePair<int, int> cost in costs) {
				if (cut) {
					if (cost.Value > 0) {
						Item item = new(cost.Key);
						int num = cost.Value;
						while (num > 0) {
							int num2 = item.maxStack;
							if (num < num2) {
								num2 = num;
							}
							Item.NewItem(source, player.Center, cost.Key, num2);
							num -= num2;
						}
					}
				} else {
					if (NetmodeActive.Server) {
						NetMessage.SendData(MessageID.MassWireOperationPay, player.whoAmI, -1, null, cost.Key, cost.Value, player.whoAmI);
					}
				}
			}
		}
		public static Point[] GetWirePositions(Player player, Point start, Point end) {
			Point pos = start;
			Point diff = new(end.X - start.X, end.Y - start.Y);
			Point dir = new(int.Sign(diff.X), int.Sign(diff.Y));
			diff *= dir;
			bool yFirst = player.direction == 1;
			int index = 1;
			Point[] positions = new Point[diff.X + diff.Y + 1];
			positions[0] = start;
			for (int i = yFirst ? diff.Y : diff.X; i > 0; i--) {
				if (yFirst) pos.Y += dir.Y;
				else pos.X += dir.X;
				if (pos != positions[index - 1]) positions[index++] = pos;
			}
			for (int i = !yFirst ? diff.Y : diff.X; i > 0; i--) {
				if (!yFirst) pos.Y += dir.Y;
				else pos.X += dir.X;
				if (pos != positions[index - 1]) positions[index++] = pos;
			}
			return positions;
		}
	}
}

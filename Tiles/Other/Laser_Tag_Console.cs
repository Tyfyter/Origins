using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Reflection;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.ObjectData;
using Terraria.UI;
using Terraria.UI.Chat;
using ThoriumMod.NPCs;
using ThoriumMod.NPCs.BossViscount;

namespace Origins.Tiles.Other {
	public class Laser_Tag_Console : ModTile, IGlowingModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(81, 81, 81), CreateMapEntryName());
			DustType = DustID.Lead;
		}
		public bool? Hardmode => true;
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => CheckInteract(true);
		public override bool RightClick(int i, int j) => CheckInteract(false);
		public static bool CheckInteract(bool justCheck) {
			//if (Main.netMode == NetmodeID.SinglePlayer) return false;
			if (!OriginPlayer.LocalOriginPlayer.laserTagVest) return false;
			if (!LaserTagGameActive) {
				if (!justCheck) {
					Main.LocalPlayer.SetTalkNPC(-1);
					Main.npcChatCornerItem = 0;
					SoundEngine.PlaySound(SoundID.MenuOpen);
					IngameFancyUI.OpenUIState(new Laser_Tag_Rules_UI());
				}
				return true;
			} else if (!Main.LocalPlayer.OriginPlayer().laserTagVest) {
				if (!justCheck) {
					Main.LocalPlayer.SetTalkNPC(-1);
					Main.npcChatCornerItem = 0;
					SoundEngine.PlaySound(SoundID.MenuOpen);
					IngameFancyUI.OpenUIState(new Laser_Tag_Rules_UI());
				}
				return true;
			}
			return false;
		}
		public static ref int LaserTagActiveTeams => ref OriginSystem.Instance.laserTagActiveTeams;
		public static ref int LaserTagActivePlayers => ref OriginSystem.Instance.laserTagActivePlayers;
		public static ref int[] LaserTagTeamPlayers => ref OriginSystem.Instance.laserTagTeamPlayers;
		public static bool AnyLaserTagActive => LaserTagActivePlayers > 0;
		public static bool LaserTagGameActive => AnyLaserTagActive || (LaserTagRules.Time > 0 && LaserTagTimeLeft > -1);
		public static bool LaserTagMultipleTeamsActive {
			get {
				if (LaserTagActiveTeams == 0) return false;
				if (LaserTagActiveTeams == 1) return LaserTagActivePlayers > 1;
				return (LaserTagActiveTeams & (LaserTagActiveTeams - 1)) != 0;
			}
		}
		public static ref Laser_Tag_Rules LaserTagRules => ref OriginSystem.Instance.laserTagRules;
		public static ref int LaserTagTimeLeft => ref OriginSystem.Instance.laserTagTimeLeft;
		public static int[] LaserTagTeamPoints => OriginSystem.Instance.laserTagTeamPoints;
		public static int[] LaserTagTeamHits => OriginSystem.Instance.laserTagTeamHits;
		public static int[] LaserTagTeamGems => OriginSystem.Instance.laserTagTeamGems;
		public static void ScorePoint(Player scorer, bool fromNet = false, int ignoreClient = -1) {
			if (LaserTagRules.Teams) {
				LaserTagTeamPoints[scorer.team]++;
			}
			scorer.OriginPlayer().laserTagPoints++;
			if (fromNet ? (Main.netMode == NetmodeID.Server) : (Main.netMode != NetmodeID.SinglePlayer)) {
				// Forward the changes to the other clients
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.laser_tag_score);
				packet.Write((byte)scorer.whoAmI);
				packet.Send(ignoreClient: ignoreClient);
			}
			if (Main.netMode != NetmodeID.Server && (scorer.whoAmI == Main.myPlayer || !LaserTagRules.HitsArePoints)) {
				SoundEngine.PlaySound(Origins.Sounds.LaserTag.Score, scorer.Center);
				ChatMessageContainerMethods.CreateCustomMessage(
					ChatManager.ParseMessage(Language.GetTextValue("Mods.Origins.Laser_Tag.Score", scorer.name), Main.teamColor[scorer.team])
				);
			}
		}
		//static Time_Radix[] radices;
		public static void ProcessLaserTag() {
			//radices ??= Time_Radix.ParseRadices(Language.GetOrRegister("Mods.Origins.Laser_Tag.LongTime").Value);
			//Debugging.ChatOverhead(Time_Radix.FormatTime(LaserTagTimeLeft, radices));
			LaserTagActiveTeams = 0;
			LaserTagActivePlayers = 0;
			for (int i = 0; i < 6; i++) {
				LaserTagTeamGems[i] = -1;
				LaserTagTeamPlayers[i] = 0;
			}
			foreach (Player player in Main.ActivePlayers) {
				OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
				OriginPlayer.playersByGuid.TryAdd(originPlayer.guid, player.whoAmI);
				if (originPlayer.laserTagVest) {
					LaserTagTeamPlayers[player.team]++;
				}
				if (originPlayer.laserTagVestActive) {
					LaserTagActiveTeams |= 1 << player.team;
					LaserTagActivePlayers++;
					if (LaserTagRules.CTG) {
						for (int i = 0; i < LaserTagTeamGems.Length; i++) {
							if (player.ownedLargeGems[GetLargeGemID(i)]) LaserTagTeamGems[i] = player.whoAmI;
						}
					}
				}
			}
			Color winnerColor = Main.teamColor[0];
			object winner = null;
			if (LaserTagGameActive) {
				foreach (Player player in Main.ActivePlayers) {
					player.hostile = player.OriginPlayer().laserTagVestActive;
				}
				if (Main.netMode == NetmodeID.Server) {
					foreach(LaserTagWinCondition winCondition in LaserTagRules.GetWinConditions()) {
						if (LaserTagRules.Teams) {
							int winTeam = winCondition.GetTeamWinner(LaserTagRules);
							if (winTeam != -1) {
								winnerColor = Main.teamColor[winTeam];
								switch (winTeam) {
									case 1:
									winner = Language.GetOrRegister("Mods.Origins.Laser_Tag.Team_Red");
									break;
									case 2:
									winner = Language.GetOrRegister("Mods.Origins.Laser_Tag.Team_Green");
									break;
									case 3:
									winner = Language.GetOrRegister("Mods.Origins.Laser_Tag.Team_Blue");
									break;
									case 4:
									winner = Language.GetOrRegister("Mods.Origins.Laser_Tag.Team_Yellow");
									break;
									case 5:
									winner = Language.GetOrRegister("Mods.Origins.Laser_Tag.Team_Purple");
									break;
									default:
									winner = "misingno";
									break;
								}
								break;
							}
						} else {
							if (winCondition.GetWinner(LaserTagRules) is Player winPlayer) {
								winner = winPlayer.name;
								break;
							}
						}
					}
				}
				if (LaserTagRules.Time > 0 && --LaserTagTimeLeft <= 0) LaserTagTimeLeft = 0;
			}
			if (winner is not null) {
				if (Main.netMode == NetmodeID.Server) {
					ChatHelper.BroadcastChatMessage(Language.GetOrRegister("Mods.Origins.Laser_Tag.Victory").ToNetworkText(winner), winnerColor);
					ModPacket packet = Origins.instance.GetPacket();
					packet.Write(Origins.NetMessageType.end_laser_tag);
					packet.Send();
				}
				LaserTagActiveTeams = 0;
				LaserTagActivePlayers = 0;
				LaserTagTimeLeft = -1;
				foreach (Player player in Main.ActivePlayers) {
					player.OriginPlayer().laserTagVestActive = false;
				}
			}
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (LaserTagGameActive) return;
			drawData.glowColor = Color.White;
			drawData.glowTexture = glowTexture;
			drawData.glowSourceRect = new(0, drawData.tileFrameY, 16, 16);
		}
		readonly AutoLoadingAsset<Texture2D> glowTexture = typeof(Laser_Tag_Console).GetDefaultTMLName() + "_Glow";
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public Color GlowColor => new(196, 196, 196, 100);
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, Main.teamColor[tile.TileFrameX].ToVector3());
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public static int GetLargeGem(int team) {
			switch ((Team)team) {
				case Team.None:
				default:
				return ItemID.LargeDiamond;
				case Team.Red:
				return ItemID.LargeRuby;
				case Team.Green:
				return ItemID.LargeEmerald;
				case Team.Blue:
				return ItemID.LargeSapphire;
				case Team.Yellow:
				return ItemID.LargeTopaz;
				case Team.Pink:
				return ItemID.LargeAmethyst;
			}
		}
		public static int GetLargeGemID(int team) => GetLargeGem(team) - ItemID.LargeAmethyst;
	}
	public class Laser_Tag_Console_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Laser_Tag_Console>());
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CopperBar, 2)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 5)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Silicon_Bar>(), 8)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
	file static class Laser_Tag_Extensions {
		public static T SetSize<T>(this T element) where T : UIElement {
			element.Width.Set(-10, 1f);
			element.Height.Set(32, 0);
			return element;
		}
		public static T SetHidden<T>(this T element, Func<bool> isHidden) where T : UI_Auto_Hide_Panel {
			element.IsHidden = isHidden;
			return element;
		}
	}
	public class Laser_Tag_Rules(int RespawnTime = -1, int Time = -1, int HP = 1, int PointsToWin = 0, bool Teams = true, bool CTG = false, bool CTGNeedOwnGem = true, bool Building = false) {
		const string prefix = "Mods.Origins.Laser_Tag.";
		public int RespawnTime = RespawnTime;
		public int Time = Time;
		public int HP = HP;
		public int PointsToWin = PointsToWin;
		public bool Teams = Teams;
		public bool CTG = CTG;
		public bool CTGNeedOwnGem = CTGNeedOwnGem;
		public bool Building = Building;
		public bool HitsArePoints => !CTG;
		public bool IsElimination => RespawnTime < 0;
		public IEnumerable<UIElement> GetSettingUIElements() {
			static UI_Time_Button GetTimeController(ButtonIntnessGetter variable, string name, Time_Radix[] radices, int increment = 1, int indefiniteThreshold = 0, string indefiniteName = null, int maxValue = int.MaxValue) {
				return new UI_Time_Button(variable, Language.GetOrRegister(prefix + name), radices, increment, indefiniteThreshold, Language.GetOrRegister(prefix + (indefiniteName ?? "Indefinite")), maxValue).SetSize();
			}
			static UI_HP_Button GetHealthController(ButtonIntnessGetter variable, string name, Texture2D texture) {
				return new UI_HP_Button(variable, Language.GetOrRegister(prefix + name), texture).SetSize();
			}
			static UI_Points_Button GetPointsController(ButtonIntnessGetter variable, string name, Texture2D noneTexture, Texture2D countTexture, int noneCount = 0) {
				return new UI_Points_Button(variable, Language.GetOrRegister(prefix + name), noneTexture, countTexture, noneCount).SetSize();
			}
			static UI_Toggle_Button GetToggle(ButtonTogglenessGetter variable, string name, float textScaleMax = 1, bool large = false) {
				return new UI_Toggle_Button(variable, Language.GetOrRegister(prefix + name)).SetSize();
			}

			yield return GetTimeController(() => ref RespawnTime, "RespawnTime", Time_Radix.ParseRadices(Language.GetOrRegister(prefix + "ShortTime").Value), 30, indefiniteName: "Elimination");
			yield return GetTimeController(() => ref Time, "Time", Time_Radix.ParseRadices(Language.GetOrRegister(prefix + "LongTime").Value), 60 * 10, 1);
			yield return GetHealthController(() => ref HP, "HP", TextureAssets.Heart.Value);
			yield return GetPointsController(() => ref PointsToWin, "PointsToWin", TextureAssets.Heart.Value, TextureAssets.Mana.Value);
			yield return GetToggle(() => ref Teams, "Teams");
			yield return GetToggle(() => ref CTG, "CTG");
			yield return GetToggle(() => ref CTGNeedOwnGem, "CTGNeedOwnGem").SetHidden(() => !CTG);
			yield return GetToggle(() => ref Building, "Building");
		}
		public string GetGameSummaryText(float availableWidth) {
			if (Teams) {
				StringBuilder builder = new();
				for (int i = 1; i < 6; i++) {
					if (Laser_Tag_Console.LaserTagTeamPlayers[i] > 0) {
						if (builder.Length > 0) builder.Append(" - ");
						builder.Append($"[c/{Main.teamColor[i].Hex3()}:{Laser_Tag_Console.LaserTagTeamPoints[i]}]");
					}
				}
				return builder.ToString();
			} else {
				List<(int points, Player player)> players = [];
				float totalWidth = 0;
				float myWidth = 0;
				foreach (Player player in Main.ActivePlayers) {
					OriginPlayer originPlayer = player.OriginPlayer();
					if (originPlayer.laserTagVest) {
						players.Add((originPlayer.laserTagPoints, player));
						float width = 32 + FontAssets.MouseText.Value.MeasureString(originPlayer.laserTagPoints + " ").X;
						totalWidth += width;
						if (player.whoAmI == Main.myPlayer) myWidth = width;
					}
				}
				players = players.OrderBy(v => v.points).ToList();
				if (totalWidth <= availableWidth) {
					return string.Join(" ", players.Select(static v => $"[head:{v.player.whoAmI}]{v.points}"));
				}
				StringBuilder builder = new();
				float x = 0;
				for (int i = 0; i < players.Count; i++) {
					if (x + myWidth > availableWidth) break;
					builder.Append($"[head:{players[i].player.whoAmI}]{players[i].points}");
					x += 32 + FontAssets.MouseText.Value.MeasureString(players[i].points + " ").X;
				}
				builder.Append($"[head:{Main.myPlayer}]{Main.LocalPlayer.OriginPlayer().laserTagPoints}");
				return builder.ToString();
			}
		}
		public void Write(BinaryWriter writer) {
			writer.Write(RespawnTime);
			writer.Write(Time);
			writer.Write(HP);
			writer.Write(PointsToWin);
			writer.Write(Teams);
			writer.Write(CTG);
			writer.Write(CTGNeedOwnGem);
			writer.Write(Building);
		}
		public static Laser_Tag_Rules Read(BinaryReader reader) => new(
			RespawnTime: reader.ReadInt32(),
			Time: reader.ReadInt32(),
			HP: reader.ReadInt32(),
			PointsToWin: reader.ReadInt32(),
			Teams: reader.ReadBoolean(),
			CTG: reader.ReadBoolean(),
			CTGNeedOwnGem: reader.ReadBoolean(),
			Building: reader.ReadBoolean()
		);
		public IEnumerable<LaserTagWinCondition> GetWinConditions() {
			for (int i = 0; i < LaserTagWinCondition.WinConditions.Count; i++) {
				if (LaserTagWinCondition.WinConditions[i].IsActive(this)) yield return LaserTagWinCondition.WinConditions[i];
			}
		}
	}
	public abstract class LaserTagWinCondition : ILoadable {
		internal static List<LaserTagWinCondition> WinConditions { get; private set; } = [];
		public void Load(Mod mod) => WinConditions.Add(this);
		public void Unload() => WinConditions = null;
		public abstract bool IsActive(Laser_Tag_Rules rules);
		public abstract Player GetWinner(Laser_Tag_Rules rules);
		public virtual int GetTeamWinner(Laser_Tag_Rules rules) => GetWinner(rules) is Player winner ? winner.team : -1;
	}
	public class Elimination_Win_Condition : LaserTagWinCondition {
		public override Player GetWinner(Laser_Tag_Rules rules) {
			if (Laser_Tag_Console.LaserTagMultipleTeamsActive) return null;
			foreach (Player player in Main.ActivePlayers) {
				if (!player.OriginPlayer().laserTagVestActive) {
					return player;
				}
			}
			return null;
		}
		public override bool IsActive(Laser_Tag_Rules rules) => rules.IsElimination;
	}
	public class Timeout_Win_Condition : LaserTagWinCondition {
		public override Player GetWinner(Laser_Tag_Rules rules) {
			if (Laser_Tag_Console.LaserTagTimeLeft <= 0) return null;
			int bestPointsOwner = 0;
			int bestPointsCount = 0;
			int tieCount = 0;
			foreach (Player player in Main.ActivePlayers) {
				int laserTagPoints = player.OriginPlayer().laserTagPoints;
				if (laserTagPoints >= bestPointsCount) {
					tieCount = laserTagPoints == bestPointsCount ? tieCount + 1 : 1;
					bestPointsOwner = player.whoAmI;
					bestPointsCount = laserTagPoints;
				}
			}
			if (tieCount <= 0) return Main.player[bestPointsOwner];
			return null;
		}
		public override int GetTeamWinner(Laser_Tag_Rules rules) {
			if (Laser_Tag_Console.LaserTagTimeLeft <= 0) return -1;
			int bestPointsOwner = -1;
			int bestPointsCount = 0;
			int tieCount = 0;
			for (int i = 0; i < Laser_Tag_Console.LaserTagTeamPoints.Length; i++) {
				if (Laser_Tag_Console.LaserTagTeamPoints[i] >= bestPointsCount) {
					tieCount = Laser_Tag_Console.LaserTagTeamPoints[i] == bestPointsCount ? tieCount + 1 : 1;
					bestPointsOwner = i;
					bestPointsCount = Laser_Tag_Console.LaserTagTeamPoints[i];
				}
			}
			if (tieCount <= 0) return bestPointsOwner;
			return -1;
		}
		public override bool IsActive(Laser_Tag_Rules rules) => rules.Time > 0;
	}
	public class Points_Win_Condition : LaserTagWinCondition {
		public override Player GetWinner(Laser_Tag_Rules rules) {
			foreach (Player player in Main.ActivePlayers) {
				if (player.OriginPlayer().laserTagPoints >= rules.PointsToWin) {
					return player;
				}
			}
			return null;
		}
		public override int GetTeamWinner(Laser_Tag_Rules rules) {
			for (int i = 0; i < Laser_Tag_Console.LaserTagTeamPoints.Length; i++) {
				if (Laser_Tag_Console.LaserTagTeamPoints[i] >= rules.PointsToWin) {
					return i;
				}
			}
			return -1;
		}
		public override bool IsActive(Laser_Tag_Rules rules) => rules.PointsToWin > 0;
	}
}

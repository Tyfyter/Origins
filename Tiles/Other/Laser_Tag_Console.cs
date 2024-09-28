using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Origins.Tiles.Other {
	public class Laser_Tag_Console : ModTile, IGlowingModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(81, 81, 81), CreateMapEntryName());
		}
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
			}
			return false;
		}
		public static ref int LaserTagActiveTeams => ref OriginSystem.Instance.laserTagActiveTeams;
		public static ref int LaserTagActivePlayers => ref OriginSystem.Instance.laserTagActivePlayers;
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
		}
		//static Time_Radix[] radices;
		public static void ProcessLaserTag() {
			//radices ??= Time_Radix.ParseRadices(Language.GetOrRegister("Mods.Origins.Laser_Tag.LongTime").Value);
			//Debugging.ChatOverhead(Time_Radix.FormatTime(LaserTagTimeLeft, radices));
			LaserTagActiveTeams = 0;
			LaserTagActivePlayers = 0;
			for (int i = 0; i < LaserTagTeamGems.Length; i++) {
				LaserTagTeamGems[i] = -1;
			}
			foreach (Player player in Main.ActivePlayers) {
				OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
				OriginPlayer.playersByGuid.TryAdd(originPlayer.guid, player.whoAmI);
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
			void SelectWinner(Player player = null, int team = 0) {
				if (player is not null) team = player.team;
				if (!LaserTagRules.Teams) team = 0;
				winnerColor = Main.teamColor[team];
				switch (team) {
					case 1:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Red");
					break;
					case 2:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Green");
					break;
					case 3:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Blue");
					break;
					case 4:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Yellow");
					break;
					case 5:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Purple");
					break;
					default:
					case 0:
					winner = player?.name ?? "missingno";
					break;
				}
			}
			if (LaserTagGameActive) {
				foreach (Player player in Main.ActivePlayers) {
					player.hostile = player.OriginPlayer().laserTagVestActive;
				}
				if (LaserTagRules.RespawnTime < 0) {// elimination
					if (Main.netMode == NetmodeID.Server) {
						Player survivor = null;
						foreach (Player player in Main.ActivePlayers) {
							ref bool laserTagVestActive = ref player.OriginPlayer().laserTagVestActive;
							if (laserTagVestActive) {
								laserTagVestActive = false;
								survivor ??= player;
							}
						}
						SelectWinner(survivor);
					}
				}
				if (LaserTagRules.PointsToWin != 0) {
					if (LaserTagRules.Teams) {
						for (int i = 0; i < LaserTagTeamPoints.Length; i++) {
							if (LaserTagTeamPoints[i] >= LaserTagRules.PointsToWin) {
								SelectWinner(team: i);
								break;
							}
						}
					} else {
						foreach (Player player in Main.ActivePlayers) {
							if (player.OriginPlayer().laserTagPoints >= LaserTagRules.PointsToWin) {
								SelectWinner(player);
								break;
							}
						}
					}
				}
				if (LaserTagRules.Time > 0 && --LaserTagTimeLeft <= 0) {//any mode, end by timeout
					LaserTagTimeLeft = 0;
					int bestPointsOwner = 0;
					int bestPointsCount = 0;
					int tieCount = 0;
					if (LaserTagRules.Teams) {
						for (int i = 0; i < LaserTagTeamPoints.Length; i++) {
							if (LaserTagTeamPoints[i] >= bestPointsCount) {
								tieCount = LaserTagTeamPoints[i] == bestPointsCount ? tieCount + 1 : 1;
								bestPointsOwner = i;
								bestPointsCount = LaserTagTeamPoints[i];
							}
						}
					} else {
						foreach (Player player in Main.ActivePlayers) {
							int laserTagPoints = player.OriginPlayer().laserTagPoints;
							if (laserTagPoints >= bestPointsCount) {
								tieCount = laserTagPoints == bestPointsCount ? tieCount + 1 : 1;
								bestPointsOwner = player.whoAmI;
								bestPointsCount = laserTagPoints;
							}
						}
					}
					if (tieCount <= 1) {
						if (LaserTagRules.Teams) {
							SelectWinner(team: bestPointsOwner);
						} else {
							SelectWinner(Main.player[bestPointsOwner]);
						}
						for (int i = 0; i < LaserTagTeamPoints.Length; i++) {
							LaserTagTeamPoints[i] = 0;
						}
						foreach (Player player in Main.ActivePlayers) {
							player.OriginPlayer().laserTagPoints = 0;
						}
					}
				}
			}
			if (winner is not null) {
				if (Main.netMode == NetmodeID.Server) {
					ChatHelper.BroadcastChatMessage(Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Victory").ToNetworkText(winner), winnerColor);
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
			.AddIngredient(ModContent.ItemType<Silicon_Item>(), 8)
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
	public class Laser_Tag_Rules(int RespawnTime = -1, int Time = -1, int HP = 1, int PointsToWin = 0, bool Teams = true, bool CTG = false, bool Building = false) {
		const string prefix = "Mods.Origins.Laser_Tag.";
		public int RespawnTime = RespawnTime;
		public int Time = Time;
		public int HP = HP;
		public int PointsToWin = PointsToWin;
		public bool Teams = Teams;
		public bool CTG = CTG;
		public bool Building = Building;
		public bool HitsArePoints => !CTG;
		public IEnumerable<UIElement> GetUIElements() {
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
			yield return GetToggle(() => ref Building, "Building");
		}
		public void Write(BinaryWriter writer) {
			writer.Write(RespawnTime);
			writer.Write(Time);
			writer.Write(HP);
			writer.Write(PointsToWin);
			writer.Write(Teams);
			writer.Write(CTG);
			writer.Write(Building);
		}
		public static Laser_Tag_Rules Read(BinaryReader reader) => new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean());
	}
}

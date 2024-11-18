using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI.Event {
	public class Laser_Tag_Hud : SwitchableUIState {
		public AutoLoadingAsset<Texture2D> texture = typeof(Laser_Tag_Hud).GetDefaultTMLName();
		public override void AddToList() => OriginSystem.Instance.SetBonusHUD.AddState(this);
		public override bool IsActive() {
			if (OriginPlayer.LocalOriginPlayer is null) return false;
			if (OriginPlayer.LocalOriginPlayer.laserTagVest) {
				return true;
			}
			return false;
		}
		public Laser_Tag_Hud() : base() {
			OverrideSamplerState = SamplerState.PointClamp;
		}
		UIHideWrapper summaryPanelWrapper;
		UIPanel summaryPanel;
		UIHideWrapper teamsListWrapper;
		UI_Hidden_Supporting_List teamsList;
		UIHideWrapper playersListWrapper;
		UIList playersList;
		UIHideWrapper ActiveScoreList => Laser_Tag_Console.LaserTagRules.Teams ? teamsListWrapper : teamsListWrapper;
		public override void OnInitialize() {
			summaryPanel = new() {
				Width = new(0, 1),
				Height = new(0, 1),
			};
			summaryPanelWrapper = new() {
				HAlign = 0.5f
			};
			summaryPanelWrapper.Width.Set(250, 0.05f);
			summaryPanelWrapper.Height.Set(32, 0f);
			summaryPanelWrapper.Append(summaryPanel);
			Append(summaryPanelWrapper);

			teamsList = [
				new Laser_Tag_Team_UIElement(1),
				new Laser_Tag_Team_UIElement(2),
				new Laser_Tag_Team_UIElement(3),
				new Laser_Tag_Team_UIElement(4),
				new Laser_Tag_Team_UIElement(5)
			];
			teamsList.Width.Set(0, 1);
			teamsList.Height.Set(0, 1);
			teamsList.ListPadding = 0;
			teamsListWrapper = new() {
				HAlign = 0.5f
			};
			teamsListWrapper.Width.Set(250, 0.05f);
			teamsListWrapper.Height.Set(64, 1f);
			teamsListWrapper.Append(teamsList);
			Append(teamsListWrapper);
		}
		public override void Update(GameTime gameTime) {
			if (summaryPanel.IsMouseHovering || teamsList.IsMouseHovering) {
				summaryPanelWrapper.isHidden = true;
				ActiveScoreList.isHidden = false;
			} else {
				summaryPanelWrapper.isHidden = false;
				teamsListWrapper.isHidden = true;
				//playersListWrapper.isHidden = true;
			}
		}
	}
	public class UIHideWrapper : UIElement {
		public bool isHidden = false;
		public override bool ContainsPoint(Vector2 point) => !isHidden && base.ContainsPoint(point);
		public override void Draw(SpriteBatch spriteBatch) {
			if (!isHidden) base.Draw(spriteBatch);
		}
	}
	public class Laser_Tag_Team_UIElement : UI_Auto_Hide_Panel, IComparable<Laser_Tag_Team_UIElement> {
		public readonly int team;
		public Laser_Tag_Team_UIElement(int team) : base(() => Laser_Tag_Console.LaserTagTeamPlayers[team] <= 0) {
			Width.Set(0, 1);
			Height.Set(32, 0);
			BackgroundColor = Main.teamColor[team];
			this.team = team;
		}
		public int CompareTo(Laser_Tag_Team_UIElement other) {
			if (Laser_Tag_Console.LaserTagRules.RespawnTime < 0) return Comparer<int>.Default.Compare(Laser_Tag_Console.LaserTagTeamPlayers[team], Laser_Tag_Console.LaserTagTeamPlayers[other.team]);
			return Comparer<int>.Default.Compare(Laser_Tag_Console.LaserTagTeamPoints[team], Laser_Tag_Console.LaserTagTeamPoints[other.team]);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			Rectangle bounds = GetInnerDimensions().ToRectangle();
			Vector2 pointsPos = bounds.Right();
			int points = Laser_Tag_Console.LaserTagTeamPoints[team];
			Vector2 pointsSize = FontAssets.MouseText.Value.MeasureString(points.ToString());
			pointsPos.X -= pointsSize.X;
			ChatManager.DrawColorCodedString(
				spriteBatch,
				FontAssets.MouseText.Value,
				points.ToString(),
				pointsPos,
				Color.Black,
				0,
				pointsSize * Vector2.UnitY * 0.4f,
				Vector2.One
			);
			if (Laser_Tag_Console.LaserTagRules.RespawnTime < 0) {
				Vector2 headPos = bounds.Left();
				foreach (Player player in Main.ActivePlayers) {
					if (player.team == team && player.OriginPlayer().laserTagVestActive) {
						Main.PlayerRenderer.DrawPlayerHead(
							Main.Camera,
							player,
							headPos + Vector2.UnitX * 16
						);
						headPos.X += 32;
						if (headPos.X > pointsPos.X - 32) break;
					}
				}
			}

		}
	}
}

using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.UI {
	public class State_Switching_UI(params SwitchableUIState[] states) : UIState() {
		readonly List<SwitchableUIState> states = states.ToList();
		public SwitchableUIState CurrentState { get; private set; }
		public void AddState(SwitchableUIState state) {
			states.Add(state);
		}
		public void AddStates(params SwitchableUIState[] states) {
			this.states.AddRange(states);
		}
		public override void Update(GameTime gameTime) {
			CurrentState = null;
			for (int i = 0; i < states.Count; i++) {
				if (states[i].IsActive()) {
					CurrentState = states[i];
					states[i].Activate();
				} else {
					states[i].Deactivate();
				}
				if (!this.Children.Contains(states[i])) {
					this.Append(states[i]);
				}
			}
			if (CurrentState is null) return;
			CurrentState.Update(gameTime);
		}
	}
	public abstract class SwitchableUIState : UIState, ILoadable {
		public void Load(Mod mod) {
			if (OriginSystem.queuedUIStates is null) {
				AddToList();
			} else {
				OriginSystem.queuedUIStates.Add(this);
			}
		}
		public void Unload() { }
		public abstract void AddToList();
		public abstract bool IsActive();
		protected bool isActive = false;
		public sealed override void OnActivate() {
			if (isActive.TrySet(true)) OnEnter();
		}
		public sealed override void OnDeactivate() {
			if (isActive.TrySet(false)) OnExit();
		}
		public virtual InterfaceScaleType ScaleType => InterfaceScaleType.UI;
		public virtual void OnEnter() { }
		public virtual void OnExit() { }
		public override bool ContainsPoint(Vector2 point) => isActive && base.ContainsPoint(point);
		public override void Update(GameTime gameTime) {
			if (isActive) base.Update(gameTime);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			if (isActive) base.Draw(spriteBatch);
		}
	}
	public class StateSwitchingInterface : UserInterface {
		readonly LegacyGameInterfaceLayer layer;
		readonly State_Switching_UI state;
		readonly string after;
		public StateSwitchingInterface(string name, string after = "Vanilla: Inventory") : base() {
			layer = new LegacyGameInterfaceLayer(
				name,
				delegate {
					Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
					return true;
				}
			);
			SetState(state = new());
			this.after = after;
		}
		public void AddState(SwitchableUIState state) {
			this.state.AddState(state);
		}
		public void AddStates(params SwitchableUIState[] states) {
			state.AddStates(states);
		}
		public void Insert(List<GameInterfaceLayer> layers) {
			if (state.CurrentState is null) return;
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals(after));
			if (inventoryIndex != -1) {//error prevention & null check
				layer.ScaleType = state.CurrentState.ScaleType;
				layers.Insert(inventoryIndex + 1, layer);
			}
		}
	}
}
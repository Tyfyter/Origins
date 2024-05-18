using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.UI {
	public class State_Switching_UI(params SwitchableUIState[] states) : UIState() {
		readonly List<SwitchableUIState> states = states.ToList();
		SwitchableUIState currentState;

		public void AddState(SwitchableUIState state) {
			states.Add(state);
		}
		public void AddStates(params SwitchableUIState[] states) {
			this.states.AddRange(states);
		}
		public override void Update(GameTime gameTime) {
			currentState = null;
			for (int i = 0; i < states.Count; i++) {
				if (states[i].IsActive()) {
					currentState = states[i];
				}
			}
			if (currentState is null) return;
			currentState.Update(gameTime);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			if (currentState is null) return;
			currentState.Draw(spriteBatch);
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
	}
}
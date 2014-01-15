using System;

namespace WorldProcessing.Planning.Actions
{
	/// <summary>
	/// An enumeration describing the state of an action.
	/// Due: action has yet to be executed.
	/// InProgress: action is currently being executed.
	/// Complete: action is completely executed.
	/// </summary>
	public enum ActionState { Due, InProgress, Complete}

	public enum ActionType { Move, Turn, Wait }

	public class ActionInfo
	{
		private static System.Collections.Generic.Dictionary<ActionType, Type> _ActionTypeInfo;
		public static System.Collections.Generic.Dictionary<ActionType, Type> ActionTypeInfo
		{
			get
			{
				if (_ActionTypeInfo == null)
				{
					_ActionTypeInfo = new System.Collections.Generic.Dictionary<ActionType, Type>();
					_ActionTypeInfo.Add(ActionType.Move, typeof(MovementAction));
					_ActionTypeInfo.Add(ActionType.Wait, typeof(WaitAction));
					_ActionTypeInfo.Add(ActionType.Turn, typeof(TurnAction));
				}
				return _ActionTypeInfo;
			}
		}
	}

	/// <summary>
	/// Abstract class providing basic functionality for actions.
	/// </summary>
	public abstract class Action
	{
		public ActionState State { get; protected set; }
		public ActionType Type { get; protected set; }
		public event EventHandler ActionStateChanged;

		public Action()
		{
			this.State = ActionState.Due;
		}

		public virtual void Start()
		{
			this.State = ActionState.InProgress;
			OnActionStateChanged(new EventArgs());
		}

		public virtual void End()
		{
			this.State = ActionState.Complete;
			OnActionStateChanged(new EventArgs());
		}

		private void OnActionStateChanged(EventArgs e)
		{
			if (ActionStateChanged != null)
			{
				ActionStateChanged(this, e);
			}
		}
	}
}

using AForge.Robotics.Lego;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AForgeNXTTest.NXT
{
	public class NXTController
	{
		public bool Connected { get; private set; }

		public string COMPort { get; private set; }
		public string BrickName { get; private set; }
		public NXTBrick Brick { get; private set; }

		//Motor A is the left wheel;
		public NXTBrick.Motor LeftWheelMotor { get { return NXTBrick.Motor.A; } }
		public NXTBrick.Motor RightWheelMotor { get { return NXTBrick.Motor.B; } }

		public enum CurrentMotorState { Forward, TurnLeft, TurnRight, Stop }
		public CurrentMotorState MotorState { get; private set; }

		public event EventHandler MotorStateChanged;

		public NXTController(string COMPort, string BrickName)
		{
			this.COMPort = COMPort;
			this.BrickName = BrickName;
			Brick = new NXTBrick();
			Connected = false;
		}

		public bool Connect()
		{
			if (Brick.Connect(COMPort)) { Connected = true; return true; }
			else { return false; }
		}

		public void Init()
		{
			Debug.Assert(Connected, "Brick " + BrickName + " is not connected to COM port " + COMPort);

			Brick.SetBrickName(BrickName);

			MotorState = CurrentMotorState.Stop;
			MotorStateChanged(this, new EventArgs());

			Brick.SetMotorState(LeftWheelMotor, CleanMotorState());
			Brick.SetMotorState(RightWheelMotor, CleanMotorState());
		}

		public void Forward(int speed)
		{
			Debug.Assert(Connected, "Brick " + BrickName + " is not connected to COM port " + COMPort);

			Brick.SetMotorState(LeftWheelMotor, ForwardMotorState(speed));
			Brick.SetMotorState(RightWheelMotor, ForwardMotorState(speed));

			MotorState = CurrentMotorState.Forward;
			MotorStateChanged(this, new EventArgs());
		}

		public void TurnLeft(int speed)
		{
			Debug.Assert(Connected, "Brick " + BrickName + " is not connected to COM port " + COMPort);

			Brick.SetMotorState(LeftWheelMotor, ForwardMotorState(-speed));
			Brick.SetMotorState(RightWheelMotor, ForwardMotorState(speed));

			MotorState = CurrentMotorState.TurnLeft;
			MotorStateChanged(this, new EventArgs());
		}

		public void TurnRight(int speed)
		{
			Debug.Assert(Connected, "Brick " + BrickName + " is not connected to COM port " + COMPort);

			Brick.SetMotorState(LeftWheelMotor, ForwardMotorState(speed));
			Brick.SetMotorState(RightWheelMotor, ForwardMotorState(-speed));

			MotorState = CurrentMotorState.TurnRight;
			MotorStateChanged(this, new EventArgs());
		}

		public void Stop()
		{
			Debug.Assert(Connected, "Brick " + BrickName + " is not connected to COM port " + COMPort);

			Brick.SetMotorState(LeftWheelMotor, CleanMotorState());
			Brick.SetMotorState(RightWheelMotor, CleanMotorState());

			MotorState = CurrentMotorState.Stop;
			MotorStateChanged(this, new EventArgs());
		}

		private NXTBrick.MotorState CleanMotorState()
		{
			//Precondition: Robot is connected
			Debug.Assert(Connected, "Brick " + BrickName + " is not connected to COM port " + COMPort);

			//Numbers in here come from sample code
			NXTBrick.MotorState state = new NXTBrick.MotorState();
			state.Power = 0;
			state.TurnRatio = 50;
			state.Mode = NXTBrick.MotorMode.On;
			state.Regulation = NXTBrick.MotorRegulationMode.Idle;
			state.RunState = NXTBrick.MotorRunState.Idle;
			state.TachoLimit = 1000;
			return state;
		}

		private NXTBrick.MotorState ForwardMotorState(int speed)
		{
			Debug.Assert(Connected, "Brick " + BrickName + " is not connected to COM port " + COMPort);

			NXTBrick.MotorState state = CleanMotorState();
			state.Power = speed;
			state.RunState = NXTBrick.MotorRunState.Running;
			return state;
		}
	}
}

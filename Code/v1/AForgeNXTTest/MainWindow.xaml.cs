using AForgeNXTTest.NXT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AForgeNXTTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public NXTController robot;
		public NXTController robot2;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void MotorStateChanged(object sender, EventArgs e)
		{
			TextBox.Text = robot.MotorState.ToString();
		}

		private void MotorState2Changed(object sender, EventArgs e)
		{
			TextBox2.Text = robot.MotorState.ToString();
		}

		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			string port = PortTextBox.Text;
			PortTextBox.Text = "Connecting...";
			robot = new NXTController(port, "KOEN LOL");
			robot.MotorStateChanged += MotorStateChanged;
			if (robot.Connect())
			{
				robot.Init();
				PortTextBox.Text = "Connected!";
			}
			else
			{
				PortTextBox.Text = "Connection failed.";
			}
		}

		private void ForwardButton_Click(object sender, RoutedEventArgs e)
		{
			if (robot.Connected) { robot.Forward(100); }
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			if (robot.Connected) { robot.Stop(); }
		}

		private void TurnLeftButton_Click(object sender, RoutedEventArgs e)
		{
			if (robot.Connected) { robot.TurnLeft(30); }
		}

		private void TurnRightButton_Click(object sender, RoutedEventArgs e)
		{
			if (robot.Connected) { robot.TurnRight(30); }
		}

		private void Connect2Button_Click(object sender, RoutedEventArgs e)
		{
			string port = Port2TextBox.Text;
			Port2TextBox.Text = "Connecting...";
			robot2 = new NXTController(port, "KOEN LOL");
			robot2.MotorStateChanged += MotorState2Changed;
			if (robot2.Connect())
			{
				robot2.Init();
				Port2TextBox.Text = "Connected!";
			}
			else
			{
				Port2TextBox.Text = "Connection failed.";
			}
		}

	}
}

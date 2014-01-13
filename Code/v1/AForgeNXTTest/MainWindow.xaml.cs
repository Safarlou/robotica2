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

		public MainWindow()
		{
			InitializeComponent();
			robot = new NXTController("COM7", "Koen de Robot");
			robot.MotorStateChanged += MotorStateChanged;
		}

		private void MotorStateChanged(object sender, EventArgs e)
		{
			TextBox.Text = robot.MotorState.ToString();
		}

		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			if (robot.Connect()) { robot.Init(); }
			else { throw new InvalidOperationException("Connecting failed!"); }
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

	}
}

﻿using AForgeNXTTest.NXT;
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

		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			string port = PortTextBox.Text;
			PortTextBox.Text = "Connecting...";
			robot = new NXTController(port, "ROBOTA");
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
			if (robot.Connected) { robot.Forward(70); }
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			if (robot.Connected) { robot.Stop(); }
		}

		private void TurnLeftButton_Click(object sender, RoutedEventArgs e)
		{
			if (robot.Connected) { robot.TurnLeft(70); }
		}

		private void TurnRightButton_Click(object sender, RoutedEventArgs e)
		{
			if (robot.Connected) { robot.TurnRight(70); }
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up: ForwardButton_Click(sender, e); break;
				case Key.Down: StopButton_Click(sender, e); break;
				case Key.Left: TurnLeftButton_Click(sender, e); break;
				case Key.Right: TurnRightButton_Click(sender, e); break;
				default: break;
			}
		}
	}
}

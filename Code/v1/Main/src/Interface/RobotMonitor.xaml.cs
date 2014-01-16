using System.Windows;
using WorldProcessing.Controller;

namespace WorldProcessing.Interface
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class RobotMonitor : Window
	{
		public NXTController Transport { get; private set; }
		public NXTController Guard { get; private set; }

		public RobotMonitor(NXTController transport, NXTController guard)
		{
			InitializeComponent();
			this.Transport = transport;
			this.Guard = guard;
		}

		private void ConnectTransport_Click(object sender, RoutedEventArgs e)
		{
			if (Transport.Connected) { return; }
			else
			{
				TransportStatusLabel.Content = "Connecting...";
				string port = TransportPortBox.Text;
				Transport.COMPort = port;
				if (Transport.Connect())
				{
					TransportStatusLabel.Content = "Connected.";
					Transport.Connected = true;
					Transport.Init();
				}
				else
				{
					TransportStatusLabel.Content = "Connection failed.";
				}
			}
		}

		private void ConnectGuard_Click(object sender, RoutedEventArgs e)
		{
			if (Guard.Connected) { return; }
			else
			{
				GuardStatusLabel.Content = "Connecting...";
				string port = GuardPortBox.Text;
				Guard.COMPort = port;
				if (Guard.Connect())
				{
					GuardStatusLabel.Content = "Connected.";
					Guard.Connected = true;
					Guard.Init();
				}
				else
				{
					GuardStatusLabel.Content = "Connection failed.";
				}
			}
		}

		private void ForwardSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Constants.ForwardSpeed = (int)e.NewValue;
			if (ForwardSpeedLabel != null) ForwardSpeedLabel.Content = e.NewValue.ToString();
		}

		private void TurnSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Constants.TurnSpeed = (int)e.NewValue;
			if (TurnSpeedLabel != null) TurnSpeedLabel.Content = e.NewValue.ToString();
		}
	}
}

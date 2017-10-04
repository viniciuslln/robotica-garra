/* Copyright 2012 Marco Minerva, marco.minerva@gmail.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Threading;
using System.ComponentModel;
using System.IO;

namespace BluetoothControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BluetoothClient bluetooth;
        private Stream bluetoothStream;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!BluetoothRadio.IsSupported)
            {
                lblStatus.Text = "Bluetooth not supported";
                btnSearchDevice.IsEnabled = false;
            }
            else if (BluetoothRadio.PrimaryRadio.Mode == RadioMode.PowerOff)
            {
                lblStatus.Text = "Bluetooth is off";
                btnSearchDevice.IsEnabled = false;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void btnSearchDevice_Click(object sender, RoutedEventArgs e)
        {
            lblStatus.Text = "Searching for devices...";
            btnSearchDevice.IsEnabled = false;
            BackgroundWorker bwDiscoverDevices = new BackgroundWorker();
            bwDiscoverDevices.DoWork += new DoWorkEventHandler(bwDiscoverDevices_DoWork);
            bwDiscoverDevices.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwDiscoverDevices_RunWorkerCompleted);
            bwDiscoverDevices.RunWorkerAsync();
        }

        private void bwDiscoverDevices_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                bluetooth = new BluetoothClient();
                var gadgeteerDevice = bluetooth.DiscoverDevices().Where(d => d.DeviceName == "HC-06").FirstOrDefault();
                if (gadgeteerDevice != null)
                {
                    bluetooth.SetPin("1234");
                    bluetooth.Connect(gadgeteerDevice.DeviceAddress, BluetoothService.SerialPort);
                    bluetoothStream = bluetooth.GetStream();
                    e.Result = true;
                }
                else
                {
                    e.Result = false;
                }
            }
            catch (Exception)
            {
                e.Result = false;
            }
        }

        private void bwDiscoverDevices_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnSearchDevice.IsEnabled = true;
            var deviceFound = (bool)e.Result;
            if (!deviceFound)
            {
                lblStatus.Text = "No device found";
            }
            else
            {
                lblStatus.Text = "Connected to HC-06";
                btnSendMessage.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (bluetoothStream != null)
            {
                bluetoothStream.Close();
                bluetoothStream.Dispose();
            }
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (bluetooth.Connected && bluetoothStream != null)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(txtMessage.Text);
                bluetoothStream.Write(buffer, 0, buffer.Length);
                txtMessage.Text = string.Empty;
            }
        }
    }
}

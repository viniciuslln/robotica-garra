using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace BluetoothControl {
    public partial class MainWindow : Window {
        private BluetoothClient bluetooth;
        private Stream bluetoothStream;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded( object sender, RoutedEventArgs e ) {
            if( !BluetoothRadio.IsSupported ) {
                lblStatus.Text = "Bluetooth not supported";
                btnSearchDevice.IsEnabled = false;
            }
            else if( BluetoothRadio.PrimaryRadio.Mode == RadioMode.PowerOff ) {
                lblStatus.Text = "Bluetooth is off";
                btnSearchDevice.IsEnabled = false;
            }
        }

        private void btnSearchDevice_Click( object sender, RoutedEventArgs e ) {
            bluetooth = new BluetoothClient();
            SelecionarDispositivo j = new SelecionarDispositivo( bluetooth );
            j.SeleciodadoDispositivoEvent += J_SeleciodadoDispositivoEvent;
            j.ShowDialog();

        }

        private void J_SeleciodadoDispositivoEvent( BluetoothDeviceInfo device ) {
            lblStatus.Text = "Conectando...";
            btnSearchDevice.IsEnabled = false;

            Conectar( device );

            //BackgroundWorker bwDiscoverDevices = new BackgroundWorker();
            //bwDiscoverDevices.DoWork += new DoWorkEventHandler( bwDiscoverDevices_DoWork );
            //bwDiscoverDevices.RunWorkerCompleted += new RunWorkerCompletedEventHandler( bwDiscoverDevices_RunWorkerCompleted );
            //bwDiscoverDevices.RunWorkerAsync();
        }

        public async Task Conectar( BluetoothDeviceInfo device ) {
            if( device != null ) {
                bluetooth.SetPin( "1234" );
                bluetooth.Connect( device.DeviceAddress, BluetoothService.SerialPort );
                bluetoothStream = bluetooth.GetStream();

                btnSearchDevice.IsEnabled = true;

                Application.Current.Dispatcher.Invoke( new Action( () => { lblStatus.Text = "No device found"; } ) );
            }
            else {
                Application.Current.Dispatcher.Invoke( new Action( () => {
                    lblStatus.Text = $"Connected to {device.DeviceName}";
                    btnSendMessage.IsEnabled = true;
                } )
                );
            }
        }

        private void bwDiscoverDevices_DoWork( object sender, DoWorkEventArgs e ) {
            try {
                bluetooth = new BluetoothClient();
                var gadgeteerDevice = bluetooth.DiscoverDevices().Where( d => d.DeviceName == "HC-06" ).FirstOrDefault();
                if( gadgeteerDevice != null ) {
                    bluetooth.SetPin( "1234" );
                    bluetooth.Connect( gadgeteerDevice.DeviceAddress, BluetoothService.SerialPort );
                    bluetoothStream = bluetooth.GetStream();
                    e.Result = true;
                }
                else {
                    e.Result = false;
                }
            }
            catch( Exception ) {
                e.Result = false;
            }
        }

        private void bwDiscoverDevices_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e ) {
            btnSearchDevice.IsEnabled = true;
            var deviceFound = (bool) e.Result;
            if( !deviceFound ) {
                lblStatus.Text = "No device found";
            }
            else {
                lblStatus.Text = "Connected to HC-06";
                btnSendMessage.IsEnabled = true;
            }
        }

        private void Window_Closing( object sender, CancelEventArgs e ) {
            if( bluetoothStream != null ) {
                bluetoothStream.Close();
                bluetoothStream.Dispose();
            }
        }

        private void btnSendMessage_Click( object sender, RoutedEventArgs e ) {
            if( bluetooth.Connected && bluetoothStream != null ) {
                var buffer = System.Text.Encoding.UTF8.GetBytes( txtMessage.Text );
                bluetoothStream.Write( buffer, 0, buffer.Length );
                txtMessage.Text = string.Empty;
            }
        }
    }
}

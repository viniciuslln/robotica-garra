using InTheHand.Net.Sockets;
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
using System.Windows.Shapes;

namespace BluetoothControl {
    /// <summary>
    /// Interaction logic for SelecionarDispositivo.xaml
    /// </summary>
    public partial class SelecionarDispositivo : Window {

        public delegate void SeleciodadoDispositivoDelegate( BluetoothDeviceInfo device );
        public event SeleciodadoDispositivoDelegate SeleciodadoDispositivoEvent;

        public SelecionarDispositivo( BluetoothClient bluetooth ) {
            InitializeComponent();
            if( bluetooth == null )
                bluetooth = new BluetoothClient();
            LabelProcurando.Visibility = Visibility.Visible;
            ProcurarDispositivos( bluetooth );
        }

        public async Task ProcurarDispositivos( BluetoothClient bluetooth ) {
            await Task.Run( () => {
                var bs = bluetooth.DiscoverDevices();
                Application.Current.Dispatcher.Invoke( new Action( () => {
                    ListaDispositivos.ItemsSource = bs;
                    LabelProcurando.Visibility = Visibility.Collapsed;
                } )
                );
            }

            );
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            var item = ListaDispositivos.SelectedItem as BluetoothDeviceInfo;

            SeleciodadoDispositivoEvent?.Invoke( item );
            this.Close();
        }
    }
}

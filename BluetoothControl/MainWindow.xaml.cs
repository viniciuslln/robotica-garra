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
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace BluetoothControl {
    public partial class MainWindow : Window {
        public class ItemListaSequencia {
            public string nome;
            public ObservableCollection<Sequencia> Lista = new ObservableCollection<Sequencia>();
            public override string ToString() {
                return nome;
            }
        }
        private BluetoothClient bluetooth;
        private Stream bluetoothStream;
        public ObservableCollection<ItemListaSequencia> Sequencias = new ObservableCollection<ItemListaSequencia>();
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
            ListaSequencia.ItemsSource = Sequencias;
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e ) {
            Process.Start( e.Uri.ToString() );
        }

        private void btnSearchDevice_Click( object sender, RoutedEventArgs e ) {
            lblStatus.Text = "Searching for devices...";
            btnSearchDevice.IsEnabled = false;
            BackgroundWorker bwDiscoverDevices = new BackgroundWorker();
            bwDiscoverDevices.DoWork += new DoWorkEventHandler( bwDiscoverDevices_DoWork );
            bwDiscoverDevices.RunWorkerCompleted += new RunWorkerCompletedEventHandler( bwDiscoverDevices_RunWorkerCompleted );
            bwDiscoverDevices.RunWorkerAsync();
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

        public bool EnviarMensagem( string mensagem ) {
            try {
                if( bluetooth != null && bluetooth.Connected && bluetoothStream != null ) {
                    var buffer = System.Text.Encoding.UTF8.GetBytes( mensagem );
                    bluetoothStream.Write( buffer, 0, buffer.Length );
                    return true;
                }
            }
            catch( Exception ex ) { Console.WriteLine( ex.StackTrace ); }
            return false;
        }

        private void btnSendMessage_Click( object sender, RoutedEventArgs e ) {
            if( EnviarMensagem( txtMessage.Text ) ) {
                txtMessage.Text = string.Empty;
            }
            else {
                MessageBox.Show( "Erro ao enviar mensagem" );
            }
        }

        public Sequencia PegarSequencia( int index ) {
            char vel = 'l';
            if( RadioLento.IsChecked ?? false ) {
                vel = 'l';
            }
            else if( RadioLento.IsChecked ?? false ) {
                vel = 'm';
            }
            else {
                vel = 'r';

            }
            double[] sequencia = new double[] {
                SliderGarra.Value,
                SliderPulsoSd.Value,
                SliderPulsoGira.Value,
                SliderCotovelo.Value,
                SliderOmbro.Value,
                SliderCintura.Value,
            };

            return new Sequencia { indice = index, numeros = sequencia, velocidade = vel };
        }

        private void Button_Click_Mover( object sender, RoutedEventArgs e ) {
            if( !EnviarMensagem( PegarSequencia( 0 ).FormatarSequencia() ) ) {
                MessageBox.Show( "Erro ao enviar mensagem" );
            }
        }

        private void Button_Click_Gravar_Posicao( object sender, RoutedEventArgs e ) {
            var selecioado = ListaSequencia.SelectedItem as ItemListaSequencia;
            if( selecioado != null ) {
                selecioado.Lista.Add( PegarSequencia( selecioado.Lista.Count ) );
            }
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {

        }

        private void Button_Click_Nova_Sequencia( object sender, RoutedEventArgs e ) {
            var dialog = new MyDialog();
            if( dialog.ShowDialog() == true ) {
                Sequencias.Add( new ItemListaSequencia { nome = dialog.ResponseText } );
                //MessageBox.Show( "You said: "   );
            }

        }

        private void Button_Click_Apagar_Sequencia( object sender, RoutedEventArgs e ) {
            var index = ListaSequencia.SelectedIndex;
            var item = ListaItemsSequencia.SelectedItem as Sequencia;
            if( item != null ) {
                Sequencias?.ElementAt( index ).Lista.Remove( item );
            }
        }

        private void ListaSequencia_Selected( object sender, RoutedEventArgs e ) {
            var item = ( sender as ListView ).SelectedItem as ItemListaSequencia;
            if( item != null ) {
                ListaItemsSequencia.ItemsSource = item.Lista;
            }
        }
        private void clickSequenciaSalvar( object sender, RoutedEventArgs e ) {
            string json = FileUtils.Serializar( Sequencias );
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog1.DefaultExt = "json";
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.FileName = DateTime.Now.ToString();
            saveFileDialog1.RestoreDirectory = true;


            if( saveFileDialog1.ShowDialog() == true ) {
                FileUtils.Escrever( json, saveFileDialog1.FileName );
            }
        }

        private void clickSequenciaCarregar( object sender, RoutedEventArgs e ) {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            if( op.ShowDialog() == true ) {
                var s = FileUtils.Deserializar( FileUtils.CarregarArquivo( op.FileName ) ) as ObservableCollection<ItemListaSequencia>;
                Sequencias.Clear();
                foreach( var item in s ) {
                    Sequencias.Add( item );
                }

            }
        }

        private void Button_Click_Executar_Sequencia( object sender, RoutedEventArgs e ) {
            var item = ListaSequencia.SelectedItem as ItemListaSequencia;
            foreach( var x in item.Lista ) {
                EnviarMensagem( x.FormatarSequencia() );
            }
        }

        private void ListaItemsSequencia_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
            var item = ( sender as ListView ).SelectedItem as Sequencia;
            if( item == null ) return;
            SliderGarra.Value = item.numeros[0];
            SliderPulsoSd.Value = item.numeros[1];
            SliderPulsoGira.Value = item.numeros[2];
            SliderCotovelo.Value = item.numeros[3];
            SliderOmbro.Value = item.numeros[4];
            SliderCintura.Value = item.numeros[5];

            RadioLento.IsChecked = item.velocidade == 'l';
            RadioNormal.IsChecked = item.velocidade == 'm';
            RadioRapido.IsChecked = item.velocidade == 'r';
        }
    }
}

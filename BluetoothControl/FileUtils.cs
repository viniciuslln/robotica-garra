using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using static BluetoothControl.MainWindow;

namespace BluetoothControl {
    class FileUtils {
        public static string Serializar( object t ) {
            return JsonConvert.SerializeObject( t );
        }

        public static ObservableCollection<ItemListaSequencia> Deserializar( string t ) {
            return JsonConvert.DeserializeObject<ObservableCollection<ItemListaSequencia>>( t );
        }

        public static void Escrever( string json, string caminho ) {
            try {
                if( !File.Exists( caminho ) ) {
                    Directory.CreateDirectory( Path.GetDirectoryName( caminho ) );
                }
                else {
                    File.Delete( caminho );
                }

                using( StreamWriter w = File.AppendText( caminho ) ) {
                    w.WriteLine( json );
                }
            }
            catch( Exception ex ) {
                Console.WriteLine( ex.StackTrace );
            }
        }

        public static string CarregarArquivo( string filePath ) {
            var resp = System.IO.File.ReadAllText( filePath );
            return resp;
        }
    }
}


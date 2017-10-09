using System.Text;

namespace BluetoothControl {
    public partial class MainWindow {
        public class Sequencia {
            public int indice { get; set; }
            public double[] numeros = new double[6];
            public char velocidade { get; set; }

            public string FormatarSequencia( ) {
                string sequencia = "";
                foreach( var item in this.numeros ) {
                    sequencia += (char) item;
                }
                sequencia += velocidade;
                return sequencia;
            }

            public override string ToString() {
                StringBuilder s = new StringBuilder();
                s.Append( $"Sequencia[{indice}]" );
                for( int i = 0; i < numeros.Length; i++ ) {
                    s.Append( numeros[i] );
                    if( i != numeros.Length - 1 ) {
                        s.Append( '-' );
                    }
                }
                return s.ToString();
            }
        }
    }
}

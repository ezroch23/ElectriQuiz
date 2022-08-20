using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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

namespace ElectriQuiz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// El código a continuación esta basado en el desarrollado por Andrés de Obaldía
    /// un respetado miembro del comité de proyectos del cfie 2017
  
    public partial class MainWindow : Window
    {
        // variables globales de la ventana principal
        OpenFileDialog AbrirArchivo = new OpenFileDialog();
        Window controldejuego;
        Window ventanadejuego;
        bool depurando = false;
        // Variables globales publicas
        public static string constring = "nada";
        public static MainWindow main;

        //Para conectar la base de datos con el programa
        static Boolean OpenConnection(string connectionString)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show("conección exitosa con " + connection.DataSource, "Estado");
                    connection.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                    return false;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            main = this;
            Reset.Visibility = Visibility.Hidden;
            //minmax.Visibility = Visibility.Hidden;
            this.KeyDown += new KeyEventHandler(Depuracion);
        }

        private void Depuracion(object sender, KeyEventArgs evento)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.D))
            {
                if (depurando)
                {
                    Reset.Visibility = Visibility.Hidden;
                    //minmax.Visibility = Visibility.Hidden;
                    Reset.IsEnabled = false;
                    depurando = false;
                }
                else
                {
                    Reset.Visibility = Visibility.Visible;
                    //minmax.Visibility = Visibility.Visible;
                    depurando = true;
                }
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.K))
            {
                if (depurando)
                    Reset.IsEnabled = !Reset.IsEnabled;
            }
        }

        private void Buscar_click(object sender, RoutedEventArgs e)
        {
            AbrirArchivo.Filter = "Access Database|*.mdb;*.accdb|All Files|*.*";
            AbrirArchivo.InitialDirectory = "c:\\";
            dynamic result = AbrirArchivo.ShowDialog();
            if (result == true)
            {
                try
                {
                    textBox.Text = AbrirArchivo.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void Conectar_Click(object sender, RoutedEventArgs e)
        {
            string constringprueba = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source =" + textBox.Text;
            if (OpenConnection(constringprueba))
            {
                textBox1.Text = textBox.Text;
                constring = constringprueba;
            }
            else
            {
                textBox1.Text = "";
                constring = "error";
            }
        }
        
            //Aqui debo cambiar cosas (21/8)
        private void Listo_Click(object sender, RoutedEventArgs e)
        {
                controldejuego = new ControlDeJuego();
                ventanadejuego = new VentanaDeJuego();
                controldejuego.Show();
                ventanadejuego.Show();
                this.Hide();
                controldejuego.Closed += new EventHandler(controldejuegocerrado);
                ventanadejuego.Closed += new EventHandler(ventanadejuegocerrada);
        } 

        private void Llenar_click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable tabla = new DataTable();
                OleDbConnection connection = new OleDbConnection(constring);
                connection.Open();
                OleDbCommand comando = new OleDbCommand();
                comando.Connection = connection;
                comando.CommandText = "Select * from PreguntasRespuestas";
                OleDbDataAdapter adaptador = new OleDbDataAdapter(comando);
                adaptador.Fill(tabla);
                dataGrid.DataContext = tabla.DefaultView;
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        void controldejuegocerrado(object sender, EventArgs e)
        {
            this.Show();
            ventanadejuego.Close();
        }

        void ventanadejuegocerrada(object sender, EventArgs e)
        {
            this.Show();
            controldejuego.Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OleDbConnection conexion = new OleDbConnection();
                conexion.ConnectionString = constring;
                conexion.Open();
                OleDbCommand comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandText = "Update PreguntasRespuestas Set Realizada = false";
                comando.ExecuteNonQuery();
                conexion.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
    }
}

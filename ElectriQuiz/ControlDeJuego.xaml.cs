using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Media;
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
using System.IO.Ports;


namespace ElectriQuiz
{
    /// <summary>
    /// Interaction logic for ControlDeJuego.xaml
    /// </summary>
    public partial class ControlDeJuego : Window
    {
        // Variables Globales Privadas
        OleDbConnection conexion;
        Random rand = new Random();
        int minrand = Convert.ToUInt16(MainWindow.main.minid.Text), maxrand = Convert.ToUInt16(MainWindow.main.maxid.Text), maxrepetidas = 300;
        private bool nomas = false;
        public string ELMR, mCategoria, mDificultad, mTipo;
        public int ELMR_respuesta;
        SoundPlayer player = new SoundPlayer("Resources/mostrar_respuesta.wav");
        ImageSource Activo = new BitmapImage(new Uri("Resources/verde.png", UriKind.Relative));
        ImageSource InActivo = new BitmapImage(new Uri("Resources/rosada.png", UriKind.Relative));
        ImageSource CategoriaLogoPredeterminado = new BitmapImage(new Uri("Resources/electriquiz.png", UriKind.Relative));
        private SerialPort serialPort1;

        public ControlDeJuego()
        {
            InitializeComponent();
            desactivar_mostrarB();
            No_ELMR();
            try
            {
                conexion = new OleDbConnection(MainWindow.constring);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        //////////////////////////////////////////////////////////////

        private void Obtener_Click(object sender, RoutedEventArgs e)
        {
            /*en esta funcion al apretar el boton se llenan los espacios en la ventana de control con la informacion
             recogida en las otras funciones, previo a ello se borra el contenido anterior con "reset". Si no hay más preguntas manda error, pero si se obtiene satisfactoriamente
             entonces se marca dicha pregunta y se incrementa el contador de preguntas*/
            Pregunta.Text = ""; 
            Respuesta.Text = "";
            Categoria.Text = "";
            Dificultad.Text = "";
            Tipo.Text = "";
            ID.Text = "";

            if (nomas) //inicialmente "nomas" debe ser false, hay que hacer una condicion en la que sea true
            {
                MessageBox.Show("No hay mas preguntas, consultar la base de datos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (ObtenerPregunta())
            {
                ResetJuego();
                numerodeP.Text = Convert.ToString(Convert.ToInt16(numerodeP.Text) + 1);
                MarcarPregunta();
                //MostrarCategoria.IsEnabled = true;
            }

        }
        
        private void No_ELMR()
        {
            En_caso_de_ELMR.Visibility = Visibility.Hidden;
        }

        private void Si_ELMR()
        {
            En_caso_de_ELMR.Visibility = Visibility.Visible;
        }

        private void desactivar_mostrarB()
        {
            mostrarP_button.IsEnabled = false;
            mostrarR_button.IsEnabled = false;
            mostrarC_button.IsEnabled = false;
        }

        ///////////////////////////////////////////////////////////////
        private void ResetJuego()
        {
            //cuando haga la ventana de juego podre poner este reset
            VentanaDeJuego.ventanita.categoria_x.Text = "";
            VentanaDeJuego.ventanita.pregunta_x.Text = "";
            VentanaDeJuego.ventanita.respuesta_A.Text = "";
            VentanaDeJuego.ventanita.respuesta_B.Text = "";
            VentanaDeJuego.ventanita.respuesta_C.Text = "";
            VentanaDeJuego.ventanita.respuesta_A.Visibility = Visibility.Hidden;
            VentanaDeJuego.ventanita.label_rA.Visibility = Visibility.Hidden;
            VentanaDeJuego.ventanita.respuesta_B.Visibility = Visibility.Hidden;
            VentanaDeJuego.ventanita.label_rB.Visibility = Visibility.Hidden;
            VentanaDeJuego.ventanita.respuesta_C.Visibility = Visibility.Hidden;
            VentanaDeJuego.ventanita.label_rC.Visibility = Visibility.Hidden;
            VentanaDeJuego.ventanita.dificultad_x.Text = "";
            VentanaDeJuego.ventanita.tipo_x.Text = "";
            mostrarP_button.IsEnabled = false;
            mostrarR_button.IsEnabled = false;
            mostrarC_button.IsEnabled = true;
            //ResetActivo();
            VentanaDeJuego.ventanita.LogoCategoria.Source = CategoriaLogoPredeterminado;
            VentanaDeJuego.ventanita.CatListo.Source = InActivo;
            //VentanaDeJuego.ventanita.DetenerTemporizador();
            //VentanaDeJuego.ventanita.Conteo.Visibility = Visibility.Hidden;
            //VentanaDeJuego.ventanita.Conteo.Text = "10";
        }

        private void CambiarNombres_Click(object sender, RoutedEventArgs e)
        {
            VentanaDeJuego.ventanita.E1.Text = E1CJ.Text;
            VentanaDeJuego.ventanita.E2.Text = E2CJ.Text;
            VentanaDeJuego.ventanita.E3.Text = E3CJ.Text;
        }

        private void ActivarE1_Click(object sender, RoutedEventArgs e)
        {            
            reset.IsEnabled = true;
            VentanaDeJuego.ventanita.E1a.Source = Activo;
        }

        private void ActivarE2_Click(object sender, RoutedEventArgs e)
        {
            reset.IsEnabled = true;
            VentanaDeJuego.ventanita.E2a.Source = Activo;
        }

        private void ActivarE3_Click(object sender, RoutedEventArgs e)
        {
            reset.IsEnabled = true;
            VentanaDeJuego.ventanita.E3a.Source = Activo;
        }


        private void ResetActivo()
        {            
            VentanaDeJuego.ventanita.E1a.Source = InActivo;
            VentanaDeJuego.ventanita.E2a.Source = InActivo;
            VentanaDeJuego.ventanita.E3a.Source = InActivo;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetActivo();
        }

        private void Reset_ronda(object sender, RoutedEventArgs e)
        {
            numerodeP.Text = "0";
        }

        private void iniciar_contador(object sender, RoutedEventArgs e)
        {

        }


        private bool ObtenerPregunta()
        {
            bool prueba = false, realizada = true;
            int id, repetidas = 0;
            string pregunta = "", respuesta = "", categoria = "", dificultad ="", tipo= "", Sid="", respuestab="", respuestac="";

            Baitzadastu: //Una etiqueta para volver a repetir este proceso

            if (repetidas > maxrepetidas)
            {
                //En caso de exceder el numero de preguntas esta funcion muestra error
                MessageBox.Show("Error: No se pudo obtener una nueva pregunta", "No más preguntas");
                nomas = true;
                return false;
            }

            id = rand.Next(minrand, maxrand); //Genera un ID random entre los valores max-min   

            try
            { 
                conexion.Open();
                OleDbCommand comando = new OleDbCommand
                {
                    Connection = conexion,
                    CommandText = "Select * from PreguntasRespuestas Where ID =" + id + ""
                };
                OleDbDataReader lectura = comando.ExecuteReader();

                while (lectura.Read())
                {
                    pregunta = lectura["PREGUNTA"].ToString();
                    respuesta = lectura["RESPUESTA"].ToString();
                    categoria = lectura["CATEGORIA"].ToString();
                    dificultad = lectura["DIFICULTAD"].ToString();
                    tipo = lectura["TIPO"].ToString();
                    Sid = lectura["ID"].ToString();
                    realizada = Convert.ToBoolean(lectura["Realizado"]);
                    respuestab= lectura["RESPUESTA B"].ToString();
                    respuestac = lectura["RESPUESTA c"].ToString();

                    if (Convert.ToUInt16(numerodeP.Text) <= (4-1))
                    {
                        if ((Convert.ToUInt16(dificultad) > 0) & (Convert.ToUInt16(dificultad) <=2)) //Verificar si esta "Dificultad.Text" es la que es
                        {
                            goto Soryeketon;                            
                        }
                        else
                        {
                            conexion.Close();
                            goto Baitzadastu;
                        }
                    }
                    else
                    {
                        if(Convert.ToUInt16(numerodeP.Text) == (5-1))
                        {
                            if(Convert.ToUInt16(dificultad) == 3)
                            {
                                goto Soryeketon;
                            }
                            else
                            {
                                conexion.Close();
                                goto Baitzadastu;
                            }
                        }
                        else
                        {
                            if(Convert.ToUInt16(numerodeP.Text) == (6-1))
                            {
                                if(categoria == "Eléctrica")
                                {                                    
                                    goto Soryeketon;
                                }
                                else
                                {
                                    conexion.Close();
                                    goto Baitzadastu;
                                }
                            }
                            if (Convert.ToUInt16(numerodeP.Text) == (7-1))
                            {
                                if(Convert.ToUInt16(dificultad) == 0)
                                {
                                    goto Soryeketon;
                                }
                                else
                                {
                                    conexion.Close();
                                    goto Baitzadastu;
                                }
                            }
                        }
                    }

                    Soryeketon:
                    ELMR = tipo;
                    if (realizada)
                    {
                        //Si la pregunta ya fue realizada entonces se vuelve al comienzo "Baitzadastu"
                        conexion.Close();
                        goto Baitzadastu;
                    }

                    prueba = true; //si no funciona probar en cada "if"

                    if (ELMR=="Escoger la mejor respuesta")
                    {
                        Si_ELMR();
                    }
                    else
                    {
                        No_ELMR();
                    }

                    if (prueba)
                    {
                        Pregunta.Text = pregunta;
                        Respuesta.Text = respuesta;
                        Categoria.Text = categoria;
                        Dificultad.Text = dificultad;
                        Tipo.Text = tipo;
                        ID.Text = Sid;
                        Respuesta2.Text = respuestab;
                        Respuesta3.Text = respuestac;

                        //conexion.Close();
                    }
                    else
                    {
                        goto Baitzadastu;
                    }

                }
                conexion.Close();
            }
            catch(Exception ex)
            {
                conexion.Close();
                MessageBox.Show(ex.Message, "Error");
                return false;
            }

            /*
            if (prueba)
            {
                Pregunta.Text = pregunta;
                Respuesta.Text = respuesta;
                Categoria.Text = categoria;
                Dificultad.Text = dificultad;
                Tipo.Text = tipo;
                ID.Text = Sid;
                //conexion.Close();
            }
            else
            {
                goto Baitzadastu;
            }
            */
            return true;
        }        

        /////////////////////////////////////////////////////////////////       


        private void MostrarPregunta_Click(object sender, RoutedEventArgs e)
        {
            int Rcorrecta;
            Rcorrecta = rand.Next(1, 7);
            mostrarR_button.IsEnabled = true;            
            VentanaDeJuego.ventanita.tipo_x.Text = Tipo.Text;

            try
            {

                switch (Convert.ToUInt16(Dificultad.Text))
                {
                    case 1:
                        VentanaDeJuego.ventanita.dificultad_x.Text = "Fácil (1)";
                        break;
                    case 2:
                        VentanaDeJuego.ventanita.dificultad_x.Text = "Intermedio (2)";
                        break;
                    case 3:
                        VentanaDeJuego.ventanita.dificultad_x.Text = "Dificil (3)";
                        break;
                }
                mostrarP_button.IsEnabled = false;
                mostrarR_button.IsEnabled = true;
                VentanaDeJuego.ventanita.ID.Text = ID.Text;

                if (ELMR == "Escoger la mejor respuesta")
                {
                    VentanaDeJuego.ventanita.pregunta_x.Text = Pregunta.Text;
                    VentanaDeJuego.ventanita.respuesta_A.Visibility = Visibility.Visible;
                    VentanaDeJuego.ventanita.respuesta_B.Visibility = Visibility.Visible;
                    VentanaDeJuego.ventanita.respuesta_C.Visibility = Visibility.Visible;
                    VentanaDeJuego.ventanita.label_rA.Visibility = Visibility.Visible;
                    VentanaDeJuego.ventanita.label_rA.Content = "Opción  A)";
                    VentanaDeJuego.ventanita.label_rB.Visibility = Visibility.Visible;
                    VentanaDeJuego.ventanita.label_rB.Content = "Opción  B)";
                    VentanaDeJuego.ventanita.label_rC.Visibility = Visibility.Visible;
                    VentanaDeJuego.ventanita.label_rC.Content = "Opción  C)";
                    switch (Rcorrecta)
                    {
                        case 1:
                            VentanaDeJuego.ventanita.respuesta_A.Text = Respuesta.Text;
                            VentanaDeJuego.ventanita.respuesta_B.Text = Respuesta2.Text;
                            VentanaDeJuego.ventanita.respuesta_C.Text = Respuesta3.Text;
                            break;
                        case 2:
                            VentanaDeJuego.ventanita.respuesta_A.Text = Respuesta.Text;
                            VentanaDeJuego.ventanita.respuesta_B.Text = Respuesta3.Text;
                            VentanaDeJuego.ventanita.respuesta_C.Text = Respuesta2.Text;
                            break;
                        case 3:
                            VentanaDeJuego.ventanita.respuesta_A.Text = Respuesta2.Text;
                            VentanaDeJuego.ventanita.respuesta_B.Text = Respuesta.Text;
                            VentanaDeJuego.ventanita.respuesta_C.Text = Respuesta3.Text;
                            break;
                        case 4:
                            VentanaDeJuego.ventanita.respuesta_A.Text = Respuesta3.Text;
                            VentanaDeJuego.ventanita.respuesta_B.Text = Respuesta.Text;
                            VentanaDeJuego.ventanita.respuesta_C.Text = Respuesta2.Text;
                            break;
                        case 5:
                            VentanaDeJuego.ventanita.respuesta_A.Text = Respuesta2.Text;
                            VentanaDeJuego.ventanita.respuesta_B.Text = Respuesta3.Text;
                            VentanaDeJuego.ventanita.respuesta_C.Text = Respuesta.Text;
                            break;
                        case 6:
                            VentanaDeJuego.ventanita.respuesta_A.Text = Respuesta3.Text;
                            VentanaDeJuego.ventanita.respuesta_B.Text = Respuesta2.Text;
                            VentanaDeJuego.ventanita.respuesta_C.Text = Respuesta.Text;
                            break;
                    }
                    VentanaDeJuego.ventanita.CatListo.Source = Activo;
                }
                else
                {
                    VentanaDeJuego.ventanita.pregunta_x.Text = Pregunta.Text;
                    VentanaDeJuego.ventanita.CatListo.Source = Activo;
                    VentanaDeJuego.ventanita.respuesta_A.Visibility = Visibility.Hidden;
                    VentanaDeJuego.ventanita.respuesta_B.Visibility = Visibility.Hidden;
                    VentanaDeJuego.ventanita.respuesta_C.Visibility = Visibility.Hidden;
                    VentanaDeJuego.ventanita.label_rA.Visibility = Visibility.Hidden;
                    VentanaDeJuego.ventanita.label_rB.Visibility = Visibility.Hidden;
                    VentanaDeJuego.ventanita.label_rC.Visibility = Visibility.Hidden;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

            ELMR_respuesta = Rcorrecta;
        }

        private void MostrarCategoria_Click(object sender, RoutedEventArgs e)
        {
            mostrarP_button.IsEnabled = true;
            VentanaDeJuego.ventanita.CambiarCategoria(Categoria.Text);
            VentanaDeJuego.ventanita.categoria_x.Text = Categoria.Text;
            mostrarR_button.IsEnabled = false;
            mostrarC_button.IsEnabled = false;
        }

        private void MostrarRespuesta_Click(object sender, RoutedEventArgs e)
        {

            //player.Play();
            mostrarP_button.IsEnabled = false;
            mostrarR_button.IsEnabled = false;
            mostrarC_button.IsEnabled = false;
            try
            {
                if (ELMR == "Escoger la mejor respuesta")
                {
                    switch (ELMR_respuesta)
                    {
                        case 1:
                            VentanaDeJuego.ventanita.label_rA.Content = "Respuesta: Opción A)";
                            VentanaDeJuego.ventanita.respuesta_B.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.respuesta_C.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rB.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rC.Visibility = Visibility.Hidden;
                            break;
                        case 2:
                            VentanaDeJuego.ventanita.label_rA.Content = "Respuesta: Opción A)";
                            VentanaDeJuego.ventanita.respuesta_B.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.respuesta_C.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rB.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rC.Visibility = Visibility.Hidden;
                            break;
                        case 3:
                            VentanaDeJuego.ventanita.label_rB.Content = "Respuesta: Opción B)";
                            VentanaDeJuego.ventanita.respuesta_A.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.respuesta_C.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rA.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rC.Visibility = Visibility.Hidden;
                            break;
                        case 4:
                            VentanaDeJuego.ventanita.label_rB.Content = "Respuesta: Opción B)";
                            VentanaDeJuego.ventanita.respuesta_A.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.respuesta_C.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rA.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rC.Visibility = Visibility.Hidden;
                            break;
                        case 5:
                            VentanaDeJuego.ventanita.label_rC.Content = "Respuesta: Opción C)";
                            VentanaDeJuego.ventanita.respuesta_A.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.respuesta_B.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rB.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rA.Visibility = Visibility.Hidden;
                            break;
                        case 6:
                            VentanaDeJuego.ventanita.label_rC.Content = "Respuesta: Opción C)";
                            VentanaDeJuego.ventanita.respuesta_A.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.respuesta_B.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rB.Visibility = Visibility.Hidden;
                            VentanaDeJuego.ventanita.label_rA.Visibility = Visibility.Hidden;
                            break;
                    }

                }
                else
                {
                    VentanaDeJuego.ventanita.label_rA.Content = "Respuesta";
                    VentanaDeJuego.ventanita.label_rA.Visibility = Visibility.Visible;
                    VentanaDeJuego.ventanita.respuesta_A.Visibility = Visibility.Visible;
                    VentanaDeJuego.ventanita.respuesta_A.Text = Respuesta.Text;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

        }

        //
        private void MarcarPregunta()
        {
            try
            {
                conexion.Open();
                OleDbCommand comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandText = "Update PreguntasRespuestas Set Realizada = true Where ID =" + ID.Text + "";
                //comando.ExecuteNonQuery();
                conexion.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ElectriQuiz
{

    public partial class VentanaDeJuego : Window
    {

        // Variables goblales publicas
        public static VentanaDeJuego ventanita;
        const int delay = 60;
        //DispatcherTimer temporizador;

        // Variables globlales privadas
        
        ImageSource Ciencias = new BitmapImage(new Uri("Resources/Ciencias.png", UriKind.Relative));
        ImageSource Bono = new BitmapImage(new Uri("Resources/Bono.png", UriKind.Relative));
        ImageSource Farandula = new BitmapImage(new Uri("Resources/Farándula.png", UriKind.Relative));
        ImageSource HyG = new BitmapImage(new Uri("Resources/HyG.png", UriKind.Relative));
        ImageSource Electrica = new BitmapImage(new Uri("Resources/Eléctrica.png", UriKind.Relative));
        ImageSource Deporte = new BitmapImage(new Uri("Resources/Deportes.png", UriKind.Relative));
        ImageSource UTP = new BitmapImage(new Uri("Resources/UTP.png", UriKind.Relative));
        ImageSource CulturaPop = new BitmapImage(new Uri("Resources/culturaPop.png", UriKind.Relative));
        ImageSource Activo = new BitmapImage(new Uri("Resources/verde.png", UriKind.Relative));
        ImageSource InActivo = new BitmapImage(new Uri("Resources/rosada.png", UriKind.Relative));
        

        public VentanaDeJuego()
        {
            InitializeComponent();
            ventanita = this;
            categoria_x.Text = "";
            pregunta_x.Text = "";
            respuesta_A.Text = "";
            respuesta_B.Text = "";
            respuesta_C.Text = "";
            tipo_x.Text = "";
            dificultad_x.Text = "";
            numerodeP.Visibility = Visibility.Hidden;
            E1.Text = "";
            E2.Text = "";
            E3.Text = "";
 
        }

        
         //Logica de la animación de las categorias, creado por Andres de Obaldia (2017)
        public async void CambiarCategoria(string categoria)
        {
            try
            {
                categoria_x.Visibility = Visibility.Hidden;
                int ciclo = 0;
                while (ciclo != 3)
                {
                    await Task.Delay(delay);
                    LogoCategoria.Source = Ciencias;
                    await Task.Delay(delay);
                    LogoCategoria.Source = Bono;
                    await Task.Delay(delay);
                    LogoCategoria.Source = Farandula;
                    await Task.Delay(delay);
                    LogoCategoria.Source = HyG;
                    await Task.Delay(delay);
                    LogoCategoria.Source = CulturaPop;
                    await Task.Delay(delay);
                    LogoCategoria.Source = Electrica;
                    await Task.Delay(delay);
                    LogoCategoria.Source = Deporte;
                    await Task.Delay(delay);
                    LogoCategoria.Source = UTP;
                    ciclo++;
                }
                await Task.Delay(delay);
                LogoCategoria.Source = Ciencias;
                CatListo.Source = Activo;
                if (categoria == "Ciencias")
                    return;
                await Task.Delay(delay);
                LogoCategoria.Source = Bono;
                CatListo.Source = Activo;
                if (categoria == "Bono")
                    return;
                await Task.Delay(delay);
                LogoCategoria.Source = Farandula;
                CatListo.Source = Activo;
                if (categoria == "Farándula")
                    return;
                await Task.Delay(delay);
                LogoCategoria.Source = HyG;
                CatListo.Source = Activo;
                if (categoria == "Historia y geografía")
                    return;
                await Task.Delay(delay);
                LogoCategoria.Source = CulturaPop;
                CatListo.Source = Activo;
                if (categoria == "Cultura popular")
                    return;
                await Task.Delay(delay);
                LogoCategoria.Source = Electrica;
                CatListo.Source = Activo;
                if (categoria == "Eléctrica")
                    return;
                await Task.Delay(delay);
                LogoCategoria.Source = Deporte;
                CatListo.Source = Activo;
                if (categoria == "Deportes")
                    return;
                await Task.Delay(delay);
                LogoCategoria.Source = UTP;
                CatListo.Source = Activo;
                if (categoria == "UTP")
                    return;                
                Icon img = SystemIcons.Warning;
                Bitmap bitmap = img.ToBitmap();
                IntPtr hBitmap = bitmap.GetHbitmap();
                ImageSource wpfBitmap =
                     Imaging.CreateBitmapSourceFromHBitmap(
                          hBitmap, IntPtr.Zero, Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
                LogoCategoria.Source = wpfBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                //CatListo.Source = Activo;
                categoria_x.Visibility = Visibility.Visible;
            }
        }
        
    }
}

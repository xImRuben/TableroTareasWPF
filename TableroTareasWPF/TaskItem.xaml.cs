using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media; // Necesario para los colores

namespace TableroTareasWPF
{
    public partial class TaskItem : UserControl
    {
        public Tarea DatosTarea { get; private set; }

        // Eventos para comunicar al tablero que se pulsó un botón
        public event EventHandler EditarClicked;
        public event EventHandler EliminarClicked;

        public TaskItem(Tarea tarea)
        {
            InitializeComponent();
            DatosTarea = tarea;
            CargarDatos();
        }

        // Método para pintar los datos en la tarjeta visualmente
        public void CargarDatos()
        {
            if (DatosTarea != null)
            {
                txtTitulo.Text = DatosTarea.Titulo;
                txtDescripcion.Text = DatosTarea.Descripcion;

                // Mostramos la fecha en formato corto (Día/Mes Hora:Minuto)
                txtFecha.Text = DatosTarea.FechaCreacion.ToString("dd/MM HH:mm");

                // --- LÓGICA DE COLOR ---
                // Calculamos el color aquí en la Vista, en lugar de guardarlo en el archivo JSON.
                // Esto evita el error de guardado y mantiene los colores dinámicos.
                SolidColorBrush colorPrioridad;

                switch (DatosTarea.Prioridad)
                {
                    case "Alta":
                        // Rojo suave
                        colorPrioridad = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5252"));
                        break;
                    case "Baja":
                        // Verde suave
                        colorPrioridad = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                        break;
                    default:
                        // "Media" u otros -> Naranja/Amarillo
                        colorPrioridad = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAB00"));
                        break;
                }

                // Pintamos la franja lateral
                PriorityStrip.Background = colorPrioridad;
            }
        }

        // --- BOTONES ---

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            EditarClicked?.Invoke(this, EventArgs.Empty);
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            EliminarClicked?.Invoke(this, EventArgs.Empty);
        }

        // --- ARRASTRAR (DRAG & DROP) ---

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // Solo iniciamos el arrastre si se mantiene pulsado el botón izquierdo
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Empaquetamos este control (this) para enviarlo al tablero
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
        }
    }
}
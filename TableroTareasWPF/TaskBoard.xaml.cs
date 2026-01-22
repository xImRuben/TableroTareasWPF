using System;
using System.Collections.Generic;
using System.Linq; // Necesario para Linq
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TableroTareasWPF
{
    public partial class TaskBoard : UserControl
    {
        public event EventHandler DatosCambiados;

        // Diccionario para controlar las columnas: "NombreEstado" -> StackPanelVisual
        private Dictionary<string, StackPanel> _columnas = new Dictionary<string, StackPanel>();

        // Lista de colores pastel para ir rotando en las nuevas columnas
        private readonly string[] _coloresPastel = {
            "#FFF0F0", // Rojo (Pendiente)
            "#FFFBE6", // Amarillo (En Proceso)
            "#E6FFED", // Verde (Completado)
            "#E3F2FD", // Azul suave
            "#F3E5F5", // Lila suave
            "#E0F2F1", // Teal suave
            "#FFF3E0"  // Naranja suave
        };
        private int _contadorColor = 0;

        public TaskBoard()
        {
            InitializeComponent();

            // Inicializamos las 3 columnas por defecto
            AgregarNuevaColumna("Pendiente");
            AgregarNuevaColumna("En Proceso");
            AgregarNuevaColumna("Completado");
        }

        // --- GESTIÓN DINÁMICA DE COLUMNAS ---

        public void AgregarNuevaColumna(string titulo)
        {
            if (_columnas.ContainsKey(titulo)) return; // Evitar duplicados

            // 1. Elegir color
            string colorHex = _coloresPastel[_contadorColor % _coloresPastel.Length];
            _contadorColor++;

            // 2. Crear Estructura Visual (Equivalente al XAML que tenías antes)
            var borderPrincipal = new Border
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex),
                CornerRadius = new CornerRadius(12),
                Width = 300, // Ancho fijo por columna
                Margin = new Thickness(0, 0, 15, 0), // Espacio a la derecha
                Padding = new Thickness(0)
            };

            // Efecto de sombra
            var effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                ShadowDepth = 1,
                BlurRadius = 5,
                Opacity = 0.1,
                Color = Colors.Black
            };
            borderPrincipal.Effect = effect;

            // Grid interno
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Contenido

            // -- Header --
            var headerBorder = new Border { Background = Brushes.Transparent, Padding = new Thickness(15) };
            var headerStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            // Bolita de color (un poco más oscuro que el fondo)
            var colorBola = darkenColor((SolidColorBrush)borderPrincipal.Background);
            var bola = new Border { Width = 10, Height = 10, CornerRadius = new CornerRadius(10), Background = colorBola, Margin = new Thickness(0, 0, 10, 0) };

            var tituloBlock = new TextBlock { Text = titulo.ToUpper(), FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(66, 82, 110)), FontSize = 14 };

            headerStack.Children.Add(bola);
            headerStack.Children.Add(tituloBlock);
            headerBorder.Child = headerStack;

            // -- Área de Tareas (StackPanel) --
            var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(8, 0, 8, 8) };
            var stackTareas = new StackPanel { MinHeight = 150, Background = Brushes.Transparent };

            // Habilitar Drag & Drop
            stackTareas.AllowDrop = true;
            stackTareas.Drop += Columna_Drop;
            stackTareas.DragOver += Columna_DragOver;

            // Etiqueta para saber qué estado es este StackPanel al soltar
            stackTareas.Tag = titulo;

            scrollViewer.Content = stackTareas;

            // Montar todo
            grid.Children.Add(headerBorder); // Row 0
            Grid.SetRow(scrollViewer, 1);
            grid.Children.Add(scrollViewer); // Row 1
            borderPrincipal.Child = grid;

            // 3. Añadir al UI y al Diccionario
            PanelColumnas.Children.Add(borderPrincipal);
            _columnas.Add(titulo, stackTareas);
        }

        // Helper para oscurecer el color de la bolita
        private SolidColorBrush darkenColor(SolidColorBrush brush)
        {
            Color c = brush.Color;
            return new SolidColorBrush(Color.FromRgb((byte)(c.R * 0.8), (byte)(c.G * 0.8), (byte)(c.B * 0.8)));
        }

        // --- LÓGICA DE TAREAS ACTUALIZADA ---

        public void AgregarTarea(Tarea nuevaTarea)
        {
            // Si la columna no existe (ej. cargando JSON antiguo), la creamos al vuelo
            if (!_columnas.ContainsKey(nuevaTarea.Estado))
            {
                AgregarNuevaColumna(nuevaTarea.Estado);
            }

            TaskItem item = new TaskItem(nuevaTarea);
            item.EliminarClicked += Item_EliminarClicked;
            item.EditarClicked += Item_EditarClicked;

            // Añadir al StackPanel correspondiente buscándolo en el diccionario
            _columnas[nuevaTarea.Estado].Children.Add(item);

            DatosCambiados?.Invoke(this, EventArgs.Empty);
        }

        // --- MÉTODOS DE SOPORTE (Conteos, Limpieza, Filtros) ---

        public void LimpiarTablero()
        {
            // Opcional: ¿Borrar columnas o solo tareas? 
            // Para mantener consistencia con el JSON, borramos solo tareas.
            foreach (var panel in _columnas.Values)
            {
                panel.Children.Clear();
            }
        }

        public (int total, int completadas) ObtenerConteo()
        {
            int total = 0;
            int completadas = 0;

            foreach (var kvp in _columnas)
            {
                total += kvp.Value.Children.Count;
                if (kvp.Key == "Completado") // Asumimos que "Completado" es la clave de éxito
                    completadas += kvp.Value.Children.Count;
            }
            return (total, completadas);
        }

        public List<Tarea> ObtenerTodasLasTareas()
        {
            List<Tarea> lista = new List<Tarea>();
            foreach (var panel in _columnas.Values)
            {
                foreach (TaskItem item in panel.Children)
                {
                    lista.Add(item.DatosTarea);
                }
            }
            return lista;
        }

        public void FiltrarTareas(string texto)
        {
            texto = texto.ToLower();
            foreach (var panel in _columnas.Values)
            {
                foreach (UIElement element in panel.Children)
                {
                    if (element is TaskItem item)
                    {
                        bool coincide = string.IsNullOrEmpty(texto) ||
                                        item.DatosTarea.Titulo.ToLower().Contains(texto) ||
                                        item.DatosTarea.Descripcion.ToLower().Contains(texto);
                        item.Visibility = coincide ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        // --- EVENTOS INTERNOS DE TAREA ---

        private void Item_EliminarClicked(object sender, EventArgs e)
        {
            if (sender is TaskItem tarea)
            {
                var result = MessageBox.Show($"¿Eliminar '{tarea.DatosTarea.Titulo}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    (tarea.Parent as Panel)?.Children.Remove(tarea);
                    DatosCambiados?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void Item_EditarClicked(object sender, EventArgs e)
        {
            if (sender is TaskItem tarea)
            {
                var ventana = new EditTaskWindow(tarea.DatosTarea.Titulo, tarea.DatosTarea.Descripcion, tarea.DatosTarea.Prioridad);
                if (ventana.ShowDialog() == true)
                {
                    tarea.DatosTarea.Titulo = ventana.TituloResult;
                    tarea.DatosTarea.Descripcion = ventana.DescripcionResult;
                    tarea.DatosTarea.Prioridad = ventana.PrioridadResult;
                    tarea.CargarDatos();
                    DatosCambiados?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // --- DRAG & DROP DINÁMICO ---

        private void Columna_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TaskItem))) e.Effects = DragDropEffects.Move;
            else e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Columna_Drop(object sender, DragEventArgs e)
        {
            TaskItem tarea = e.Data.GetData(typeof(TaskItem)) as TaskItem;
            StackPanel nuevaColumna = sender as StackPanel;

            if (tarea != null && nuevaColumna != null)
            {
                Panel padreAnterior = tarea.Parent as Panel;
                if (padreAnterior != null)
                {
                    padreAnterior.Children.Remove(tarea);
                    nuevaColumna.Children.Add(tarea);

                    // ACTUALIZACIÓN DINÁMICA DEL ESTADO
                    // Usamos el .Tag que le pusimos al crear la columna
                    if (nuevaColumna.Tag != null)
                    {
                        tarea.DatosTarea.Estado = nuevaColumna.Tag.ToString();
                    }

                    DatosCambiados?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
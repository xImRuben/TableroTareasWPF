using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Necesario para impresión
using LiveCharts;
using LiveCharts.Wpf;

namespace TableroTareasWPF
{
    public partial class FrmInformeKanban : Window
    {
        private List<Tarea> _todasLasTareas;
        private List<Tarea> _tareasVisibles;

        public FrmInformeKanban(List<Tarea> tareas)
        {
            InitializeComponent();
            _todasLasTareas = tareas ?? new List<Tarea>();
            _tareasVisibles = new List<Tarea>(_todasLasTareas);

            // NUEVO: Poner fecha actual automáticamente
            txtFechaInforme.Text = $"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}";

            ActualizarInterfaz();
        }

        // --- LÓGICA DE IMPRESIÓN (EL FACTOR "WOW") ---
        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Ocultamos temporalmente los botones de control para que no salgan en el papel
                // (Esto es un truco visual para que el PDF quede limpio)
                // Opcional: podrías ocultar el botón de exportar si quisieras.

                // Imprimir el Grid Principal ("GridInforme")
                printDialog.PrintVisual(GridInforme, "Informe Kanban");
            }
        }

        // --- EL RESTO DEL CÓDIGO SIGUE IGUAL ---

        private void CmbFiltroPrioridad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_todasLasTareas == null) return;
            var comboItem = cmbFiltroPrioridad.SelectedItem as ComboBoxItem;
            string prioridadSeleccionada = comboItem?.Content.ToString();

            if (prioridadSeleccionada == "Todas" || string.IsNullOrEmpty(prioridadSeleccionada))
                _tareasVisibles = new List<Tarea>(_todasLasTareas);
            else
                _tareasVisibles = _todasLasTareas.Where(t => t.Prioridad == prioridadSeleccionada).ToList();

            ActualizarInterfaz();
        }

        private void ActualizarInterfaz()
        {
            CargarEstadisticas();
            if (cmbTipoGrafico.SelectedIndex == 0) CargarGraficoCircular();
            else CargarGraficoColumnas();
            CargarTabla();
        }

        private void CargarEstadisticas()
        {
            if (_tareasVisibles == null) return;
            int total = _tareasVisibles.Count;
            int completadas = _tareasVisibles.Count(t => t.Estado == "Completado");
            double porcentaje = total > 0 ? (double)completadas / total * 100 : 0;

            txtTotalTareas.Text = total.ToString();
            txtCompletadas.Text = completadas.ToString();
            txtPorcentaje.Text = $"{porcentaje:F1}%";
        }

        private void CargarGraficoCircular()
        {
            if (_tareasVisibles == null) return;
            var grupos = _tareasVisibles.GroupBy(t => t.Estado);
            SeriesCollection series = new SeriesCollection();

            foreach (var grupo in grupos)
            {
                series.Add(new PieSeries
                {
                    Title = grupo.Key,
                    Values = new ChartValues<int> { grupo.Count() },
                    DataLabels = true
                });
            }
            ContenedorGrafico.Content = new PieChart { Series = series, LegendLocation = LegendLocation.Right, InnerRadius = 50 };
        }

        private void CargarGraficoColumnas()
        {
            if (_tareasVisibles == null) return;
            var grupos = _tareasVisibles.GroupBy(t => t.Estado);
            SeriesCollection series = new SeriesCollection();
            List<string> etiquetas = new List<string>();
            var columnaSerie = new ColumnSeries { Title = "Tareas", Values = new ChartValues<int>() };

            foreach (var grupo in grupos)
            {
                columnaSerie.Values.Add(grupo.Count());
                etiquetas.Add(grupo.Key);
            }
            series.Add(columnaSerie);
            var cartesian = new CartesianChart { Series = series, LegendLocation = LegendLocation.None };
            cartesian.AxisX.Add(new Axis { Labels = etiquetas });
            ContenedorGrafico.Content = cartesian;
        }

        private void CmbTipoGrafico_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContenedorGrafico == null || _tareasVisibles == null) return;
            if (cmbTipoGrafico.SelectedIndex == 0) CargarGraficoCircular();
            else CargarGraficoColumnas();
        }

        private void CargarTabla()
        {
            if (_tareasVisibles == null) return;
            gridDetalles.ItemsSource = null;
            gridDetalles.ItemsSource = _tareasVisibles;
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (_tareasVisibles == null || _tareasVisibles.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.");
                return;
            }
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Archivo CSV (*.csv)|*.csv";
                saveFileDialog.FileName = $"ReporteKanban_{DateTime.Now:yyyyMMdd}.csv"; // Nombre mejorado

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        sw.WriteLine("Titulo,Estado,Prioridad,Responsable,Fecha");
                        foreach (var t in _tareasVisibles)
                        {
                            string titulo = t.Titulo?.Replace(",", " ") ?? "";
                            string resp = t.Responsable ?? "Sin asignar";
                            sw.WriteLine($"{titulo},{t.Estado},{t.Prioridad},{resp},{t.FechaCreacion}");
                        }
                    }
                    MessageBox.Show("Informe exportado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
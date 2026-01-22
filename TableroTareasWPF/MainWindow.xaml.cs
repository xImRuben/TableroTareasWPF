using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TableroTareasWPF
{
    public partial class MainWindow : Window
    {
        private const string ArchivoDatos = "mis_tareas_kanban_pro.json";

        public MainWindow()
        {
            InitializeComponent();

            CargarTareasDesdeArchivo();
            this.Closing += MainWindow_Closing;
            tableroPrincipal.DatosCambiados += TableroPrincipal_DatosCambiados;
            ActualizarDashboard();
        }

        // --- EVENTO DEL INFORME (AQUÍ ESTABA EL ERROR) ---
        private void BtnInforme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var todasLasTareas = tableroPrincipal.ObtenerTodasLasTareas();
                var reporteWindow = new FrmInformeKanban(todasLasTareas);
                reporteWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir informe: {ex.Message}");
            }
        }

        private void BtnNuevaTarea_Click(object sender, RoutedEventArgs e)
        {
            var ventanaCreacion = new EditTaskWindow { Title = "Nueva Tarea" };

            if (ventanaCreacion.ShowDialog() == true)
            {
                var nuevaTarea = new Tarea
                {
                    Titulo = ventanaCreacion.TituloResult,
                    Descripcion = ventanaCreacion.DescripcionResult,
                    Prioridad = ventanaCreacion.PrioridadResult,
                    Estado = "Pendiente",
                    FechaCreacion = DateTime.Now,
                    Responsable = "Sin asignar"
                };

                tableroPrincipal.AgregarTarea(nuevaTarea);
            }
        }

        private void BtnNuevaColumna_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputWindow();
            if (input.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(input.ResponseText))
                {
                    tableroPrincipal.AgregarNuevaColumna(input.ResponseText);
                }
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            tableroPrincipal.FiltrarTareas(txtBuscar.Text);
        }

        private void TableroPrincipal_DatosCambiados(object sender, EventArgs e)
        {
            ActualizarDashboard();
        }

        private void ActualizarDashboard()
        {
            var (total, completadas) = tableroPrincipal.ObtenerConteo();
            txtProgresoTexto.Text = $"{completadas}/{total} Tareas";

            if (total > 0)
            {
                double porcentaje = (double)completadas / total;
                Dispatcher.Invoke(() =>
                {
                    double anchoTotal = ((Panel)barraProgresoRelleno.Parent).ActualWidth;
                    if (anchoTotal <= 0) anchoTotal = 300;
                    barraProgresoRelleno.Width = anchoTotal * porcentaje;
                });
            }
            else
            {
                barraProgresoRelleno.Width = 0;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GuardarTareasEnArchivo();
        }

        private void GuardarTareasEnArchivo()
        {
            try
            {
                List<Tarea> todasLasTareas = tableroPrincipal.ObtenerTodasLasTareas();
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(todasLasTareas, opciones);
                File.WriteAllText(ArchivoDatos, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}");
            }
        }

        private void CargarTareasDesdeArchivo()
        {
            if (File.Exists(ArchivoDatos))
            {
                try
                {
                    string json = File.ReadAllText(ArchivoDatos);
                    List<Tarea> tareasCargadas = JsonSerializer.Deserialize<List<Tarea>>(json);
                    if (tareasCargadas != null)
                    {
                        tableroPrincipal.LimpiarTablero();
                        foreach (var t in tareasCargadas) tableroPrincipal.AgregarTarea(t);
                    }
                }
                catch { }
            }
        }
    }
}
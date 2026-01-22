using System.Windows;
using System.Windows.Controls;

namespace TableroTareasWPF
{
    public partial class EditTaskWindow : Window
    {
        public string TituloResult { get; private set; }
        public string DescripcionResult { get; private set; }
        public string PrioridadResult { get; private set; }

        public EditTaskWindow(string titulo = "", string descripcion = "", string prioridad = "Media")
        {
            InitializeComponent();
            txtTitulo.Text = titulo;
            txtDescripcion.Text = descripcion;

            // Seleccionar combo box
            foreach (ComboBoxItem item in cmbPrioridad.Items)
            {
                if (item.Content.ToString() == prioridad)
                {
                    cmbPrioridad.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            TituloResult = txtTitulo.Text;
            DescripcionResult = txtDescripcion.Text;
            PrioridadResult = (cmbPrioridad.SelectedItem as ComboBoxItem)?.Content.ToString();

            DialogResult = true; // Cierra la ventana devolviendo "True"
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
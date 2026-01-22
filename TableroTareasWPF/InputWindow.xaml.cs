using System.Windows;

namespace TableroTareasWPF
{
    public partial class InputWindow : Window
    {
        public string ResponseText { get; private set; }

        public InputWindow()
        {
            InitializeComponent();
            txtInput.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = txtInput.Text;
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
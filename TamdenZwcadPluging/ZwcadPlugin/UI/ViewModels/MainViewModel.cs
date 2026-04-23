using System.Windows.Input;

namespace ZwcadPlugin.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _estadoConexion = "Sin conexión";
        private string _mensajeEstado = "Plugin Tandem 2026 listo.";

        public string EstadoConexion
        {
            get => _estadoConexion;
            set => SetProperty(ref _estadoConexion, value);
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        public ICommand DetectarMurosCommand { get; }
        public ICommand Generar3dCommand { get; }
        public ICommand ConfigEncofradoCommand { get; }

        public MainViewModel()
        {
            DetectarMurosCommand  = new RelayCommand(DetectarMuros);
            Generar3dCommand      = new RelayCommand(Generar3d, () => !string.IsNullOrEmpty(EstadoConexion));
            ConfigEncofradoCommand = new RelayCommand(ConfigEncofrado);
        }

        private void DetectarMuros()
        {
            MensajeEstado = "Ejecutando DETECTARMUROS...";
        }

        private void Generar3d()
        {
            MensajeEstado = "Ejecutando GENERAR3D...";
        }

        private void ConfigEncofrado()
        {
            MensajeEstado = "Abriendo configuración de encofrado...";
        }
    }
}

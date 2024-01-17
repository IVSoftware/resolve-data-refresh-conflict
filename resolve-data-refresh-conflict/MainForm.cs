
using Microsoft.VisualBasic.Devices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace resolve_data_refresh_conflict
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            textbox.DoubleClick += (sender, e) =>
            {
                textbox.BeginInvoke(() =>
                {
                    textbox.ReadOnly = false;
                    textbox.SelectAll();
                });
            };
            textbox.KeyDown += (sender, e) =>
            {
                switch (e.KeyData) 
                {
                    case Keys.Enter:
                        if (!textbox.ReadOnly)
                        {
                            e.Handled = e.SuppressKeyPress = true; 
                            SendChangemessage(textbox.Text);
                            textbox.ReadOnly = true;
                        }
                        break;
                }
            };
            textbox.LostFocus += (sender, e) =>
            {
                if(this == ActiveForm) textbox.ReadOnly = true;
            };
            textbox.DataBindings.Add(
                new Binding(
                    propertyName: nameof(TextBox.Text),
                    dataSource: Model,
                    dataMember: nameof(Model.Value),
                    formattingEnabled: false,
                    dataSourceUpdateMode: DataSourceUpdateMode.OnPropertyChanged
            ));
            textbox.DataBindings.Add(
                new Binding(
                    propertyName: nameof(TextBox.ReadOnly),
                    dataSource: Model,
                    dataMember: nameof(Model.IsReadOnly),
                    formattingEnabled: false,
                    dataSourceUpdateMode: DataSourceUpdateMode.OnPropertyChanged
            ));
        }

        private void SendChangemessage(string text) =>
            MessageBox.Show(text, "Change Message");

        MainFormViewModel Model { get; } = new MainFormViewModel();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ = ExecRefreshLoop();
        }

        private async Task ExecRefreshLoop()
        {
            while(!Disposing)
            {
                if (Model.IsReadOnly) 
                {
                    Model.Value = Random.Next(1000, 10000);
                }
                await Task.Delay(TimeSpan.FromSeconds(1)); 
            }
        }
        private Random Random { get; } = new Random();
    }

    public class MainFormViewModel : INotifyPropertyChanged
    {
        public int _value;
        public int Value
        {
            set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
            get { return _value; }
        }
        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set
            {
                if (!Equals(_IsReadOnly, value))
                {
                    _IsReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _IsReadOnly = true;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

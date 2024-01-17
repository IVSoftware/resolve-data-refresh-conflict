## Resolve Data Refresh Conflict
I think you're on the right track with adding a boolean value in the Model. What I think would make this successful is to give the textbox a concept of direction. One approach you could experiment with is to allow incoming value refresh only when the textbox is `ReadOnly`. Then (for example) by double-clicking on the textbox, the direction can be effectively changed to 'outgoing' until the `SendChangeMessage` commit is completed. An additional binding for textbox.ReadOnly has been added to the Model.

[![screenshots][1]][1]

```
    public MainForm()
    {
        InitializeComponent();
        // Set bindings
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
    MainFormViewModel Model { get; } = new MainFormViewModel();
```
#### Outgoing change message

```
    private void SendChangemessage(string text) =>
        MessageBox.Show(text, "Change Message");
```
#### Mock incoming data from server

```
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
```

**Mock Update from Server**

_Typical view model that can act as a binding context for the entire MainForm_

```
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
```


  [1]: https://i.stack.imgur.com/6tYTm.png
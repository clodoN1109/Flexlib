using Terminal.Gui;

namespace Flexlib.Interface.TUI;

public class TUIPage
{
    public string? Address { get; set; }
    private TextView? Pane { get; set; }
    public TUIPage(TextView pane)
    {
        Pane = pane;
    }
    public TUIPage(){}
    public void Update(string newText, string newAddress)
    {
        UpdateAddress(newAddress);
        if (Pane is TextView)
            SetPageText(newText);            
    }
    private void UpdateAddress(string newAddress)
    {
        Address = newAddress;
    }
    private void SetPageText(string newText)
    {
        if (Pane is TextView)
            Pane.Text = newText;
    }

}
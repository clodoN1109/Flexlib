using Terminal.Gui;

namespace Flexlib.Interface.TUI;

public class TUIPage
{
    public string[] Address = [];

    public string AddressAsString
    {
        get => string.Join(" ", Address.Select(x =>
            x.Contains(' ') ? $"\"{x}\"" : x));
    }

    private TextView BodyPane { get; set; }
    private Label TitleLabel { get; set; }
    private Label TopLeftLabel { get; set; }
    private Label TopRightLabel { get; set; }
    private Label BottomLeftLabel { get; set; }
    private Label BottomRightLabel { get; set; }
    public TUIPage(TextView bodyPane,
                                Label titleLabel,
                                Label topLeftLabel,
                                Label topRightLabel,
                                Label bottomLeftLabel,
                                Label bottomRightLabel
                            )
    {
        BodyPane = bodyPane;
        TitleLabel = titleLabel;
        TopLeftLabel = topLeftLabel;
        TopRightLabel = topRightLabel;
        BottomLeftLabel = bottomLeftLabel;
        BottomRightLabel = bottomRightLabel;
    }
    public void Update(Input.Command newAddress,
                        string body,
                        string title = "",
                        string topLeftInfo = "",
                        string topRightInfo = "",
                        string bottomLeftInfo = "",
                        string bottomRightInfo = ""
                        )
    {
        Update(newAddress);
        BodyPane.Text = body;

        TitleLabel.Text = title.TranslateToProfile();
        TopLeftLabel.Text = topLeftInfo;
        
        TopRightLabel.Text = topRightInfo;
        TopRightLabel.X = Pos.AnchorEnd(topRightInfo.Length);    

        BottomLeftLabel.Text = bottomLeftInfo;

        BottomRightLabel.Text = bottomRightInfo.TranslateToProfile();
        BottomRightLabel.X = Pos.AnchorEnd(bottomRightInfo.Length);    
    }

    private void Update(Input.Command newAddress)
    {
        Address = new[] { newAddress.Type }
            .Concat(newAddress.Options)
            .ToArray();
    }

}
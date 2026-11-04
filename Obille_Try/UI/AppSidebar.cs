using System.ComponentModel;
using Kit;

namespace Obille_Try.UI {
  // The app's sidebar as one control: the brand at the top (the SS mark and
  // the company), the pages in a kit SideNav, and the person signed in at the
  // bottom with a menu button, a line along its right edge. The designer shows and
  // selects it as one element; the nav and the menu button inside are its
  // own, reached through Items, SelectedIndex and AccountMenu.
  [ToolboxItem(true)]
  [DefaultProperty(nameof(Items)), DefaultEvent(nameof(SelectedIndexChanged))]
  [Description("The app's sidebar: the brand, the pages and the person signed in.")]
  public class AppSidebar : KitControl {
    readonly SideNav nav = new() { AccessibleName = "Pages" };
    readonly Avatar avatar = new() { TabStop = false };
    readonly MenuButton accountButton = new() {
      Icon = "ellipsis-vertical",
      IconOnly = true,
      IconRight = "",
      Variant = ButtonVariant.Ghost,
      AccessibleName = "Account",
    };
    string brandName = "SS Construction Co.", brandDetail = "Inventory", personDetail = "";

    public AppSidebar() {
      SetStyle(ControlStyles.Selectable, false);
      TabStop = false;
      Controls.Add(nav);
      Controls.Add(avatar);
      Controls.Add(accountButton);
      nav.SelectedIndexChanged += (_, _) => SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
      Size = new Size(248, 720);
    }

    [Category("Appearance"), DefaultValue("SS Construction Co.")]
    [Description("The company, beside the mark.")]
    public string BrandName { get => brandName; set { brandName = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("Inventory")]
    [Description("What the app is, under the company's name.")]
    public string BrandDetail { get => brandDetail; set { brandDetail = value ?? ""; Invalidate(); } }

    [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Description("The pages in order, each a label, an icon, an optional count and a key.")]
    public ItemList<NavItem> Items => nav.Items;

    [Category("Data"), DefaultValue(0)]
    [Description("The current page's position in Items.")]
    public int SelectedIndex { get => nav.SelectedIndex; set => nav.SelectedIndex = value; }

    public event EventHandler? SelectedIndexChanged;

    [Category("Appearance"), DefaultValue("")]
    [Description("The person signed in; the avatar's initials and color come from it.")]
    public string PersonName { get => avatar.PersonName; set { avatar.PersonName = value ?? ""; Invalidate(); } }

    [Category("Appearance"), DefaultValue("")]
    [Description("The person's role, under the name.")]
    public string PersonDetail { get => personDetail; set { personDetail = value ?? ""; Invalidate(); } }

    [Category("Behavior"), DefaultValue(null)]
    [Description("The account's commands (About, Sign out), in a ContextMenuStrip on the form.")]
    public ContextMenuStrip? AccountMenu { get => accountButton.DropDown; set => accountButton.DropDown = value; }

    // Where row i of the nav sits, in the sidebar's coordinates.
    public Rectangle RowBounds(int i) {
      var r = nav.RowBounds(i);
      r.Offset(nav.Location);
      return r;
    }

    int BrandHeight => S(72);
    int AccountHeight => S(64);

    protected override void OnLayout(LayoutEventArgs levent) {
      base.OnLayout(levent);
      var right = Width - S(1);
      nav.SetBounds(S(4), BrandHeight, right - S(8), Math.Max(0, Height - BrandHeight - AccountHeight));
      var top = Height - AccountHeight;
      avatar.SetBounds(S(16), top + (AccountHeight - S(36)) / 2, S(36), S(36));
      accountButton.SetBounds(right - S(12) - S(32), top + (AccountHeight - S(32)) / 2, S(32), S(32));
    }

    protected override void OnPaint(PaintEventArgs e) {
      var g = e.Graphics;
      g.Clear(P.Bg);
      using (var line = new Pen(P.Line)) {
        g.DrawLine(line, Width - 1, 0, Width - 1, Height);
        g.DrawLine(line, S(12), Height - AccountHeight, Width - S(13), Height - AccountHeight);
      }

      // The brand: the mark, the company's name level with its top, what
      // the app is level with its bottom.
      var mark = new RectangleF(S(16), S(16), S(40), S(40));
      LogoMark.DrawMark(g, mark);
      var tx = (int)mark.Right + S(12);
      var tw = Width - tx - S(12);
      var name = KitTheme.Font(15, true);
      var small = KitTheme.Font(13);
      Painter.Text(g, brandName, name, new Rectangle(tx, (int)mark.Y - S(1), tw, name.Height), P.Ink, TextFormatFlags.Left | TextFormatFlags.Top);
      Painter.Text(g, brandDetail, small, new Rectangle(tx, (int)mark.Bottom - small.Height + S(1), tw, small.Height), P.Ink2, TextFormatFlags.Left | TextFormatFlags.Top);

      // The person: name over role, between the avatar and the menu.
      var top = Height - AccountHeight;
      var px = avatar.Right + S(12);
      var pw = accountButton.Left - px - S(8);
      var who = KitTheme.Font(14, true);
      var block = who.Height + small.Height;
      var y = top + (AccountHeight - block) / 2;
      Painter.Text(g, PersonName, who, new Rectangle(px, y, pw, who.Height), P.Ink, TextFormatFlags.Left | TextFormatFlags.Top);
      Painter.Text(g, personDetail, small, new Rectangle(px, y + who.Height, pw, small.Height), P.Ink2, TextFormatFlags.Left | TextFormatFlags.Top);
    }
  }
}

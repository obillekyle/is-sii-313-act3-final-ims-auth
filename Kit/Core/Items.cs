using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Kit {
  // An entry in a kit control's list (a tab, a nav link, a step). Editing
  // one in the Properties window repaints the control that holds it.
  [TypeConverter(typeof(KitItemConverter))]
  public abstract class KitItem {
    internal Action? Changed;

    protected void Notify() => Changed?.Invoke();
  }

  // A list of items that tells its control when anything in it changes.
  public sealed class ItemList<T> : Collection<T> where T : KitItem {
    readonly Action changed;

    public ItemList(Action changed) => this.changed = changed;

    public void AddRange(IEnumerable<T> items) {
      foreach (var item in items) Add(item);
    }

    protected override void InsertItem(int index, T item) {
      item.Changed = changed;
      base.InsertItem(index, item);
      changed();
    }

    protected override void SetItem(int index, T item) {
      item.Changed = changed;
      base.SetItem(index, item);
      changed();
    }

    protected override void RemoveItem(int index) {
      base.RemoveItem(index);
      changed();
    }

    protected override void ClearItems() {
      base.ClearItems();
      changed();
    }
  }

  // Lets the designer write an item as one constructor call in the form's
  // code: the item's constructor with the most parameters, each filled from
  // the property of the same name. Also lets the Properties window open an
  // item to edit its fields.
  public sealed class KitItemConverter : ExpandableObjectConverter {
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(InstanceDescriptor) || base.CanConvertTo(context, destinationType);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
      if (destinationType == typeof(InstanceDescriptor) && value is KitItem item) {
        var ctor = item.GetType().GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        var args = ctor.GetParameters()
            .Select(p => item.GetType().GetProperty(p.Name!, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)!.GetValue(item))
            .ToArray();
        return new InstanceDescriptor(ctor, args, true);
      }
      if (destinationType == typeof(string) && value is KitItem shown) return shown.ToString();
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }

  public sealed class TabItem : KitItem {
    string text = "Tab";
    int? count;
    bool enabled = true;

    public TabItem() { }

    public TabItem(string text, int? count, bool enabled) {
      this.text = text;
      this.count = count;
      this.enabled = enabled;
    }

    [DefaultValue("Tab")] public string Text { get => text; set { text = value ?? ""; Notify(); } }

    [DefaultValue(null), Description("A count shown in a small badge after the label.")]
    public int? Count { get => count; set { count = value; Notify(); } }

    [DefaultValue(true)] public bool Enabled { get => enabled; set { enabled = value; Notify(); } }

    public override string ToString() => text;
  }

  public sealed class NavItem : KitItem {
    string text = "Page", icon = "", key = "";
    int? count;

    public NavItem() { }

    public NavItem(string text, string icon, int? count, string key) {
      this.text = text;
      this.icon = icon;
      this.count = count;
      this.key = key;
    }

    [DefaultValue("Page")] public string Text { get => text; set { text = value ?? ""; Notify(); } }

    [DefaultValue(""), IconName, TypeConverter(typeof(IconNameConverter))]
    public string Icon { get => icon; set { icon = value ?? ""; Notify(); } }

    [DefaultValue(null), Description("A count at the row's right end (items waiting).")]
    public int? Count { get => count; set { count = value; Notify(); } }

    [DefaultValue(""), Description("What the app knows the page by; SelectedKey matches it.")]
    public string Key { get => key; set { key = value ?? ""; Notify(); } }

    public override string ToString() => text;
  }

  public enum StepState { Todo, Current, Done, Error }

  public sealed class StepItem : KitItem {
    string text = "Step", detail = "";
    StepState state = StepState.Todo;

    public StepItem() { }

    public StepItem(string text, StepState state, string detail) {
      this.text = text;
      this.state = state;
      this.detail = detail;
    }

    [DefaultValue("Step")] public string Text { get => text; set { text = value ?? ""; Notify(); } }
    [DefaultValue(StepState.Todo)] public StepState State { get => state; set { state = value; Notify(); } }

    [DefaultValue(""), Description("A short line under the step's name (Submitted Oct 3).")]
    public string Detail { get => detail; set { detail = value ?? ""; Notify(); } }

    public override string ToString() => text;
  }

  public sealed class ChartSeries : KitItem {
    string name = "Series";
    float[] values = Array.Empty<float>();

    public ChartSeries() { }

    public ChartSeries(string name, float[] values) {
      this.name = name;
      this.values = values;
    }

    [DefaultValue("Series")] public string Name { get => name; set { name = value ?? ""; Notify(); } }

    [Description("One value per label, in the same order.")]
    public float[] Values { get => values; set { values = value ?? Array.Empty<float>(); Notify(); } }

    public override string ToString() => name;
  }
}

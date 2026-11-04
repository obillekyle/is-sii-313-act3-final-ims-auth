using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Text;

namespace Kit.Gallery {
  // Writes the C# that sets a component up as it stands: every property
  // whose value differs from its default, the way the designer would write
  // it, and the items of its lists as constructor calls.
  internal static class CodeWriter {
    public static string For(object target, string name) {
      var sb = new StringBuilder();
      sb.AppendLine($"var {name} = new {target.GetType().Name}();");
      foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(target)) {
        if (!PropertyView.IsKitProperty(target, pd)) continue;
        var visibility = pd.SerializationVisibility;
        if (visibility == DesignerSerializationVisibility.Hidden) continue;
        var value = pd.GetValue(target);
        if (visibility == DesignerSerializationVisibility.Content && value is IEnumerable list && value is not string) {
          foreach (var item in list)
            if (item != null) sb.AppendLine($"{name}.{pd.Name}.Add({Literal(item)});");
          continue;
        }
        if (pd.IsReadOnly || !pd.ShouldSerializeValue(target)) continue;
        sb.AppendLine($"{name}.{pd.Name} = {Literal(value)};");
      }
      return sb.ToString().TrimEnd();
    }

    static string Quote(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";

    static string Number(double d) => d.ToString("R", CultureInfo.InvariantCulture);

    public static string Literal(object? v) => v switch {
      null => "null",
      string s => Quote(s),
      bool b => b ? "true" : "false",
      Enum e => $"{e.GetType().Name}.{e}",
      int i => i.ToString(CultureInfo.InvariantCulture),
      long l => l.ToString(CultureInfo.InvariantCulture) + "L",
      float f => Number(f) + "f",
      double d => Number(d),
      decimal m => m.ToString(CultureInfo.InvariantCulture) + "m",
      string[] a => $"new[] {{ {string.Join(", ", a.Select(Quote))} }}",
      float[] a => $"new[] {{ {string.Join(", ", a.Select(x => Number(x) + "f"))} }}",
      int[] a => $"new[] {{ {string.Join(", ", a)} }}",
      Color c when c.IsNamedColor => $"Color.{c.Name}",
      Color c => $"Color.FromArgb({c.A}, {c.R}, {c.G}, {c.B})",
      Size s => $"new Size({s.Width}, {s.Height})",
      Point p => $"new Point({p.X}, {p.Y})",
      Padding p => $"new Padding({p.Left}, {p.Top}, {p.Right}, {p.Bottom})",
      Component c => c.Site?.Name ?? "/* a " + c.GetType().Name + " on the form */",
      KitItem item => Item(item),
      _ => "/* " + v + " */",
    };

    static string Item(KitItem item) {
      var converter = TypeDescriptor.GetConverter(item);
      if (converter.ConvertTo(item, typeof(InstanceDescriptor)) is InstanceDescriptor d && d.Arguments is ICollection args)
        return $"new {item.GetType().Name}({string.Join(", ", args.Cast<object?>().Select(Literal))})";
      return "/* " + item + " */";
    }
  }

  // What the PropertyGrid shows: by default only the kit's own properties
  // (and Text and Enabled), so they are not lost among the sixty every
  // WinForms control carries; with KitOnly off, all of them.
  internal sealed class PropertyView : CustomTypeDescriptor {
    static readonly HashSet<string> alwaysShown = new() { "Text", "Enabled" };
    readonly object target;
    readonly bool kitOnly;

    public PropertyView(object target, bool kitOnly) : base(TypeDescriptor.GetProvider(target).GetTypeDescriptor(target)) {
      this.target = target;
      this.kitOnly = kitOnly;
    }

    public static bool IsKitProperty(object target, PropertyDescriptor pd) {
      if (alwaysShown.Contains(pd.Name)) return pd.IsBrowsable;
      var declared = target.GetType().GetProperties().FirstOrDefault(p => p.Name == pd.Name)?.DeclaringType;
      return declared?.Assembly == typeof(KitButton).Assembly && pd.IsBrowsable;
    }

    PropertyDescriptorCollection Filter(PropertyDescriptorCollection all) => !kitOnly
        ? all
        : new PropertyDescriptorCollection(all.Cast<PropertyDescriptor>().Where(p => IsKitProperty(target, p)).ToArray());

    public override PropertyDescriptorCollection GetProperties() => Filter(base.GetProperties());

    public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => Filter(base.GetProperties(attributes));

    public override object? GetPropertyOwner(PropertyDescriptor? pd) => target;
  }
}

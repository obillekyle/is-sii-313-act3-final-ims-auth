using System.Drawing.Drawing2D;
using System.Globalization;

namespace Kit {
  internal readonly record struct IconShape(string D, bool Fill, bool Stroke);

  // Reads SVG path data (M L H V C S Q T A Z, absolute and relative) into a
  // GraphicsPath. Arcs become cubic Béziers of at most 90° each, which is
  // how GDI+ draws curves.
  internal static class SvgPath {
    public static GraphicsPath Parse(string d) {
      var path = new GraphicsPath();
      var r = new Reader(d);
      PointF cur = default, start = default, lastCtrl = default;
      var lastCmd = ' ';
      var cmd = ' ';

      while (r.More()) {
        if (r.PeekCommand(out var c)) cmd = c;
        else if (cmd == ' ') throw new FormatException($"path data starts without a command: {d}");
        var rel = char.IsLower(cmd);
        PointF P(float x, float y) => rel ? new PointF(cur.X + x, cur.Y + y) : new PointF(x, y);

        switch (char.ToUpperInvariant(cmd)) {
          case 'M':
            cur = P(r.Num(), r.Num());
            start = cur;
            path.StartFigure();
            // Coordinates after the first pair of a moveto are linetos.
            cmd = rel ? 'l' : 'L';
            lastCmd = 'M';
            continue;
          case 'L': {
              var p = P(r.Num(), r.Num());
              path.AddLine(cur, p);
              cur = p;
              break;
            }
          case 'H': {
              var x = r.Num();
              var p = new PointF(rel ? cur.X + x : x, cur.Y);
              path.AddLine(cur, p);
              cur = p;
              break;
            }
          case 'V': {
              var y = r.Num();
              var p = new PointF(cur.X, rel ? cur.Y + y : y);
              path.AddLine(cur, p);
              cur = p;
              break;
            }
          case 'C': {
              var c1 = P(r.Num(), r.Num());
              var c2 = P(r.Num(), r.Num());
              var p = P(r.Num(), r.Num());
              path.AddBezier(cur, c1, c2, p);
              lastCtrl = c2;
              cur = p;
              break;
            }
          case 'S': {
              var c1 = "CS".Contains(char.ToUpperInvariant(lastCmd)) ? Reflect(lastCtrl, cur) : cur;
              var c2 = P(r.Num(), r.Num());
              var p = P(r.Num(), r.Num());
              path.AddBezier(cur, c1, c2, p);
              lastCtrl = c2;
              cur = p;
              break;
            }
          case 'Q': {
              var q = P(r.Num(), r.Num());
              var p = P(r.Num(), r.Num());
              AddQuad(path, cur, q, p);
              lastCtrl = q;
              cur = p;
              break;
            }
          case 'T': {
              var q = "QT".Contains(char.ToUpperInvariant(lastCmd)) ? Reflect(lastCtrl, cur) : cur;
              var p = P(r.Num(), r.Num());
              AddQuad(path, cur, q, p);
              lastCtrl = q;
              cur = p;
              break;
            }
          case 'A': {
              var rx = r.Num();
              var ry = r.Num();
              var rot = r.Num();
              var large = r.Flag();
              var sweep = r.Flag();
              var p = P(r.Num(), r.Num());
              AddArc(path, cur, p, rx, ry, rot, large, sweep);
              cur = p;
              break;
            }
          case 'Z':
            path.CloseFigure();
            cur = start;
            lastCmd = 'Z';
            cmd = ' ';
            continue;
          default:
            throw new FormatException($"unknown path command '{cmd}' in {d}");
        }
        lastCmd = cmd;
      }
      return path;
    }

    static PointF Reflect(PointF ctrl, PointF about) => new(2 * about.X - ctrl.X, 2 * about.Y - ctrl.Y);

    static void AddQuad(GraphicsPath path, PointF p0, PointF q, PointF p) =>
        path.AddBezier(p0,
            new PointF(p0.X + 2f / 3 * (q.X - p0.X), p0.Y + 2f / 3 * (q.Y - p0.Y)),
            new PointF(p.X + 2f / 3 * (q.X - p.X), p.Y + 2f / 3 * (q.Y - p.Y)),
            p);

    // The SVG arc, from its endpoint form to its center form (SVG 1.1,
    // appendix F.6.5), then drawn as Béziers of at most a quarter turn.
    static void AddArc(GraphicsPath path, PointF p0, PointF p1, float rx, float ry, float rotDeg, bool large, bool sweep) {
      if (p0 == p1) return;
      if (rx == 0 || ry == 0) {
        path.AddLine(p0, p1);
        return;
      }
      double phi = rotDeg * Math.PI / 180, cos = Math.Cos(phi), sin = Math.Sin(phi);
      double dx = (p0.X - p1.X) / 2.0, dy = (p0.Y - p1.Y) / 2.0;
      double x1 = cos * dx + sin * dy, y1 = -sin * dx + cos * dy;
      double arx = Math.Abs(rx), ary = Math.Abs(ry);
      var lambda = x1 * x1 / (arx * arx) + y1 * y1 / (ary * ary);
      if (lambda > 1) {
        arx *= Math.Sqrt(lambda);
        ary *= Math.Sqrt(lambda);
      }
      var num = arx * arx * ary * ary - arx * arx * y1 * y1 - ary * ary * x1 * x1;
      var den = arx * arx * y1 * y1 + ary * ary * x1 * x1;
      var coef = Math.Sqrt(Math.Max(0, num / den)) * (large == sweep ? -1 : 1);
      double cxp = coef * arx * y1 / ary, cyp = -coef * ary * x1 / arx;
      double cx = cos * cxp - sin * cyp + (p0.X + p1.X) / 2.0;
      double cy = sin * cxp + cos * cyp + (p0.Y + p1.Y) / 2.0;

      static double Angle(double ux, double uy, double vx, double vy) {
        var a = Math.Atan2(ux * vy - uy * vx, ux * vx + uy * vy);
        return a;
      }

      var theta = Angle(1, 0, (x1 - cxp) / arx, (y1 - cyp) / ary);
      var delta = Angle((x1 - cxp) / arx, (y1 - cyp) / ary, (-x1 - cxp) / arx, (-y1 - cyp) / ary);
      if (!sweep && delta > 0) delta -= 2 * Math.PI;
      else if (sweep && delta < 0) delta += 2 * Math.PI;

      var segments = (int)Math.Ceiling(Math.Abs(delta) / (Math.PI / 2) - 1e-9);
      var step = delta / segments;
      var t = 4.0 / 3 * Math.Tan(step / 4);
      var from = p0;
      for (var i = 0; i < segments; i++) {
        double a1 = theta + i * step, a2 = a1 + step;
        double c1 = Math.Cos(a1), s1 = Math.Sin(a1), c2 = Math.Cos(a2), s2 = Math.Sin(a2);
        PointF Map(double x, double y) => new(
            (float)(cos * arx * x - sin * ary * y + cx),
            (float)(sin * arx * x + cos * ary * y + cy));
        var q1 = Map(c1 - t * s1, s1 + t * c1);
        var q2 = Map(c2 + t * s2, s2 - t * c2);
        var to = i == segments - 1 ? p1 : Map(c2, s2);
        path.AddBezier(from, q1, q2, to);
        from = to;
      }
    }

    sealed class Reader {
      readonly string s;
      int i;

      public Reader(string s) { this.s = s; }

      void SkipSeparators() {
        while (i < s.Length && (char.IsWhiteSpace(s[i]) || s[i] == ',')) i++;
      }

      public bool More() {
        SkipSeparators();
        return i < s.Length;
      }

      public bool PeekCommand(out char c) {
        SkipSeparators();
        c = i < s.Length ? s[i] : ' ';
        if (i < s.Length && char.IsLetter(c) && c is not ('e' or 'E')) {
          i++;
          return true;
        }
        return false;
      }

      // Arc flags are single digits and may run into the next number:
      // "a2 2 0 012 2" is flags 0 and 1, then 2 2.
      public bool Flag() {
        SkipSeparators();
        var c = s[i++];
        if (c is not ('0' or '1')) throw new FormatException($"arc flag expected at {i - 1} in {s}");
        return c == '1';
      }

      // A number ends where the next sign or a second decimal point
      // begins: ".25.25" is two numbers, "1-.48" is 1 and -0.48.
      public float Num() {
        SkipSeparators();
        var startAt = i;
        if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++;
        var dot = false;
        while (i < s.Length) {
          var c = s[i];
          if (char.IsDigit(c)) i++;
          else if (c == '.' && !dot) { dot = true; i++; } else if ((c == 'e' || c == 'E') && i + 1 < s.Length && (char.IsDigit(s[i + 1]) || s[i + 1] == '-' || s[i + 1] == '+')) {
            i += 2;
            while (i < s.Length && char.IsDigit(s[i])) i++;
            break;
          } else break;
        }
        if (startAt == i) throw new FormatException($"number expected at {i} in {s}");
        return float.Parse(s.AsSpan(startAt, i - startAt), NumberStyles.Float, CultureInfo.InvariantCulture);
      }
    }
  }
}

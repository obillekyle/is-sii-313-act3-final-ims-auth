namespace Obille_Try.Data {
  public sealed record MaterialRow(
      string Name, string Code, string Bin, int Stock, string Unit, decimal Cost, int ReorderPoint,
      string Category, string Supplier) {
    // The warehouse reorders when stock falls to the reorder point.
    public bool IsLow => Stock <= ReorderPoint;

    // "712 lengths", "4 pails", "1,069 cu.m": a unit that is a word takes a
    // plural, an abbreviation does not.
    public string Quantity(int n) =>
        $"{n:N0} {(n == 1 || Unit is "cu.m" or "pc" or "kg" or "gal" or "m" or "bd.ft" ? Unit : Unit == "box" ? "boxes" : Unit + "s")}";
  }

  // Fixed sample data for the prototype; nothing here is saved. The five
  // stock figures the materials sketch draws are kept as drawn. The dashboard's figures
  // are worked out from these rows, so the card and the list it opens agree.
  public static class SampleData {
    public const int PageSize = 25;

    public static readonly string[] Categories =
    {
            "Cement and aggregates", "Steel", "Masonry", "Lumber and plywood", "Roofing", "Plumbing", "Electrical",
            "Paint and finishes", "Hardware and fasteners",
        };

    public static readonly string[] StockFilters = { "Low stock", "Out of stock", "In stock" };

    public static readonly string[] Suppliers =
    {
            "Agos Pipe and Fittings", "Bagong Silang Roofing Center", "Bulihan Aggregates and Hauling",
            "Kalasag Electrical Supply", "Matibay Cement Distributor", "Primeline Builders Depot",
            "Pulilan Lumber and Plywood", "Santo Rosario Steel Trading", "Tropikal Paints and Tiles",
        };

    // What a material is counted in: the name the form shows, the
    // abbreviation the table shows, and the word the cost label uses.
    public static readonly (string Display, string Abbr, string Singular)[] Units =
    {
            ("Bags", "bag", "bag"),
            ("Pieces (pc)", "pc", "piece"),
            ("Lengths", "length", "length"),
            ("Cubic meters (cu.m)", "cu.m", "cubic meter"),
            ("Board feet (bd.ft)", "bd.ft", "board foot"),
            ("Rolls", "roll", "roll"),
            ("Gallons (gal)", "gal", "gallon"),
            ("Kilograms (kg)", "kg", "kilogram"),
            ("Boxes", "box", "box"),
            ("Pails", "pail", "pail"),
            ("Meters (m)", "m", "meter"),
        };

    public static readonly string[] Locations =
    {
            "Malolos yard", "Bulihan house", "Gatbuca warehouse", "Plaridel road", "Tangos building", "Tabe bungalow",
        };

    const string Cement = "Cement and aggregates", Steel = "Steel", Masonry = "Masonry", Lumber = "Lumber and plywood",
        Roofing = "Roofing", Plumbing = "Plumbing", Electrical = "Electrical", Finishes = "Paint and finishes",
        Hardware = "Hardware and fasteners";

    const string Agos = "Agos Pipe and Fittings", BagongSilang = "Bagong Silang Roofing Center",
        Bulihan = "Bulihan Aggregates and Hauling", Kalasag = "Kalasag Electrical Supply",
        Primeline = "Primeline Builders Depot", Pulilan = "Pulilan Lumber and Plywood",
        SantoRosario = "Santo Rosario Steel Trading", Tropikal = "Tropikal Paints and Tiles";

    // All 59 materials in the Malolos yard. Six sit at or below their
    // reorder point: CEM-0101, STL-0020, ROF-0201, PLB-0301, ELE-0302, PNT-0201.
    public static readonly MaterialRow[] Materials =
    {
            new("Portland cement, Type 1, 40 kg", "CEM-0001", "Cement shed", 1180, "bag", 265m, 400, Cement, Primeline),
            new("Masonry cement, 40 kg", "CEM-0002", "Cement shed", 214, "bag", 238m, 80, Cement, Primeline),
            new("Concrete curing compound, 18 L", "CEM-0101", "Rack C2", 4, "pail", 2380m, 6, Cement, Primeline),
            new("Washed sand", "AGG-0001", "Bay 1", 96, "cu.m", 1250m, 40, Cement, Bulihan),
            new("Gravel, 3/4 in", "AGG-0002", "Bay 2", 118, "cu.m", 1450m, 40, Cement, Bulihan),
            new("Gravel, 3/8 in", "AGG-0003", "Bay 3", 26, "cu.m", 1520m, 10, Cement, Bulihan),
            new("Aggregate base course, Item 201", "AGG-0004", "Bay 4", 1069, "cu.m", 950m, 60, Cement, Bulihan),
            new("Deformed bar, 10 mm x 6 m, Grade 33", "STL-0010", "Steel rack 1", 640, "length", 185m, 300, Steel, SantoRosario),
            new("Deformed bar, 12 mm x 6 m, Grade 40", "STL-0012", "Steel rack 1", 912, "length", 265m, 300, Steel, SantoRosario),
            new("Deformed bar, 16 mm x 6 m, Grade 40", "STL-0016", "Steel rack 2", 386, "length", 468m, 150, Steel, SantoRosario),
            new("Deformed bar, 20 mm x 6 m, Grade 40", "STL-0020", "Steel rack 2", 42, "length", 735m, 60, Steel, SantoRosario),
            new("Smooth dowel bar, 32 mm x 450 mm", "STL-0032", "Steel rack 3", 450, "pc", 218m, 200, Steel, SantoRosario),
            new("Tie wire, #16", "STL-0101", "Rack A1", 238, "kg", 95m, 100, Steel, SantoRosario),
            new("C-purlin, 2 x 4 in x 1.5 mm x 6 m", "STL-0201", "Steel rack 4", 134, "length", 790m, 60, Steel, BagongSilang),
            new("Angle bar, 2 x 2 in x 3/16 in x 6 m", "STL-0301", "Steel rack 4", 712, "length", 1180m, 20, Steel, SantoRosario),
            new("Concrete hollow block, 4 in", "MAS-0004", "Bay 5", 5200, "pc", 16m, 1500, Masonry, Primeline),
            new("Concrete hollow block, 6 in", "MAS-0006", "Bay 5", 3650, "pc", 22.5m, 1000, Masonry, Primeline),
            new("Premixed mortar, 40 kg", "MAS-0101", "Cement shed", 88, "bag", 290m, 40, Masonry, Primeline),
            new("Coco lumber, 2 x 3 in x 10 ft", "LUM-0101", "Lumber shed", 520, "pc", 120m, 200, Lumber, Pulilan),
            new("Coco lumber, 2 x 2 in x 10 ft", "LUM-0102", "Lumber shed", 410, "pc", 85m, 200, Lumber, Pulilan),
            new("Kiln-dried lumber, S4S, 2 x 4 in", "LUM-0201", "Lumber shed", 960, "bd.ft", 72m, 300, Lumber, Pulilan),
            new("Ordinary plywood, 1/4 in x 4 x 8 ft", "PLY-0006", "Plywood rack", 115, "pc", 420m, 40, Lumber, Pulilan),
            new("Marine plywood, 1/2 in x 4 x 8 ft", "PLY-0012", "Plywood rack", 92, "pc", 1150m, 40, Lumber, Pulilan),
            new("Marine plywood, 3/4 in x 4 x 8 ft", "PLY-0019", "Plywood rack", 47, "pc", 1640m, 20, Lumber, Pulilan),
            new("Phenolic board, 3/4 in x 4 x 8 ft", "PLY-0101", "Plywood rack", 64, "pc", 1480m, 30, Lumber, Pulilan),
            new("Fiber cement board, 4.5 mm x 4 x 8 ft", "PLY-0201", "Plywood rack", 85, "pc", 410m, 40, Lumber, Primeline),
            new("Long span roofing, pre-painted, 0.4 mm", "ROF-0001", "Roofing rack", 420, "m", 395m, 150, Roofing, BagongSilang),
            new("Corrugated GI sheet, gauge 26 x 8 ft", "ROF-0002", "Roofing rack", 76, "pc", 485m, 30, Roofing, BagongSilang),
            new("Ridge roll, pre-painted, 0.4 mm x 2.4 m", "ROF-0101", "Roofing rack", 38, "pc", 520m, 15, Roofing, BagongSilang),
            new("Double-sided foil insulation, 10 mm, 1.2 x 25 m", "ROF-0201", "Rack B3", 3, "roll", 2950m, 6, Roofing, BagongSilang),
            new("Polyurethane sealant, 300 ml", "ROF-0301", "Rack B3", 66, "pc", 280m, 24, Roofing, BagongSilang),
            new("PVC pipe, 2 in x 3 m, Series 1000", "PLB-0002", "Pipe rack", 58, "length", 248m, 20, Plumbing, Agos),
            new("PVC pipe, 4 in x 3 m, Series 1000", "PLB-0004", "Pipe rack", 41, "length", 595m, 20, Plumbing, Agos),
            new("PPR pipe, 1/2 in x 4 m, PN20", "PLB-0101", "Pipe rack", 72, "length", 330m, 30, Plumbing, Agos),
            new("PPR elbow, 1/2 in", "PLB-0102", "Rack D1", 340, "pc", 18m, 100, Plumbing, Agos),
            new("PVC solvent cement, 400 cc", "PLB-0201", "Rack D1", 22, "pc", 320m, 10, Plumbing, Agos),
            new("Water closet, two-piece, white", "PLB-0301", "Bay 6", 2, "pc", 4850m, 2, Plumbing, Agos),
            new("Teflon tape, 1/2 in", "PLB-0401", "Rack D1", 180, "pc", 15m, 50, Plumbing, Agos),
            new("THHN stranded wire, 2.0 sq.mm, 150 m", "ELE-0020", "Rack E1", 14, "roll", 2350m, 6, Electrical, Kalasag),
            new("THHN stranded wire, 3.5 sq.mm, 150 m", "ELE-0035", "Rack E1", 11, "roll", 3480m, 6, Electrical, Kalasag),
            new("THHN stranded wire, 5.5 sq.mm, 150 m", "ELE-0055", "Rack E1", 5, "roll", 5320m, 3, Electrical, Kalasag),
            new("PVC conduit, 20 mm x 3 m", "ELE-0101", "Pipe rack", 380, "length", 68m, 150, Electrical, Kalasag),
            new("Utility box, 2 x 4 in", "ELE-0201", "Rack E2", 260, "pc", 22m, 100, Electrical, Kalasag),
            new("Circuit breaker, 20 A, 1 pole", "ELE-0301", "Rack E2", 68, "pc", 425m, 20, Electrical, Kalasag),
            new("Panel board, 8 branches", "ELE-0302", "Rack E2", 1, "pc", 2650m, 2, Electrical, Kalasag),
            new("Latex paint, flat, white, 4 L", "PNT-0001", "Paint room", 84, "gal", 780m, 30, Finishes, Tropikal),
            new("Concrete primer and sealer, 4 L", "PNT-0002", "Paint room", 52, "gal", 690m, 20, Finishes, Tropikal),
            new("Enamel paint, gloss, white, 4 L", "PNT-0003", "Paint room", 27, "gal", 950m, 10, Finishes, Tropikal),
            new("Skim coat, 20 kg", "PNT-0101", "Cement shed", 75, "bag", 520m, 30, Finishes, Tropikal),
            new("Lacquer thinner, 4 L", "PNT-0201", "Paint room", 5, "gal", 560m, 6, Finishes, Tropikal),
            new("Ceramic floor tile, 60 x 60 cm, box of 4", "TIL-0060", "Bay 7", 250, "box", 720m, 60, Finishes, Tropikal),
            new("Tile adhesive, 25 kg", "TIL-0101", "Cement shed", 70, "bag", 425m, 30, Finishes, Tropikal),
            new("Common nail, 3 in", "HDW-0003", "Rack A2", 135, "kg", 85m, 50, Hardware, Primeline),
            new("Common nail, 4 in", "HDW-0004", "Rack A2", 120, "kg", 85m, 50, Hardware, Primeline),
            new("Concrete nail, 2 in", "HDW-0102", "Rack A2", 28, "kg", 120m, 10, Hardware, Primeline),
            new("Tekscrew, 2-1/2 in, box of 100", "HDW-0201", "Rack A3", 24, "box", 455m, 10, Hardware, BagongSilang),
            new("Cutting disc, 4 in", "HDW-0301", "Rack A3", 160, "pc", 38m, 50, Hardware, Primeline),
            new("Anchor bolt, 1/2 x 6 in", "HDW-0401", "Rack A3", 678, "pc", 45m, 40, Hardware, Primeline),
            new("Welding rod, 6013, 1/8 in", "HDW-0501", "Rack A3", 31, "kg", 165m, 10, Hardware, Primeline),
        };

    // Eleven weeks of pesos in and out of the yard, Mondays from Aug 10.
    public static readonly string[] Weeks =
    {
            "Aug 10", "Aug 17", "Aug 24", "Aug 31", "Sep 7", "Sep 14", "Sep 21", "Sep 28", "Oct 5", "Oct 12", "Oct 19",
        };

    public static readonly float[] Received =
    {
            184_500f, 352_800f, 468_200f, 512_600f, 706_400f, 548_900f, 402_300f, 612_700f, 455_100f, 688_300f, 691_300f,
        };

    public static readonly float[] Issued =
    {
            438_200f, 366_700f, 301_500f, 487_900f, 532_400f, 498_100f, 286_400f, 541_800f, 723_600f, 512_200f, 842_600f,
        };

    public static decimal StockValue => Materials.Sum(m => m.Stock * m.Cost);

    // Stock value at the end of each week, worked back from today's: a
    // week ends at the next week's end, less what came in that next week,
    // plus what went out.
    public static float[] StockValueByWeek() {
      var values = new float[Received.Length];
      var v = (float)StockValue;
      for (var w = values.Length - 1; w >= 0; w--) {
        values[w] = v;
        v = v - Received[w] + Issued[w];
      }
      return values;
    }

    // The rest of the dashboard comes from screens the sketches do not
    // draw, so these are given, not worked out.
    public const int LowStockLastWeek = 4;
    public const int ToolsOverdue = 3;
    public const int ToolsOverdueLastWeek = 4;
  }
}

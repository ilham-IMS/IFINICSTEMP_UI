using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

public class IfinNodeModel : NodeModel
{
   public IfinNodeModel(Point? position = null) : base(position) { }

   public string Label { get; set; } = string.Empty;
   public string Value { get; set; } = string.Empty;
   public string Style { get; set; } = string.Empty;

}


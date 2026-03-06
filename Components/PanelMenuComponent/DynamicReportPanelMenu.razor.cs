using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.PanelMenuComponent
{
  public partial class DynamicReportPanelMenu
  {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? DynamicReportID { get; set; }


    List<Menu> menus = [
    ];

    protected override async Task OnParametersSetAsync()
    {

      if (!string.IsNullOrWhiteSpace(DynamicReportID))
      {
        string BasePath = $"dynamicsetting/dynamicreportsetting/{DynamicReportID}";

        menus.AddRange([
          new Menu { Title = "Dynamic Report", Url = BasePath, Exact = true },
          new Menu { Title = "Table", Url = $"{BasePath}/dynamicreporttable" },
          new Menu { Title = "Column", Url = $"{BasePath}/dynamicreportcolumn" },
          new Menu { Title = "Order", Url = $"{BasePath}/dynamicreportcolumnorder" },
          new Menu { Title = "Parameter", Url = $"{BasePath}/dynamicreportparameter" },
          new Menu { Title = "User Access", Url = $"{BasePath}/dynamicreportuser" },
        ]);

        menus = menus.DistinctBy(x => x.Title).ToList();
      }
      await base.OnParametersSetAsync();
    }
  }
}

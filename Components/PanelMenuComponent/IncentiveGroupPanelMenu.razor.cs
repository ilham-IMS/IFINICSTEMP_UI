using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.PanelMenuComponent
{
  public partial class IncentiveGroupPanelMenu
  {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? IncentiveGroupID { get; set; }


    List<Menu> menus = [
    ];

    protected override async Task OnParametersSetAsync()
    {

      if (!string.IsNullOrWhiteSpace(IncentiveGroupID))
      {
        string BasePath = $"setting/incentivegroup/{IncentiveGroupID}";

        menus.AddRange([
          new Menu { Title = "Info", Url = BasePath, Exact = true },
          new Menu { Title = "Criteria", Url = $"{BasePath}/criteria" },
          new Menu { Title = "Position", Url = $"{BasePath}/position" },
        ]);

        menus = menus.DistinctBy(x => x.Title).ToList();
      }
      await base.OnParametersSetAsync();
    }
  }
}

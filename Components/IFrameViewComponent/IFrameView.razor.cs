using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.IFrameViewComponent
{
  public partial class IFrameView
  {
    #region Parameter
    [Parameter] public string? moduleUrl { get; set; }
    #endregion

    #region OnInitialized
    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();
    }
    #endregion
  }
}
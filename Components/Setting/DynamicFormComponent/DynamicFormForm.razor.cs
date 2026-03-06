using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicFormComponent
{
  public partial class DynamicFormForm
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter] public string? ID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    #endregion

    #region Field
    public JsonObject row = new();
    #endregion

    #region OnInitialized
    protected override async Task OnInitializedAsync()
    {
      if (ID != null)
      {
        await GetRow();
      }
      else
      {
      }
      await base.OnInitializedAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("MasterForm", "GetRowByID", new
      {
        ID
      });

      if (res?.Data != null)
      {
        row = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region OnSubmit
    private async void OnSubmit(JsonObject data)
    {
      Loading.Show();

      data = SetAuditInfo(data);

      data = row.Merge(data);

      #region Insert
      if (ID == null)
      {
        var res = await IFINICSClient.Post("MasterForm", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicform/{res.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("MasterForm", "UpdateByID", data);
        await GetRow();
      }

      Loading.Close();
      StateHasChanged();
      #endregion
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicform");
    }
    #endregion

    #region ChangeActive
    private async Task ChangeActive()
    {
      if (ID != null)
      {
        Loading.Show();
        var data = SetAuditInfo(row);
        var res = await IFINICSClient.Put("MasterForm", "ChangeStatus", data);

        if (res != null)
        {
          await GetRow();
          Loading.Close();
        }

        StateHasChanged();
      }
    }
    #endregion

  }
}
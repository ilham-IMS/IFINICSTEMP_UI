using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.JournalGlLinkComponent
{
  public partial class JournalGlLinkForm
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    #endregion

    #region Field
    public JsonObject row = new();
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      if (ID != null)
      {
        await GetRow();
      }
      else
      {
        row["IsActive"] = 1;
      }
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("JournalGlLink", "GetRowByID", new
      {
        ID = ID
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
        var res = await IFINICSClient.Post("JournalGliLink", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"{ParentMenuURL}/gllink/{res.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("JournalGlLink", "UpdateByID", data);
        await GetRow();
      }

      Loading.Close();
      StateHasChanged();
      #endregion
    }
    #endregion

    #region ChangeActive
    private async Task ChangeActive()
    {
      // if (ID != null)
      // {
      //   Loading.Show();
      //   var res = await JournalGlLinkService.ChangeIsActive(row);

      //   if (res != null)
      //   {
      //     await GetRow();
      //     Loading.Close();
      //   }

      //   StateHasChanged();
      // }
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/gllink");
    }
    #endregion

  }
}

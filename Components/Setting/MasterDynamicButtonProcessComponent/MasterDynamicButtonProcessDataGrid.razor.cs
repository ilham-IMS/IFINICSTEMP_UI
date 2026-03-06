using System.Text.Json.Nodes;


using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.MasterDynamicButtonProcessComponent
{
  public partial class MasterDynamicButtonProcessDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Parameter
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region OnInitialized
    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicButtonProcess", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit
      });
      return res?.Data;
    }
    #endregion

    #region Add
    private void Add()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/masterdynamicbuttonprocess/add");
    }
    #endregion

    #region Delete
    private async void Delete()
    {
      var selectedData = dataGrid.selectedData;

      if (!selectedData.Any())
      {
        await NoDataSelectedAlert();
        return;
      }

      bool? result = await Confirm();

      if (result == true)
      {
        Loading.Show();

        await IFINICSClient.Delete("MasterDynamicButtonProcess", "DeleteByID", dataGrid.selectedData.Select(row => row["ID"]).ToArray());

        await dataGrid.Reload();
        dataGrid.selectedData.Clear();

        Loading.Close();

        StateHasChanged();
      }
    }
    #endregion

    #region CheckMethod
    private async void CheckMethod(JsonObject row)
    {
      var result = await IFINICSClient.Post("MasterDynamicButtonProcess", "CheckDllExist", row);
      if (result?.Data != null)
      {
        var res = result?.Data;
        if (res["Found"].GetValue<bool>())
          await Alert("Info", "Method is exsist");
        else
          await Alert("Info", "Method is Not Exist");
      }

    }
    #endregion

    #region Upload DLL

    private async void Upload(FileInput file)
    {
      Loading.Show();

      JsonObject data = new()
      {
        ["Content"] = file.Content.ToJsonNode(),
        ["Name"] = file.Name,
        ["FileMimeType"] = file.MimeType,
      };

      data = SetAuditInfo(data);

      var res = await IFINICSClient.Post<JsonObject>("MasterDynamicButtonProcess", "UploadDLL", data);
      if (res?.Data != null)
      {
        await Alert("Info", $"{res?.Data["File"]?.GetValue<string>()}");
      }

      StateHasChanged();

      await dataGrid.Reload();
      StateHasChanged();

      Loading.Close();
    }

    #endregion
  }
}

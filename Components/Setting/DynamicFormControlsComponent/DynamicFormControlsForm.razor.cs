using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicFormControlsComponent
{
  public partial class DynamicFormControlsForm
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter] public string? ID { get; set; }
    [Parameter] public string? MasterFormID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    Dictionary<string, string> componentType = new()
{
{"Text Box","FormFieldTextBox"},
{"Text Area","FormFieldTextArea"},
{"Numeric","FormFieldNumeric"},
{"Decimal","FormFieldDecimal"},
{"Date Time","FormFieldDatePicker"},
{"Switch","FormFieldSwitch"},
// {"DDL","FormFieldDropdown"}
};

    Dictionary<string, string> numericType = new()
  {
    {"Decimal","N2"},
    {"Rate","N6"},
  };

    string ddlInput;
    #endregion

    #region Field
    public JsonObject row = new();
    public JsonObject rowParent = new();
    #endregion

    #region OnInitialized
    protected override async Task OnInitializedAsync()
    {
      if (ID != null)
      {
        await GetRow();
        // var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(row["Items"]?.GetValue<string>() ?? "{}");
        // if (dict != null && row["ComponentName"]?.GetValue<string>() == "FormFieldDropdown")
        //    ddlInput = ConvertToString(row.Items);
      }
      else
      {
        row["MasterFormID"] = MasterFormID;
      }
      await GetRowParent();
      await base.OnInitializedAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("FormControls", "GetRowByID", new
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

    #region GetRowParent
    public async Task GetRowParent()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("MasterForm", "GetRowByID", new
      {
        ID = MasterFormID
      });

      if (res?.Data != null)
      {
        rowParent = res.Data;
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
        var res = await IFINICSClient.Post("FormControls", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicform/{MasterFormID}/formcontrol/{res.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("FormControls", "UpdateByID", data);
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
      NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicform/{MasterFormID}");
    }
    #endregion

     #region ChangeActive
    private async Task ChangeActive()
    {
      if (ID != null)
      {
        Loading.Show();
        var data = SetAuditInfo(row);
        var res = await IFINICSClient.Put("FormControls", "ChangeStatus", data);

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
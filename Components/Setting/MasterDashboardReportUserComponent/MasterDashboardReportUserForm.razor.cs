
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.MasterDashboardReportUserComponent
{
  public partial class MasterDashboardReportUserForm
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    [Inject] IFINSYSClient IFINSYSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    #endregion

    #region Component Ref
    SingleSelectLookup<JsonObject>? employeeLookup;

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
        row["IsEditable"] = 1;
      }
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("MasterUser", "GetRowByID", new
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
        var res = await IFINICSClient.Post("MasterUser", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"{ParentMenuURL}/masterdashboardreportuser/{res.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Post("MasterUser", "UpdateByID", data);
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
      NavigationManager.NavigateTo($"{ParentMenuURL}/masterdashboardreportuser");
    }
    #endregion

    #region lookup employee
    public async Task<List<JsonObject>?> LoadEmployeeLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterUser", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
      });

      return res?.Data;
    }
    #endregion


    #region lookup employee ifinsys
    public async Task<List<JsonObject>?> LoadEmployeeLookupExcluede(DataGridLoadArgs args)
    {
      var MaxValue = int.MaxValue!;
      var res = await IFINSYSClient.GetRows<JsonObject>("EmployeeMain", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        MaxValue = MaxValue
      });
      var Employees = res?.Data ?? [];
      // (keyword, 0, int.MaxValue!);

      // List<string> getemployeeID = new List<string>();
      // foreach (var Employee in Employees)
      // {
      //   getemployeeID.Add(Employee.EmployeeID);
      // }
      // string[] employeeID = getemployeeID.ToArray();
      var response = await IFINSYSClient.GetRows<JsonObject>("EmployeeMain", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        EmployeeID = Employees.Select(row => row["EmployeeID"]?.GetValue<string>()).ToList()
      });

      return response?.Data;
    }
    #endregion


  }
}

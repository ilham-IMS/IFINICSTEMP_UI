using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportParameterComponent
{
	public partial class DynamicReportParameterForm
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter] public string? ID { get; set; }
		[Parameter] public string? DynamicReportID { get; set; }
		[Parameter] public string? ParentMenuURL { get; set; }
		#endregion

		#region Component Field
		SingleSelectLookup<JsonObject> masterColumnLookup = default!;
		#endregion

		#region Field
		public JsonObject rowDynamicReport { get; set; } = [];
		public JsonObject row = new();
		JsonObject masterForm = new();
		List<ExtendModel>? extend = new();
		List<FormControlsModel> controls = new();

		Dictionary<string, string> componentType = new()
		{
			["Text Box"] = "FormFieldTextBox",
			["Numeric"] = "FormFieldNumeric",
			["Date Time"] = "FormFieldDatePicker",
			["Switch"] = "FormFieldSwitch"
			// ["DDL"] ="FormFieldDropdown"
		};
		Dictionary<string, string> operatorDict = new()
		{
			["Equals"] = "=",
			["Greater Than"] = ">",
			["Greater Than or Equals"] = ">=",
			["Less Than"] = "<",
			["Less Than or Equals"] = "<=",
			["Not Equals"] = "!=",
		};
		public bool IsPublished
		{
			get
			{
				return rowDynamicReport["IsPublished"]?.GetValue<int>() == 1;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return IsPublished;
			}
		}
		#endregion
		RenderFragment Form => builder =>
	   {

		   int seq = 0;
		   foreach (var control in controls)
		   {
			   DynamicRenderForm(builder, ref seq, control);
		   }
	   };

		#region OnParametersSet
		protected override async Task OnParametersSetAsync()
		{
			masterForm = await LoadMasterForm("DRP");
      controls = await LoadFormControls(masterForm["ID"]?.GetValue<string>());
			if (ID != null)
			{
				await GetRow();
				var extRes = await IFINICSClient.GetRows<ExtendModel>("DynamicReportParameter", "GetRowByExt", new { ID = ID });
        extend = extRes?.Data;

        var controlNames = controls.Select(control => control.Name).ToHashSet();
        extend = extend?.Where(ext => controlNames.Contains(ext.Keyy)).ToList();

        AddExtendProperty(controls, extend, row);
			}
			else
			{
				row["DynamicReportID"] = DynamicReportID;
				row["Properties"] = JsonValue.Create(controls.ToDictionary(x => x.Name, x => x.Value));
			}
			SetInitialValue(row, controls);
			await GetRowrowDynamicReport();
			await base.OnParametersSetAsync();
		}
		#endregion

		#region Load Dynamic Form
    private async Task<JsonObject> LoadMasterForm(string code)
    {
      var res = await IFINICSClient.GetRow<JsonObject>("MasterForm", "GetRowByCode", new { code });
      return res?.Data ?? [];
    }
    private async Task<List<FormControlsModel>> LoadFormControls(string formID)
    {
      var res = await IFINICSClient.GetRows<FormControlsModel>("FormControls", "GetRows", new { MasterFormID = formID });
      return res?.Data ?? [];
    }
    #endregion

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();
			var res = await IFINICSClient.GetRow<JsonObject>("DynamicReportParameter", "GetRowByID", new
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

		#region LoadLookup
		protected async Task<List<JsonObject>?> LoadMasterDynamicReportColumnLookup(DataGridLoadArgs args)
		{
			var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportColumn", "GetRowsForLookupByDynamicReport", new
			{
				args.Keyword,
				args.Offset,
				args.Limit,
				DynamicReportID
			});

			return res?.Data;
		}
		#endregion

		#region GetRowrowDynamicReport
		public async Task GetRowrowDynamicReport()
		{
			Loading.Show();
			var res = await IFINICSClient.GetRow<JsonObject>("DynamicReport", "GetRowByID", new
			{
				ID = DynamicReportID
			});

			if (res?.Data != null)
			{
				rowDynamicReport = res.Data;
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
			SetExtensionProperties(data, controls, "Properties");

			// ✅ Normalize DefaultValue
			if (data.ContainsKey("DefaultValue") && data["DefaultValue"] != null)
			{
				// Force everything to string so API always receives string
				data["DefaultValue"] = data["DefaultValue"].ToString();
			}

			#region Insert
			if (ID == null)
			{
				var res = await IFINICSClient.Post("DynamicReportParameter", "Insert", data);

				if (res?.Data != null)
				{
					NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting/{DynamicReportID}/dynamicreportparameter/{res.Data["ID"]}");
				}
			}
			#endregion

			#region Update
			else
			{
				var res = await IFINICSClient.Put("DynamicReportParameter", "UpdateByID", data);
				await GetRow();
			}

			Loading.Close();
			StateHasChanged();
			#endregion
		}
		#endregion

		#region SelectColumn
		void SelectColumn(JsonObject select)
		{
			row["MasterDynamicReportColumnID"] = select["ID"]?.DeepClone();
			row["ColumnName"] = select["Alias"]?.DeepClone();
			row["DynamicReportTableID"] = select["ReportTableID"]?.DeepClone();
			row["TableName"] = select["TableName"]?.DeepClone();
		}
		#endregion

		#region Back
		private void Back()
		{
			NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting/{DynamicReportID}/dynamicreportparameter");
		}
		#endregion

		#region ChangeIsDefaultValue
		private void ChangeIsDefaultValue(int? value)
		{
			row["IsDefaultValue"] = value;
			row["DefaultValue"] = "";
			StateHasChanged();
		}
		#endregion

		#region ChangeComponentType
		private void ChangeComponentType(string? value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			row["ComponentName"] = value;

			// Optionally reset DefaultValue so the new component has a clean value
			row["DefaultValue"] = value switch
			{
				"Numeric" => "0",
				"Date Time" => DateTime.Now.ToString("yyyy-MM-dd"),
				"Switch" => "false",
				_ => string.Empty
			};
		}
		#endregion

		#region Go to Dynamic Form Setting
		private void GoToSetting()
		{
			string masterFormID = masterForm["ID"]?.GetValue<string>();
			NavigationManager.NavigateTo($"setting/dynamicform/{masterFormID}");
		}
		#endregion
	}
}

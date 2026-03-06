
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.MasterDynamicReportTableComponent
{
	public partial class MasterDynamicReportTableDataGrid
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Component Field
		DataGrid<JsonObject> dataGrid = null!;
		MultipleSelectLookup<JsonObject> informationSchemaTableLookup = null!;
		#endregion

		#region Field

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
			var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportTable", "GetRows", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit
			});

			return res?.Data;
		}
		#endregion

		#region LoadLookup
		protected async Task<List<JsonObject>?> LoadInformationSchemaTableLookup(DataGridLoadArgs args)
		{
			var existingTables = dataGrid.Data?.Select(row => row["Name"]?.GetValue<string>()).ToArray();
			var res = await IFINICSClient.GetRows<JsonObject>("InformationSchemaTable", "GetRowsForLookupExcludeByMasterDynamicReport", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit,
			});

			return res?.Data;
		}
		#endregion

		#region Add
		private void Add()
		{
			informationSchemaTableLookup.Open();
		}
		#endregion
		#region Save
		private async void Save()
		{
			var selectedData = informationSchemaTableLookup!.GetSelected();

			if (!selectedData.Any())
			{
				await NoDataSelectedAlert();
				return;
			}
			var data = informationSchemaTableLookup.GetSelected().Select(row => SetAuditInfo(new JsonObject()
			{
				["Name"] = row["Name"]?.GetValue<string>(),
			})).ToList();
			Loading.Show();

			await IFINICSClient.Post("MasterDynamicReportTable", "Insert", data);

			await dataGrid.Reload();
			await informationSchemaTableLookup.Reload();

			Loading.Close();
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

				List<string?> id = dataGrid.selectedData.Select(row => row["ID"]?.GetValue<string>()).ToList();

				await IFINICSClient.Delete("MasterDynamicReportTable", "DeleteByID", id);

				await dataGrid.Reload();
				dataGrid.selectedData.Clear();

				Loading.Close();

				StateHasChanged();
			}
		}
		#endregion

		#region print
		protected async Task Print()
		{

			Console.WriteLine("Print");

			var file = await IFINICSClient.GetRow<JsonObject>("MasterDynamicReportTable", "GetReportData", null);


			if (file?.Data != null)
			{
				var data = file.Data;

				var content = data["Content"]?.GetValueAsByteArray();
				var fileName = data["Name"]?.GetValue<string>();
				var mimeType = data["MimeType"]?.GetValue<string>();

				PreviewFile(content, fileName, mimeType);
			}
		}
		#endregion print
	}
}
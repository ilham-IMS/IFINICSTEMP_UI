using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportTableRelationComponent
{
	public partial class DynamicReportTableRelationForm
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Parameter
		[CascadingParameter] public DynamicReportTableRelationDataGrid? DataGrid { get; set; }
		[Parameter] public string? ID { get; set; }
		[Parameter] public string? DynamicReportTableID { get; set; }
		[Parameter] public string? ParentMenuURL { get; set; }
		#endregion

		#region Component Field
		SingleSelectLookup<JsonObject> columnLookup = null!;
		SingleSelectLookup<JsonObject> referenceReportTableLookup = null!;
		SingleSelectLookup<JsonObject> referenceColumnLookup = null!;
		#endregion

		#region Field
		public JsonObject row = new();
		#endregion

		#region OnParametersSet
		protected override async Task OnParametersSetAsync()
		{
			if (ID != null)
			{
				await GetRow();
			}
			else
			{
				row["ReferenceDynamicReportTableID"] = DynamicReportTableID;
			}
			await base.OnParametersSetAsync();
		}
		#endregion

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();
			var res = await IFINICSClient.GetRow<JsonObject>("DynamicReportTableRelation", "GetRowByID", new
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
			var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportColumn", "GetRowsForLookupByDynamicReportTableForRelatedColumn", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit,
				DynamicReportTableID = row["DynamicReportTableID"]?.GetValue<string>(),
				RelatedMasterDynamicReportColumnID = row["ReferenceMasterDynamicReportColumnID"]
			});

			return res?.Data;
		}
		protected async Task<List<JsonObject>?> LoadReferenceDynamicReportTableLookup(DataGridLoadArgs args)
		{
			var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportTable", "GetRowsExclude", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit,
				DynamicReportTableID = DynamicReportTableID
			});

			return res?.Data;
		}
		protected async Task<List<JsonObject>?> LoadReferenceMasterDynamicReportColumnLookup(DataGridLoadArgs args)
		{
			var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportColumn", "GetRowsForLookupByDynamicReportTable", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit,
				DynamicReportTableID = DynamicReportTableID,
			});

			return res?.Data;
		}
		#endregion

		#region OnSubmit
		private async void OnSubmit(JsonObject data)
		{
			Loading.Show();

			data = SetAuditInfo(data);

			data = row.Merge(data);

			data["ReferenceDynamicReportTableID"] = DynamicReportTableID;

			#region Insert
			if (ID == null)
			{
				var res = await IFINICSClient.Post("DynamicReportTableRelation", "Insert", new List<JsonObject> { data });

				if (DataGrid != null)
				{
					await DataGrid.Reload();
					DataGrid.CloseModal();
				}
			}
			#endregion

			#region Update
			else
			{
				var res = await IFINICSClient.Put("DynamicReportTableRelation", "UpdateByID", data);
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
			row["ColumnAlias"] = select["Alias"]?.DeepClone();
		}
		#endregion

		#region SelectReferenceTable
		void SelectReferenceTable(JsonObject select)
		{
			row["DynamicReportTableID"] = select["ID"]?.DeepClone();
			row["TableAlias"] = select["Alias"]?.DeepClone();
		}
		#endregion

		#region SelectReferenceColumn
		void SelectReferenceColumn(JsonObject select)
		{
			row["ReferenceMasterDynamicReportColumnID"] = select["ID"]?.DeepClone();
			row["ReferenceColumnAlias"] = select["Alias"]?.DeepClone();
		}
		#endregion

		#region Back
		private void Back()
		{
			NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting/{DynamicReportTableID}/");
		}
		#endregion
	}
}

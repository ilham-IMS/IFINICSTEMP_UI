
using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.MasterDynamicReportColumnComponent
{
	public partial class MasterDynamicReportColumnDataGrid
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Parameters
		[Parameter] public string? TableID { get; set; }
		#endregion

		#region Component Field
		DataGrid<JsonObject> dataGrid = null!;
		#endregion

		#region Field

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
			var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportColumn", "GetRows", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit,
				MasterDynamicReportTableID = TableID
			});

			return res?.Data;
		}
		#endregion

		#region LoadLookup
		protected async Task<List<JsonObject>?> LoadInformationSchemaColumnLookup(DataGridLoadArgs args)
		{
			var res = await IFINICSClient.GetRows<JsonObject>("InformationSchemaColumn", "GetRowsForLookup", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit
			});

			return res?.Data;
		}
		#endregion

		#region Save
		private async void Save(JsonObject data)
		{
			Loading.Show();
			List<JsonObject> list = new();

			foreach (var (key, value) in data)
			{
				var id = key.Split("_").Last();
				var objKey = key.Split("_").First();

				if (list.Find(x => x["ID"]?.GetValue<string>() == id) == null)
				{
					list.Add(SetAuditInfo(
						new JsonObject()
						{
							["ID"] = id,
						}
					));
				}

				list.Find(x => x["ID"]?.GetValue<string>() == id)![objKey] = value?.DeepClone();
			}

			await IFINICSClient.Put("MasterDynamicReportColumn", "UpdateByID", list);

			await dataGrid.Reload();
			Loading.Close();
		}
		#endregion

		#region ChangeAvailable
		private async void ChangeAvailable(JsonObject data)
		{
			Loading.Show();
			data = SetAuditInfo(data);

			await IFINICSClient.Put("MasterDynamicReportColumn", "ChangeAvailable", data);

			await dataGrid.Reload();
			Loading.Close();
		}
		#endregion
		#region ChangeMaskingStatus
		private async void ChangeMaskingStatus(JsonObject data)
		{
			Loading.Show();
			data = SetAuditInfo(data);

			await IFINICSClient.Put("MasterDynamicReportColumn", "ChangeMaskingStatus", data);
			await dataGrid.Reload();
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

				await IFINICSClient.Delete("MasterDynamicReportColumn", "Delete", id);

				await dataGrid.Reload();
				dataGrid.selectedData.Clear();

				Loading.Close();

				StateHasChanged();
			}
		}
		#endregion
	}
}
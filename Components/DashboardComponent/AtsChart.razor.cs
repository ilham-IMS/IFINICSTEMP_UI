namespace IFinancing360_ICS_UI.Components.DashboardComponent;

using System.Text.Json;
using System.Text.Json.Nodes;
using DotNetEnv;
using iFinancing360.Helper;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Helper.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Sprache;

public partial class AtsChart
{
  #region Service
  [Inject] BaseHttpClient BaseHttpClient { get; set; } = null!;
  [Inject] IFINSYSClient IFINSYSClient { get; set; } = default!;
  [Inject] protected AuthStateProvider AuthStateProvider { get; set; } = default!;
  #endregion

  #region Parameter
  [Parameter] public JsonObject MasterDashboard { get; set; } = [];
  #endregion

  #region Components
  public Dictionary<string, SingleSelectLookup<JsonObject>> FilterLookups { get; set; } = null!;
  public Dictionary<string, FormFieldDropdown<string>> FilterDropdowns { get; set; } = null!;
  #endregion

  #region Class Field
  public DotNetObjectReference<AtsChart>? BlazorInstanceRef { get; set; }

  public bool IsLoading { get; set; }
  public bool IsFirstRender = false;
  public bool IsFilterExpand = false;
  public bool IsDescriptionExpand = false;
  public bool IsCardSuccess = true;
  public string Day = string.Empty;
  public string Month = string.Empty;
  public string Year = string.Empty;
  public string NameDate = string.Empty;
  public string InputTimeType = string.Empty;
  public Dictionary<string, string> FilterNameDates = [];

  public List<FormField> FormFieldFilters { get; set; } = [];
  public JsonObject Filter { get; set; } = [];
  public JsonObject FilterData = [];
  public List<JsonObject> FilterComponents = [];
  public Dictionary<string, Dictionary<string, string>> FilterOptions = [];
  public Dictionary<string, string> MonthOptions = null!;
  public Dictionary<string, string> YearOptions = null!;
  private List<string> Errors = [];

  public int ChartTabletSize { get => MasterDashboard["TabletGrid"]?.GetValue<int>() ?? 4; }
  public int ChartDesktopSize { get => MasterDashboard["DesktopGrid"]?.GetValue<int>() ?? 3; }

  public string ChartType { get => MasterDashboard["Type"]?.GetValue<string>() ?? "column"; }
  public string ChartTitle
  {
    get
    {
      var tempList =
          (MasterDashboard["Name"]?.GetValue<string>() ?? "")
            .ToUpper()
            .Split(' ')
            .ToList();

      return string.Join(" ", tempList);
    }
  }
  public string RandomColor => PickRandom()[0];
  public string RandomIcon => PickRandom()[1];

  public override string IDPrefix => $"{base.IDPrefix}-chart";
  public override string id
  {
    get
    {
      if (string.IsNullOrWhiteSpace(ID)) ID = Guid.NewGuid().ToString("N").Substring(0, 5);

      var formattedID = $"{IDPrefix}-{ID}";

      return formattedID.ToLower().Replace(" ", "-");
    }
  }
  #endregion

  protected DateTime? GetSystemDate()
  {
    var user = AuthStateProvider.CurrentUser?.SystemDate;
    return user;
  }



  #region OnInitializedAsync() - Lifecycle
  protected override async Task OnInitializedAsync()
  {
    IsLoading = true;
    IsFirstRender = true;

    try
    {
      Errors.Clear();

      await FilterSetup();
    }
    catch (Exception ex)
    {
      Errors.Add($"OnInitializedAsync() | Error msg:{ex.Message}");
      IsCardSuccess = false;
    }

    IsLoading = false;

    await base.OnInitializedAsync();
  }
  #endregion

  #region OnAfterRenderAsync() - Lifecycle
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    try
    {
      if (!IsCardSuccess && Errors.Count > 0) throw new Exception();

      if (firstRender)
      {
        await RegisterBlazorInterop();
        await LoadChart();
      }
    }
    catch (Exception)
    {
      IsCardSuccess = false;
      StateHasChanged(); // Hanya panggil StateHasChanged jika ada error
    }

    await base.OnAfterRenderAsync(firstRender);
  }
  #endregion

  #region Dispose() - Lifecycle
  public override async void Dispose()
  {
    try
    {
      if (!string.IsNullOrEmpty(id))
      {
        await JSRuntime.InvokeVoidAsync("removeChart", id);
      }

      BlazorInstanceRef?.Dispose();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error disposing chart {id}: {ex.Message}");
    }

    base.Dispose();
  }
  #endregion



  #region ReloadChart
  public async Task ReloadChart()
  {
    IsLoading = true;

    try
    {
      Errors.Clear();

      await FilterSetup();

      await LoadChart();
    }
    catch (Exception ex)
    {
      Errors.Add($"ReloadChart() | Error msg:{ex.Message}");
      IsCardSuccess = false;
    }

    IsLoading = false;
  }
  #endregion

  #region LoadChart()
  public async Task LoadChart()
  {
    try
    {
      var filter = FilterData.DeepClone() as JsonObject;
      var chartConfig = await LoadChartData(filter!);
      var chartOptions = chartConfig.Option ?? throw new Exception("Failed to retrieve data option.");

      var scaleValueFormat = MasterDashboard["ScaleValueFormat"]?.GetValue<string>();
      var scaleLabelFormat = MasterDashboard["ScaleLabelFormat"]?.GetValue<string>();

      var thresholdLabel = MasterDashboard["ThresholdLabel"]?.GetValue<string>();
      var thresholdValue = MasterDashboard["ThresholdValue"]?.AsValue().GetValue<decimal>();
      var thresholdColor = MasterDashboard["ThresholdColor"]?.GetValue<string>();

      int? isStacked = MasterDashboard["IsStacked"]?.GetValue<int>();

      string tempID = "";

      if (id == null)
      {
        if (string.IsNullOrWhiteSpace(ID)) ID = Guid.NewGuid().ToString("N").Substring(0, 5);

        var formattedID = $"{IDPrefix}-{ID}";

        tempID = formattedID.ToLower().Replace(" ", "-");
      }
      string chartContainerID = id ?? tempID;

      // Add a small delay to ensure DOM is ready
      await Task.Delay(100);

      IsCardSuccess = await JSRuntime.InvokeAsync<bool>("initChart", chartContainerID, ChartType.ToLower(), chartOptions.Name, chartOptions.Categories, chartOptions.Data, chartOptions.Series, NameDate, scaleValueFormat, scaleLabelFormat,
        thresholdLabel, thresholdValue, thresholdColor, isStacked);

      if (!IsCardSuccess && Errors.Count < 1)
      {
        // Retry once more after a longer delay
        await Task.Delay(500);
        IsCardSuccess = await JSRuntime.InvokeAsync<bool>("initChart", chartContainerID, ChartType.ToLower(), chartOptions.Name, chartOptions.Categories, chartOptions.Data, chartOptions.Series, NameDate, scaleValueFormat, scaleLabelFormat, thresholdLabel, thresholdValue, thresholdColor, isStacked);

        if (!IsCardSuccess)
        {
          throw new Exception("Failed to load chart");
        }
      }
      else if (!IsCardSuccess && Errors.Count > 0)
      {
        throw new Exception();
      }
    }
    catch (Exception ex)
    {
      Errors.Add($"LoadChart() | Error msg:{ex.Message}");
      throw;
    }
  }
  #endregion

  #region LoadChartData()
  private async Task<ChartConfig> LoadChartData(JsonObject filter)
  {
    try
    {
      // Getting URI
      var uri = string.Empty;
      var apiSourceType = MasterDashboard["DataSourceType"]?.GetValue<string>()?.ToUpper();

      switch (apiSourceType)
      {
        case "INTERNAL":
          string moduleUri = Env.GetString((MasterDashboard["ModuleCode"] ?? throw new Exception("Module code is null (IFINSYS)")).GetValue<string>());
          if (moduleUri.EndsWith('/')) moduleUri = moduleUri.TrimEnd('/');
          uri = $"{moduleUri}/{(MasterDashboard["APIName"] ?? throw new Exception("API is null (IFINSYS)")).GetValue<string>()}";

          if (filter["ChartType"] == null) filter["ChartType"] = MasterDashboard["Type"]?.GetValue<string>().ToString().ToLower() ?? "";
          break;

        case "EXTERNAL":
          string externalUri = MasterDashboard["APIExternalURL"]?.GetValue<string>() ?? throw new Exception("External API is null.");
          if (externalUri.EndsWith('/')) externalUri = externalUri.TrimEnd('/');
          uri = $"{externalUri}/{MasterDashboard["APIName"]?.GetValue<string>()}";

          if (filter["ChartType"] == null) filter["ChartType"] = MasterDashboard["Type"]?.GetValue<string>().ToString().ToLower() ?? "";

          Console.WriteLine($"LoadChartData() | API URI: {uri}");
          break;

        default:
          throw new Exception($"Unsupported API source: {apiSourceType}");
      }

      Console.WriteLine($"LoadChartData() | API URI: {uri}");

      var res = await BaseHttpClient.GetRow<ChartConfig>(
        uri, "", new JsonObject { ["FilterData"] = GetInputName(filter) }
      ) ?? throw new Exception("API response is null.");

      return res?.Data ?? new();
    }
    catch (Exception ex)
    {
      Errors.Add($"LoadChartData() | Error msg:{ex.Message}");
      throw;
    }
  }
  #endregion

  #region GetInputName
  private static JsonObject GetInputName(JsonObject data)
  {
    var tempData = new JsonObject { };

    foreach (var (key, value) in data)
    {
      var inputName = key.Split("-")[0];

      if (tempData.ContainsKey(inputName)) continue;

      tempData[inputName] = value?.DeepClone();
    }

    return tempData;
  }
  #endregion

  #region GetChildData() [JSInvokable]
  [JSInvokable]
  public async Task<ChartConfig> GetChildData(string drilldownID, int OrderKey)
  {
    try
    {
      var uri = string.Empty;

      //Ambil informasi drilldown ke sys
      var getNextDrilldown = await IFINSYSClient.GetRow<JsonObject>(
       "MasterDashboardDrilldown",
       "GetRowsNextDrilldown",
       new
       {
         MasterDashboardID = MasterDashboard["DashboardID"]?.ToString(),
         OrderKey = OrderKey,
         DrilldownID = drilldownID
       }
     );

      // Debug logging
      Console.WriteLine($"MasterDashboardID: {MasterDashboard["DashboardID"]?.ToString()}");
      Console.WriteLine($"OrderKey: {OrderKey}");
      Console.WriteLine($"DrilldownID: {drilldownID}");
      Console.WriteLine($"Drilldown response: {getNextDrilldown?.Data}");

      // Add null check for the response
      if (getNextDrilldown?.Data == null)
      {
        throw new Exception($"No drilldown data found for DrilldownID: {drilldownID}, OrderKey: {OrderKey}");
      }

      var apiSourceType = getNextDrilldown.Data?["DataSourceType"]?.GetValue<string>()?.ToUpper();

      // Add null check for apiSourceType
      if (string.IsNullOrEmpty(apiSourceType))
      {
        throw new Exception($"DataSourceType is null or empty for DrilldownID: {drilldownID}, OrderKey: {OrderKey}");
      }

      switch (apiSourceType)
      {
        case "INTERNAL":
          //Setelah dapet informasi drilldown, ambil data chartnya dari controller modul itu sendiri
          // Getting URI
          string moduleUri = Env.GetString((getNextDrilldown?.Data!["ModuleCode"] ?? throw new Exception("Module code is null (IFINSYS)")).GetValue<string>());
          if (moduleUri.EndsWith('/')) moduleUri = moduleUri.TrimEnd('/');
          uri = $"{moduleUri}/{(getNextDrilldown.Data["APIEndpoint"] ?? throw new Exception("API is null (IFINSYS)")).GetValue<string>()}";
          if (uri.EndsWith('/')) uri = uri.TrimEnd('/');
          break;

        case "EXTERNAL":
          string externalUri = getNextDrilldown?.Data?["APIExternalURL"]?.GetValue<string>() ?? throw new Exception("External API is null.");
          if (externalUri.EndsWith('/')) externalUri = externalUri.TrimEnd('/');
          uri = $"{externalUri}/{(getNextDrilldown.Data?["APIEndpoint"] ?? throw new Exception("API Name is null (IFINSYS)")).GetValue<string>()}";
          break;

        default:
          throw new Exception($"Unsupported API source: {apiSourceType}");
      }

      var parameter = new JsonObject
      {
        ["DrilldownID"] = drilldownID,
        ["OrderKey"] = OrderKey // Add OrderKey to parameter
      };

      var res = await BaseHttpClient.GetRow<ChartConfig>(
          uri, "", parameter
      ) ?? throw new Exception("API response is null.");

      var drilldownType = getNextDrilldown.Data["Type"]?.GetValue<string>();
      res.Data!.Option!.DrilldownChartType = drilldownType?.ToLower();

      // var DrilldownChart = new ChartConfig
      // {
      //   FilterData = res.Data?.FilterData,
      //   Option = new ChartOption
      //   {
      //     ID = drilldownID,
      //     Name = getNextDrilldown.Data["ChartName"]?.GetValue<string>(),
      //     Categories = res.Data?.Option?.Categories,
      //     Data = res.Data?.Option?.Data,
      //     Series = res.Data?.Option?.Series,
      //     DrilldownChartType = drilldownType?.ToLower(),
      //     DrilldownChartName = getNextDrilldown.Data["ChartName"]?.GetValue<string>()
      //   }
      // };

      return res.Data!;
    }
    catch (Exception ex)
    {
      Errors.Add($"GetChildData()\nError msg:{ex.Message}.");
      Console.WriteLine($"GetChildData Error: {ex.Message}");
      Console.WriteLine($"DrilldownID: {drilldownID}, OrderKey: {OrderKey}");
      throw;
    }
  }

  // Helper method untuk safe parsing string values
  private string? GetSafeStringValue(JsonObject? jsonObject, string propertyName)
  {
    try
    {
      if (jsonObject == null) return null;

      var property = jsonObject[propertyName];
      if (property == null) return null;

      // Check if it's a JsonValue before calling GetValue
      if (property is JsonValue jsonValue)
      {
        return jsonValue.GetValue<string>();
      }

      // Fallback to ToString() for other types
      return property.ToString();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error parsing string property '{propertyName}': {ex.Message}");
      return null;
    }
  }

  // Helper method untuk safe parsing decimal values
  private decimal GetSafeDecimalValue(JsonObject? jsonObject, string propertyName)
  {
    try
    {
      if (jsonObject == null) return 0;

      var property = jsonObject[propertyName];
      if (property == null) return 0;

      // Check if it's a JsonValue before calling GetValue
      if (property is JsonValue jsonValue)
      {
        // Try different numeric types
        if (jsonValue.TryGetValue<decimal>(out var decimalValue))
          return decimalValue;
        if (jsonValue.TryGetValue<double>(out var doubleValue))
          return (decimal)doubleValue;
        if (jsonValue.TryGetValue<int>(out var intValue))
          return intValue;
        if (jsonValue.TryGetValue<long>(out var longValue))
          return longValue;
      }

      // Fallback: try to parse as string
      if (decimal.TryParse(property.ToString(), out var parsedValue))
        return parsedValue;

      return 0;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error parsing decimal property '{propertyName}': {ex.Message}");
      return 0;
    }
  }
  #endregion


  #region FilterSetup()
  public async Task FilterSetup()
  {
    try
    {
      await GetFilters();

      if (FilterComponents.Count > 0)
      {
        SetMonthOrYearOptions();
        SetInitialFilterValues(forceReset: false); // Jangan reset jika data sudah ada
        SetLookupRefs();
      }

      await SetFilterDropdownValues();
    }
    catch (Exception)
    {
      throw;
    }
  }
  #endregion

  #region GetFilters()
  public async Task GetFilters()
  {
    Loading.Show();

    var res = await IFINSYSClient.GetRows<JsonObject>("MasterDashboardFilter", "GetRowsForModules", new { MasterDashboardID = MasterDashboard["DashboardID"]?.ToString() });

    if (res?.Data != null)
    {
      FilterComponents = res.Data;

      if (FilterComponents.Any(x => x["InputTimeType"]?.ToString() == "full_date"))
      {
        var a = FilterComponents.Where(x => x["InputTimeType"]?.ToString() == "full_date");
        System.Console.WriteLine(a);
      }
    }
    Loading.Close();
  }
  #endregion

  #region FilterValueChanged()
  public async Task FilterValueChanged(string name, string id, string? value = null, DateTime? dateValue = null, string? suffix = null)
  {
    try
    {
      // Tentukan kunci berdasarkan apakah ada suffix atau tidak
      var key = string.IsNullOrEmpty(suffix) ? $"{name}-{id}" : $"{name}_{suffix}-{id}";
      FilterData[key] = !string.IsNullOrEmpty(value) ? value : dateValue;

      // Update NameDate berdasarkan InputTimeType untuk filter ini
      UpdateNameDateFromInputTimeType(id, value, dateValue);

      // Gabungkan semua NameDate dari berbagai filter
      UpdateCombinedNameDate();

      IsFirstRender = false;
      await LoadChart();

      StateHasChanged();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error dalam FilterValueChanged: {ex.Message}");
      // Tampilkan error ke user jika diperlukan
      if (!Errors.Contains(ex.Message))
      {
        Errors.Add($"Filter error: {ex.Message}");
      }
    }
  }
  #endregion

  #region UpdateNameDateFromInputTimeType()
  private void UpdateNameDateFromInputTimeType(string filterId, string? value = null, DateTime? dateValue = null)
  {
    // Dapatkan filter component berdasarkan ID
    var filterComponent = FilterComponents.FirstOrDefault(x => x["ID"]?.ToString() == filterId);
    if (filterComponent == null) return;

    var inputType = filterComponent["InputType"]?.ToString() ?? "";
    if (inputType != "time") return;

    var inputTimeType = filterComponent["InputTimeType"]?.ToString() ?? "";
    var filterNameDate = "";

    switch (inputTimeType.ToLower())
    {
      case "month":
        // Format: "MMM YYYY" (contoh: "AUG 2025")
        var monthDate = dateValue ?? GetSystemDate() ?? DateTime.Now;
        if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int monthNumber))
        {
          // Validasi monthNumber harus antara 1-12
          if (monthNumber >= 1 && monthNumber <= 12)
          {
            try
            {
              monthDate = new DateTime(GetSystemDate()?.Year ?? DateTime.Now.Year, monthNumber, 1);
            }
            catch (ArgumentOutOfRangeException ex)
            {
              Console.WriteLine($"Error membuat DateTime untuk bulan {monthNumber}: {ex.Message}");
              monthDate = GetSystemDate() ?? DateTime.Now; // Fallback ke tanggal sekarang
            }
          }
          else
          {
            Console.WriteLine($"Nilai bulan tidak valid: {monthNumber}. Harus antara 1-12.");
            monthDate = GetSystemDate() ?? DateTime.Now; // Fallback ke tanggal sekarang
          }
        }
        filterNameDate = monthDate.ToString("MMM yyyy").ToUpper();
        break;

      case "year":
        // Format: "YYYY" (contoh: "2025")
        var yearValue = value ?? GetSystemDate()?.Year.ToString() ?? DateTime.Now.Year.ToString();

        // Validasi tahun jika value berupa string numerik
        if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int yearNumber))
        {
          // Validasi range tahun yang masuk akal (1900-3000)
          if (yearNumber >= 1900 && yearNumber <= 3000)
          {
            yearValue = yearNumber.ToString();
          }
          else
          {
            Console.WriteLine($"Nilai tahun tidak valid: {yearNumber}. Harus antara 1900-3000.");
            yearValue = GetSystemDate()?.Year.ToString() ?? DateTime.Now.Year.ToString(); // Fallback ke tahun sekarang
          }
        }

        filterNameDate = yearValue.ToUpper();
        break;

      case "time_range":
        // Format: "dd MMM yyyy - dd MMM yyyy" (contoh: "01 AUG 2025 - 31 AUG 2025")
        // Ambil tanggal dari dan sampai dari FilterData
        var fromDate = GetDateFromFilterData("_From");
        var toDate = GetDateFromFilterData("_To");
        if (fromDate.HasValue && toDate.HasValue)
        {
          filterNameDate = $"{fromDate.Value.ToString("dd MMM yyyy")} - {toDate.Value.ToString("dd MMM yyyy")}".ToUpper();
        }
        break;

      case "full_date":
        // Format: "dd MMM yyyy" (contoh: "01 AUG 2025")
        var fullDate = dateValue ?? GetSystemDate() ?? DateTime.Now;
        if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out DateTime parsedDate))
        {
          fullDate = parsedDate;
        }
        filterNameDate = fullDate.ToString("dd MMM yyyy").ToUpper();
        break;

      default:
        // Default fallback
        filterNameDate = (dateValue ?? GetSystemDate() ?? DateTime.Now).ToString("dd MMM yyyy").ToUpper();
        break;
    }

    // Simpan NameDate untuk filter ini
    if (!string.IsNullOrEmpty(filterNameDate))
    {
      FilterNameDates[filterId] = filterNameDate;
    }
  }
  #endregion

  #region UpdateCombinedNameDate()
  private void UpdateCombinedNameDate()
  {
    if (FilterNameDates.Count == 0)
    {
      NameDate = GetSystemDate()?.ToString("dd MMM yyyy").ToUpper() ?? DateTime.Now.ToString("dd MMM yyyy").ToUpper();
      return;
    }

    // Kelompokkan berdasarkan tipe (untuk prioritas urutan)
    var dateComponents = new Dictionary<string, string>();
    var rangeComponents = new List<string>();
    var otherComponents = new List<string>();

    foreach (var filter in FilterNameDates)
    {
      var filterId = filter.Key;
      var nameDate = filter.Value;

      var filterComponent = FilterComponents.FirstOrDefault(x => x["ID"]?.ToString() == filterId);
      if (filterComponent == null) continue;

      var inputTimeType = filterComponent["InputTimeType"]?.ToString()?.ToLower() ?? "";

      switch (inputTimeType)
      {
        case "full_date":
          dateComponents["day"] = nameDate;
          break;
        case "month":
          dateComponents["month"] = nameDate;
          break;
        case "year":
          dateComponents["year"] = nameDate;
          break;
        case "time_range":
          rangeComponents.Add(nameDate);
          break;
        default:
          otherComponents.Add(nameDate);
          break;
      }
    }

    // Gabungkan berdasarkan prioritas: Tanggal > Bulan > Tahun
    var combinedParts = new List<string>();

    // Jika ada full_date, month, atau year, gabungkan dengan urutan yang benar
    if (dateComponents.ContainsKey("day"))
    {
      combinedParts.Add(dateComponents["day"]);
    }
    else
    {
      // Jika tidak ada full_date, gabungkan month dan year
      if (dateComponents.ContainsKey("month") && dateComponents.ContainsKey("year"))
      {
        // Gabungkan month dan year menjadi "MMM YYYY"
        var monthPart = dateComponents["month"];
        var yearPart = dateComponents["year"];

        // Ekstrak bulan dari format month dan gabungkan dengan year
        if (monthPart.Contains(" "))
        {
          var monthName = monthPart.Split(' ')[0];
          combinedParts.Add($"{monthName} {yearPart}");
        }
        else
        {
          combinedParts.Add($"{monthPart} {yearPart}");
        }
      }
      else if (dateComponents.ContainsKey("month"))
      {
        combinedParts.Add(dateComponents["month"]);
      }
      else if (dateComponents.ContainsKey("year"))
      {
        combinedParts.Add(dateComponents["year"]);
      }
    }

    // Tambahkan range components
    combinedParts.AddRange(rangeComponents);

    // Tambahkan other components
    combinedParts.AddRange(otherComponents);

    // Gabungkan semua parts
    NameDate = string.Join(" | ", combinedParts.Where(x => !string.IsNullOrEmpty(x)));

    // Fallback jika tidak ada data
    if (string.IsNullOrEmpty(NameDate))
    {
      NameDate = GetSystemDate()?.ToString("dd MMM yyyy").ToUpper() ?? DateTime.Now.ToString("dd MMM yyyy").ToUpper();
    }
  }
  #endregion

  #region GetDateFromFilterData()
  private DateTime? GetDateFromFilterData(string suffix)
  {
    foreach (var filterData in FilterData)
    {
      if (filterData.Key.Contains(suffix) && filterData.Value != null)
      {
        // Handle JsonNode conversion to DateTime
        var value = filterData.Value;

        if (value is JsonValue jsonValue)
        {
          if (jsonValue.TryGetValue<DateTime>(out DateTime dateTime))
          {
            return dateTime;
          }
          if (jsonValue.TryGetValue<string>(out string? stringValue) && !string.IsNullOrEmpty(stringValue) && DateTime.TryParse(stringValue, out DateTime parsedDate))
          {
            return parsedDate;
          }
        }
        else if (DateTime.TryParse(value.ToString(), out DateTime directParsedDate))
        {
          return directParsedDate;
        }
      }
    }
    return null;
  }
  #endregion

  #region SetInitialFilterValues()
  public void SetInitialFilterValues(bool forceReset = false)
  {
    if (forceReset)
    {
      FilterData = [];
      FilterNameDates.Clear(); // Reset FilterNameDates
    }

    foreach (var filterComponent in FilterComponents)
    {
      var id = filterComponent["ID"]?.ToString() ?? throw new ArgumentException("Filter component ID is null or empty.");
      var inputName = filterComponent["InputName"]?.ToString() ?? throw new ArgumentException("Filter component InputName is null or empty.");
      var inputType = filterComponent["InputType"]?.ToString() ?? throw new ArgumentException("Filter component InputType is null or empty.");
      var inputTimeType = inputType == "time" ? (filterComponent["InputTimeType"] ?? throw new ArgumentException("Filter component InputTimeType is null or empty.")).ToString() ?? string.Empty : string.Empty;

      var defaultValue = filterComponent["DefaultValue"]?.ToString() ?? "";

      // Jangan inisialisasi ulang jika data sudah ada (kecuali forceReset = true)
      var keyToCheck = inputType == "time" && inputTimeType == "time_range" 
          ? $"{inputName}_From-{id}" 
          : $"{inputName}-{id}";
      
      if (FilterData.ContainsKey(keyToCheck) && !forceReset) continue;

      if (inputType == "time")
      {
        if (inputTimeType == "month")
        {
          // Pastikan format bulan adalah 2 digit dengan leading zero
          var currentMonth = DateTime.Now.Month;
          FilterData[$"{inputName}-{id}"] = currentMonth.ToString("00");
          FilterOptions[id] = new() {
            {"01", "JANUARY (JAN)"},
            {"02", "FEBRUARY (FEB)"},
            {"03", "MARCH (MAR)"},
            {"04", "APRIL (APR)"},
            {"05", "MAY (MAY)"},
            {"06", "JUNE (JUN)"},
            {"07", "JULY (JUL)"},
            {"08", "AUGUST (AUG)"},
            {"09", "SEPTEMBER (SEP)"},
            {"10", "OCTOBER (OCT)"},
            {"11", "NOVEMBER (NOV)"},
            {"12", "DECEMBER (DEC)"},
          };
          // Set NameDate untuk month: "MMM YYYY"
          var monthDate = DateTime.Now;
          FilterNameDates[id] = monthDate.ToString("MMM yyyy").ToUpper();
        }
        else if (inputTimeType == "year")
        {
          var currentYear = DateTime.Now.Year;
          FilterData[$"{inputName}-{id}"] = currentYear.ToString();

          // Buat range tahun yang valid (50 tahun ke belakang sampai 50 tahun ke depan)
          var startYear = Math.Max(1900, currentYear - 50);
          var endYear = Math.Min(3000, currentYear + 50);
          var yearRange = Enumerable.Range(startYear, endYear - startYear + 1);

          FilterOptions[id] = yearRange.ToDictionary(x => x.ToString(), x => x.ToString());

          // Set NameDate untuk year: "YYYY"
          FilterNameDates[id] = currentYear.ToString();
        }
        else if (inputTimeType == "time_range")
        {
          // Tidak memberikan default value untuk time_range, biarkan kosong
          // FilterData[$"{inputName}_From-{id}"] = null;
          // FilterData[$"{inputName}_To-{id}"] = null;
          // Tidak set NameDate untuk time_range yang kosong
        }
        else
        {
          FilterData[$"{inputName}-{id}"] = DateTime.Now;
          // Set NameDate untuk full_date: "dd MMM yyyy"
          FilterNameDates[id] = DateTime.Now.ToString("dd MMM yyyy").ToUpper();
        }
      }
      else if (inputType == "lookup")
      {
        FilterData[$"{inputName}_Code-{id}"] = defaultValue;
        FilterData[$"{inputName}_Description-{id}"] = string.IsNullOrEmpty(defaultValue) ? filterComponent["FilterLabel"]?.ToString() : defaultValue.ToUpper();
        FilterData[$"{inputName}_Value-{id}"] = defaultValue;
      }
    }

    // Update combined NameDate setelah semua filter diinisialisasi (selalu panggil ini)
    UpdateCombinedNameDate();
  }
  #endregion

  #region SetMonthOrYearOptions()
  public void SetMonthOrYearOptions()
  {
    MonthOptions = new() {
      {"01", "JANUARI (JAN)"},
      {"02", "FEBRUARI (FEB)"},
      {"03", "MARET (MAR)"},
      {"04", "APRIL (APR)"},
      {"05", "MEI (MAY)"},
      {"06", "JUNI (JUN)"},
      {"07", "JULI (JUL)"},
      {"08", "AGUSTUS (AUG)"},
      {"09", "SEPTEMBER (SEP)"},
      {"10", "OKTOBER (OKT)"},
      {"11", "NOVEMBER (NOV)"},
      {"12", "DESEMBER (DEC)"},
    };


    // Buat range tahun yang valid dengan validasi
    var currentYear = GetSystemDate()?.Year ?? DateTime.Now.Year;
    var startYear = Math.Max(1900, currentYear - 50);
    var endYear = Math.Min(3000, currentYear + 50);
    var yearRange = Enumerable.Range(startYear, endYear - startYear + 1);

    YearOptions = yearRange.ToDictionary(x => x.ToString(), x => x.ToString());
  }
  #endregion

  #region SetLookupRefs()
  public void SetLookupRefs()
  {
    if (FilterComponents.Any(x => x["InputType"]?.ToString().ToLower() == "lookup") == true)
    {
      FilterLookups = [];
    }
    else return;

    foreach (var filterComponent in FilterComponents)
    {
      if (filterComponent["InputType"]?.ToString().ToLower() == "lookup")
      {
        var id = filterComponent["ID"]?.ToString() ?? "";

        if (!FilterLookups.ContainsKey(id))
        {
          FilterLookups[id] = new SingleSelectLookup<JsonObject>();
        }
      }
    }
  }
  #endregion

  #region SetDropdownRefs()
  public void SetDropdownRefs()
  {
    var hasDropdown = FilterComponents.Any(x => x["InputType"]?.ToString().ToLower() == "dropdown")
                      || FilterComponents.Any(x => x["InputType"]?.ToString().ToLower() == "time" && x["InputTimeType"]?.ToString().ToLower() == "year"
                      || FilterComponents.Any(x => x["InputType"]?.ToString().ToLower() == "time" && x["InputTimeType"]?.ToString().ToLower() == "month"));

    if (hasDropdown == true)
    {
      FilterDropdowns = [];
    }
    else return;

    foreach (var filterComponent in FilterComponents)
    {
      if (filterComponent["InputType"]?.ToString().ToLower() == "dropdown")
      {
        var id = filterComponent["ID"]?.ToString() ?? "";

        if (!FilterDropdowns.ContainsKey(id))
        {
          FilterDropdowns[id] = new FormFieldDropdown<string>();
        }
      }
    }
  }
  #endregion

  #region SetFilterDropdownValues()
  public async Task SetFilterDropdownValues()
  {
    var hasEmptyFilterValue = FilterComponents
                                .Where(x => x["InputType"]?.GetValue<string>() == "dropdown")
                                .Any(x => x["FilterValueCounter"]?.GetValue<int>() < 1);
    if (hasEmptyFilterValue)
    {
      throw new ArgumentException("Please add dropdown filter value at least 1 option at config (IFINSYS).");
    }

    List<string> DropdownComponentIDs = FilterComponents
                                        .Where(x => x["InputType"]?.GetValue<string>() == "dropdown")
                                        .Select(x => x["ID"]?.ToString() ?? "")
                                        .ToList();

    if (DropdownComponentIDs.Count < 1) return;

    foreach (var filterID in DropdownComponentIDs)
    {
      var res = await IFINSYSClient.GetRows<JsonObject>("MasterDashboardFilterValue", "GetRowsForModules", new { MasterDashboardFilterID = filterID });

      if (res?.Data != null)
      {
        if (res.Data.Count > 0)
        {
          FilterOptions[filterID] = res.Data.ToDictionary(
            x => x["Description"]?.ToString()!,
            x => x["Value"]?.ToString()!
          );
        }
        else
        {
          FilterOptions[filterID] = new Dictionary<string, string>() {
            {"Default Value", "Default Value"}
          };
        }
      }
      else
      {
        FilterOptions[filterID] = new Dictionary<string, string>() {
          {"Default Value", "Default Value"}
        };
      }
    }
  }
  #endregion

  #region GetLookupItems()
  public void GetLookupItems(DataGridLoadArgs args, JsonObject FilterComponent)
  {
    string moduleUri = Env.GetString(FilterComponent["ModuleCode"]?.GetValue<string>());

    if (moduleUri.EndsWith('/')) moduleUri = moduleUri.TrimEnd('/');

    string uri = $"{moduleUri}/{FilterComponent["APIName"]?.GetValue<string>()}";
  }
  #endregion

  #region FilterApply
  public void FilterApply()
  {
    IsFilterExpand = false;
  }
  #endregion

  #region FilterReset
  public void FilterReset()
  {
    IsFilterExpand = false;

    FilterNameDates.Clear(); // Reset FilterNameDates
    SetInitialFilterValues(forceReset: true); // Paksa reset karena ini adalah fungsi reset
  }
  #endregion



  #region RegisterBlazorInterop()
  private async Task RegisterBlazorInterop()
  {
    try
    {
      BlazorInstanceRef = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("registerBlazorInstance", id, BlazorInstanceRef);
    }
    catch (Exception)
    {
      Errors.Add("Failed to register Blazor Interop");
      throw;
    }
  }
  #endregion

  #region LoadDataForLookup()
  public async Task<List<JsonObject>> LoadDataForLookup(DataGridLoadArgs args, JsonObject DashboardFilter)
  {
    if (DashboardFilter["DataSourceType"]?.ToString() == "custom_value")
    {
      var res = await IFINSYSClient.GetRows<JsonObject>("MasterDashboardFilterValue", "GetRowsForModules", new { MasterDashboardFilterID = DashboardFilter["ID"]?.ToString() });

      return res?.Data?
              .Where(x =>
              {
                return (x["Code"]?.GetValue<string>() ?? "").Contains(args.Keyword ?? "") || (x["Description"]?.GetValue<string>() ?? "").Contains(args.Keyword ?? "");
              })
              .ToList() ?? [];
    }

    return [];
  }
  #endregion

  #region PickARandomColor()
  private List<string> PickRandom()
  {
    var randomNum = new Random().Next(0, 10);

    var colorArr = new List<string>() { "#FFD700"
      ,"#FF0000"
      ,"#00BFFF"
      ,"#ADFF2F"
      ,"#FF4500"
      ,"#FF1493"
      ,"#FFFFFF"
      ,"#00FFFF"
      ,"#8A2BE2"
      ,"#32CD32"
    };

    var iconArr = new List<string>() {
      "currency_bitcoin",
      "shopping_cart",
      "money_bag",
      "attach_money",
      "assured_workload",
      "credit_score",
      "paid",
      "euro_symbol",
      "attach_money",
      "redeem"
    };

    return [colorArr[randomNum], iconArr[randomNum]];
  }
  #endregion

  #region GetChartSize()
  protected int GetChartSize(string size, string breakpoint) => size.ToUpper() switch
  {
    "QUARTER" => 3,
    "THREE-QUARTERS" => 4,
    "HALF" => 6,
    "FULL" => 12,
    _ => 12 // Default case untuk nilai yang tidak dikenal
    // "QUARTER" => breakpoint == "md" ? 4 : 3,
    // "HALF" => breakpoint == "md" ? 8 : 6,
    // "THREE-QUARTERS" => breakpoint == "md" ? 12 : 9,
    // "FULL" => 12,
  };
  #endregion




  #region AddFilters() - unused
  public void AddFilters(FormField formField)
  {
    FormFieldFilters.Add(formField);
  }
  #endregion

  #region RemoveFilters() - unused
  public void RemoveFilters(FormField formField)
  {
    FormFieldFilters.Remove(formField);
  }
  #endregion

  #region SetFiltersValues - unused()
  public void SetFiltersValues()
  {
    foreach (var formField in FormFieldFilters)
    {
      var name = formField.Name;
      var jsonString = JsonSerializer.Serialize(formField.GetValue());

      if (Filter.ContainsKey(name))
      {
        Filter[name] = JsonNode.Parse(jsonString)!;
      }
      else
      {
        Filter.Add(name, JsonNode.Parse(jsonString)!);
      }
    }
  }
  #endregion
}
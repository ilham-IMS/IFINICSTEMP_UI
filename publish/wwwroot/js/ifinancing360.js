let charts = {}, chartInstances = {}, chartOrderKeys = {}, originalChartData = {}, drilldownHistory = {};
const limitData = 8;

console.log('ifinancing360.js loaded');

// Function untuk membatasi data dan menggabungkan sisanya menjadi other
const limitDataItem = (data, limitData) => {
    console.log('limitDataItem called with:', { dataLength: data?.length || 0, limitData, dataType: typeof data, isArray: Array.isArray(data), firstItem: data?.[0] || null });

    if (!data || data.length <= limitData) return data;
    if (!Array.isArray(data) || !data.length || typeof data[0] !== 'object' || !('y' in data[0])) return data;

    // Keep original order from API - no sorting
    const topData = data.slice(0, limitData);
    const rest = data.slice(limitData);
    const otherTotal = rest.reduce((sum, item) => sum + item.y, 0);
    const otherTooltip = rest.map(item => `${item.name}: ${item.y}`).join('<br>');

    if (rest.length > 0) {
        topData.push({ name: 'Other', y: otherTotal, customTooltip: otherTooltip, color: '#cccccc' });
    }
    return topData;
};

// Function helper untuk menerapkan limitData secara aman
const applyDataLimit = (data, limitData) => {
    return Array.isArray(data) && data.length > 0 && typeof data[0] === 'object' && 'y' in data[0]
        ? limitDataItem(data, limitData)
        : data;
};

function registerBlazorInstance(chartId, instance) {
    chartInstances[chartId] = instance;
    chartOrderKeys[chartId] = 0;
    drilldownHistory[chartId] = {};
}

const getData = async (chartId, drilldownID, orderKey) => {
    const blazorInstance = chartInstances[chartId];
    if (blazorInstance) {
        try {
            return await blazorInstance.invokeMethodAsync("GetChildData", drilldownID, orderKey);
        } catch (err) {
            console.error(`Error calling Blazor method for chart ${chartId}: ${err}`);
        }
    } else {
        console.error(`Blazor instance is not registered for chart ${chartId}!`);
    }
};

const getSeries = (chartType, chartTitle, categories, data, series, limitData, scaleValueFormat, scaleLabelFormat) => {
    // Determine scale
    let scale;
    if (scaleValueFormat && scaleValueFormat !== "AUTO") {
        scale = getScaleFromFormat(scaleValueFormat, scaleLabelFormat);
    } else {
        const allValues = extractAllValues(data, series, categories);
        scale = getDataScale(allValues);
    }

    // Calculate interval helper
    const calculateInterval = (values) => {
        if (!values?.length) return { interval: 1, maxValue: 5 };
        const maxValue = Math.max(...values);

        // For small values (≤ 10), use simple integer intervals starting from 0
        if (maxValue <= 10) {
            const niceMax = Math.ceil(maxValue);
            const interval = niceMax <= 5 ? 1 : Math.ceil(niceMax / 5);
            const adjustedMax = Math.max(5, Math.ceil(niceMax / interval) * interval);
            return { interval: interval, maxValue: adjustedMax };
        }

        const roundToNiceNumber = (num) => {
            if (num === 0) return 5;
            const magnitude = Math.pow(10, Math.floor(Math.log10(Math.abs(num))));
            const normalized = num / magnitude;
            let roundedNormalized;
            if (normalized <= 1) roundedNormalized = 1;
            else if (normalized <= 2) roundedNormalized = 2;
            else if (normalized <= 5) roundedNormalized = 5;
            else roundedNormalized = 10;
            return roundedNormalized * magnitude;
        };

        const roundedMax = roundToNiceNumber(maxValue);
        return { interval: roundedMax / 5, maxValue: roundedMax };
    };

    // BAR/COLUMN processing
    if (chartType == "bar" || chartType == "column") {
        if (data?.length > 0) {
            if (!categories?.length) throw 'Categories wajib diisi ketika menggunakan data array';
            const rawData = [...data];
            data = categories.map((categoryName, i) => ({ name: categoryName, y: rawData[i], originalValue: rawData[i] }));
            data = applyScaleToData(data, scale);

            if (!scale.tickInterval) {
                const intervalResult = calculateInterval(data.map(item => item.y));
                scale.tickInterval = intervalResult.interval;
                scale.maxValue = intervalResult.maxValue;
            }

            data = limitDataItem(data, limitData);
            series = [{ colorByPoint: true, data, scale }];
        } else if (series?.length > 0) {
            series = applyScaleToSeries(series, scale);

            if (!scale.tickInterval) {
                const allValues = [];
                series.forEach(s => {
                    if (Array.isArray(s.data)) {
                        s.data.forEach(item => {
                            if (typeof item === 'number') allValues.push(item);
                            else if (typeof item === 'object' && item.y !== undefined) allValues.push(item.y);
                        });
                    }
                });
                if (allValues.length > 0) {
                    const intervalResult = calculateInterval(allValues);
                    scale.tickInterval = intervalResult.interval;
                    scale.maxValue = intervalResult.maxValue;
                }
            }

            const hasMultipleDataPoints = series.some(s => s.data?.length > 1);

            if (hasMultipleDataPoints) {
                series = series.map(({ name, data, drilldownID }) => ({
                    name, data, scale,
                    ...(drilldownID ? { drilldown: drilldownID } : {})
                }));
            } else {
                const tempData = series.map(tempSeries => ({
                    name: tempSeries.name,
                    y: tempSeries.data[0],
                    originalValue: tempSeries.originalValue || tempSeries.data[0],
                    ...(tempSeries.drilldownID ? { drilldown: tempSeries.drilldownID } : {})
                }));
                series = [{ colorByPoint: true, data: limitDataItem(tempData, limitData), scale }];
            }
        } else {
            return [{ name: 'NO DATA', colorByPoint: true, data: [{ name: 'NO DATA', y: 0, color: '#cccccc' }], scale }];
        }
    }

    // LINE/SPLINE processing
    if (chartType == "line" || chartType == "spline") {
        if (categories?.length > 0 && data?.length > 0) {
            const tempData = [...data];
            data = tempData.map((x, i) => ({ name: categories[i], y: x, originalValue: x }));
            data = applyScaleToData(data, scale);

            if (!scale.tickInterval) {
                const intervalResult = calculateInterval(data.map(item => item.y));
                scale.tickInterval = intervalResult.interval;
                scale.maxValue = intervalResult.maxValue;
            }

            data = limitDataItem(data, limitData);
            series = [{ data, scale }];
        } else if (series?.length > 0) {
            series = applyScaleToSeries(series, scale);

            if (!scale.tickInterval) {
                const allValues = [];
                series.forEach(s => {
                    if (Array.isArray(s.data)) {
                        s.data.forEach(item => {
                            if (typeof item === 'number') allValues.push(item);
                            else if (typeof item === 'object' && item.y !== undefined) allValues.push(item.y);
                        });
                    }
                });
                if (allValues.length > 0) {
                    const intervalResult = calculateInterval(allValues);
                    scale.tickInterval = intervalResult.interval;
                    scale.maxValue = intervalResult.maxValue;
                }
            }

            if (series.length === 1) {
                const singleSeries = series[0];
                const processedData = applyDataLimit(singleSeries.data, limitData);
                series = [{
                    name: singleSeries.name,
                    data: processedData,
                    scale,
                    ...(singleSeries.drilldownID ? { drilldown: singleSeries.drilldownID } : {})
                }];
            } else {
                series = series.map(({ name, data, drilldownID }) => ({
                    name,
                    data: applyDataLimit(data, limitData),
                    scale,
                    ...(drilldownID ? { drilldown: drilldownID } : {})
                }));
            }
        }
    }

    // PIE processing
    if (chartType == "pie") {
        let pieData = [];

        if (categories?.length > 0 && data?.length > 0) {
            const tempData = [...data];
            pieData = tempData.map((x, i) => ({ name: categories[i], y: x, originalValue: x }));
            pieData = limitDataItem(pieData, limitData);
        } else if (series?.length > 0) {
            pieData = series.map(({ name, data, drilldownID }) => ({
                name,
                y: data[0],
                originalValue: data[0],
                ...(drilldownID ? { drilldown: drilldownID } : {})
            }));
            pieData = limitDataItem(pieData, limitData);
        }

        pieData = pieData.filter(item => item && typeof item.y === 'number' && !isNaN(item.y) && item.y > 0);
        if (!pieData.length) console.warn('No valid positive data found for pie chart');

        series = [{ name: null, colorByPoint: true, data: pieData, scale }];
    }

    return series;
};

// Drilldown handlers
const handleDrilldown = async (event, chartId) => {
    const chart = charts[chartId];
    chart.showLoading("Loading...");

    if (!event.seriesOptions) {
        if (!chartOrderKeys[chartId]) chartOrderKeys[chartId] = 0;
        chartOrderKeys[chartId]++;

        const seriesData = getData(chartId, event.point.drilldown, chartOrderKeys[chartId]);
        let tempData = {};

        await seriesData
            .then((result) => {
                if (!result?.option) {
                    console.warn(`No drilldown data for chart ${chartId}, drilldown: ${event.point.drilldown}`);
                    chartOrderKeys[chartId]--;
                    chart.hideLoading();
                    return;
                }

                const { option } = result;
                tempData = option;
                const drilldownChartType = option.drilldownChartType;

                const parentSeries = chart.series[0];
                const scale = parentSeries?.options.scale || { divider: 1, label: '', suffix: '' };

                const currentLevel = chartOrderKeys[chartId] - 1;
                if (!drilldownHistory[chartId]) drilldownHistory[chartId] = {};

                if (currentLevel >= 0 && !drilldownHistory[chartId][currentLevel]) {
                    drilldownHistory[chartId][currentLevel] = [...chart.xAxis[0].categories || []];
                }

                if (option.categories?.length > 0) {
                    chart.xAxis[0].setCategories(option.categories);
                } else {
                    const newCategories = option.series.map(s => s.name);
                    chart.xAxis[0].setCategories(newCategories);
                }

                tempData.data = tempData.series.map(({ name, data, drilldownID }) => {
                    const originalValue = data[0];
                    const scaledValue = scale.divider > 1 ? originalValue / scale.divider : originalValue;

                    return drilldownID
                        ? { name, y: scaledValue, originalValue, drilldown: drilldownID }
                        : { name, y: scaledValue, originalValue };
                });

                if (drilldownChartType) {
                    tempData.type = drilldownChartType;
                    tempData.colorByPoint = true;
                    tempData.name = 'Drilldown Data';
                    tempData.scale = scale;
                } else {
                    tempData.colorByPoint = true;
                    tempData.name = 'Drilldown Data';
                    tempData.scale = scale;
                }

                chart.addSeriesAsDrilldown(event.point, tempData);

                // Drilldown axis adjustment
                try {
                    if (chart.yAxis?.length > 0) {
                        const drillValues = (tempData.data || [])
                            .map(d => (d && typeof d.y === 'number' ? d.y : null))
                            .filter(v => v !== null && !isNaN(v));

                        if (drillValues.length > 0) {
                            const rawMax = Math.max(...drillValues);

                            let niceMax, tickInterval;

                            // For small values (≤ 10), use simple integer intervals starting from 0
                            if (rawMax <= 10) {
                                niceMax = Math.ceil(rawMax);
                                tickInterval = niceMax <= 5 ? 1 : Math.ceil(niceMax / 5);
                                niceMax = Math.max(5, Math.ceil(niceMax / tickInterval) * tickInterval);
                            } else {
                                const roundToNiceNumber = (num) => {
                                    if (num === 0) return 5;
                                    const magnitude = Math.pow(10, Math.floor(Math.log10(Math.abs(num))));
                                    const candidates = [1, 2, 2.5, 5, 10].map(base => base * magnitude);
                                    let nice = candidates.find(c => c >= num);
                                    if (!nice) nice = 10 * magnitude;
                                    return nice;
                                };

                                niceMax = roundToNiceNumber(rawMax);
                                tickInterval = niceMax / 5;
                            }

                            chart.yAxis[0].update({
                                min: 0,
                                max: niceMax,
                                tickInterval,
                                labels: chart.yAxis[0].options.labels
                            }, false);

                            chart.redraw();
                        }
                    }
                } catch (axisErr) {
                    console.warn(`Failed adjusting drilldown axis for chart ${chartId}:`, axisErr);
                }
            })
            .catch((err) => {
                console.error(`Failed get data for chart ${chartId}!\n\nError: ${err}`);
                chartOrderKeys[chartId]--;
            });
    }

    chart.hideLoading();
};

const handleDrillup = (chartId) => {
    if (chartOrderKeys[chartId] && chartOrderKeys[chartId] > 0) {
        chartOrderKeys[chartId]--;
        const chart = charts[chartId];

        if (drilldownHistory[chartId]?.[chartOrderKeys[chartId]]) {
            const categoriesToRestore = drilldownHistory[chartId][chartOrderKeys[chartId]];
            chart.xAxis[0].setCategories(categoriesToRestore);

            const higherLevels = Object.keys(drilldownHistory[chartId])
                .map(Number)
                .filter(level => level > chartOrderKeys[chartId]);

            higherLevels.forEach(level => {
                delete drilldownHistory[chartId][level];
            });
        }
    }

    if (chartOrderKeys[chartId] === 0) {
        setTimeout(() => {
            const originalData = originalChartData[chartId];
            if (originalData) {
                const options = getOptions(
                    chartId,
                    originalData.chartType,
                    originalData.name,
                    originalData.categories,
                    originalData.data,
                    originalData.series,
                    originalData.limitData || limitData,
                    originalData.nameDate,
                    originalData.scaleValueFormat,
                    originalData.scaleLabelFormat,
                    originalData.thresholdLabel,
                    originalData.thresholdValue,
                    originalData.thresholdColor,
                    originalData.isStacked
                );

                if (charts[chartId]) charts[chartId].destroy();
                charts[chartId] = Highcharts.chart(chartId, options);
                drilldownHistory[chartId] = {};
            }
        }, 100);
    }
};

const getOptions = (id, chartType, chartTitle, categories, data, series, limitData, nameDate, scaleValueFormat, scaleLabelFormat, thresholdLabel, thresholdValue, thresholdColor, isStacked) => {

    // Debug threshold only when values are present
    if (thresholdValue !== null && thresholdValue !== undefined) {
        console.log(`Chart ${id} (${chartType}): Threshold Value = ${thresholdValue}, Color = ${thresholdColor}, Label = ${thresholdLabel}`);
    }

    // Stacked scale calculation
    const calculateStackedScale = (series, categories) => {
        if (!series?.length || !categories?.length) return { interval: 5, maxValue: 25 };

        const categoryTotals = [];
        for (let categoryIndex = 0; categoryIndex < categories.length; categoryIndex++) {
            let categoryTotal = 0;
            series.forEach(seriesItem => {
                if (seriesItem.data?.length > categoryIndex) {
                    const value = typeof seriesItem.data[categoryIndex] === 'object'
                        ? seriesItem.data[categoryIndex].y || 0
                        : seriesItem.data[categoryIndex] || 0;
                    categoryTotal += value;
                }
            });
            categoryTotals.push(categoryTotal);
        }

        const maxTotal = Math.max(...categoryTotals);

        const roundToNiceNumber = (num) => {
            if (num === 0) return 10;
            const magnitude = Math.pow(10, Math.floor(Math.log10(Math.abs(num))));
            const baseNumbers = [1, 2, 2.5, 5, 10];
            const candidates = [];
            baseNumbers.forEach(base => {
                const candidate = base * magnitude;
                if (candidate >= num) candidates.push(candidate);
                const higherCandidate = base * magnitude * 10;
                if (higherCandidate >= num) candidates.push(higherCandidate);
            });
            return Math.min(...candidates);
        };

        const roundedMax = roundToNiceNumber(maxTotal);
        return { interval: roundedMax / 5, maxValue: roundedMax };
    };

    let generatedSeries = getSeries(chartType, chartTitle, categories, data, series, limitData, scaleValueFormat, scaleLabelFormat);
    let scale = generatedSeries?.[0]?.scale || { divider: 1, label: '', suffix: '' };

    // Stacked chart scale recalculation
    if (isStacked === 1 && (chartType === "column" || chartType === "bar") && categories) {
        const originalSeries = series || [];
        if (originalSeries.length > 0) {
            const stackedScale = calculateStackedScale(originalSeries, categories);
            scale.tickInterval = stackedScale.interval;
            scale.maxValue = stackedScale.maxValue;
        }
    }

    const exportTitle = nameDate ? `${chartTitle || 'Chart'} (${nameDate.toUpperCase()})` : (chartTitle || 'Chart');

    // Common chart options
    const commonOptions = {
        title: { text: null },
        credits: { enabled: false },
        exporting: {
            enabled: true,
            filename: exportTitle.replace(/\s+/g, '_').toLowerCase(),
            chartOptions: {
                title: { text: exportTitle, style: { fontSize: '16px', fontWeight: 'bold' } },
                subtitle: { text: null }
            },
            buttons: {
                contextButton: {
                    menuItems: [
                        'downloadPNG', 'downloadJPEG', 'downloadSVG', 'separator',
                        { textKey: 'downloadCSV', text: 'Download CSV', onclick: function () { exportToCSV(this, exportTitle); } },
                        { textKey: 'downloadXLSX', text: 'Download XLSX', onclick: function () { exportToExcel(this, exportTitle); } }
                    ]
                }
            },
            csv: {
                columnHeaderFormatter: function (item, key) {
                    return !item || item instanceof Highcharts.Axis ? "CATEGORIES" : (item.name ? item.name.replace(/\s+/g, '_') : "DATA");
                }
            },
            beforeExport: function () {
                if (exportTitle) this.setTitle({ text: exportTitle, style: { fontSize: '16px', fontWeight: 'bold' } });
            },
            afterExport: function () { this.setTitle({ text: null }); }
        },
        tooltip: {
            useHTML: true,
            formatter: function () {
                let pointName = this.point.name || this.point.category;
                if (!pointName && this.x !== undefined && this.series.chart.xAxis?.[0]?.categories) {
                    pointName = this.series.chart.xAxis[0].categories[this.x];
                }

                if (pointName === 'Other' && this.point.customTooltip) {
                    let percentageText = typeof this.point.percentage !== "undefined" ? ' (' + this.point.percentage.toFixed(6) + '%)' : '';
                    let formattedTooltip = this.point.customTooltip.replace(/: (\d+(?:\.\d+)?)/g, (match, num) => {
                        let parts = num.split('.');
                        let integerPart = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ".");
                        let decimalPart = parts[1] ? ',' + parts[1] : '';
                        return ': ' + integerPart + decimalPart;
                    });
                    return '<b>Other ' + percentageText + '</b><br>' + formattedTooltip;
                } else {
                    let percentageText = typeof this.point.percentage !== "undefined" ? ' (' + this.point.percentage.toFixed(6) + '%)' : '';
                    let displayName = pointName || 'Unknown';

                    let formattedValue;
                    const scale = this.series.options.scale;

                    if (this.point.originalValue !== undefined) {
                        formattedValue = formatNumber(this.point.originalValue, 2);
                    } else if (scale?.divider > 1) {
                        formattedValue = formatNumber(this.y, 2) + scale.suffix;
                    } else {
                        formattedValue = formatNumber(this.y, 2);
                    }

                    let seriesName = this.series.name;
                    if (this.series.options.colorByPoint && pointName) seriesName = pointName;
                    if ((seriesName === 'Series 1' || seriesName === 'Drilldown Series') && pointName) seriesName = pointName;

                    return '<b>' + displayName + percentageText + '</b><br/>' +
                        '<span style="color:' + this.series.color + '">' + seriesName + '</span>: ' +
                        formattedValue;
                }
            }
        }
    };

    switch (chartType) {
        case "line":
        case "spline":
        case "bar":
        case "column":
            let option = {
                ...commonOptions,
                chart: {
                    type: chartType,
                    events: {
                        drilldown: (event) => handleDrilldown(event, id),
                        drillup: () => handleDrillup(id)
                    }
                },
                xAxis: {
                    labels: {
                        useHTML: false,
                        style: { cursor: 'default', textDecoration: 'none', color: '#000000' }
                    },
                    events: { click: (e) => { e.preventDefault(); return false; } }
                },
                legend: {},
                yAxis: {
                    title: {
                        text: buildScaleTitle(scale, scaleLabelFormat, scaleValueFormat),
                        style: { fontSize: '12px', fontWeight: 'normal', color: '#000000' }
                    },
                    min: 0,
                    tickInterval: scale.tickInterval || (chartType === 'bar' ? 4 : chartType === 'column' ? 8 : 5),
                    max: scale.maxValue || undefined,
                    allowDecimals: true,
                    labels: {
                        formatter: function () {
                            return scale?.divider > 1 ? formatNumber(this.value, 2) + scale.suffix : formatNumber(this.value, 2);
                        },
                        style: { fontSize: '11px', color: '#000000' }
                    },
                    plotLines: (() => {
                        if (thresholdValue !== null && thresholdValue !== undefined && thresholdValue !== '') {
                            const plotLineValue = scale?.divider > 1 ? thresholdValue / scale.divider : thresholdValue;
                            console.log(`Creating plotLine for chart ${id}: value=${plotLineValue} (original=${thresholdValue})`);

                            // Format threshold value dengan skala yang sesuai
                            const formatThresholdValue = (value) => {
                                if (value >= 1000000000000) {
                                    return (value / 1000000000000).toFixed(1).replace(/\.0$/, '') + 'T';
                                } else if (value >= 1000000000) {
                                    return (value / 1000000000).toFixed(1).replace(/\.0$/, '') + 'B';
                                } else if (value >= 1000000) {
                                    return (value / 1000000).toFixed(1).replace(/\.0$/, '') + 'M';
                                } else if (value >= 1000) {
                                    return (value / 1000).toFixed(1).replace(/\.0$/, '') + 'K';
                                } else {
                                    return value.toString();
                                }
                            };

                            const formattedThresholdValue = formatThresholdValue(thresholdValue);
                            const capitalizedLabel = (thresholdLabel || 'THRESHOLD').toUpperCase();
                            const labelText = `${capitalizedLabel}: ${formattedThresholdValue}`;

                            return [{
                                value: plotLineValue,
                                color: thresholdColor || '#ff0000',
                                width: 2,
                                zIndex: 4,
                                label: {
                                    text: labelText,
                                    style: {
                                        color: thresholdColor || '#ff0000',
                                        fontWeight: 'bold',
                                        fontSize: '11px'
                                    }
                                }
                            }];
                        }
                        return [];
                    })()
                },
                plotOptions: {
                    series: {
                        borderWidth: 0,
                        dataLabels: {
                            enabled: true,
                            style: { color: '#000000', fontWeight: 'bold' },
                            formatter: function () {
                                const scale = this.series.options.scale;
                                const shouldUseComma = scaleValueFormat &&
                                    scaleValueFormat.toUpperCase() != "AUTO" &&
                                    scaleValueFormat.toUpperCase() != "IN PERCENTAGE" &&
                                    scaleValueFormat.toUpperCase() != "IN NUMBER";

                                let value = this.y;
                                if (shouldUseComma) {
                                    let formattedValue = formatNumber(value, 1);
                                    if (scale?.suffix) formattedValue += scale.suffix;
                                    return formattedValue;
                                } else {
                                    let formattedValue = value.toFixed(0);
                                    if (scale?.suffix) formattedValue += scale.suffix;
                                    return formattedValue;
                                }
                            }
                        }
                    },
                    column: { stacking: isStacked === 1 ? 'normal' : undefined }
                },
                series: generatedSeries,
                drilldown: { series: [] }
            };

            // Set categories or type
            if (!categories) {
                option.xAxis.type = "category";
            } else {
                option.xAxis.categories = categories;
            }

            // Legend visibility
            option.legend.enabled = !(generatedSeries.length === 1 ||
                (generatedSeries.length === 1 && generatedSeries[0].colorByPoint));

            return option;

        case "pie":
            return {
                ...commonOptions,
                chart: {
                    type: "pie",
                    events: {
                        drilldown: (event) => handleDrilldown(event, id),
                        drillup: () => handleDrillup(id)
                    }
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: true,
                        cursor: "pointer",
                        borderRadius: 8,
                        
                        dataLabels: [{
                            enabled: true,
                            distance: 6,
                            formatter: function () {
                                const percentage = this.point.percentage.toFixed(2);
                                return '<span style="color: black; text-decoration: none;"><b>' + this.point.name + '</b></span><br>' +
                                    '<span style="opacity: 0.6; color: black; text-decoration: none;">' + percentage + '%</span>';
                            },
                            connectorColor: "rgba(128,128,128,0.5)"
                        }],
                        showInLegend: false
                    }
                },
                series: generatedSeries
            };

        default:
            return {};
    }
};

function initChart(id, chartType, name, categories, data, series, nameDate, scaleValueFormat, scaleLabelFormat, thresholdLabel, thresholdValue, thresholdColor, isStacked) {
    let isSuccess = true;

    try {
        const effectiveLimitData = 8;
        chartOrderKeys[id] = 0;
        drilldownHistory[id] = {};

        originalChartData[id] = {
            chartType, name, categories, data, series,
            limitData: effectiveLimitData, nameDate, scaleValueFormat, scaleLabelFormat,
            thresholdLabel, thresholdValue, thresholdColor, isStacked
        };

        const tryInitChart = (retryCount = 0) => {
            try {
                const element = document.getElementById(id);

                if (!element) {
                    if (retryCount < 5) {
                        setTimeout(() => tryInitChart(retryCount + 1), 100);
                        return;
                    } else {
                        console.error(`Element with ID '${id}' not found after 5 retries`);
                        isSuccess = false;
                        return;
                    }
                }

                if (chartType === "pie") {
                    console.log(`=== PIE CHART DEBUG for ${id} ===`);
                    console.log(`Categories:`, categories, `Data:`, data, `Series:`, series);
                    console.log(`=== END PIE CHART DEBUG ===`);
                }

                const options = getOptions(id, chartType, name, categories, data, series, effectiveLimitData, nameDate, scaleValueFormat, scaleLabelFormat, thresholdLabel, thresholdValue, thresholdColor, isStacked);
                if (!options || !Object.keys(options).length) {
                    throw new Error("Chart options not generated properly");
                }

                if (chartType === "pie" && options.series?.[0]) {
                    const pieSeriesData = options.series[0].data;
                    if (!pieSeriesData?.length) {
                        options.series = [{ name: null, colorByPoint: true, data: [{ name: 'NO DATA', y: 0, color: '#cccccc' }] }];
                    } else {
                        const validDataPoints = pieSeriesData.filter(point =>
                            point && typeof point.y === 'number' && !isNaN(point.y) && point.y > 0);
                        if (!validDataPoints.length) {
                            console.error(`Pie chart ${id} has no valid data points!`);
                            isSuccess = false;
                            return;
                        }
                    }
                }

                charts[id] = Highcharts.chart(id, options);
                isSuccess = true;

            } catch (error) {
                console.error(`Error creating chart ${id}:`, error);
                isSuccess = false;
            }
        };

        tryInitChart();
    } catch (error) {
        console.error(`Error in initChart for ${id}:`, error);
        isSuccess = false;
    }

    return isSuccess;
}

function updateChart(id, chartType, name, categories, data, series, nameDate, scaleValueFormat, scaleLabelFormat, thresholdLabel, thresholdValue, thresholdColor, isStacked) {
    try {
        if (originalChartData[id]) {
            originalChartData[id] = {
                chartType, name, categories, data, series,
                limitData: originalChartData[id].limitData || 8,
                nameDate, scaleValueFormat, scaleLabelFormat,
                thresholdLabel, thresholdValue, thresholdColor, isStacked
            };
        }

        if (charts[id]) {
            const newOptions = getOptions(id, chartType, name, categories, data, series, 8, nameDate, scaleValueFormat, scaleLabelFormat, thresholdLabel, thresholdValue, thresholdColor, isStacked);
            charts[id].destroy();
            charts[id] = Highcharts.chart(id, newOptions);
            return true;
        } else {
            return initChart(id, chartType, name, categories, data, series, nameDate, scaleValueFormat, scaleLabelFormat, thresholdLabel, thresholdValue, thresholdColor, isStacked);
        }
    } catch (error) {
        console.error(`Error updating chart ${id}:`, error);
        return false;
    }
}

function removeChart(id) {
    try {
        if (charts[id]) charts[id].destroy();
        delete charts[id];
        delete chartOrderKeys[id];
        delete drilldownHistory[id];
        delete originalChartData[id];
        delete chartInstances[id];
    } catch (error) {
        console.error(`Error removing chart ${id}:`, error);
    }
}

// Utility functions
function formatNumber(value, decimals = 2) {
    if (value == null || value === undefined || isNaN(value)) return '0';
    return new Intl.NumberFormat('id-ID', {
        minimumFractionDigits: 0,
        maximumFractionDigits: decimals
    }).format(value);
}

function extractAllValues(data, series, categories) {
    const values = [];
    try {
        if (Array.isArray(data)) {
            data.forEach(item => {
                if (typeof item === 'number') values.push(item);
                else if (typeof item === 'object' && item.y !== undefined) values.push(item.y);
            });
        }
        if (Array.isArray(series)) {
            series.forEach(seriesItem => {
                if (Array.isArray(seriesItem.data)) {
                    seriesItem.data.forEach(dataPoint => {
                        if (typeof dataPoint === 'number') values.push(dataPoint);
                        else if (typeof dataPoint === 'object' && dataPoint.y !== undefined) values.push(dataPoint.y);
                    });
                }
            });
        }
    } catch (error) {
        console.warn('Error extracting values:', error);
    }
    return values.filter(v => !isNaN(v) && v !== null && v !== undefined);
}

function getScaleFromFormat(scaleValueFormat) {
    const scales = {
        "IN NUMBER": { divider: 1, suffix: '' },
        "IN PERCENTAGE": { divider: 1, suffix: '%' },
        "IN THOUSAND": { divider: 1000, suffix: 'K' },
        "IN MILLION": { divider: 1000000, suffix: 'M' },
        "IN TEN-MILLION": { divider: 10000000, suffix: '10M' },
        "IN HUNDRED-MILLION": { divider: 100000000, suffix: '100M' },
        "IN BILLION": { divider: 1000000000, suffix: 'B' },
        "IN TRILLION": { divider: 1000000000000, suffix: 'T' }
    };
    return scales[scaleValueFormat] || { divider: 1, suffix: '' };
}

function buildScaleTitle(scale, scaleLabelFormat, scaleValueFormat) {
    if (!scale || !scaleValueFormat || scaleValueFormat === "AUTO" || scaleValueFormat === "IN NUMBER") {
        return scaleLabelFormat || ' ';
    }
    return `${scaleLabelFormat} (${scaleValueFormat})`;
}

function getDataScale(values) {
    if (!Array.isArray(values) || !values.length) return { divider: 1, label: '', suffix: '' };
    const maxValue = Math.max(...values);
    if (maxValue >= 1000000000) return { divider: 1000000000, label: 'Miliar', suffix: 'B' };
    else if (maxValue >= 1000000) return { divider: 1000000, label: 'Juta', suffix: 'M' };
    else if (maxValue >= 1000) return { divider: 1000, label: 'Ribu', suffix: 'K' };
    return { divider: 1, label: '', suffix: '' };
}

function applyScaleToData(data, scale) {
    if (!scale || scale.divider <= 1 || !Array.isArray(data)) return data;

    const scaledData = data.map(item => {
        if (typeof item === 'object' && item.y !== undefined) {
            return { ...item, y: item.y / scale.divider };
        }
        return item;
    });

    const maxValue = Math.max(...scaledData.map(item =>
        typeof item === 'object' && item.y !== undefined ? item.y : 0));

    const roundToNiceNumber = (num) => {
        if (num === 0) return 5;
        const magnitude = Math.pow(10, Math.floor(Math.log10(Math.abs(num))));
        const normalized = num / magnitude;
        let roundedNormalized;
        if (normalized <= 1) roundedNormalized = 1;
        else if (normalized <= 2) roundedNormalized = 2;
        else if (normalized <= 5) roundedNormalized = 5;
        else roundedNormalized = 10;
        return roundedNormalized * magnitude;
    };

    const roundedMax = roundToNiceNumber(maxValue);
    scale.tickInterval = roundedMax / 5;
    scale.maxValue = roundedMax;
    return scaledData;
}

function applyScaleToSeries(series, scale) {
    if (!scale || scale.divider <= 1 || !Array.isArray(series)) return series;

    const scaledSeries = series.map(seriesItem => {
        if (Array.isArray(seriesItem.data)) {
            return {
                ...seriesItem,
                data: seriesItem.data.map(dataPoint => {
                    if (typeof dataPoint === 'number') return dataPoint / scale.divider;
                    else if (typeof dataPoint === 'object' && dataPoint.y !== undefined) {
                        return { ...dataPoint, y: dataPoint.y / scale.divider };
                    }
                    return dataPoint;
                })
            };
        }
        return seriesItem;
    });

    let maxValue = 0;
    scaledSeries.forEach(seriesItem => {
        if (Array.isArray(seriesItem.data)) {
            const seriesMax = Math.max(...seriesItem.data.map(dataPoint => {
                if (typeof dataPoint === 'number') return dataPoint;
                else if (typeof dataPoint === 'object' && dataPoint.y !== undefined) return dataPoint.y;
                return 0;
            }));
            maxValue = Math.max(maxValue, seriesMax);
        }
    });

    const roundToNiceNumber = (num) => {
        if (num === 0) return 5;
        const magnitude = Math.pow(10, Math.floor(Math.log10(Math.abs(num))));
        const normalized = num / magnitude;
        let roundedNormalized;
        if (normalized <= 1) roundedNormalized = 1;
        else if (normalized <= 2) roundedNormalized = 2;
        else if (normalized <= 5) roundedNormalized = 5;
        else roundedNormalized = 10;
        return roundedNormalized * magnitude;
    };

    const roundedMax = roundToNiceNumber(maxValue);
    scale.tickInterval = roundedMax / 5;
    scale.maxValue = roundedMax;
    return scaledSeries;
}

// Series name extraction helper
const extractSeriesName = (chart, series, seriesIndex, originalData, exportTitle) => {
    // 1. Cek nama series yang valid
    if (series.name && series.name.trim() !== '' && series.name !== 'Drilldown Data' && !series.name.match(/^Series \d+$/)) {
        return series.name;
    }

    // 2. Cek original data
    if (originalData?.series?.[seriesIndex]?.name?.trim()) {
        return originalData.series[seriesIndex].name;
    }

    // 3. Cek user options
    if (series.userOptions?.name?.trim()) {
        return series.userOptions.name;
    }

    // 4. Gunakan nama chart/title untuk single series
    if (chart.series.length === 1) {
        const title = originalData?.name || exportTitle;
        if (title && title.trim() !== '') {
            return title.trim();
        }
    }

    // 5. Fallback
    return chart.series.length > 1 ? `Data ${seriesIndex + 1}` : 'Value';
};

function exportToExcel(chart, title) {
    try {
        if (typeof XLSX === 'undefined') {
            console.error('SheetJS library not loaded. Please include XLSX library.');
            alert('Excel export library not available.');
            return;
        }

        const data = [[title], []];
        const headers = ['CATEGORIES'];
        const chartId = chart.renderTo.id;
        const originalData = originalChartData[chartId];

        chart.series.forEach((series, seriesIndex) => {
            const seriesName = extractSeriesName(chart, series, seriesIndex, originalData, title);
            if (chart.series.length === 1) {
                headers.push("DATA");
            } else {
                headers.push(seriesName.toUpperCase());
            }
        });

        data.push(headers);
        console.log(headers);
        let categories = chart.xAxis[0].categories;
        if (!categories?.length) {
            categories = [];
            if (chart.series[0]?.data) {
                chart.series[0].data.forEach(point => {
                    if (point.name) categories.push(point.name);
                });
            }



        }

        if (categories?.length) {
            categories.forEach((category, index) => {
                const row = [category];

                chart.series.forEach((series, seriesIndex) => {
                    if (series.data?.[index]) {
                        const value = series.data[index].originalValue || series.data[index].y || 0;
                        row.push(value);
                    } else {
                        row.push(0);
                    }
                });
                data.push(row);
            });
        } else if (chart.series[0]?.data) {
            chart.series[0].data.forEach((point, index) => {
                const categoryName = point.name || point.category || `Item ${index + 1}`;


                const row = [categoryName];
                chart.series.forEach(series => {
                    if (series.data?.[index]) {
                        const value = series.data[index].originalValue || series.data[index].y || 0;
                        row.push(value);
                    } else {
                        row.push(0);
                    }
                });
                data.push(row);
            });
        }

        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.aoa_to_sheet(data);

        if (ws['A1']) {
            ws['A1'].s = {
                font: { bold: true, sz: 14 },
                alignment: { horizontal: 'center' }
            };
        }

        XLSX.utils.book_append_sheet(wb, ws, 'Chart Data');
        const filename = title.replace(/\s+/g, '_').toLowerCase() + '.xlsx';
        XLSX.writeFile(wb, filename);

    } catch (error) {
        console.error('Error exporting to Excel:', error);
        alert('Error exporting to Excel. Please try again.');
    }
}

function exportToCSV(chart, title) {
    try {
        const data = [[title], []];
        const headers = ['CATEGORIES'];
        const chartId = chart.renderTo.id;
        const originalData = originalChartData[chartId];

        chart.series.forEach((series, seriesIndex) => {
            const seriesName = extractSeriesName(chart, series, seriesIndex, originalData, title);
            if (chart.series.length === 1) {
                headers.push("DATA");
            } else {
                headers.push(seriesName.toUpperCase());
            }
        });

        data.push(headers);

        let categories = chart.xAxis[0].categories;
        if (!categories?.length) {
            categories = [];
            if (chart.series[0]?.data) {
                chart.series[0].data.forEach(point => {
                    if (point.name) categories.push(point.name);
                });
            }
        }

        if (categories?.length) {
            categories.forEach((category, index) => {
                const row = [category];

                chart.series.forEach((series, seriesIndex) => {
                    if (series.data?.[index]) {
                        const value = series.data[index].originalValue || series.data[index].y || 0;
                        row.push(value);
                    } else {
                        row.push(0);
                    }
                });
                data.push(row);
            });
        } else if (chart.series[0]?.data) {
            chart.series[0].data.forEach((point, index) => {
                const categoryName = point.name || point.category || `Item ${index + 1}`;
                const row = [categoryName];
                chart.series.forEach(series => {
                    if (series.data?.[index]) {
                        const value = series.data[index].originalValue || series.data[index].y || 0;
                        row.push(value);
                    } else {
                        row.push(0);
                    }
                });
                data.push(row);
            });
        }

        // Convert array to CSV string
        const csvContent = data.map(row =>
            row.map(cell => {
                if (typeof cell === 'string' && (cell.includes(',') || cell.includes('"') || cell.includes('\n'))) {
                    return `"${cell.replace(/"/g, '""')}"`;
                }
                return cell;
            }).join(',')
        ).join('\n');

        // Download CSV
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", title.replace(/\s+/g, '_').toLowerCase() + ".csv");
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);

    } catch (error) {
        console.error('Error exporting to CSV:', error);
        alert('Error exporting to CSV. Please try again.');
    }
}
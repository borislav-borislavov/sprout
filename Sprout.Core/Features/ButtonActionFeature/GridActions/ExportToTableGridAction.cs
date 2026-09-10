using Microsoft.Win32;
using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Features.ButtonActions;
using Sprout.Core.Services.Clipboard;
using Sprout.Core.SproutControlVMs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Windows;

namespace Sprout.Core.Features.ButtonActionFeature.GridActions;

internal class ExportToTableGridAction : IButtonAction
{
    private readonly string _ownControlName;
    private readonly IClipboardService _clipboardService;

    public ExportToTableGridAction(string ownControlName, IClipboardService clipboardService)
    {
        _ownControlName = ownControlName;
        _clipboardService = clipboardService;
    }

    public Task Perform(VMRegistry vmRegistry, IDataServiceFactory dataServiceFactory)
    {
        var ownDataAdapter = vmRegistry.GetAdapterOrThrow(_ownControlName);

        var data = ownDataAdapter.DataProvider.Data;

        if (data == null || data.Rows.Count == 0)
            return Task.CompletedTask;

        CopyDataTableToClipboard(data, vmRegistry);

        return Task.CompletedTask;
    }

    public void CopyDataTableToClipboard(DataTable table, VMRegistry vmRegistry)
    {
        var fragment = new StringBuilder();

        fragment.Append("<table border=\"1\" cellspacing=\"0\" cellpadding=\"3\">");

        // Respect the user's column settings (visibility and order) coming from the grid.
        var orderedColumnNames = GetExportColumnNames(table, vmRegistry);

        // Header
        fragment.Append("<thead><tr>");
        foreach (var colName in orderedColumnNames)
        {
            fragment.Append("<th>");
            fragment.Append(WebUtility.HtmlEncode(colName));
            fragment.Append("</th>");
        }
        fragment.Append("</tr></thead>");

        // Data
        fragment.Append("<tbody>");

        foreach (DataRow row in table.Rows)
        {
            fragment.Append("<tr>");

            foreach (var colName in orderedColumnNames)
            {
                fragment.Append("<td>");
                fragment.Append(WebUtility.HtmlEncode(row[colName]?.ToString() ?? ""));
                fragment.Append("</td>");
            }

            fragment.Append("</tr>");
        }

        fragment.Append("</tbody>");
        fragment.Append("</table>");

        string html = CreateClipboardHtml(fragment.ToString());

        //Clipboard.SetData(DataFormats.Html, html);
        _clipboardService.SetHtml(html);
    }

    private static string CreateClipboardHtml(string htmlFragment)
    {
        const string headerTemplate =
            "Version:1.0\r\n" +
            "StartHTML:{0:D10}\r\n" +
            "EndHTML:{1:D10}\r\n" +
            "StartFragment:{2:D10}\r\n" +
            "EndFragment:{3:D10}\r\n";

        const string startFragment = "<!--StartFragment-->";
        const string endFragment = "<!--EndFragment-->";

        string html =
            "<html><body>" +
            startFragment +
            htmlFragment +
            endFragment +
            "</body></html>";

        // First construct with dummy offsets so we can calculate the
        // actual byte offsets.
        string header = string.Format(
            headerTemplate,
            0, 0, 0, 0);

        int startHtml = Encoding.UTF8.GetByteCount(header);
        int startFragmentOffset =
            startHtml +
            Encoding.UTF8.GetByteCount("<html><body>" + startFragment);

        int endFragmentOffset =
            startFragmentOffset +
            Encoding.UTF8.GetByteCount(htmlFragment);

        int endHtml =
            startHtml +
            Encoding.UTF8.GetByteCount(html);

        header = string.Format(
            headerTemplate,
            startHtml,
            endHtml,
            startFragmentOffset,
            endFragmentOffset);

        return header + html;
    }

    /// <summary>
    /// Determines which DataTable columns to export and in what order, honoring the
    /// grid's column settings (visibility and display order) when available.
    /// Falls back to all non-internal columns in their natural order.
    /// </summary>
    private List<string> GetExportColumnNames(DataTable data, VMRegistry vmRegistry)
    {
        var availableColumns = new List<string>();
        foreach (System.Data.DataColumn column in data.Columns)
        {
            if (!column.ColumnName.StartsWith("_"))
                availableColumns.Add(column.ColumnName);
        }

        var gridState = vmRegistry.Get<SproutDataGridVM>(_ownControlName);
        var visibleKeys = gridState?.Grid?.GetVisibleColumnKeysInDisplayOrder();

        if (visibleKeys == null || visibleKeys.Count == 0)
            return availableColumns;

        var ordered = new List<string>();
        foreach (var key in visibleKeys)
        {
            if (data.Columns.Contains(key) && !key.StartsWith("_"))
                ordered.Add(key);
        }

        return ordered.Count > 0 ? ordered : availableColumns;
    }
}
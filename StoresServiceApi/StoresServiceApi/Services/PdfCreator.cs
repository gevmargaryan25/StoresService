using PdfSharp.Drawing.Layout;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.Snippets.Drawing;
using PdfSharp;
using System.Numerics;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using iTextSharp.text.html.simpleparser;
using Autofac.Extensions.DependencyInjection;
using Autofac;

namespace StoresServiceApi.Services
{
    public class PdfCreator : IPdfCreator
    {
        public async Task<byte[]> Create(string content)
        {
            using (var document = new PdfDocument())
            {
                InitializeNewPage(document, out var page, out var gfx, out var tf);

                var yPoint = 20;

                // Split content by lines
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    var columns = line.Split('\t');

                    // Determine font size and style based on the content type
                    var font = GetFont(columns);

                    if (columns.Length == 1)
                    {
                        tf.DrawString(line, font, XBrushes.Black, new XRect(20, yPoint, page.Width - 40, page.Height - 40));
                        yPoint += 30;
                    }
                    else
                    {
                        var xPoint = 20;
                        foreach (var column in columns)
                        {
                            tf.DrawString(column, font, XBrushes.Black, new XRect(xPoint, yPoint, 150, 20));
                            xPoint += 150;
                        }
                        yPoint += 25;
                    }

                    // Check if the page is full
                    if (yPoint > page.Height - 40)
                    {
                        InitializeNewPage(document, out page, out gfx, out tf);
                        yPoint = 20;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    document.Save(stream);
                    return stream.ToArray();
                }
            }
        }

        private void InitializeNewPage(PdfDocument document, out PdfPage page, out XGraphics gfx, out XTextFormatter tf)
        {
            page = document.AddPage();
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
        }

        private XFont GetFont(string[] columns)
        {
            if (columns.Length == 1)
            {
                return new XFont("Verdana", 16);
            }
            else
            {
                return new XFont("Verdana", 12);
            }
        }

        //public async Task<byte[]> Create(string content)
        //{
        //    using (var document = new PdfDocument())
        //    {
        //        var page = document.AddPage();
        //        var gfx = XGraphics.FromPdfPage(page);
        //        var font = new XFont("Verdana", 12, XFontStyleEx.Regular);
        //        var tf = new XTextFormatter(gfx);

        //        var yPoint = 20;

        //        // Split content by lines
        //        var lines = content.Split('\n');
        //        foreach (var line in lines)
        //        {
        //            // Split line by tab for table formatting
        //            var columns = line.Split('\t');

        //            // Check if it is a title or table header
        //            if (columns.Length == 1)
        //            {
        //                font = new XFont("Verdana", 16, XFontStyleEx.Bold);
        //                tf.DrawString(line, font, XBrushes.Black, new XRect(20, yPoint, page.Width - 40, page.Height - 40));
        //                yPoint += 30;
        //            }
        //            else if (columns.Length > 1)
        //            {
        //                font = new XFont("Verdana", 12, XFontStyleEx.Regular);
        //                var xPoint = 20;

        //                foreach (var column in columns)
        //                {
        //                    tf.DrawString(column, font, XBrushes.Black, new XRect(xPoint, yPoint, 150, 20));
        //                    xPoint += 150; // Adjust space between columns
        //                }
        //                yPoint += 25;
        //            }

        //            // Check if the page is full
        //            if (yPoint > page.Height - 40)
        //            {
        //                page = document.AddPage();
        //                gfx = XGraphics.FromPdfPage(page);
        //                tf = new XTextFormatter(gfx);
        //                yPoint = 20;
        //            }
        //        }

        //        using (var stream = new MemoryStream())
        //        {
        //            document.Save(stream);
        //            return stream.ToArray();
        //        }
        //    }
        //}

    }
}

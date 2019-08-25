using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using NUnit.Framework;
using Selenium;
using Selenium.Umbraco;

namespace NUnit.Tests
{
#if !DEBUG
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Firefox)]
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Edge)]
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.InternetExplorer)]
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Chrome)]
#else
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Firefox)]
	[TestFixture(Selenium.Selenium.BrowserTypeEnum.Chrome)]
#endif

	public class VisualCompareTest
	{
		#region Privates

		private readonly Selenium.Selenium.BrowserTypeEnum _browserType;
		private VisualCompare _visualCompare;
		private static readonly string RootUrl1;
		private static readonly string RootUrl2;
		private static readonly string ResultImages;
		private static string PageListFile => ConfigurationManager.AppSettings["PageListFile"];

		private static readonly IEnumerable<string> PageList;
		private int _perceptualResult;
		private int _fastResult;

		private void ReportDiff(ICollection<string> lines, string pageUrl, Uri rootUrl1, Uri rootUrl2)
		{
			lines.Add("<tr>");
			lines.Add($"<td>{pageUrl}</td>");
			lines.Add($"<td><a href='{new Uri(rootUrl1, pageUrl)}'>Baseline</a></td>");
			lines.Add($"<td><a href='{new Uri(rootUrl2, pageUrl)}'>Current</a></td>");
			var baselineImagePath = _visualCompare.ImagePath(rootUrl1.ToString(), pageUrl);
			lines.Add(
				$"<td><a href='{baselineImagePath}'><img src=\"{baselineImagePath}\" alt=\"alt\" width=\"80\" height=\"80\" border=\"0\"></a></td>");
			var currentImagePath = _visualCompare.ImagePath(rootUrl2.ToString(), pageUrl);
			lines.Add(
				$"<td><a href='{currentImagePath}'><img src=\"{currentImagePath}\" alt=\"alt\" width=\"80\" height=\"80\" border=\"0\"></a></td>");
			var deltaImagePath = _visualCompare.DeltaImagePath(pageUrl);
			lines.Add(
				$"<td><a href='{deltaImagePath}'><img src=\"{deltaImagePath}\" alt=\"alt\" width=\"80\" height=\"80\" border=\"0\"></a></td>");
			lines.Add($"<td>{_fastResult}</td>");
			lines.Add($"<td>{_perceptualResult}</td>");
			lines.Add("</tr>");
		}

		private int CompareImage(string pageUrl, ErrorMetric errorMetric, int treshold)
		{
			return _visualCompare.CompareImage(pageUrl, errorMetric, treshold, RootUrl1, RootUrl2);
		}

		private void Current(bool overwrite)
		{
			_visualCompare = new VisualCompare(RootUrl2, _browserType, ResultImages);
			_visualCompare.SetPageLoadTimeOut(200000);
			if (overwrite)
			{
				EraseFolderContent(_visualCompare.ImagePath(RootUrl2, "xx"));
			}

			foreach (var pageUrl in PageList)
			{
				TakeScreenshot(RootUrl2, pageUrl, _visualCompare.ImagePath(RootUrl2, pageUrl), overwrite);
			}
		}

		private bool Baseline(bool overwrite)
		{
			_visualCompare = new VisualCompare(RootUrl1, _browserType, ResultImages);
			_visualCompare.SetPageLoadTimeOut(200000);

			if (overwrite)
			{
				EraseFolderContent(_visualCompare.ImagePath(RootUrl1, "xx"));
			}

			var ok = true;
			foreach (var pageUrl in PageList)
			{
				try
				{
					TakeScreenshot(RootUrl1, pageUrl, _visualCompare.ImagePath(RootUrl1, pageUrl), overwrite);
				}
				catch (Exception e)
				{
					ok = false;
					Console.WriteLine($"{pageUrl} file://{_visualCompare.ImagePath(RootUrl1, pageUrl)} {e.Message}");
				}
			}
			return ok;
		}

		private static void EraseFolderContent(string path)
		{

			var di = new DirectoryInfo(Path.GetDirectoryName(path));
			foreach (var file in di.GetFiles())
			{
				file.Delete();
			}
		}

		private void TakeScreenshot(string root, string pageUrl, string imagePath, bool overwrite)
		{
			if (!overwrite && File.Exists(imagePath))
			{
				return;
			}
			var uri = new Uri(new Uri(root), pageUrl);
			_visualCompare.Selenium.Driver.Navigate().GoToUrl(uri);
			//Navigate 2nd time to get better results
			_visualCompare.Selenium.Driver.Navigate().GoToUrl(uri);
			_visualCompare.TakeScreenshot(imagePath);
		}

		private static string GetConfigValue(IEnumerable<string> currentConfig, string key)
		{
			var line = currentConfig.First(s => s.StartsWith($"${key}="));
			return line.Split('=')[1];
		}

		#endregion

		static VisualCompareTest()
		{
			var currentConfig = File.ReadLines(PageListFile).Where(s => !(s.StartsWith("#") || string.IsNullOrEmpty(s))).ToList();
			RootUrl1 = GetConfigValue(currentConfig, "Root1");
			RootUrl2 = GetConfigValue(currentConfig, "Root2");
			ResultImages = GetConfigValue(currentConfig, "ResultImages");

			PageList = currentConfig.Where(s => !s.StartsWith("$")).ToList();
		}

		public VisualCompareTest(Selenium.Selenium.BrowserTypeEnum browserType)
		{
			_browserType = browserType;
		}

		[SetUp]
		public void SetUp()
		{
			if (!Selenium.Selenium.CheckBrowserEnabled(_browserType))
			{
				throw new InconclusiveException($"Browser {_browserType} disabled by configuration");
			}
		}

        [TearDown]
        public void Cleanup() => _visualCompare?.Selenium?.Quit();

        [Test]
		public void CreateBaselineImages()
		{

			var ok = Baseline(true);
			Assert.IsTrue(ok, "See console for details");
		}

		[Test]
		public void CreateBaselineImagesContinue()
		{
			var ok = Baseline(false);
			Assert.IsTrue(ok, "See console for details");
		}

        [Test]
        public void CreateCurrentImages() => Current(true);

        [Test]
        public void CreateCurrentImagesContinue() => Current(false);

        [Test]
		public void CompareResults()
		{
			var fuzzMax = 0;
			var perceptualMax = 0;
			var fastWarning = 50;
			var fastFail = 96;
			var perceptualWarning = 300;
			var perceptualFail = 1500;
			_visualCompare = new VisualCompare(_browserType, ResultImages);

			var imageRootPath = _visualCompare.ImageRootPath("CompareResults.html");
			var lines = new List<string>();

			Console.WriteLine($"Result: file://{imageRootPath}");
			try
			{
				var baselineUrl = new Uri(RootUrl1);
				var currenturl = new Uri(RootUrl2);
				lines.Add("<table>");
				lines.Add("<tr>");
				lines.Add("<th></th>");
				lines.Add($"<th>{RootUrl1}</th>");
				lines.Add($"<th>{RootUrl2}</th>");
				lines.Add("<th>Baseline</th>");
				lines.Add("<th>Current</th>");
				lines.Add("<th>Delta</th>");
				lines.Add("<th>PeakAbsolute</th>");
				lines.Add("<th>PerceptualHash</th>");
				lines.Add("</tr>");

				foreach (var pageUrl in PageList)
				{
					_perceptualResult = -1;
					var errorMetric = ErrorMetric.PeakAbsolute;
					_fastResult = CompareImage(pageUrl, errorMetric, fastFail);

					if (_fastResult > fastFail)
					{
						ReportDiff(lines, pageUrl, baselineUrl, currenturl);
					}
					else if (_fastResult > fastWarning)
					{
						errorMetric = ErrorMetric.PerceptualHash;
						_perceptualResult = CompareImage(pageUrl, errorMetric, perceptualWarning);
						perceptualMax = Math.Max(perceptualMax, _perceptualResult);
						if (_perceptualResult > perceptualWarning)
						{
							ReportDiff(lines, pageUrl, baselineUrl, currenturl);
						}
						
					}
				}
				lines.Add("</table>");
			}
			finally
			{
				File.WriteAllLines(imageRootPath, lines);
			}

			if (fuzzMax > 100 || perceptualMax > perceptualFail)
			{
				Assert.Fail($"fuzzMax = {fuzzMax}, perceptualMax = {perceptualMax}");
			}
			if (fuzzMax > 50 || perceptualMax > perceptualWarning)
			{
				Assert.Inconclusive($"fuzzMax = {fuzzMax}, perceptualMax = {perceptualMax}");
			}
		}

	}
}

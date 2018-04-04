using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ImageMagick;
using OpenQA.Selenium;
using Selenium.Interfaces;

namespace Selenium
{
	public class VisualCompare
	{
		private readonly Selenium.BrowserTypeEnum _browserType;
		private readonly string _imagesRoot;

		private readonly ScreenshotImageFormat _imageFormat;
		/// <summary>
		/// Replace illegal chars and make sure that the whole path is shorter than 248 chars
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static string ReplaceIllegalChars(string name)
		{
			var illegalChars = new List<string> {":","/", "?", "%"};
			var shortName = name;
			if (name.Length > 50)
			{
				shortName = name.Substring(1, 50) + name.GetHashCode();
			}
			var sb = new StringBuilder();
			var normalized = shortName.Normalize(NormalizationForm.FormD);
			foreach (var c in normalized)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					sb.Append(c);
				}
			}
			var result = sb.ToString();
			return illegalChars.Aggregate(result, (current, illegalChar) => current.Replace(illegalChar, "-"));
		}

		//private string actualRootUrl;

		public string Images(string rootUrl)
		{
			return Path.Combine(_imagesRoot, Url2Path(rootUrl), _browserType.ToString());
		}
		public string DeltaImages => Path.Combine(_imagesRoot, _browserType.ToString(), "Delta");
		public Exception LastException { get; set; }

		public VisualCompare(string baseUrl, Selenium.BrowserTypeEnum browserType, string imagesRoot):this(browserType, imagesRoot)
		{
			//actualRootUrl = baseUrl;
			Selenium = new Selenium(baseUrl, browserType);
		}
		/// <summary>
		/// Constructor for image compare, no browser started
		/// </summary>
		/// <param name="browserType"></param>
		/// <param name="imagesRoot"></param>
		public VisualCompare(Selenium.BrowserTypeEnum browserType, string imagesRoot)
		{
			_browserType = browserType;
			_imagesRoot = imagesRoot;
			_imageFormat = ScreenshotImageFormat.Png;
		}

		public void TakeScreenshot(string fileName)
		{
			var screenshot = ((ITakesScreenshot) Selenium.Driver).GetScreenshot();
			screenshot.SaveAsFile(fileName, _imageFormat);
		}

		public ISelenium Selenium { get; }

		public string ImageRootPath(string fileName)
		{
			var path = Path.Combine(_imagesRoot,_browserType.ToString(), ReplaceIllegalChars(fileName));
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			return path;
		}

		private string Url2Path(string url)
		{
			if (url != null)
			{
				var uri = new Uri(url);
				return ReplaceIllegalChars(uri.DnsSafeHost);
			}
			else
			{
				return string.Empty;
			}
		}

		public string ImagePath(string rootUrl, string name)
		{
			var currentImagePath = Path.Combine(Images(rootUrl), $"{ReplaceIllegalChars(name)}.png");
			Directory.CreateDirectory(Path.GetDirectoryName(currentImagePath));
			return currentImagePath;
		}

		public string DeltaImagePath(string name)
		{
			var currentImagePath = Path.Combine(DeltaImages, $"{ReplaceIllegalChars(name)}.png");
			Directory.CreateDirectory(Path.GetDirectoryName(currentImagePath));
			return currentImagePath;
		}

		public int CompareImage(string name, ErrorMetric errorMetric, int imageWriteTreshold, string rootUrl1, string rootUrl2)
		{
			var activeImage = new MagickImage(ImagePath(rootUrl2,name));
			var approvedImage = new MagickImage(ImagePath(rootUrl1,name));

			//Assert.IsNotNull(activeImage);
			//Assert.IsNotNull(approvedImage);
			try
			{
				using (var delta = new MagickImage())
				{
					var compareResult = Convert.ToInt32(Math.Round(activeImage.Compare(approvedImage, errorMetric, delta)*100));

					if (compareResult > imageWriteTreshold)
					{
						delta.Write(DeltaImagePath(name));
					}
					return Convert.ToInt32(compareResult);
				}
			}
			catch (Exception ex)
			{
				LastException = ex;
				return 0;
			}

		}

		public void SetPageLoadTimeOut(int timeoutMilliSeconds)
		{
			//Selenium.Driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromMilliseconds(timeoutMilliSeconds));
			Selenium.Driver.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(timeoutMilliSeconds);
		}

	}
}
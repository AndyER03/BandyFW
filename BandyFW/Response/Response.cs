using System.Collections.Generic;

namespace BandyFW.Model
{
	public class Response
	{
		public string firmwareVersion { get; set; }
		public string firmwareUrl { get; set; }
		public string deviceType { get; set; }
		public int deviceSource { get; set; }
		public string firmwareMd5 { get; set; }
		public int firmwareLength { get; set; }
		public int firmwareFlag { get; set; }
		public string fontMd5 { get; set; }
		public int fontLength { get; set; }
		public int fontVersion { get; set; }
		public int fontFlag { get; set; }
		public string fontUrl { get; set; }
		public int resourceVersion { get; set; }
		public int resourceFlag { get; set; }
		public string resourceUrl { get; set; }
		public string resourceMd5 { get; set; }
		public int resourceLength { get; set; }
		public string lang { get; set; }
		public int productionSource { get; set; }
		public string changeLog { get; set; }
		public int upgradeType { get; set; }
		public long buildTime { get; set; }
		public bool ignore { get; set; }
		public bool support8Bytes { get; set; }
		public List<string> downloadBackupPaths { get; set; }
	}
}
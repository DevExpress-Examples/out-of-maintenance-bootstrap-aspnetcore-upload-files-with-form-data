using System;

namespace UploadControlCore.Models {
    public class FileInfo {
        public Guid PortfolioId { get; set; }
        public string FileName { get; set; }
        public byte[] Bytes { get; set; }
        public long Length { get; set; }
        public DateTime Created { get; set; }
    }
}

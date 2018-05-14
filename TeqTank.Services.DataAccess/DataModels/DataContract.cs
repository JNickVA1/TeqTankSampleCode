using System;

namespace TeqTank.Services.DataAccess.DataModels
{
	public class DataContract
    {
        public dynamic Data { get; set; }
        public dynamic DataHeader { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCd { get; set; }
        public Guid Transaction { get; set; }
    }
}

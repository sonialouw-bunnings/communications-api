namespace Communications.API.Contracts
{
	public abstract class BaseResponse
	{
		public BaseResponse()
		{
			Errors = new List<Error>();
		}

		public bool Success { get; set; }
		public List<Error> Errors { get; set; }
	}
}


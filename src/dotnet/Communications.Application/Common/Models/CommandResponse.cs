namespace Communications.Application.Common.Models
{
	public class CommandResponse<T>
	{
		public List<Error> Errors { get; set; }
		public T Data { get; set; }
	}
}
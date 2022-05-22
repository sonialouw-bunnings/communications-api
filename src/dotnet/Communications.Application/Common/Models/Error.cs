using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communications.Application.Common.Models
{
	public abstract class Error
	{
		public string Field { get; set; } = string.Empty;
		public string Message { get; set; }


		public override string ToString()
		{
			return Message;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communications.Application.Common.Models;

namespace Communications.Application.Common.Errors
{
	public class InvalidEmailAddressError : Error
	{
		public InvalidEmailAddressError(string field, string emailAddress)
		{
			Field = field;
			Message = $"{emailAddress} is not a valid email address";
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communications.Application.Common.Models;

namespace Communications.Application.Common.Errors
{
	public class RequiredFieldMissingError : Error
		{
			public RequiredFieldMissingError(string field, string message)
			{
				Field = field;
				Message = message;
			}
		}
}

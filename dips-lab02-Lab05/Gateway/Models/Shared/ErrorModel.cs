using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models.Shared
{
    public class ErrorModel
    {
        public ErrorModel(ObjectResult objectResult)
        {
            this.Code = objectResult.StatusCode;
            this.Message = objectResult.Value as string;
        }
        public int? Code { get; set; }
        public string Message { get; set; }
    }
}

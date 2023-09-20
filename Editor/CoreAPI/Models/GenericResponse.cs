using System;
using AmazonGameLiftPlugin.Core.Shared;

namespace Editor.CoreAPI.Models
{
    public class GenericResponse : Response
    {
        internal GenericResponse()
        {
        }
        
        public GenericResponse(string errorCode, string errorMessage = null)
        {
            if (errorCode is null)
            {
                throw new ArgumentNullException(nameof(errorCode));
            }

            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}
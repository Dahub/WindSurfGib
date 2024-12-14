using System;

namespace WindSurfApi.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }

        public BusinessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ResourceNotFoundException : BusinessException
    {
        public ResourceNotFoundException(string message) : base(message)
        {
        }
    }

    public class InvalidBusinessDataException : BusinessException
    {
        public InvalidBusinessDataException(string message) : base(message)
        {
        }
    }
}

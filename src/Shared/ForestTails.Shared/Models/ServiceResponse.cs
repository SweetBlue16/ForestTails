using ForestTails.Shared.Enums;
using System.Runtime.Serialization;

namespace ForestTails.Shared.Models
{
    [DataContract]
    public class ServiceResponse<T>
    {
        [DataMember]
        public T? Data { get; set; }
        [DataMember]
        public MessageCode MessageCode { get; set; }
        [DataMember]
        public string Message { get; set; } = string.Empty;
        [DataMember]
        public bool IsSuccess { get; set; }

        public ServiceResponse() {}

        public static ServiceResponse<T> SuccessResult(T data, string message = "OK") => new()
        {
            Data = data,
            MessageCode = MessageCode.Success,
            Message = message,
            IsSuccess = true
        };

        public static ServiceResponse<T> FailureResult(MessageCode messageCode, string message) => new()
        {
            Data = default,
            MessageCode = messageCode,
            Message = message,
            IsSuccess = false
        };
    }
}

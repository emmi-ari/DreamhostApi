namespace ApiClient.Model
{
    public class ModifyResponseModel
    {
        /// <summary>
        /// Success message of command
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Describes the performed action
        /// </summary>
        public string Data { get; set; }
    }
}

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class DeskbandServiceRunner : IpcClientServiceRunner
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public DeskbandServiceRunner(IpcClientService service) : base("deskband", service)
        {
        }

        #endregion
    }
}

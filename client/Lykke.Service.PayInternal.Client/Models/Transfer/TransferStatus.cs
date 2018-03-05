namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    /// <summary>
    /// Transfer Status
    /// </summary>
    public enum TransferStatus
    {
        /// <summary>
        /// In Progress. The status apply if the transaction was registred in blockchain successful 
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        InProgress,
        /// <summary>
        /// Completed. The status apply when PRC transaction does have enought comfirmations
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        Completed,
        /// <summary>
        /// Error. The status apply when somthing wrong happens
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        Error
    }
}

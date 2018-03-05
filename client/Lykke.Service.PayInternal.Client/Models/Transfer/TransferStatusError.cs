namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    /// <summary>
    /// Transfer status error. If Transfer status is error, the property describe why. If status is not Error - None present.
    /// </summary>
    public enum TransferStatusError
    {
        /// <summary>
        /// No errors. If status of transaction request is not Error.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        None = 0,
        /// <summary>
        /// Transaction is not confirmed.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        NotConfirmed,
        /// <summary>
        /// Show invalid amount. Means source wallets could have not enought amount or amount is not enought to pay fee.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        InvalidAmount,
        /// <summary>
        /// Invalid address.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        InvalidAddress,
        /// <summary>
        /// Any other errors. Take a look thow the logs.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        InternalError
    }
}

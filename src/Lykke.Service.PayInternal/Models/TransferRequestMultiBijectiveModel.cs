using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lykke.Service.PayInternal.Models
{
    /// <summary>
    /// The data to be obtained by TransferRequestController via http-request for multiple bijective transfer.
    /// </summary>
    public class TransferRequestMultiBijectiveModel : ITransferRequestModel
    {
        /// <summary>
        /// ID of the Merchant who is responsible for money transfer.
        /// </summary>
        [Required]
        public string MerchantId { get; set; }
        /// <summary>
        /// The list of pair addresses for source-destination with the amount of transfer specified.
        /// </summary>
        [Required]
        public List<BiAddressAmount> BiAddresses { get; set; }

        /// <summary>
        /// Indicates the first revealed validation error.
        /// </summary>
        /// <returns>The first encountered format mismatch or null if nothing was found.</returns>
        /// <remarks>The mismatches that can be found here are:
        /// * if one of bijective addresses has amount less than or equal to 0.</remarks>
        public string CheckAmountsValidity()
        {
            if (BiAddresses.Any(x => x.Amount < 0)) return "Some destination address has the requested transfer amount <= 0. The transfer is impossible.";

            return null; // No errors on this level of analysis. But transfer may fail deeper for some reasons.
        }

        /// <summary>
        /// Converts the model data to transfer request object with multiple bijective transactions: each transaction may contain 
        /// the single sources and the single destination address, but different transactions (may) have the same source or
        /// destination addresses.
        /// </summary>
        /// <returns>The newly created instance of <see cref="TransferRequest"/>.</returns>
        public ITransferRequest ToTransferRequest()
        {
            // Please, note: there is no additional call for CheckAmountsValidity(). It is assumed that the caller code already tested it.
            var result = new TransferRequest()
            {
                TransferId = Guid.NewGuid().ToString(),
                TransferStatus = TransferStatus.InProgress,
                TransferStatusError = TransferStatusError.None,
                CreateDate = DateTime.Now,
                MerchantId = MerchantId
            };

            result.TransactionRequests = new List<ITransactionRequest>();
            
            // Just translate BiAddresses to a list of single-source transactions
            foreach (var biad in BiAddresses)
            {
                var newTransaction = new TransactionRequest();
                newTransaction.Amount = biad.Amount;
                newTransaction.CountConfirm = 1; // TODO: maybe it would be more awesome to move this constant to settings
                newTransaction.AssetId = LykkeConstants.BitcoinAssetId;
                newTransaction.DestinationAddress = biad.DestinationAddress;
                newTransaction.SourceAmounts = new List<IAddressAmount>();

                newTransaction.SourceAmounts.Add(new AddressAmount()
                {
                    Address = biad.SourceAddress,
                    Amount = biad.Amount
                });

                result.TransactionRequests.Add(newTransaction);
            }

            return result;
        }
    }
}

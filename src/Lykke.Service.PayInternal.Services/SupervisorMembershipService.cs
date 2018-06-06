﻿using Lykke.Service.PayInternal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.AzureRepositories;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Exceptions;
using KeyNotFoundException = Lykke.Service.PayInternal.Core.Exceptions.KeyNotFoundException;

namespace Lykke.Service.PayInternal.Services
{
    public class SupervisorMembershipService : ISupervisorMembershipService
    {
        private readonly ISupervisorMembershipRepository _supervisorMembershipRepository;
        private readonly IMerchantGroupService _merchantGroupService;
        private readonly ILog _log;
        
        public SupervisorMembershipService(
            [NotNull] ISupervisorMembershipRepository supervisorMembershipRepository,
            [NotNull] ILog log, 
            [NotNull] IMerchantGroupService merchantGroupService)
        {
            _supervisorMembershipRepository = supervisorMembershipRepository ?? throw new ArgumentNullException(nameof(supervisorMembershipRepository));
            _merchantGroupService = merchantGroupService ?? throw new ArgumentNullException(nameof(merchantGroupService));
            _log = log.CreateComponentScope(nameof(SupervisorMembershipService)) ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<IMerchantsSupervisorMembership> AddAsync(IMerchantsSupervisorMembership src)
        {
            IMerchantGroup merchantGroup = await _merchantGroupService.CreateAsync(new MerchantGroup
            {
                Merchants = string.Join(Constants.Separator, src.Merchants),
                MerchantGroupUse = MerchantGroupUse.Supervising,
                OwnerId = src.MerchantId
            });

            try
            {
                ISupervisorMembership membership = await _supervisorMembershipRepository.AddAsync(
                    new SupervisorMembership
                    {
                        MerchantId = src.MerchantId,
                        EmployeeId = src.EmployeeId,
                        MerchantGroups = new[] {merchantGroup.Id}
                    });

                return new MerchantsSupervisorMembership
                {
                    MerchantId = membership.MerchantId,
                    EmployeeId = membership.EmployeeId,
                    Merchants = merchantGroup.Merchants.Split(Constants.Separator)
                };
            }
            catch (DuplicateKeyException ex)
            {
                _log.WriteError(nameof(AddAsync), src, ex);

                throw new SupervisorMembershipAlreadyExistsException(src.EmployeeId);
            }
        }

        public Task<ISupervisorMembership> GetAsync(string employeeId)
        {
            return _supervisorMembershipRepository.GetAsync(employeeId);
        }

        public async Task<IMerchantsSupervisorMembership> GetWithMerchantsAsync(string employeeId)
        {
            ISupervisorMembership membership = await _supervisorMembershipRepository.GetAsync(employeeId);

            if (membership != null)
            {
                var merchants = new List<string>();

                foreach (string merchantGroupId in membership.MerchantGroups)
                {
                    IMerchantGroup merchantGroup = await _merchantGroupService.GetAsync(merchantGroupId);

                    if (merchantGroup != null)
                        merchants.AddRange(merchantGroup.Merchants.Split(Constants.Separator));
                }

                return new MerchantsSupervisorMembership
                {
                    MerchantId = membership.MerchantId,
                    EmployeeId = membership.EmployeeId,
                    Merchants = merchants.Distinct()
                };
            }

            return null;
        }

        public Task UpdateAsync(ISupervisorMembership src)
        {
            try
            {
                return _supervisorMembershipRepository.UpdateAsync(src);
            }
            catch (KeyNotFoundException ex)
            {
                _log.WriteError(nameof(UpdateAsync), src, ex);

                throw new SupervisorMembershipNotFoundException(src.EmployeeId);
            }
        }

        public Task<ISupervisorMembership> AddAsync(ISupervisorMembership src)
        {
            try
            {
                return _supervisorMembershipRepository.AddAsync(src);
            }
            catch (DuplicateKeyException ex)
            {
                _log.WriteError(nameof(AddAsync), src, ex);

                throw new SupervisorMembershipAlreadyExistsException(src.EmployeeId);
            }
        }

        public async Task RemoveAsync(string employeeId)
        {
            try
            {
                await _supervisorMembershipRepository.RemoveAsync(employeeId);
            }
            catch (KeyNotFoundException ex)
            {
                _log.WriteError(nameof(RemoveAsync), new {employeeId}, ex);

                throw new SupervisorMembershipNotFoundException(employeeId);
            }
        }
    }
}

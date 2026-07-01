// Copyright 2020-present Etherna SA
// This file is part of Etherna SDK .Net.
// 
// Etherna SDK .Net is free software: you can redistribute it and/or modify it under the terms of the
// GNU Lesser General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna SDK .Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License along with Etherna SDK .Net.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.Sdk.Sso.GenClients;
using Etherna.SwarmSdk.Models;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.Sdk.Users.Sso.Models
{
    public class PrivateUserInfo
    {
        internal PrivateUserInfo(PrivateUserDto privateInfo)
        {
            AccountType = privateInfo.AccountType;
            Email = privateInfo.Email;
            EtherAddress = privateInfo.EtherAddress;
            EtherManagedPrivateKey = privateInfo.EtherManagedPrivateKey;
            EtherPreviousAddresses = privateInfo.EtherPreviousAddresses.Select(EthAddress.FromString).ToArray();
            EtherLoginAddress = privateInfo.EtherLoginAddress is null
                ? null
                : (EthAddress?)EthAddress.FromString(privateInfo.EtherLoginAddress);
            PhoneNumber = privateInfo.PhoneNumber;
            Username = privateInfo.Username;
        }
        
        public string AccountType { get; }
        public string? Email { get; }
        public EthAddress EtherAddress { get; }
        public string? EtherManagedPrivateKey { get; }
        public IEnumerable<EthAddress> EtherPreviousAddresses { get; }
        public EthAddress? EtherLoginAddress { get; }
        public string? PhoneNumber { get; }
        public string? Username { get; }
    }
}
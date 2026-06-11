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

using Etherna.Sdk.Credit.GenClients;
using Etherna.SwarmSdk.Models;
using System;

namespace Etherna.Sdk.Internal.Models
{
    public class UserOpLog
    {
        internal UserOpLog(OperationLogDto opLog)
        {
            Amount = opLog.Amount;
            Author = opLog.Author;
            CreationDateTime = opLog.CreationDateTime;
            IsApplied = opLog.IsApplied;
            OperationName = opLog.OperationName;
            Reason = opLog.Reason;
            UserAddress = opLog.UserAddress;
        }
        
        public XDaiValue Amount { get; }
        public string Author { get; }
        public DateTimeOffset CreationDateTime { get; }
        public bool? IsApplied { get; }
        public string OperationName { get; }
        public string? Reason { get; }
        public string UserAddress { get; }
    }
}
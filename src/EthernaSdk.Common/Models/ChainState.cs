// Copyright 2020-present Etherna SA
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Etherna.Sdk.Common.Models
{
    public class ChainState
    {
        // Constructors.
        internal ChainState(GenClients.Gateway.ChainStateDto chainState)
        {
            Block = chainState.Block;
            CurrentPrice = chainState.CurrentPrice;
            SourceNodeId = chainState.SourceNodeId;
            TimeStamp = chainState.TimeStamp;
            TotalAmount = chainState.TotalAmount;
        }
        
        // Properties.
        public long Block { get; }
        public long CurrentPrice { get; }
        public string SourceNodeId { get; }
        public System.DateTimeOffset TimeStamp { get; }
        public long TotalAmount { get; }
    }
}
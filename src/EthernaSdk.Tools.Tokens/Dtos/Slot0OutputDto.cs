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

using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace Etherna.Sdk.Tools.Tokens.Dtos
{
    [FunctionOutput]
    internal sealed class Slot0OutputDto
    {
        [Parameter("uint160", "sqrtPriceX96", 1)]
        public BigInteger SqrtPriceX96 { get; set; }
    
        [Parameter("int24", "tick", 2)]
        public int Tick { get; set; }
    
        [Parameter("uint16", "observationIndex", 3)]
        public ushort ObservationIndex { get; set; }
    
        [Parameter("uint16", "observationCardinality", 4)]
        public ushort ObservationCardinality { get; set; }
    
        [Parameter("uint16", "observationCardinalityNext", 5)]
        public ushort ObservationCardinalityNext { get; set; }
    
        [Parameter("uint8", "feeProtocol", 6)]
        public byte FeeProtocol { get; set; }
    
        [Parameter("bool", "unlocked", 7)]
        public bool Unlocked { get; set; }
    }
}
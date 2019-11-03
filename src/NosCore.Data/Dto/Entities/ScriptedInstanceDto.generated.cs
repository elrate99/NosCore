//  __  _  __    __   ___ __  ___ ___  
// |  \| |/__\ /' _/ / _//__\| _ \ __| 
// | | ' | \/ |`._`.| \_| \/ | v / _|  
// |_|\__|\__/ |___/ \__/\__/|_|_\___| 
// 
// Copyright (C) 2019 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations;
using NosCore.Data.I18N;
using NosCore.Data.Dto;
using NosCore.Data.StaticEntities;
using NosCore.Data.DataAttributes;
using NosCore.Data.Enumerations.I18N;
using Mapster;

namespace NosCore.Data.Dto
{
	/// <summary>
	/// Represents a DTO class for NosCore.Database.Entities.ScriptedInstance.
	/// NOTE: This class is generated by GenerateDtos.tt
	/// </summary>
	public class ScriptedInstanceDto : IDto
	{
		[AdaptIgnore]
		public MapDto Map { get; set; }

	 	public short MapId { get; set; }

	 	public short PositionX { get; set; }

	 	public short PositionY { get; set; }

	 	public string Label { get; set; }

	 	public string Script { get; set; }

	 	[Key]
		public short ScriptedInstanceId { get; set; }

	 	public NosCore.Data.Enumerations.Interaction.ScriptedInstanceType Type { get; set; }

	 }
}
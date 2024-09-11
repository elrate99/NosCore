﻿//  __  _  __    __   ___ __  ___ ___
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

using System.Collections.Generic;
using NosCore.Data.Enumerations.I18N;
using NosCore.Shared.Enumerations;
using System.Globalization;
using Microsoft.Extensions.Localization;
using NosCore.Shared.I18N;

namespace NosCore.Core.I18N
{
    public class GameLanguageLocalizer(ILogLanguageLocalizer<LanguageKey> stringLocalizer) : IGameLanguageLocalizer
    {
        public LocalizedString this[LanguageKey key, RegionType region]
        {
            get
            {
                var currentUi = CultureInfo.CurrentUICulture;
                var current = CultureInfo.CurrentCulture;
                CultureInfo.CurrentUICulture = new CultureInfo(region.ToString());
                CultureInfo.CurrentCulture = new CultureInfo(region.ToString());

                var result = stringLocalizer[key];

                CultureInfo.CurrentUICulture = new CultureInfo("ru");
                CultureInfo.CurrentCulture = new CultureInfo("ru");
                return result;
            }
        }

        public LocalizedString this[LanguageKey key, RegionType region, params object[] arguments]
        {
            get
            {
                var currentUi = CultureInfo.CurrentUICulture;
                var current = CultureInfo.CurrentCulture;
                CultureInfo.CurrentUICulture = new CultureInfo(region.ToString());
                CultureInfo.CurrentCulture = new CultureInfo(region.ToString());

                var result = stringLocalizer[key, arguments];

                CultureInfo.CurrentUICulture = new CultureInfo("ru");
                CultureInfo.CurrentCulture = new CultureInfo("ru");
                return result;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings() => stringLocalizer.GetAllStrings();
    }
}